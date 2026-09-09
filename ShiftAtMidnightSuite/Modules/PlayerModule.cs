using System;
using Il2Cpp;
using ShiftAtMidnightSuite.Util;
using UnityEngine;
using UnityEngine.UI;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// Local player cheats. Everything here is typed against the real game fields rather than the
    /// old reflection member-guessing, so it either works or logs why it did not.
    /// </summary>
    internal sealed class PlayerModule
    {
        internal bool InfiniteStamina;
        internal bool GodMode;
        internal bool InfiniteMoney;
        internal bool InfiniteAmmo;
        internal bool FreezeItems;
        internal bool Noclip;

        internal float SpeedMultiplier = 1f;
        internal float JumpMultiplier = 1f;
        internal float NoclipSpeed = 8f;
        internal int MaxSlots;              // 0 = leave the game's value alone.
        internal int AmmoTarget;            // 0 = restore only to what the weapon actually held
        internal bool InfiniteRefreshes;
        internal float MoneyFloor = 100000f;
        internal int FundsFloor = 100000;

        // Baselines so toggling a cheat off restores the original values.
        private float _baseWalk, _baseRun, _baseCrouch, _baseJump;
        private bool _movementCaptured;
        private FPSController _movementOwner;

        // Highest per-slot stack size seen, used to pin counts while FreezeItems is on.
        private readonly System.Collections.Generic.Dictionary<int, int> _amountHighWater =
            new System.Collections.Generic.Dictionary<int, int>();
        private readonly System.Collections.Generic.Dictionary<int, int> _storageHighWater =
            new System.Collections.Generic.Dictionary<int, int>();
        private readonly System.Collections.Generic.Dictionary<int, int> _storage2HighWater =
            new System.Collections.Generic.Dictionary<int, int>();

        private bool _noclipPrevDetectCollisions;
        private bool _noclipActive;

        internal bool AnyActive
        {
            get
            {
                return InfiniteStamina || GodMode || InfiniteMoney || InfiniteAmmo || FreezeItems || Noclip
                       || InfiniteRefreshes
                       || MaxSlots > 0
                       || Math.Abs(SpeedMultiplier - 1f) > 0.001f
                       || Math.Abs(JumpMultiplier - 1f) > 0.001f;
            }
        }

        internal void OnSceneChanged()
        {
            _movementCaptured = false;
            _movementOwner = null;
            _controller = null;
            _storyMan = null;
            _slotsAppliedFor = -1;
            _grownTo = 0;
            _nextGrowCheck = 0f;
            _growFailLogged = false;   // a new HUD may well succeed where the last one could not
            _amountHighWater.Clear();
            _layoutLogged = false;
            _storageHighWater.Clear();
            _storage2HighWater.Clear();
            _noclipActive = false;
        }

        internal void Tick()
        {
            PlayerManager pm = Net.LocalPlayer;
            InventoryManager inv = Net.LocalInventory;

            if (InfiniteStamina && Net.Alive(pm)) ApplyStamina(pm);
            if (GodMode && Net.Alive(pm)) ApplyGodMode(pm);
            if (InfiniteMoney) ApplyMoney();
            if (InfiniteRefreshes) ApplyRefreshes();
            if (Net.Alive(inv))
            {
                if (MaxSlots > 0) ApplySlots(inv);
                LogInventoryLayout(inv);
                if (FreezeItems) ApplyFreezeItems(inv);
                if (InfiniteAmmo) ApplyInfiniteAmmo(inv);
            }
            ApplyMovementMultipliers();
        }

        private void ApplyStamina(PlayerManager pm)
        {
            try
            {
                float max = pm.maxStamina;
                if (max > 0f && pm.stamina < max) pm.stamina = max;
            }
            catch (Exception ex) { Log.Debug("stamina: " + ex.Message); }
        }

        private void ApplyGodMode(PlayerManager pm)
        {
            try
            {
                float max = pm.maxHealth;
                if (max <= 0f) max = 100f;
                if (pm.health < max) pm.health = max;
                pm._health = max;
                if (pm.dead) pm.dead = false;
                if (pm._downed) pm._downed = false;
            }
            catch (Exception ex) { Log.Debug("god mode: " + ex.Message); }
        }

        private void ApplyMoney()
        {
            try
            {
                SaveManager sm = SaveManager.Instance;
                if (!Net.Alive(sm)) return;
                if (sm.money < MoneyFloor) sm.money = MoneyFloor;
                if (sm.tokens < 9999) sm.tokens = 9999;

                // Story mode keeps a second, separate wallet: the personal funds Clyde bills you
                // against. It lives on the save as an int and is shown by StoryModeManager, so the
                // display is refreshed through the game's own RPC when the number actually changes.
                int funds = SaveManager.PersonalFunds;
                if (funds < FundsFloor)
                {
                    SaveManager.PersonalFunds = FundsFloor;
                    PushPersonalFunds(FundsFloor);
                }
            }
            catch (Exception ex) { Log.Debug("money: " + ex.Message); }
        }

        /// <summary>Tells every client about the new personal funds, the way the game does at load.</summary>
        private static StoryModeManager _storyMan;

        private static void PushPersonalFunds(int funds)
        {
            try
            {
                // StoryModeManager is not a singleton, so it has to be found - once, then remembered.
                if (!Net.Alive(_storyMan))
                {
                    System.Collections.Generic.List<StoryModeManager> found = Net.FindActive<StoryModeManager>();
                    _storyMan = found.Count > 0 ? found[0] : null;
                }
                if (!Net.Alive(_storyMan)) return;
                StoryModeManager smm = _storyMan;
                Rpc.Call(smm, delegate { smm.Rpc_InstantLoadStats(funds); }, "InstantLoadStats");
            }
            catch (Exception ex) { Log.Debug("personal funds: " + ex.Message); }
        }

        /// <summary>Store refreshes are a plain counter on the save; keep it topped up.</summary>
        private void ApplyRefreshes()
        {
            try
            {
                SaveManager sm = SaveManager.Instance;
                if (!Net.Alive(sm)) return;
                if (sm.refreshes < 99) sm.refreshes = 99;
            }
            catch (Exception ex) { Log.Debug("refreshes: " + ex.Message); }
        }

        private int _slotsAppliedFor = -1;
        private float _nextSlotResync;

        /// <summary>The slot count the game itself considers current: what is on the save.</summary>
        internal int SavedSlots
        {
            get
            {
                try { SaveManager sm = SaveManager.Instance; return Net.Alive(sm) ? sm.maxInventorySpace : -1; }
                catch { return -1; }
            }
        }

        internal string LastSlotResult = "";

        /// <summary>
        /// Stop overriding and put the inventory back to the save's value through the game's own
        /// RPC - the same call SaveManager and the shop upgrade use.
        /// </summary>
        internal void ResetSlotsToSave()
        {
            MaxSlots = 0;
            _slotsAppliedFor = -1;
            InventoryManager inv = Net.LocalInventory;
            int saved = SavedSlots;
            if (!Net.Alive(inv) || saved <= 0) { LastSlotResult = "Save value unavailable"; return; }
            try
            {
                inv.maxInventorySlots = saved;
                inv.Rpc_SetMaxInventorySlots(saved);
                try { inv.UpdateInventorySlotsUI(); } catch { }
                LastSlotResult = "Inventory slots reset to save value " + saved;
                Log.Msg(LastSlotResult + ".");
            }
            catch (Exception ex) { Log.Ex("reset slots", ex); LastSlotResult = "Reset failed"; }
        }

        /// <summary>
        /// Make the requested count the game's own permanent value. Once it is on the save, the 10s
        /// resync agrees with us and there is nothing left to fight.
        /// </summary>
        internal void WriteSlotsToSave()
        {
            int want = MaxSlots > 0 ? MaxSlots : SavedSlots;
            if (want <= 0) { LastSlotResult = "Nothing to write"; return; }
            try
            {
                SaveManager sm = SaveManager.Instance;
                if (!Net.Alive(sm)) { LastSlotResult = "SaveManager not ready"; return; }
                sm.maxInventorySpace = want;
                try { sm._maxInventorySpace = want; } catch { }
                try { sm.Save(); } catch (Exception ex) { Log.Debug("save: " + ex.Message); }

                InventoryManager inv = Net.LocalInventory;
                if (Net.Alive(inv))
                {
                    inv.maxInventorySlots = want;
                    inv.Rpc_SetMaxInventorySlots(want);
                    try { inv.UpdateInventorySlotsUI(); } catch { }
                }
                _slotsAppliedFor = want;
                LastSlotResult = "Save now says " + want + " slot(s)";
                Log.Msg(LastSlotResult + ".");
            }
            catch (Exception ex) { Log.Ex("write slots", ex); LastSlotResult = "Write failed"; }
        }

        /// <summary>
        /// The game re-syncs maxInventorySlots on a fixed ~10s cadence. The first version answered
        /// every resync with the RPC plus UpdateInventorySlotsUI - a full UI rebuild - which is a
        /// stutter every ten seconds. Now the expensive path runs once per requested value; the
        /// resync is answered by re-setting the plain field, which is what the UI reads anyway.
        /// </summary>
        /// <summary>Slot count this session has already built the rig out to.</summary>
        private int _grownTo;

        /// <summary>
        /// Adds inventory slots the game does not ship.
        ///
        /// A slot is three separate things: an entry in inventoryIds/inventoryAmounts (both length
        /// four), and a widget in the HUD - an Animator in inventorySlots plus two Images in
        /// inventorySprites and inventorySprites_. UpdateInventorySlotsUI walks those arrays, so
        /// raising maxInventorySlots on its own just gets clamped back and draws nothing.
        ///
        /// The data arrays are replaced with longer copies (an empty slot is id -1, amount 0, which
        /// is what the game itself leaves in an unused slot) and the widgets are cloned from the last
        /// real one, keeping whatever spacing the existing slots use.
        /// </summary>
        private float _nextGrowCheck;

        /// <summary>Per-slot arrays the game keeps in step with inventoryIds but does not expose by name here.</summary>
        private static readonly string[] SlotArrays = { "inventoryAmountTexts", "inventoryAmountTextFades" };

        private void GrowInventory(InventoryManager inv, int target)
        {
            if (target <= 4) return;

            // _grownTo used to short-circuit this outright, which meant the rig was built once and
            // never checked again. The inventory is replicated - Rpc_UpdateInventoryForAll hands back
            // arrays sized the way the host has them - so a resync can quietly shorten what was grown
            // and the extra slots stop working with nothing in the log. Re-check instead, cheaply.
            float now = Time.unscaledTime;
            if (_grownTo >= target && now < _nextGrowCheck) return;
            _nextGrowCheck = now + 0.5f;
            if (_grownTo >= target && !NeedsRegrow(inv, target)) return;

            try
            {
                // ---- data arrays
                var ids = inv.inventoryIds;
                var amounts = inv.inventoryAmounts;
                if (ids != null && amounts != null && ids.Length < target)
                {
                    int old = ids.Length;
                    var newIds = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<int>(target);
                    var newAmounts = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<int>(target);
                    for (int i = 0; i < target; i++)
                    {
                        newIds[i] = i < old ? ids[i] : -1;
                        newAmounts[i] = i < old && i < amounts.Length ? amounts[i] : 0;
                    }
                    inv.inventoryIds = newIds;
                    inv.inventoryAmounts = newAmounts;
                }

                // ---- every other array the game indexes by slot
                for (int i = 0; i < SlotArrays.Length; i++) GrowSlotArray(inv, SlotArrays[i], target);

                // ---- HUD widgets
                if (!GrowSlotWidgets(inv, target)) return;

                if (_grownTo < target) Log.Msg("Inventory rig grown to " + target + " slots.");
                _grownTo = target;
            }
            catch (Exception ex) { Log.Ex("grow inventory", ex); }
        }

        /// <summary>True when something that was grown has been shortened again behind our back.</summary>
        private static bool NeedsRegrow(InventoryManager inv, int target)
        {
            try
            {
                if (inv.inventoryIds != null && inv.inventoryIds.Length < target) return true;
                if (inv.inventoryAmounts != null && inv.inventoryAmounts.Length < target) return true;
                if (inv.inventorySlots != null && inv.inventorySlots.Length < target) return true;
                if (inv.inventorySprites != null && inv.inventorySprites.Length < target) return true;
                if (inv.inventorySprites_ != null && inv.inventorySprites_.Length < target) return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Lengthens one per-slot array by name, keeping what is in it.
        ///
        /// Done by reflection rather than by field, because these are UI arrays whose element types
        /// are not worth hard-coding against - the Il2Cpp array knows its own element type, and its
        /// size constructor and indexer are all that is needed to copy one into a longer one. An array
        /// this misses is not fatal, but one the game indexes by slot and finds too short throws
        /// inside UpdateInventorySlotsUI, which aborts the redraw and leaves the new slot blank.
        /// </summary>
        private static void GrowSlotArray(InventoryManager inv, string name, int target)
        {
            try
            {
                System.Reflection.PropertyInfo p = typeof(InventoryManager).GetProperty(
                    name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (p == null || !p.CanRead || !p.CanWrite) return;

                object cur = p.GetValue(inv);
                if (cur == null) return;

                Type t = cur.GetType();
                System.Reflection.PropertyInfo lengthProp = t.GetProperty("Length");
                System.Reflection.PropertyInfo item = t.GetProperty("Item");
                if (lengthProp == null || item == null) return;

                int len = Convert.ToInt32(lengthProp.GetValue(cur));
                if (len == 0 || len >= target) return;

                object grown = Activator.CreateInstance(t, new object[] { (long)target });
                if (grown == null) return;
                for (int i = 0; i < len; i++)
                    item.SetValue(grown, item.GetValue(cur, new object[] { i }), new object[] { i });

                p.SetValue(inv, grown);
                Log.Debug("grew " + name + " " + len + " -> " + target);
            }
            catch (Exception ex) { Log.Debug("grow " + name + ": " + ex.Message); }
        }

        /// <summary>
        /// Builds the missing HUD widgets.
        ///
        /// The first version cloned the Animator's own GameObject and then looked for the two Images
        /// underneath it. That assumption is what made extra slots dead: it only holds if the sprites
        /// are descendants of the animator, and when they are not - siblings under a shared slot
        /// object, which is the usual way to lay this out - RelativePath returned null and the whole
        /// thing bailed out. It bailed out silently, too, so ApplySlots just clamped the request back
        /// to four and the log said "Inventory slots set to 4" as though nothing had been asked for.
        ///
        /// So take the smallest object that contains all three - the animator and both images - and
        /// clone that instead, whatever its shape. Every exit now says why.
        /// </summary>
        private bool GrowSlotWidgets(InventoryManager inv, int target)
        {
            var anims = inv.inventorySlots;
            var spr = inv.inventorySprites;
            var spr2 = inv.inventorySprites_;
            if (anims == null || spr == null || spr2 == null) { GrowFailed("slot arrays are null"); return false; }
            if (anims.Length >= target) return true;

            int old = anims.Length;
            if (old == 0) { GrowFailed("there are no slot widgets to copy"); return false; }

            Transform unit = SlotUnit(anims, spr, spr2, old - 1);
            if (unit == null) { GrowFailed("could not find a common parent for slot " + (old - 1)); return false; }

            Transform parent = unit.parent;
            if (parent == null) { GrowFailed("the slot widget has no parent to clone into"); return false; }

            // Where each piece sits inside the unit, so the same three can be found again in a copy.
            string pathAnim = RelativePath(unit, Net.Alive(anims[old - 1]) ? anims[old - 1].transform : null);
            string pathA = RelativePath(unit, old - 1 < spr.Length && Net.Alive(spr[old - 1]) ? spr[old - 1].transform : null);
            string pathB = RelativePath(unit, old - 1 < spr2.Length && Net.Alive(spr2[old - 1]) ? spr2[old - 1].transform : null);
            if (pathAnim == null) { GrowFailed("the animator is not inside the slot unit"); return false; }
            if (pathA == null || pathB == null) { GrowFailed("a slot sprite is not inside the slot unit"); return false; }

            // Follow the spacing the existing slots already use, in case nothing lays them out.
            Vector2 step = Vector2.zero;
            RectTransform lastRect = unit as RectTransform;
            Transform prevUnit = old >= 2 ? SlotUnit(anims, spr, spr2, old - 2) : null;
            if (lastRect != null && prevUnit is RectTransform)
                step = lastRect.anchoredPosition - ((RectTransform)prevUnit).anchoredPosition;

            var newAnims = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Animator>(target);
            var newSpr = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Image>(target);
            var newSpr2 = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Image>(target);
            for (int i = 0; i < old; i++)
            {
                newAnims[i] = anims[i];
                newSpr[i] = i < spr.Length ? spr[i] : null;
                newSpr2[i] = i < spr2.Length ? spr2[i] : null;
            }

            int built = 0;
            for (int i = old; i < target; i++)
            {
                string name = unit.gameObject.name + "_Extra" + i;

                // A resync can shorten the arrays without destroying the widgets, so a rebuild must
                // adopt the copy it already made rather than stacking another one on top of it.
                GameObject clone = null;
                try
                {
                    Transform existing = parent.Find(name);
                    if (existing != null) clone = existing.gameObject;
                }
                catch { }

                if (clone == null)
                {
                    try
                    {
                        UnityEngine.Object made = UnityEngine.Object.Instantiate(unit.gameObject, parent);
                        clone = made == null ? null : made.TryCast<GameObject>();
                    }
                    catch (Exception ex) { Log.Debug("clone slot: " + ex.Message); }
                }
                if (clone == null) { GrowFailed("Instantiate returned nothing for slot " + i); break; }

                try
                {
                    clone.name = name;
                    clone.SetActive(true);

                    RectTransform cloneRect = clone.transform as RectTransform;
                    if (cloneRect != null && lastRect != null && step != Vector2.zero)
                        cloneRect.anchoredPosition = lastRect.anchoredPosition + step * (i - (old - 1));
                    clone.transform.SetSiblingIndex(unit.GetSiblingIndex() + (i - (old - 1)));

                    Transform an = FindByPath(clone.transform, pathAnim);
                    Transform a = FindByPath(clone.transform, pathA);
                    Transform b = FindByPath(clone.transform, pathB);
                    newAnims[i] = an == null ? null : an.GetComponent<Animator>();
                    newSpr[i] = a == null ? null : a.GetComponent<Image>();
                    newSpr2[i] = b == null ? null : b.GetComponent<Image>();
                    built++;
                }
                catch (Exception ex) { Log.Debug("wire slot " + i + ": " + ex.Message); }
            }

            if (built == 0) { GrowFailed("no slot widget could be built"); return false; }

            inv.inventorySlots = newAnims;
            inv.inventorySprites = newSpr;
            inv.inventorySprites_ = newSpr2;
            return true;
        }

        private bool _growFailLogged;

        /// <summary>Says why the rig could not grow - once, so a per-frame retry does not flood.</summary>
        private void GrowFailed(string why)
        {
            if (_growFailLogged) return;
            _growFailLogged = true;
            Log.Warn("Cannot add inventory slots: " + why + ". The count stays at what the HUD can draw.");
        }

        /// <summary>
        /// The smallest transform containing the animator and both images of one slot - the thing that
        /// is actually "a slot" in the HUD, whichever of the three happens to be the outermost.
        /// </summary>
        private static Transform SlotUnit(
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Animator> anims,
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Image> spr,
            Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Image> spr2,
            int i)
        {
            try
            {
                Transform a = i < anims.Length && Net.Alive(anims[i]) ? anims[i].transform : null;
                Transform b = i < spr.Length && Net.Alive(spr[i]) ? spr[i].transform : null;
                Transform c = i < spr2.Length && Net.Alive(spr2[i]) ? spr2[i].transform : null;
                return CommonAncestor(CommonAncestor(a, b), c);
            }
            catch { return null; }
        }

        /// <summary>Lowest transform that is an ancestor of - or is - both of the two given.</summary>
        private static Transform CommonAncestor(Transform a, Transform b)
        {
            if (a == null) return b;
            if (b == null) return a;
            try
            {
                var seen = new System.Collections.Generic.HashSet<int>();
                for (Transform t = a; t != null; t = t.parent) seen.Add(t.GetInstanceID());
                for (Transform t = b; t != null; t = t.parent)
                    if (seen.Contains(t.GetInstanceID())) return t;
            }
            catch { }
            return null;
        }

        /// <summary>Path of <paramref name="child"/> under <paramref name="root"/>, or "" if it is the root.</summary>
        private static string RelativePath(Transform root, Transform child)
        {
            if (child == null || root == null) return null;
            if (ReferenceEquals(child, root)) return "";
            string path = "";
            Transform t = child;
            for (int guard = 0; guard < 16 && t != null; guard++)
            {
                if (ReferenceEquals(t, root)) return path;
                path = path.Length == 0 ? t.name : t.name + "/" + path;
                t = t.parent;
            }
            return null;
        }

        private static Transform FindByPath(Transform root, string path)
        {
            if (root == null) return null;
            if (string.IsNullOrEmpty(path)) return root;
            try { return root.Find(path); }
            catch { return null; }
        }

        private void ApplySlots(InventoryManager inv)
        {
            try
            {
                int want = MaxSlots;
                if (want <= 0) return;

                // The game ships exactly four of everything: four ids, four amounts, four slot
                // widgets. Anything past that has to be built before it can be used, so grow the rig
                // first and only then clamp to what actually exists.
                if (want > 4) GrowInventory(inv, want);

                if (inv.inventorySlots != null && want > inv.inventorySlots.Length)
                    want = inv.inventorySlots.Length;
                if (inv.inventoryIds != null && want > inv.inventoryIds.Length)
                    want = inv.inventoryIds.Length;
                // Ammo lives in itemStorages, which is a fixed ten - never hand out a slot past it.
                if (inv.itemStorages != null && want > inv.itemStorages.Length)
                    want = inv.itemStorages.Length;
                if (want <= 0) return;

                bool changedRequest = want != _slotsAppliedFor;
                if (!changedRequest && inv.maxInventorySlots == want) return;

                inv.maxInventorySlots = want;

                if (changedRequest)
                {
                    _slotsAppliedFor = want;
                    try { inv.Rpc_SetMaxInventorySlots(want); }
                    catch (Exception ex) { Log.Debug("Rpc_SetMaxInventorySlots: " + ex.Message); }
                    try { inv.UpdateInventorySlotsUI(); } catch { }
                    Log.Msg("Inventory slots set to " + want + ".");
                    return;
                }

                // A resync put it back: re-assert cheaply, and refresh the UI at most every 5s.
                if (Time.unscaledTime >= _nextSlotResync)
                {
                    _nextSlotResync = Time.unscaledTime + 5f;
                    try { inv.UpdateInventorySlotsUI(); } catch { }
                }
            }
            catch (Exception ex) { Log.Debug("slots: " + ex.Message); }
        }

        /// <summary>
        /// Pins stack counts to the highest value seen per slot, so throwables (grenades, molotovs,
        /// bricks) never run out once you are holding some.
        /// </summary>
        private bool _layoutLogged;

        /// <summary>One-shot: how many slots the inventory rig actually has room for.</summary>
        internal void LogInventoryLayout(InventoryManager inv)
        {
            if (_layoutLogged || !Net.Alive(inv)) return;
            _layoutLogged = true;
            try
            {
                string slots = "";
                try
                {
                    for (int i = 0; i < inv.inventoryIds.Length; i++)
                        slots += " [" + i + "]id=" + inv.inventoryIds[i] + ",n=" + inv.inventoryAmounts[i];
                }
                catch { }

                Log.Msg("INVENTORY SLOTS:" + slots);
                Log.Msg("INVENTORY LAYOUT: maxInventorySlots=" + inv.maxInventorySlots +
                        " save=" + SavedSlots +
                        " | ids=" + Len(inv.inventoryIds) + " amounts=" + Len(inv.inventoryAmounts) +
                        " storages=" + Len(inv.itemStorages) + " storages2=" + Len(inv.itemStorages2) +
                        " maxStack=" + Len(inv.maxStack) +
                        " | slotAnims=" + LenR(inv.inventorySlots) +
                        " sprites=" + LenR(inv.inventorySprites) + " sprites_=" + LenR(inv.inventorySprites_) +
                        " holdingObjs=" + LenR(inv.holdingObjs) + " canvases=" + LenR(inv.itemCanvases) +
                        " holdingAnims=" + LenR(inv.holdingAnims) + " dropAnchors=" + LenR(inv.dropAnchors));

                // The four 61-long arrays above are indexed by item id, not by slot, so they never
                // need growing. These do, and they are the ones there is no typed accessor for.
                string extra = "";
                for (int i = 0; i < SlotArrays.Length; i++)
                    extra += " " + SlotArrays[i] + "=" + ArrayLength(inv, SlotArrays[i]);

                // Which object the grow code will actually clone, and where the three pieces sit in
                // it. If a slot's sprites are not under its animator this is the line that shows it.
                string unit = "-";
                try
                {
                    Transform u = SlotUnit(inv.inventorySlots, inv.inventorySprites, inv.inventorySprites_, 0);
                    if (u != null)
                        unit = u.name + " (anim=" + (RelativePath(u, inv.inventorySlots[0].transform) ?? "?") +
                               " sprite=" + (RelativePath(u, inv.inventorySprites[0].transform) ?? "?") + ")";
                }
                catch { }

                Log.Msg("INVENTORY PER-SLOT:" + extra + " | slot unit = " + unit);
            }
            catch (Exception ex) { Log.Debug("layout: " + ex.Message); }
        }

        /// <summary>Length of a named Il2Cpp array property, for the layout dump.</summary>
        private static string ArrayLength(InventoryManager inv, string name)
        {
            try
            {
                System.Reflection.PropertyInfo p = typeof(InventoryManager).GetProperty(
                    name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (p == null) return "(no such field)";
                object cur = p.GetValue(inv);
                if (cur == null) return "-";
                System.Reflection.PropertyInfo lengthProp = cur.GetType().GetProperty("Length");
                return lengthProp == null ? "?" : Convert.ToInt32(lengthProp.GetValue(cur)).ToString();
            }
            catch { return "?"; }
        }

        private static string Len(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<int> a)
        {
            try { return a == null ? "-" : a.Length.ToString(); } catch { return "?"; }
        }

        private static string LenR<T>(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<T> a)
            where T : Il2CppSystem.Object
        {
            try { return a == null ? "-" : a.Length.ToString(); } catch { return "?"; }
        }

        private void ApplyFreezeItems(InventoryManager inv)
        {
            try
            {
                var amounts = inv.inventoryAmounts;
                var ids = inv.inventoryIds;
                if (amounts == null) return;
                for (int i = 0; i < amounts.Length; i++)
                {
                    // Only consumables. Freezing a weapon's stack count is what used to jam guns, and
                    // there is no reason to pin a mop or a crate either.
                    if (ids != null && i < ids.Length && !Items.IsConsumable(ids[i]))
                    {
                        _amountHighWater.Remove(i);
                        continue;
                    }

                    int cur = amounts[i];
                    int high;
                    if (!_amountHighWater.TryGetValue(i, out high) || cur > high)
                    {
                        if (cur > 0) _amountHighWater[i] = cur;
                        continue;
                    }
                    if (high > 0 && cur < high && cur >= 0) amounts[i] = high;
                }
            }
            catch (Exception ex) { Log.Debug("freeze items: " + ex.Message); }
        }

        /// <summary>
        /// Weapon ammo lives in itemStorages / itemStorages2, indexed by inventory slot.
        ///
        /// Two rules here, both learned the hard way:
        ///
        /// 1. Write through <see cref="InventoryManager.ChangePlayerItemStorage"/>, never straight
        ///    into the arrays. That method sets both arrays and then fires Rpc_CMD_ChangeItemStorage
        ///    to sync; poking the arrays directly leaves the networked state disagreeing with the
        ///    local one, which is what left guns unable to fire.
        ///
        /// 2. Never invent a number. Forcing a magazine to an arbitrary 99 pushes it past the
        ///    capacity the weapon's own reload logic expects. We only ever restore a slot to the
        ///    highest value we have actually seen it hold legitimately, so the value is always one
        ///    the game itself produced.
        /// </summary>
        private void ApplyInfiniteAmmo(InventoryManager inv)
        {
            try
            {
                var ids = inv.inventoryIds;
                var storages = inv.itemStorages;
                var storages2 = inv.itemStorages2;
                if (ids == null || storages == null) return;

                int slots = Math.Min(ids.Length, storages.Length);
                for (int slot = 0; slot < slots; slot++)
                {
                    int itemId = ids[slot];
                    if (itemId < 0 || !Items.IsWeapon(itemId)) continue;

                    int cur = storages[slot];
                    int cur2 = storages2 != null && slot < storages2.Length ? storages2[slot] : 0;

                    int peak, peak2;
                    if (!_storageHighWater.TryGetValue(slot, out peak)) peak = 0;
                    if (!_storage2HighWater.TryGetValue(slot, out peak2)) peak2 = 0;

                    // An optional ceiling, off by default. Only ever raises the peak toward a value
                    // the user asked for; it does not force a number the weapon never held.
                    if (AmmoTarget > 0 && peak < AmmoTarget && cur > 0) peak = Math.Min(AmmoTarget, Math.Max(peak, cur));

                    if (cur > peak) { peak = cur; _storageHighWater[slot] = peak; }
                    if (cur2 > peak2) { peak2 = cur2; _storage2HighWater[slot] = peak2; }
                    if (peak <= 0) continue;

                    // Only act on an actual decrease, and go through the game's own setter so the
                    // networked copy is updated too.
                    if (cur < peak)
                    {
                        try { inv.ChangePlayerItemStorage(slot, peak, peak2 > 0 ? peak2 : cur2); }
                        catch (Exception ex) { Log.Debug("ChangePlayerItemStorage: " + ex.Message); }
                    }
                }
            }
            catch (Exception ex) { Log.Debug("infinite ammo: " + ex.Message); }
        }

        private FPSController _controller;
        private float _nextControllerLookup;

        /// <summary>Cached; the GetComponentInChildren fallback is far too expensive per tick.</summary>
        private FPSController FindController()
        {
            if (Net.Alive(_controller)) return _controller;
            if (Time.unscaledTime < _nextControllerLookup) return null;
            _nextControllerLookup = Time.unscaledTime + 1f;

            PlayerManager pm = Net.LocalPlayer;
            if (!Net.Alive(pm)) return null;
            try
            {
                FPSController c = pm.fpsScript;
                if (Net.Alive(c)) { _controller = c; return c; }
            }
            catch { }
            try { _controller = pm.GetComponentInChildren<FPSController>(); }
            catch { _controller = null; }
            return _controller;
        }

        private void ApplyMovementMultipliers()
        {
            FPSController c = FindController();
            if (!Net.Alive(c)) return;

            if (!_movementCaptured || !ReferenceEquals(_movementOwner, c))
            {
                try
                {
                    _baseWalk = c.walkSpeed;
                    _baseRun = c.runSpeed;
                    _baseCrouch = c.crouchSpeed;
                    _baseJump = c.jumpPower;
                    _movementOwner = c;
                    _movementCaptured = true;
                    Log.Debug("movement baselines captured: walk=" + _baseWalk + " run=" + _baseRun + " jump=" + _baseJump);
                }
                catch (Exception ex) { Log.Debug("movement capture: " + ex.Message); return; }
            }

            try
            {
                c.walkSpeed = _baseWalk * SpeedMultiplier;
                c.runSpeed = _baseRun * SpeedMultiplier;
                c.crouchSpeed = _baseCrouch * SpeedMultiplier;
                c.jumpPower = _baseJump * JumpMultiplier;
            }
            catch (Exception ex) { Log.Debug("movement apply: " + ex.Message); }
        }

        /// <summary>Jump in mid-air. Hold to keep rising, tap to jump again.</summary>
        internal bool MoonJump;

        /// <summary>Set by the suite each frame so a jump is not thrown while the menu has focus.</summary>
        internal bool MenuOpen;

        internal int MidAirJumps;

        /// <summary>
        /// Mid-air jumping, using the jump the game already has.
        ///
        /// FPSController drives the player off verticalVelocity, and a normal jump is that field being
        /// set to jumpPower while grounded. Setting the same field to the same value while airborne is
        /// therefore a jump in every sense the controller cares about - no second physics path, no
        /// invented force, and Jump Height keeps working because jumpPower is what the multiplier
        /// already scales.
        ///
        /// Held rather than tapped, so one key covers both readings: tap it for another jump, hold it
        /// to keep climbing. Grounded jumps are left entirely alone - the game does those.
        /// </summary>
        internal void RunMoonJump()
        {
            if (!MoonJump || Noclip || MenuOpen) return;

            try
            {
                if (!Input.GetKey(KeyCode.Space)) return;

                FPSController c = FindController();
                if (!Net.Alive(c)) return;

                // On the floor is the game's business; this is only for the air.
                if (c.grounded) return;

                float power = c.jumpPower;
                if (power <= 0f) return;

                // Only ever push upward. Overwriting a rising velocity every frame would cap a real
                // jump at its own strength and make the climb slower than the game's.
                if (c.verticalVelocity < power)
                {
                    c.verticalVelocity = power;
                    MidAirJumps++;
                }
            }
            catch (Exception ex) { Log.Debug("moon jump: " + ex.Message); }
        }

        internal void RestoreMovement()
        {
            if (!_movementCaptured) return;
            FPSController c = _movementOwner;
            if (!Net.Alive(c)) return;
            try
            {
                c.walkSpeed = _baseWalk;
                c.runSpeed = _baseRun;
                c.crouchSpeed = _baseCrouch;
                c.jumpPower = _baseJump;
            }
            catch { }
        }

        internal void OnNoclipChanged(bool enabled)
        {
            CharacterController cc = null;
            FPSController c = FindController();
            if (Net.Alive(c)) { try { cc = c.characterController; } catch { } }

            if (enabled)
            {
                if (Net.Alive(cc))
                {
                    _noclipPrevDetectCollisions = cc.detectCollisions;
                    cc.detectCollisions = false;
                    cc.enabled = false;
                }
                _noclipActive = true;
                Log.Msg("No-Clip ON.");
            }
            else
            {
                if (Net.Alive(cc))
                {
                    cc.enabled = true;
                    cc.detectCollisions = _noclipPrevDetectCollisions;
                }
                _noclipActive = false;
                Log.Msg("No-Clip OFF.");
            }
        }

        internal void RunNoclip()
        {
            if (!_noclipActive) return;
            Transform t = Net.LocalTransform;
            if (t == null) return;

            Camera cam = Camera.main;
            Transform eye = Net.Alive(cam) ? cam.transform : t;

            Vector3 move = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) move += eye.forward;
            if (Input.GetKey(KeyCode.S)) move -= eye.forward;
            if (Input.GetKey(KeyCode.D)) move += eye.right;
            if (Input.GetKey(KeyCode.A)) move -= eye.right;
            if (Input.GetKey(KeyCode.Space)) move += Vector3.up;
            if (Input.GetKey(KeyCode.LeftControl)) move -= Vector3.up;
            if (move == Vector3.zero) return;

            float speed = NoclipSpeed * (Input.GetKey(KeyCode.LeftShift) ? 3f : 1f);
            t.position += move.normalized * speed * Time.unscaledDeltaTime;
        }
    }
}
