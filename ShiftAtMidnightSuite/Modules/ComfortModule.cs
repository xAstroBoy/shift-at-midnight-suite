using System;
using System.Collections.Generic;
using Il2Cpp;
using ShiftAtMidnightSuite.Util;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// The chores nobody enjoys: impatient customers, the petrol pump, the hold-to-place timers,
    /// pitch-black rooms and the pack of small monsters that comes along with the hunt entity.
    ///
    /// Everything here is host-friendly. Values the game replicates are written through the same
    /// paths the game uses, and everything that changes global render state is restored when it is
    /// switched off so a scene change does not leave the game bright.
    /// </summary>
    internal sealed class ComfortModule
    {
        // ---------------------------------------------------------------- toggles
        internal bool MaxPatience;
        internal bool HappyCustomers;
        internal bool HonestStock;
        internal bool InstantTasks;
        internal bool AutoFuel;
        internal bool AutoKillExtras;

        /// <summary>Skip the warning delay automatically whenever a hunt begins.</summary>
        internal bool AutoSkipHuntCountdown;

        /// <summary>
        /// Dismiss the hunt briefing the moment it appears.
        ///
        /// Skipping the countdown brings the entity in early but leaves the explanation panels up -
        /// StoreManager.huntExplanation and huntExplanation2, with their own countdown text - and
        /// those are the timer you are actually sat waiting on once a hunt starts. They are plain UI
        /// objects, so switching them off costs the hunt nothing: it has already begun either way.
        /// </summary>
        internal bool SkipHuntBriefing;

        internal int BriefingsSkipped;

        /// <summary>
        /// Runs the end-of-day report faster.
        ///
        /// Every part of that screen is on a timer of its own - the customer report walks the night's
        /// npcFolders one at a time, and the revenue counter ticks its way up to the real figure - and
        /// there is no single "speed" to turn up. Time.timeScale is the one lever that moves all of
        /// them at once, and it is safe here in a way it would not be during a shift: the report is a
        /// UI sequence with no gameplay behind it, and nothing about the numbers themselves changes,
        /// only how long they take to be shown. Scaling deliberately does not touch the money values.
        /// </summary>
        internal bool FastEndOfDay;

        /// <summary>How much faster, 2-10x.</summary>
        internal float EndOfDaySpeed = 4f;

        /// <summary>
        /// Stops the vents throwing you out.
        ///
        /// VentTrigger drives the eviction with ventPushOutAnim, and Vent.StayingInVentTooLongHint is
        /// the nag that comes with it. Disabling that one animator leaves entering and leaving a vent
        /// working exactly as before - the trigger still tracks playersInVent - it just stops the vent
        /// deciding you have been in there long enough.
        /// </summary>
        internal bool VentsDontKick;

        /// <summary>
        /// Releases the item lock that a conversation leaves behind.
        ///
        /// DialogueInteractable calls InventoryManager.PauseUseItem when a conversation starts, and it
        /// is the only caller of that on the dialogue path - nothing in it ever calls UnpauseUseItem
        /// again. The unpause exists (it is what re-arms the can-use-item flag) but it is left to the
        /// computer, the shelf view and the FPS controller to fire, so a plain chat with a customer
        /// can leave your hands tied for the rest of the night. This puts it back once the player is
        /// demonstrably free again.
        /// </summary>
        internal bool AutoUnlockInventory = true;

        internal int InventoryUnlocks;

        /// <summary>
        /// Mutes the entry-door chime and the counter bell.
        ///
        /// EntryDoor.DoNoise only raises a flag that the entry RPC reads, and that flag also feeds the
        /// "monsters heard the door" check - so muting the AudioSource is the honest way to silence
        /// it: the door still behaves exactly as the game expects, it just stops making the sound.
        /// </summary>
        internal bool SilenceBells
        {
            get { return _silenceBells; }
            set
            {
                if (value == _silenceBells) return;
                _silenceBells = value;
                _bellsApplied = false;
            }
        }
        private bool _silenceBells;
        private bool _bellsApplied;

        /// <summary>Multiplies each customer's own patience allowance, on top of the pinning.</summary>
        internal float PatienceFactor = 3f;

        /// <summary>
        /// Kills the fog outright, independently of fullbright.
        ///
        /// The game re-enables fog per event and per day, so this is re-asserted rather than set once.
        /// </summary>
        internal bool NoFog
        {
            get { return _noFog; }
            set
            {
                if (value == _noFog) return;
                _noFog = value;
                if (value) { CaptureFog(); ApplyNoFog(); }
                else RestoreFog();
            }
        }
        private bool _noFog;
        private bool _fogCaptured;
        private bool _savedFogEnabled;
        private float _savedFogDensity;

        private void CaptureFog()
        {
            if (_fogCaptured) return;
            try
            {
                _savedFogEnabled = RenderSettings.fog;
                _savedFogDensity = RenderSettings.fogDensity;
                _fogCaptured = true;
            }
            catch { }
        }

        private void ApplyNoFog()
        {
            try
            {
                if (RenderSettings.fog) RenderSettings.fog = false;
                if (RenderSettings.fogDensity != 0f) RenderSettings.fogDensity = 0f;
            }
            catch (Exception ex) { Log.Debug("fog: " + ex.Message); }
        }

        private void RestoreFog()
        {
            try
            {
                if (_fogCaptured)
                {
                    RenderSettings.fog = _savedFogEnabled;
                    RenderSettings.fogDensity = _savedFogDensity;
                    _hittables.Clear();
            _nextHittableScan = 0f;
            _drainNoted = false;
            _fogCaptured = false;
                }
            }
            catch (Exception ex) { Log.Debug("restore fog: " + ex.Message); }
            Log.Msg("Fog restored.");
        }

        /// <summary>Extra exposure, in EV, applied while fullbright is on.</summary>
        internal float BrightBoost = 2.5f;

        internal string LastResult = "";

        internal bool AnyActive
        {
            get
            {
                return MaxPatience || HappyCustomers || HonestStock || InstantTasks || AutoFuel || AutoUnlockInventory
                       || AutoKillExtras || AutoSkipHuntCountdown || SkipHuntBriefing || FastEndOfDay
                       || VentsDontKick || AutoHarvestTokens || NoFarClip || _clipsRaised || _eodBoosted || _ventsFreed || _silenceBells || !_bellsApplied
                       || _curbGroup.Active || _fenceGroup.Active || _roofGroup.Active || _noFog
                       || _brightOn;
            }
        }

        // ---------------------------------------------------------------- scheduling
        private const float CustomerInterval = 0.5f;   // patience drains at 1/s; twice a second is plenty
        private const float FuelInterval = 1f;
        private const float KillInterval = 1f;
        private const float StockInterval = 2f;

        private float _nextCustomer, _nextFuel, _nextKill, _nextStock, _nextBell, _nextUnlockCheck, _nextBriefing;
        private float _nextHealthSweep, _nextHittableScan;
        private List<Hittable> _hittables = new List<Hittable>();
        private bool _drainNoted;
        private float _freeSince;

        internal int PatienceHeld;
        internal int FueledCars;
        internal int ExtrasKilled;

        internal void OnSceneChanged()
        {
            _basePatience.Clear();
            _shelves.Clear();
            _nextShelfRefresh = 0f;
            _stockLogged = false;
            _nextNpcSeed = 0f;
            _nextVolumeScan = 0f;
            _fuelAttempted.Clear();
            _tankDoorsOpened.Clear();
            _tanks.Clear();
            _nextTankRefresh = 0f;
            _doors.Clear();
            _bellRegisters.Clear();
            _nextBellScan = 0f;
            _freeSince = 0f;
            // A new scene brings new interactables: the saved hold times belong to objects that no
            // longer exist, and their instance ids can be handed out again.
            _heldZeroed = null;
            _heldZeroedId = 0;
            _eodReport = null;
            _relayered.Clear();
            _revealedNpcs.Clear();
            _ghostLayer = -2;
            // Never carry a raised clock across a scene load - the report that justified it is gone.
            RestoreEndOfDay();
            _nextEodCheck = 0f;
            _coinsTaken.Clear();
            _nextCoinSweep = 0f;
            _farClips.Clear();
            _clipsRaised = false;
            _nextClipSweep = 0f;
            _vents.Clear();
            _ventsFreed = false;
            _nextVentScan = 0f;
            _bellsApplied = false;   // a new scene brings new doors, so re-apply
            // A new scene brings new scenery: forget the old colliders and start looking again.
            ReArm(_curbGroup);
            ReArm(_fenceGroup);
            ReArm(_roofGroup);
            _nextBlockerSweep = 0f;
            _graded.Clear();
            _fogCaptured = false;
            if (_noFog) { CaptureFog(); ApplyNoFog(); }

            // The scene reload rebuilds lighting, so re-apply rather than restore stale values.
            if (_brightOn) { _brightCaptured = false; ApplyFullbright(); }
        }

        internal void Tick()
        {
            float now = Time.unscaledTime;

            // Task bars are read every frame by the game, so this one is not throttled. The hold-time
            // sweep behind it is, because it walks every interactable in the scene.
            if (InstantTasks) { SkipTaskTimers(); ApplyInstantHolds(); }
            else RestoreHolds();

            if ((MaxPatience || HappyCustomers) && now >= _nextCustomer)
            {
                _nextCustomer = now + CustomerInterval;
                HoldCustomers();
            }

            if (AutoFuel && now >= _nextFuel)
            {
                _nextFuel = now + FuelInterval;
                FillTanks(now);
            }

            if (HonestStock && now >= _nextStock)
            {
                _nextStock = now + StockInterval;
                FixStockRating(now);
            }

            if ((AutoKillExtras || AutoSkipHuntCountdown) && now >= _nextKill)
            {
                _nextKill = now + KillInterval;
                if (AutoSkipHuntCountdown) SkipHuntCountdown(false);
                if (AutoKillExtras) KillHuntExtras(false, false);
            }

            // The briefing wants dismissing the frame it appears, not on the once-a-second kill sweep.
            if (SkipHuntBriefing && now >= _nextBriefing)
            {
                _nextBriefing = now + 0.2f;
                DismissHuntBriefing();
            }

            ApplyMenuItemLock();
            DismissHints(now);
            if (AutoHarvestTokens) HarvestTokens(now);
            DriveEndOfDay(now);
            ApplyVentKick(now, VentsDontKick);
            ApplyScanReveal(now, RevealScanOnly);
            ApplyFarClip(now, NoFarClip);
            if (InstantEmotiscope) RushEmotiscope();

            HoldBlockers(now);

            if (now >= _nextBell)
            {
                _nextBell = now + 5f;
                ApplyBellMute();
            }

            if (AutoUnlockInventory && now >= _nextUnlockCheck)
            {
                _nextUnlockCheck = now + 0.4f;
                ReleaseItemLock(now);
            }

            // Cheap enough to re-assert every tick: the game turns fog back on with the weather and
            // with each event, so a one-time write does not hold.
            if (_noFog) ApplyNoFog();

            if (HumanShield.Enabled && now >= _nextHealthSweep)
            {
                _nextHealthSweep = now + 0.4f;
                HoldPeopleUp(now);
            }

            if (_brightOn) HoldFullbright();
        }

        // ================================================================ customers

        /// <summary>
        /// Customers carry their own patience timer (curPatience counts down to zero, at which point
        /// they walk out and leave a remark). Pinning it to the maximum is enough to make them wait
        /// forever, and it leaves every other part of the transaction untouched - unlike cancelling
        /// the timer, which also takes the bar away and confuses the counter logic.
        /// </summary>
        /// <summary>Each customer's own patience allowance, as it was when they walked in.</summary>
        private readonly Dictionary<int, float> _basePatience = new Dictionary<int, float>();
        private float _nextNpcSeed;

        private void HoldCustomers()
        {
            // The registry is filled by the Spawned patch and costs nothing to read. Falling back to
            // a scene sweep is only worth doing occasionally - it is empty most of the night, and
            // sweeping on every empty pass is what made this module the worst tick in the profile.
            List<StoreBrowseBehaviour> npcs = NpcRegistry.Live();
            if (npcs.Count == 0)
            {
                if (Time.unscaledTime < _nextNpcSeed) return;
                _nextNpcSeed = Time.unscaledTime + 10f;
                List<StoreBrowseBehaviour> seed = Net.FindActive<StoreBrowseBehaviour>();
                for (int i = 0; i < seed.Count; i++) NpcRegistry.Add(seed[i]);
                npcs = NpcRegistry.Live();
                if (npcs.Count == 0) return;
            }

            // Customers come and go all night; drop the ones that have left.
            if (_basePatience.Count > 64) _basePatience.Clear();

            for (int i = 0; i < npcs.Count; i++)
            {
                StoreBrowseBehaviour npc = npcs[i];
                if (!Net.Alive(npc)) continue;

                try
                {
                    if (MaxPatience && npc.hasPatience)
                    {
                        int id = npc.GetInstanceID();
                        float baseMax;
                        if (!_basePatience.TryGetValue(id, out baseMax))
                        {
                            // Remember what the customer arrived with. Scaling the live value instead
                            // would multiply it again on every pass and run away to infinity.
                            baseMax = npc.maxPatience;
                            if (baseMax <= 0f) continue;
                            _basePatience[id] = baseMax;
                        }

                        float want = baseMax * Mathf.Max(1f, PatienceFactor);
                        if (Mathf.Abs(npc.maxPatience - want) > 0.01f) npc.maxPatience = want;
                        if (npc.curPatience < want)
                        {
                            npc.curPatience = want;
                            PatienceHeld++;
                        }
                    }

                    // The leaving remark is the complaint they throw out on the way to the door.
                    if (HappyCustomers && npc.hasLeavingRemark) npc.hasLeavingRemark = false;
                }
                catch (Exception ex) { Log.Debug("patience: " + ex.Message); }
            }
        }

        // ================================================================ hold-to-do timers

        /// <summary>
        /// Placing a trap, disarming one and boarding a door all run the same fill bar: taskCompletion
        /// climbs to taskCompletionMax while you hold the button, and barricades add a separate
        /// start-up delay before the bar even appears. Both are just floats on the local inventory,
        /// so completing them outright skips the wait without touching what the task then does.
        /// </summary>
        private void SkipTaskTimers()
        {
            try
            {
                InventoryManager inv = Net.LocalInventory;
                if (!Net.Alive(inv)) return;

                // The gate used to be inv.tasking alone, and that is why this worked most of the time
                // rather than every time: not every hold-to-do sets that flag, and the ones that set
                // it late lost the first frames. A bar that is part-way along is proof enough that the
                // game started something, so either signal now counts.
                if (inv.taskCompletionMax > 0f && inv.taskCompletion < inv.taskCompletionMax &&
                    (inv.tasking || inv.taskCompletion > 0f))
                    inv.taskCompletion = inv.taskCompletionMax;

                // Boarding a door waits out BarricadeHoldStartDelay before the bar even appears.
                // Pushing the timer to the delay only helps if the timer counts up; if it counts down
                // that same write restarts the wait, which is the other half of why boarding sometimes
                // refused to start. barricadeHoldStarted is what actually gates the bar, and setting
                // it is direction-agnostic - so set it, and move the timer as well for the up case.
                if (inv.barricadeHoldStartTimer > 0f && !inv.barricadeHoldStarted)
                {
                    if (inv.barricadeHoldStartTimer < InventoryManager.BarricadeHoldStartDelay)
                        inv.barricadeHoldStartTimer = InventoryManager.BarricadeHoldStartDelay;
                    inv.barricadeHoldStarted = true;
                }

                // Taking a trap back down is a different bar entirely: hold-to-interact lives on
                // InteractManager, with its own taskCompletion pair, and is what every
                // holdInteractable object uses - traps, and anything else you have to hold to use.
                InteractManager im = inv.interactMan;
                if (Net.Alive(im) && im.taskCompletionMax > 0f && im.taskCompletion < im.taskCompletionMax &&
                    (im.holdInteracting || im.taskCompletion > 0f))
                    im.taskCompletion = im.taskCompletionMax;
            }
            catch (Exception ex) { Log.Debug("instant task: " + ex.Message); }
        }

        // ---------------------------------------------------------------- hold-to-interact

        private Interactable _heldZeroed;
        private int _heldZeroedId;
        private float _heldOriginal;

        /// <summary>
        /// Removes the hold requirement at its source, for the one object it can possibly matter on.
        ///
        /// Completing the fill bar every frame races the game: it is written from Update, so anything
        /// that resets or re-reads it in the same frame can undo the push, and that race is what made
        /// arming and disarming work "most of the time". Interactable.holdInteractableTime is the
        /// value the bar is measured against, and it is a plain float on the object, so zeroing it
        /// means there is no bar to win a race against.
        ///
        /// This first swept every Interactable in the scene every two seconds to do that, which was a
        /// FindObjectsOfType over one of the most numerous base types in the game - every shelf, door,
        /// vent and pickup - and it was a needless one. You can only hold a button on the thing you
        /// are looking at, and InteractManager already knows which that is. One property read a frame
        /// replaces the sweep, and putting the value back becomes trivial because there is only ever
        /// one object to put back.
        /// </summary>
        private void ApplyInstantHolds()
        {
            InventoryManager inv = Net.LocalInventory;
            InteractManager im = null;
            if (Net.Alive(inv)) { try { im = inv.interactMan; } catch { } }

            Interactable cur = null;
            if (Net.Alive(im)) { try { cur = im.curInteractable; } catch { } }

            int curId = 0;
            if (Net.Alive(cur)) { try { curId = cur.GetInstanceID(); } catch { } }

            // Looking at something else now, or at nothing: hand the old one its timer back first.
            if (_heldZeroed != null && curId != _heldZeroedId) RestoreHolds();

            if (curId == 0 || curId == _heldZeroedId) return;

            try
            {
                if (!cur.holdInteractable || cur.holdInteractableTime <= 0f) return;
                _heldOriginal = cur.holdInteractableTime;
                _heldZeroed = cur;
                _heldZeroedId = curId;
                cur.holdInteractableTime = 0f;
            }
            catch { }
        }

        /// <summary>Puts the one flattened hold time back the way the game shipped it.</summary>
        private void RestoreHolds()
        {
            if (_heldZeroed == null) return;
            try { if (Net.Alive(_heldZeroed)) _heldZeroed.holdInteractableTime = _heldOriginal; }
            catch { }
            _heldZeroed = null;
            _heldZeroedId = 0;
        }

        // ================================================================ stock rating

        private List<RestockShelf> _shelves = new List<RestockShelf>();
        private float _nextShelfRefresh;
        internal int StockCorrections;

        private List<RestockShelf> Shelves(float now)
        {
            if (now >= _nextShelfRefresh)
            {
                _nextShelfRefresh = now + 15f;      // shelves are fixed scene objects
                _shelves = Net.FindActive<RestockShelf>();
            }
            return _shelves;
        }

        /// <summary>
        /// Why customers complain about empty shelves in front of a full one.
        ///
        /// stockPenalty is not measured from the shelves - it is a running total that
        /// ShelfItemManager nudges by -1 on every item added and +1 on every item removed. Any add or
        /// remove that happens outside those two calls (a restock the host does not own, an item
        /// despawning with an NPC, a reload) leaves the total permanently above what the shelves
        /// actually justify, and it never re-derives itself.
        ///
        /// On top of that the store's mood signs are only repainted inside UpdateReviewUI, which the
        /// game calls from UpdateStockPenalty - so writing the field alone leaves an angry face up
        /// even once the number is right.
        ///
        /// This counts what is really on the shelves, publishes that as the penalty through the
        /// game's own RPC so clients agree, and then repaints.
        /// </summary>
        private bool _stockLogged;

        private static string Txt(Il2CppTMPro.TextMeshProUGUI t)
        {
            try { return t == null ? "-" : t.text; }
            catch { return "?"; }
        }

        /// <summary>
        /// Why the store reported 0% stock in front of a full shelf.
        ///
        /// ReviewsManager.stockPenalty is not a 0..N penalty. ShelfItemManager nudges it by -1 for
        /// every item added to a shelf and +1 for every item removed, so an empty store sits at 0 and
        /// a fully stocked one sits at minus the number of products on the shelves. The board then
        /// renders  100 - (stockPenalty + total shelf capacity)  as the percentage - verified against
        /// the live game by sweeping values through UpdateReviewUI: with a 464-product store, 0, -100
        /// and -200 all showed "0%", and -464 showed "100%".
        ///
        /// Zero therefore means "nothing on the shelves", which is exactly what the old perfect-reviews
        /// pin was writing every second - so the rating sat at zero, Stock became the lowest-rated
        /// category, and customers complained about empty shelves in a store that was full.
        ///
        /// This is now the only writer of that field: it publishes minus the real product count (or
        /// minus full capacity while Always Perfect Reviews is on) through the game's own RPC, then
        /// repaints, since the signs and bars are only redrawn inside UpdateReviewUI.
        /// </summary>
        private void FixStockRating(float now)
        {
            try
            {
                ReviewsManager rm = ReviewsManager.Instance;
                if (!Net.Alive(rm)) return;

                List<RestockShelf> shelves = Shelves(now);
                if (shelves.Count == 0) return;

                int have = 0, cap = 0;
                for (int i = 0; i < shelves.Count; i++)
                {
                    RestockShelf sh = shelves[i];
                    if (!Net.Alive(sh)) continue;
                    try
                    {
                        int max = sh.maxProductsOnShelf;
                        if (max <= 0) continue;
                        cap += max;
                        int on = sh.productsOnShelf;
                        have += on < 0 ? 0 : (on > max ? max : on);
                    }
                    catch { }
                }
                if (cap <= 0) return;

                // Perfect reviews means "always fully stocked"; otherwise tell the truth.
                float target = PerfectReviews.Enabled ? -cap : -have;

                if (!_stockLogged)
                {
                    _stockLogged = true;
                    Log.Msg("Stock rating: " + have + "/" + cap + " products on " + shelves.Count +
                            " shelves; publishing stockPenalty " + target.ToString("0") +
                            " (was " + rm.stockPenalty.ToString("0") + ").");
                }

                if (Mathf.Abs(rm.stockPenalty - target) < 0.5f) return;

                Rpc.Call(rm, delegate { rm.Rpc_UpdateStockPenalty(target); }, "UpdateStockPenalty");
                try { rm.UpdateReviewUI(); } catch (Exception ex) { Log.Debug("review UI: " + ex.Message); }
                StockCorrections++;
                LastResult = "Stock " + have + "/" + cap;
            }
            catch (Exception ex) { Log.Debug("stock rating: " + ex.Message); }
        }

        // ================================================================ petrol

        private List<PetrolTank> _tanks = new List<PetrolTank>();
        private float _nextTankRefresh;
        private readonly Dictionary<int, float> _fuelAttempted = new Dictionary<int, float>();
        private readonly HashSet<int> _tankDoorsOpened = new HashSet<int>();

        private List<PetrolTank> Tanks(float now)
        {
            if (now >= _nextTankRefresh)
            {
                _nextTankRefresh = now + 10f;
                _tanks = Net.FindActive<PetrolTank>();
            }
            return _tanks;
        }

        /// <summary>
        /// Every petrol particle that lands on the tank calls PetrolPumped(), which adds a little to
        /// curMoneySpent - and, when curMoneySpent has already reached maxMoneySpent, takes the branch
        /// that marks the tank full, shuts the pump off and offers the customer the leave option.
        /// So the whole chore is: write the target in, then make that one call. Nothing polls the
        /// value on its own, which is why the tank has to be nudged rather than just filled.
        /// </summary>
        private void FillTanks(float now)
        {
            List<PetrolTank> tanks = Tanks(now);

            for (int i = 0; i < tanks.Count; i++)
            {
                PetrolTank t = tanks[i];
                if (!Net.Alive(t)) continue;

                int id;
                try { id = t.GetInstanceID(); } catch { continue; }

                try
                {
                    if (t.petrolFull) { _fuelAttempted.Remove(id); continue; }

                    float max = t.maxMoneySpent;
                    if (max <= 0f) continue;                       // no order on this tank yet

                    // Flap first: the pump cannot be used with the tank door shut.
                    if (!_tankDoorsOpened.Contains(id)) OpenTankDoor(t, id);

                    float last;
                    if (_fuelAttempted.TryGetValue(id, out last) && now - last < 1f) continue;
                    _fuelAttempted[id] = now;

                    if (t.curMoneySpent < max) t.curMoneySpent = max;
                    t.PetrolPumped();                              // the same call a petrol particle makes

                    if (t.petrolFull)
                    {
                        FueledCars++;
                        LastResult = "Petrol order filled (" + max.ToString("0") + ")";
                        Log.Msg(LastResult + ".");
                    }
                }
                catch (Exception ex) { Log.Debug("auto fuel: " + ex.Message); }
            }
        }

        /// <summary>Opens the fuel flap once per tank; the game's own toggle would close it again.</summary>
        private void OpenTankDoor(PetrolTank t, int id)
        {
            try
            {
                Car car = t.GetComponentInParent<Car>();
                if (!Net.Alive(car)) { _tankDoorsOpened.Add(id); return; }
                if (!car.petrolTankOpen) car.TogglePetrolTankOpen();
                _tankDoorsOpened.Add(id);
            }
            catch (Exception ex) { Log.Debug("petrol flap: " + ex.Message); _tankDoorsOpened.Add(id); }
        }

        // ================================================================ hunt extras

        /// <summary>
        /// A hunt spawns one entity plus a supporting cast - baby dolls, marionettes, extra spiders.
        /// Hittable.isEntity marks the one that actually is the hunt, and Spider.primaryCreature marks
        /// the lead spider, so everything else can be cleared without ending the hunt itself.
        /// </summary>
        internal int KillHuntExtras(bool announce, bool babiesOnly)
        {
            int killed = 0;
            HuntManager hm;
            try { hm = HuntManager.Instance; }
            catch { hm = null; }

            var targets = new List<Enemy>();
            try
            {
                if (Net.Alive(hm) && hm.allEnemies != null)
                {
                    int count = hm.allEnemies.Count;
                    for (int i = 0; i < count; i++)
                    {
                        Enemy e = hm.allEnemies[i];
                        if (Net.Alive(e)) targets.Add(e);
                    }
                }
            }
            catch (Exception ex) { Log.Debug("allEnemies: " + ex.Message); }

            // The list only holds what the hunt manager spawned; anything placed by an event is found
            // by sweeping instead, so the button clears the room either way.
            if (targets.Count == 0 && (announce || (Net.Alive(hm) && hm.huntInProgress)))
            {
                List<Enemy> loose = Net.FindActive<Enemy>();
                for (int i = 0; i < loose.Count; i++) if (Net.Alive(loose[i])) targets.Add(loose[i]);
            }

            for (int i = 0; i < targets.Count; i++)
            {
                Enemy e = targets[i];
                if (babiesOnly && !IsBaby(e)) continue;
                if (!babiesOnly && IsPrimary(e)) continue;
                if (Kill(e)) killed++;
            }

            ExtrasKilled += killed;
            if (announce || killed > 0)
            {
                LastResult = (babiesOnly ? "Baby dolls cleared: " : "Hunt extras cleared: ") + killed;
                Log.Msg(LastResult + ".");
            }
            return killed;
        }

        private static bool IsBaby(Enemy e)
        {
            try { return e.TryCast<BabyDoll>() != null; }
            catch { return false; }
        }

        /// <summary>The hunt's own creature - never killed by the "extras" sweep.</summary>
        private static bool IsPrimary(Enemy e)
        {
            try
            {
                Spider sp = e.TryCast<Spider>();
                if (sp != null && sp.primaryCreature) return true;
            }
            catch { }
            try
            {
                Hittable h = e.hittable;
                if (Net.Alive(h) && h.isEntity) return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Kills through the Hittable, which is the same path a shot takes: the death event fires, the
        /// hunt manager counts the death and the body is cleaned up. Falls back to the enemy's own
        /// death RPC, then to a very large hit.
        /// </summary>
        private static bool Kill(Enemy e)
        {
            Hittable h = null;
            try { h = e.hittable; } catch { }

            if (Net.Alive(h))
            {
                try { Rpc.Call(h, delegate { h.Rpc_Die(); }, "Hittable.Die"); return true; }
                catch (Exception ex) { Log.Debug("hittable die: " + ex.Message); }
            }

            try
            {
                BabyDoll bd = e.TryCast<BabyDoll>();
                if (bd != null) { Rpc.Call(bd, delegate { bd.Rpc_Die(); }, "BabyDoll.Die"); return true; }
            }
            catch (Exception ex) { Log.Debug("baby die: " + ex.Message); }

            try
            {
                Spider sp = e.TryCast<Spider>();
                if (sp != null) { Rpc.Call(sp, delegate { sp.Rpc_Leave(); }, "Spider.Leave"); return true; }
            }
            catch (Exception ex) { Log.Debug("spider leave: " + ex.Message); }

            if (Net.Alive(h))
            {
                try
                {
                    Vector3 from = e.transform.position + Vector3.up;
                    Rpc.Call(h, delegate { h.Rpc_Hit(9999f, from, true, false, "Bullet"); }, "Hittable.Hit");
                    return true;
                }
                catch (Exception ex) { Log.Debug("hittable hit: " + ex.Message); }
            }
            return false;
        }

        // ================================================================ keeping people upright

        /// <summary>
        /// The catch-all behind the human shield.
        ///
        /// Patching the ways health goes down only helps for the ways that were found - and a grenade
        /// kept finding another one. This simply refuses to let a shielded person's health sit at or
        /// below zero, whatever drained it, so no unpatched path can finish anyone off.
        /// </summary>
        private void HoldPeopleUp(float now)
        {
            if (now >= _nextHittableScan || _hittables.Count == 0)
            {
                _nextHittableScan = now + 5f;
                _hittables = Net.FindActive<Hittable>();
            }

            int saved = 0;
            for (int i = 0; i < _hittables.Count; i++)
            {
                Hittable h = _hittables[i];
                if (!Net.Alive(h)) continue;
                try
                {
                    HumanShield.Kind kind = HumanShield.Classify(h);
                    if (kind == HumanShield.Kind.Fair) continue;

                    if (kind == HumanShield.Kind.Bystander)
                    {
                        // Untouched means untouched: put them back to full, so no damage-over-time
                        // system (fire, stun, bleed) can whittle a bystander down between ticks.
                        float max = h.maxHealth;
                        if (max > 0f && h.health < max) { h.health = max; HumanShield.Blocked++; saved++; }
                        continue;
                    }

                    if (h.health < HumanShield.Floor)
                    {
                        h.health = HumanShield.Floor;
                        HumanShield.Blocked++;
                        saved++;
                    }
                }
                catch { }
            }

            if (saved > 0 && !_drainNoted)
            {
                _drainNoted = true;
                Log.Msg("Human shield: caught " + saved + " person(s) drained to zero by an unpatched path.");
            }
        }

        // ================================================================ menu item lock

        /// <summary>Set every frame by the suite: is the mod menu on screen right now?</summary>
        internal bool MenuOpen;

        private bool _menuPausedItems;

        /// <summary>
        /// Stops the game using the item in your hands while the menu is up.
        ///
        /// Clicking a checkbox was also pulling the trigger: the menu is an overlay, and the game's
        /// own item handling never learns that a click was meant for something else. The grenade
        /// launcher already refused to fire with the menu open, but that only covered the suite's own
        /// weapon - the vanilla one underneath it kept shooting. InventoryManager.PauseUseItem is the
        /// game's own answer, and it is what dialogue uses for the same reason.
        ///
        /// Only ever unpaused if this is what paused it, so a conversation or a computer screen that
        /// legitimately holds the lock keeps holding it after the menu closes.
        /// </summary>
        private void ApplyMenuItemLock()
        {
            InventoryManager inv = Net.LocalInventory;
            if (!Net.Alive(inv))
            {
                _menuPausedItems = false;
                return;
            }

            try
            {
                if (MenuOpen)
                {
                    if (!_menuPausedItems && inv.canControlItem)
                    {
                        inv.PauseUseItem();
                        _menuPausedItems = true;
                    }
                    return;
                }

                if (_menuPausedItems)
                {
                    _menuPausedItems = false;
                    if (!inv.canControlItem) inv.UnpauseUseItem();
                }
            }
            catch (Exception ex) { Log.Debug("menu item lock: " + ex.Message); }
        }

        // ================================================================ item lock

        private void ReleaseItemLock(float now)
        {
            try
            {
                // The menu holds the lock on purpose while it is open. Releasing it here would undo
                // that a tick later and put the trigger back under the checkboxes.
                if (MenuOpen || _menuPausedItems) { _freeSince = 0f; return; }

                InventoryManager inv = Net.LocalInventory;
                PlayerManager pm = Net.LocalPlayer;
                if (!Net.Alive(inv) || !Net.Alive(pm)) { _freeSince = 0f; return; }

                // Only ever act when the player is plainly back in normal play - anything else is a
                // legitimate pause that the game will lift itself.
                bool busy;
                try
                {
                    busy = pm.paused || pm.dead || pm._downed || pm.finished ||
                           pm.lookingAtComputer || pm.lookingAtShelf || inv.tasking;
                    DialogueInteractable npc = pm.curNpcScript;
                    if (!busy && Net.Alive(npc)) busy = npc.interacting || npc.inQuestioningMenu;
                }
                catch { return; }

                if (busy) { _freeSince = 0f; return; }

                // Give the game a moment to lift it on its own before stepping in.
                if (_freeSince == 0f) { _freeSince = now; return; }
                if (now - _freeSince < 1f) return;

                bool locked = false;
                try { locked = !inv.canControlItem || inv.inventoryPaused; }
                catch { return; }
                if (!locked) return;

                try { if (inv.inventoryPaused) inv.UnpauseInventory(); }
                catch (Exception ex) { Log.Debug("unpause inventory: " + ex.Message); }
                try { if (!inv.canControlItem) inv.UnpauseUseItem(); }
                catch (Exception ex) { Log.Debug("unpause use item: " + ex.Message); }

                _freeSince = now;   // do not re-fire every tick if something keeps re-locking it
                InventoryUnlocks++;
                LastResult = "Item lock released";
                Log.Msg(LastResult + " (a conversation left it paused).");
            }
            catch (Exception ex) { Log.Debug("item lock: " + ex.Message); }
        }

        // ================================================================ blocking colliders

        /// <summary>
        /// Scenery you keep catching on, switched off wholesale.
        ///
        /// These are plain colliders with no script behind them - curbs sit on their own "Curb" layer,
        /// the chain fences are just named that way - so there is nothing to patch. Disabling the
        /// collider component itself is what makes it apply to everything rather than only to you:
        /// customers, thrown grenades and the entity stop colliding with them too. Every collider
        /// switched off is remembered, so turning a group back on restores exactly those and nothing
        /// else, and each group re-asserts itself because a new day rebuilds the scene.
        /// </summary>
        private sealed class BlockerGroup
        {
            internal string Label;
            internal string LayerName;          // null when the objects have no layer of their own
            internal string[] Words;
            internal bool Active;

            /// <summary>Set after a scene change: keep looking until the scenery actually exists.</summary>
            internal bool NeedsApply;
            internal int Attempts;
            internal readonly List<Collider> Disabled = new List<Collider>();
        }

        private readonly BlockerGroup _curbGroup = new BlockerGroup
        {
            Label = "Curbs",
            LayerName = "Curb",
            Words = new[] { "curb" }
        };

        private readonly BlockerGroup _fenceGroup = new BlockerGroup
        {
            Label = "Chain fences",
            LayerName = null,
            Words = new[] { "chain fence", "chainfence", "chain_fence" }
        };

        private readonly BlockerGroup _roofGroup = new BlockerGroup
        {
            Label = "Forest roof",
            LayerName = null,
            Words = new[] { "forestroof", "forest roof" }
        };

        private float _nextBlockerSweep;

        internal bool RemoveCurbs
        {
            get { return _curbGroup.Active; }
            set { SetGroup(_curbGroup, value); }
        }

        internal bool RemoveFences
        {
            get { return _fenceGroup.Active; }
            set { SetGroup(_fenceGroup, value); }
        }

        internal bool RemoveForestRoof
        {
            get { return _roofGroup.Active; }
            set { SetGroup(_roofGroup, value); }
        }

        private void SetGroup(BlockerGroup g, bool on)
        {
            if (on == g.Active) return;
            g.Active = on;
            if (on)
            {
                g.NeedsApply = true;
                g.Attempts = 0;
                DisableGroup(g);
            }
            else RestoreGroup(g);
        }

        private void DisableGroup(BlockerGroup g)
        {
            int off = 0;
            try
            {
                int layer = string.IsNullOrEmpty(g.LayerName) ? -1 : LayerMask.NameToLayer(g.LayerName);
                List<Collider> all = Net.FindActive<Collider>();

                for (int i = 0; i < all.Count; i++)
                {
                    Collider c = all[i];
                    if (!Net.Alive(c)) continue;
                    try
                    {
                        if (!c.enabled) continue;
                        GameObject go = c.gameObject;
                        if (!Matches(g, go, layer)) continue;

                        c.enabled = false;
                        g.Disabled.Add(c);
                        off++;
                    }
                    catch { }
                }
            }
            catch (Exception ex) { Log.Ex("disable " + g.Label, ex); }

            if (off > 0) g.NeedsApply = false;
            LastResult = g.Label + " off (" + off + " collider(s))";
            if (off > 0 || g.Attempts <= 1) Log.Msg(LastResult + ".");

            // Nothing to switch off usually means another group already did it - say where they went.
            if (off == 0) LogFenceCandidates(g);
        }

        private static readonly HashSet<string> _probedGroups = new HashSet<string>();

        /// <summary>Nothing matched: print what the scene actually calls these things.</summary>
        private static void LogFenceCandidates(BlockerGroup g)
        {
            if (!_probedGroups.Add(g.Label)) return;
            try
            {
                List<Collider> all = Net.FindActive<Collider>();
                string found = "";
                int n = 0;
                for (int i = 0; i < all.Count && n < 30; i++)
                {
                    Collider c = all[i];
                    if (!Net.Alive(c)) continue;
                    try
                    {
                        GameObject go = c.gameObject;
                        string nm = go.name.ToLowerInvariant();
                        string pn = "";
                        try { if (go.transform.parent != null) pn = go.transform.parent.name.ToLowerInvariant(); }
                        catch { }

                        if (nm.Contains("fence") || nm.Contains("chain") || nm.Contains("gate") ||
                            pn.Contains("fence") || pn.Contains("chain"))
                        {
                            found += " [" + go.name + "<" + pn + " L" + go.layer +
                                     (c.enabled ? " ON" : " off") +
                                     (go.activeInHierarchy ? "" : " inactive") +
                                     (c.isTrigger ? " trig" : "") + "]";
                            n++;
                        }
                    }
                    catch { }
                }
                Log.Debug("blocker candidates:" + (found.Length == 0 ? " none found among " + all.Count + " colliders" : found));
            }
            catch (Exception ex) { Log.Debug("fence probe: " + ex.Message); }
        }

        /// <summary>Matches on the group's own layer, or on the object's name - or its parent's.</summary>
        private static bool Matches(BlockerGroup g, GameObject go, int layer)
        {
            if (layer >= 0 && go.layer == layer) return true;

            string name = go.name;
            for (int w = 0; w < g.Words.Length; w++)
                if (name.IndexOf(g.Words[w], StringComparison.OrdinalIgnoreCase) >= 0) return true;

            // A fence is usually a named parent with unnamed collider children hanging off it.
            try
            {
                Transform t = go.transform.parent;
                for (int depth = 0; depth < 2 && t != null; depth++)
                {
                    string pn = t.name;
                    for (int w = 0; w < g.Words.Length; w++)
                        if (pn.IndexOf(g.Words[w], StringComparison.OrdinalIgnoreCase) >= 0) return true;
                    t = t.parent;
                }
            }
            catch { }
            return false;
        }

        private void RestoreGroup(BlockerGroup g)
        {
            int on = 0;
            for (int i = 0; i < g.Disabled.Count; i++)
            {
                Collider c = g.Disabled[i];
                if (!Net.Alive(c)) continue;
                try { c.enabled = true; on++; } catch { }
            }
            g.Disabled.Clear();
            LastResult = g.Label + " restored (" + on + ")";
            Log.Msg(LastResult + ".");
        }

        /// <summary>Re-assert, since a new day rebuilds the scene and the game can re-enable them.</summary>
        private static void ReArm(BlockerGroup g)
        {
            g.Disabled.Clear();
            g.Attempts = 0;
            g.NeedsApply = g.Active;
        }

        private void HoldBlockers(float now)
        {
            if (now < _nextBlockerSweep) return;

            // Look often while still waiting for a scene to finish building itself, then settle down.
            bool waiting = (_curbGroup.Active && _curbGroup.NeedsApply) ||
                           (_fenceGroup.Active && _fenceGroup.NeedsApply) ||
                           (_roofGroup.Active && _roofGroup.NeedsApply);
            _nextBlockerSweep = now + (waiting ? 1f : 5f);

            HoldGroup(_curbGroup);
            HoldGroup(_fenceGroup);
            HoldGroup(_roofGroup);
        }

        private void HoldGroup(BlockerGroup g)
        {
            if (!g.Active) return;

            int off = 0;
            for (int i = g.Disabled.Count - 1; i >= 0; i--)
            {
                Collider c = g.Disabled[i];
                if (!Net.Alive(c)) { g.Disabled.RemoveAt(i); continue; }
                try { if (c.enabled) { c.enabled = false; off++; } }
                catch { g.Disabled.RemoveAt(i); }
            }

            // A new scene has to be waited for: the first sweeps after a load can land before the
            // curbs exist, and the old logic disarmed the group permanently on a single empty result.
            // Keep trying for a bounded window instead, then stop so a group that genuinely matches
            // nothing does not rescan forever.
            if (g.Disabled.Count == 0 && g.NeedsApply && g.Attempts < 30)
            {
                g.Attempts++;
                DisableGroup(g);
                if (g.Disabled.Count > 0) g.NeedsApply = false;
            }
            else if (off > 0) Log.Debug("Re-disabled " + off + " " + g.Label + " collider(s).");
        }

        // ================================================================ bells

        private List<EntryDoor> _doors = new List<EntryDoor>();
        private List<Register> _bellRegisters = new List<Register>();
        private float _nextBellScan;

        /// <summary>
        /// Mutes (or unmutes) every door chime and counter bell in the scene. Both are fixed scene
        /// objects, so they are found rarely and the mute is simply re-asserted - the game re-enables
        /// audio sources on its own in places, and a new day rebuilds them.
        /// </summary>
        private void ApplyBellMute()
        {
            // Nothing to do once the world already agrees with the toggle, unless it was just changed.
            if (_bellsApplied && !_silenceBells) return;

            float now = Time.unscaledTime;
            if (now >= _nextBellScan || (_doors.Count == 0 && _bellRegisters.Count == 0))
            {
                _nextBellScan = now + 20f;
                _doors = Net.FindActive<EntryDoor>();
                _bellRegisters = Net.FindActive<Register>();
            }

            int muted = 0;
            for (int i = 0; i < _doors.Count; i++)
            {
                EntryDoor d = _doors[i];
                if (!Net.Alive(d)) continue;
                try
                {
                    AudioSource a = d.sfx;
                    if (Net.Alive(a) && a.mute != _silenceBells) { a.mute = _silenceBells; muted++; }
                }
                catch { }
            }

            for (int i = 0; i < _bellRegisters.Count; i++)
            {
                Register r = _bellRegisters[i];
                if (!Net.Alive(r)) continue;
                try
                {
                    AudioSource a = r.bellSFX;
                    if (Net.Alive(a) && a.mute != _silenceBells) { a.mute = _silenceBells; muted++; }
                }
                catch { }
            }

            _bellsApplied = true;
            if (muted > 0)
            {
                LastResult = (_silenceBells ? "Silenced " : "Restored ") + muted + " bell(s)";
                Log.Msg(LastResult + ".");
            }
        }

        // ================================================================ hunt countdown

        /// <summary>
        /// HuntManager.StartHunt only flips huntInProgress and schedules SpawnEnemies on a delay -
        /// that delay is the warning you get before the entity actually turns up. Cancelling the
        /// pending invoke and calling SpawnEnemies now skips the wait without skipping the hunt.
        /// </summary>
        internal bool SkipHuntCountdown(bool announce)
        {
            try
            {
                HuntManager hm = HuntManager.Instance;
                if (!Net.Alive(hm))
                {
                    if (announce) { LastResult = "No hunt manager"; Log.Warn(LastResult + "."); }
                    return false;
                }

                if (!hm.huntInProgress)
                {
                    if (announce) { LastResult = "No hunt is starting"; Log.Msg(LastResult + "."); }
                    return false;
                }

                if (hm.enemiesSpawned > 0)
                {
                    if (announce) { LastResult = "Entity is already here"; Log.Msg(LastResult + "."); }
                    return false;
                }

                // Without this the scheduled call still lands later and spawns a second wave.
                try { hm.CancelInvoke("SpawnEnemies"); }
                catch (Exception ex) { Log.Debug("CancelInvoke SpawnEnemies: " + ex.Message); }

                // And drop the announced countdown to nothing. StoreManager.secondsLeft is what the
                // "the entity will arrive in N seconds" line reads from - that is what that field
                // actually is - so leaving it high means the entity turns up while the warning is
                // still counting down at you.
                try
                {
                    StoreManager sm = StoreManager.Instance;
                    if (Net.Alive(sm) && sm.secondsLeft > 0) sm.secondsLeft = 0;
                }
                catch (Exception ex) { Log.Debug("clear entity countdown: " + ex.Message); }

                hm.SpawnEnemies();
                LastResult = "Entity countdown skipped";
                Log.Msg(LastResult + ".");
                return true;
            }
            catch (Exception ex)
            {
                Log.Ex("skip hunt countdown", ex);
                LastResult = "Skip failed";
                return false;
            }
        }

        /// <summary>
        /// Switches off the hunt explanation panels while a hunt is running. Checked often but cheap:
        /// it is two SetActive calls on objects the StoreManager already holds references to.
        /// </summary>
        private void DismissHuntBriefing()
        {
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (!Net.Alive(sm) || !sm.inHunt) return;
                if (Hide(sm.huntExplanation) | Hide(sm.huntExplanation2)) BriefingsSkipped++;
            }
            catch (Exception ex) { Log.Debug("hunt briefing: " + ex.Message); }
        }

        // ================================================================ end of day

        private bool _eodBoosted;
        private float _eodRestoreTo = 1f;
        private float _nextEodCheck;
        private EndOfDayReport _eodReport;
        private float _nextEodFind;

        /// <summary>
        /// Raises the clock while the end-of-day report is on screen and puts it back the moment it is
        /// not. Only ever restores a value this raised, so it cannot fight anything else that owns
        /// timeScale, and the report's own numbers are untouched - this changes how long they take to
        /// count, not what they count to.
        /// </summary>
        private void DriveEndOfDay(float now)
        {
            if (now < _nextEodCheck) return;
            _nextEodCheck = now + 0.25f;

            // The report object lives for the whole scene, so find it once and keep it. Asking
            // FindObjectsOfType four times a second for a type with one instance still walks the
            // whole scene every time, and that cost lands on whatever else is busy that frame.
            if (!Net.Alive(_eodReport))
            {
                _eodReport = null;
                if (now >= _nextEodFind)
                {
                    _nextEodFind = now + 5f;
                    try
                    {
                        List<EndOfDayReport> reports = Net.FindActive<EndOfDayReport>();
                        if (reports.Count > 0) _eodReport = reports[0];
                    }
                    catch (Exception ex) { Log.Debug("eod scan: " + ex.Message); }
                }
            }

            // Keying this on eodReportHolder alone was too narrow: the customer report is its own
            // scroll object, and if that is what is on screen while the holder is not, the boost
            // never engaged and the report crawled exactly as before. Take either.
            bool showing = false;
            if (Net.Alive(_eodReport))
            {
                try
                {
                    GameObject holder = _eodReport.eodReportHolder;
                    if (holder != null && holder.activeInHierarchy) showing = true;
                }
                catch { }
                if (!showing)
                {
                    try
                    {
                        Transform scroll = _eodReport.customerReportScrollHolder;
                        if (scroll != null && scroll.gameObject.activeInHierarchy) showing = true;
                    }
                    catch { }
                }
                if (!showing)
                {
                    // showingRevenue is the amount still being counted out, not a flag.
                    try { showing = _eodReport.showingRevenue > 0f; } catch { }
                }
            }

            if (showing && FastEndOfDay)
            {
                if (!_eodBoosted)
                {
                    _eodRestoreTo = Time.timeScale;
                    _eodBoosted = true;
                    Log.Msg("End-of-day report sped up to " + EndOfDaySpeed.ToString("0.0") + "x.");
                }
                float want = Mathf.Clamp(EndOfDaySpeed, 1f, 20f);
                if (!Mathf.Approximately(Time.timeScale, want)) Time.timeScale = want;
                RushCustomerReport();
                _nextEodCheck = now + 0.1f;   // while it is up, check often enough to keep pushing
                return;
            }

            RestoreEndOfDay();
        }

        /// <summary>
        /// Pushes the customer report along by hand.
        ///
        /// The report deals its folders out one at a time through ShowNextCharacter, and a night with
        /// a full roster is a long wait for a list you have already read. Raising the clock helps only
        /// if that pacing is on a scaled timer, which is not something worth assuming, so ask for the
        /// next card directly as well.
        ///
        /// One per pass, never past the end of npcFolders, and each call boxed in its own try: the
        /// game is still dealing cards on its own schedule underneath this, and the two must not race
        /// each other off the end of the array.
        /// </summary>
        private void RushCustomerReport()
        {
            if (!Net.Alive(_eodReport)) return;
            try
            {
                var folders = _eodReport.npcFolders;
                if (folders == null) return;
                if (_eodReport.curCharacterIndex >= folders.Length) return;
                _eodReport.ShowNextCharacter();
                _reportCardsPushed++;
            }
            catch (Exception ex) { Log.Debug("rush customer report: " + ex.Message); }
        }

        internal int ReportCardsPushed { get { return _reportCardsPushed; } }
        private int _reportCardsPushed;

        private void RestoreEndOfDay()
        {
            if (!_eodBoosted) return;
            _eodBoosted = false;
            try { Time.timeScale = _eodRestoreTo <= 0f ? 1f : _eodRestoreTo; }
            catch { }
        }

        // ================================================================ scanning

        /// <summary>
        /// The emoti-scope finishes its scan at once.
        ///
        /// curScan is the progress the scan bar is drawn from, and emotiscopeFound is what the game
        /// sets when it is satisfied. Pushing the progress rather than forcing the result means the
        /// game's own threshold fires, and everything that hangs off it - the emotion text, the sound
        /// - happens the way it normally would, just sooner. Adding a fixed step per frame instead of
        /// slamming a value keeps this right whether curScan counts seconds or a 0-1 fill.
        /// </summary>
        internal bool InstantEmotiscope;

        internal int ScansRushed;

        private void RushEmotiscope()
        {
            try
            {
                InventoryManager inv = Net.LocalInventory;
                if (!Net.Alive(inv)) return;
                if (!inv.emotiscopeScanning || inv.emotiscopeFound) return;
                inv.curScan += 1f;
                ScansRushed++;
            }
            catch (Exception ex) { Log.Debug("emotiscope: " + ex.Message); }
        }

        /// <summary>
        /// Shows what the anomaly lens shows, without the lens.
        ///
        /// The first attempt disabled ScanOnlyObject, on the reasoning that a component keeping itself
        /// updated around a target collider is a mask. It changed nothing on screen, so that was not
        /// the mechanism, and guessing a second time is not worth anyone's evening.
        ///
        /// The lens is far more likely to be a camera that renders layers the main camera does not.
        /// That can be discovered rather than assumed: find the lens, find its camera, and take the
        /// layers in its culling mask that the player camera is missing. If the difference is real,
        /// adding it to the main camera makes the hidden figure visible everywhere. If there is no
        /// camera, or no difference, that is worth knowing too - it is logged either way, along with
        /// the layer names, so the next step is informed instead of another guess.
        ///
        /// ScanOnlyObject is still switched off alongside it, since it costs nothing and may yet be
        /// part of the picture.
        /// </summary>
        internal bool RevealScanOnly;


        /// <summary>
        /// Dumps the rig of every doppelganger in the scene.
        ///
        /// The lens only affects doppelgangers, which puts the hidden figure on the NPC rather than on
        /// the lens - a child object or a renderer that is switched off until you look through it.
        /// This prints every transform and every renderer under each one, with its active and enabled
        /// state, so the thing that differs between "seen through the lens" and "not" is visible in
        /// black and white rather than reasoned about.
        ///
        /// Once that is known the reveal is a matter of flipping it, with the jumpscare creature and
        /// the fully-invisible doppelganger left alone.
        /// </summary>
        internal void DumpDoppelgangerRig()
        {
            try
            {
                List<StoreBrowseBehaviour> all = Net.FindActive<StoreBrowseBehaviour>();
                int shown = 0;

                for (int i = 0; i < all.Count && shown < 3; i++)
                {
                    StoreBrowseBehaviour b = all[i];
                    if (!Net.Alive(b)) continue;

                    bool doppel = false;
                    try { doppel = b.isDoppelganger; } catch { }
                    if (!doppel) continue;
                    shown++;

                    Log.Msg("DOPPELGANGER RIG: " + b.gameObject.name);

                    Transform[] parts = b.GetComponentsInChildren<Transform>(true);
                    for (int k = 0; k < parts.Length && k < 80; k++)
                    {
                        Transform t = parts[k];
                        if (t == null) continue;
                        Log.Msg("   T " + t.name + " active=" + t.gameObject.activeSelf +
                                " layer=" + LayerMask.LayerToName(t.gameObject.layer));
                    }

                    Renderer[] rends = b.GetComponentsInChildren<Renderer>(true);
                    for (int k = 0; k < rends.Length && k < 40; k++)
                    {
                        Renderer r = rends[k];
                        if (r == null) continue;
                        string shader = "?";
                        try { shader = r.sharedMaterial == null ? "(no material)" : r.sharedMaterial.shader.name; }
                        catch { }
                        Log.Msg("   R " + r.gameObject.name + " enabled=" + r.enabled +
                                " active=" + r.gameObject.activeSelf +
                                " layer=" + LayerMask.LayerToName(r.gameObject.layer) +
                                " shader=" + shader);
                    }
                }

                if (shown == 0)
                {
                    LastResult = "No doppelganger in the scene to dump";
                    Log.Warn(LastResult + " - do this with one in the store.");
                    return;
                }

                Camera main = Camera.main;
                if (main != null) Log.Msg("MAIN CAMERA mask=" + MaskNames(main.cullingMask));
                LastResult = "Dumped " + shown + " doppelganger rig(s)";
            }
            catch (Exception ex) { Log.Ex("dump doppelganger rig", ex); LastResult = "Dump failed"; }
        }

        /// <summary>
        /// Layers involved, resolved once. GhostCamera is the one the main camera does not draw.
        /// </summary>
        private int _ghostLayer = -2;
        private int _visibleLayer = -2;
        private int _humanLayer = -2;

        private void ResolveLayers()
        {
            if (_ghostLayer != -2) return;
            _ghostLayer = LayerMask.NameToLayer("GhostCamera");
            _visibleLayer = LayerMask.NameToLayer("NPC");
            _humanLayer = LayerMask.NameToLayer("NPCNotVisibleInGhostCamera");
            if (_ghostLayer < 0 || _visibleLayer < 0 || _humanLayer < 0)
                Log.Warn("Anomaly reveal: expected layers not found (GhostCamera=" + _ghostLayer +
                         ", NPC=" + _visibleLayer + ", NPCNotVisibleInGhostCamera=" + _humanLayer +
                         "). Reveal disabled.");
        }

        /// <summary>Transforms moved off the GhostCamera layer, with the layer they came from.</summary>
        private readonly List<KeyValuePair<Transform, int>> _relayered = new List<KeyValuePair<Transform, int>>();

        /// <summary>Doppelgangers already dealt with, so the swap is never applied to one twice.</summary>
        private readonly HashSet<int> _revealedNpcs = new HashSet<int>();

        internal int AnomaliesRevealed;

        /// <summary>
        /// Is any part of this doppelganger drawn by the player camera as things stand?
        ///
        /// This is what separates the ordinary case from the one to leave alone. A doppelganger with a
        /// human body on NPCNotVisibleInGhostCamera and a second rig on GhostCamera is the "there is
        /// something else standing there" case, and revealing the second rig is the whole point. One
        /// whose only renderers are on GhostCamera has no body at all in the normal view - that is the
        /// fully-invisible one, and re-layering it would conjure a whole character out of nothing.
        /// </summary>
        private bool HasVisibleBody(StoreBrowseBehaviour b, int mainMask)
        {
            try
            {
                Renderer[] rends = b.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rends.Length; i++)
                {
                    Renderer r = rends[i];
                    if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
                    if ((mainMask & (1 << r.gameObject.layer)) != 0) return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>Every component and camera mask on the lens, so the mechanism stops being a guess.</summary>
        internal void DumpAnomalyLens()
        {
            try
            {
                CurrentDayManager dm = CurrentDayManager.Instance;
                if (!Net.Alive(dm)) { LastResult = "CurrentDayManager not ready"; Log.Warn(LastResult + "."); return; }

                GameObject lens = dm.anomalyLens;
                Log.Msg("ANOMALY LENS: " + (lens == null ? "(null)" : lens.name +
                        " active=" + lens.activeInHierarchy + " layer=" + LayerMask.LayerToName(lens.layer)));

                if (lens != null)
                {
                    Component[] parts = lens.GetComponentsInChildren<Component>(true);
                    for (int i = 0; i < parts.Length && i < 60; i++)
                    {
                        Component c = parts[i];
                        if (c == null) continue;
                        string line = "   " + c.GetIl2CppType().Name + " on " + c.gameObject.name +
                                      " (layer " + LayerMask.LayerToName(c.gameObject.layer) + ")";
                        Camera cam = c.TryCast<Camera>();
                        if (cam != null) line += "  CAMERA mask=" + MaskNames(cam.cullingMask) +
                                                 " depth=" + cam.depth + " clear=" + cam.clearFlags;
                        Log.Msg(line);
                    }
                }

                Camera main = Camera.main;
                if (main != null) Log.Msg("MAIN CAMERA: " + main.name + " mask=" + MaskNames(main.cullingMask));

                Log.Msg("SCAN-ONLY OBJECTS: " + Net.FindActive<ScanOnlyObject>().Count + " active in the scene.");
                LastResult = "Anomaly lens dumped to the log";
            }
            catch (Exception ex) { Log.Ex("dump anomaly lens", ex); LastResult = "Dump failed"; }
        }

        private static string MaskNames(int mask)
        {
            string s = "";
            for (int i = 0; i < 32; i++)
            {
                if ((mask & (1 << i)) == 0) continue;
                string n = LayerMask.LayerToName(i);
                s += (s.Length > 0 ? "," : "") + (string.IsNullOrEmpty(n) ? i.ToString() : n);
            }
            return s.Length == 0 ? "(none)" : s;
        }

        private readonly List<ScanOnlyObject> _scanOnly = new List<ScanOnlyObject>();
        private float _nextScanOnlySweep;
        private bool _scanOnlyRevealed;

        private void ApplyScanReveal(float now, bool reveal)
        {
            if (!reveal && !_scanOnlyRevealed) return;
            if (now < _nextScanOnlySweep) return;
            _nextScanOnlySweep = now + 2f;

            _scanOnly.Clear();
            _scanOnly.AddRange(Net.FindActive<ScanOnlyObject>());
            for (int i = 0; i < _scanOnly.Count; i++)
            {
                ScanOnlyObject s = _scanOnly[i];
                if (!Net.Alive(s)) continue;
                if (reveal && IsJumpscare(s)) continue;   // leave that one where it is
                try { if (s.enabled == reveal) s.enabled = !reveal; }
                catch { }
            }
            ApplyAnomalyMask(reveal);
            _scanOnlyRevealed = reveal;
        }

        /// <summary>
        /// Moves the doppelganger's hidden second rig onto a layer the player camera draws.
        ///
        /// The dump settled the mechanism: a doppelganger like Ren Takahashi carries two complete
        /// rigs - the human body on NPCNotVisibleInGhostCamera, and a second one, "Wade (1)", on
        /// GhostCamera. The main camera's mask has every layer in that list except GhostCamera, so the
        /// second rig is there in the scene the whole time and simply is not drawn. The lens is a PDA
        /// showing a ghost-camera feed, which is why it has no camera of its own and why the earlier
        /// attempts found nothing.
        ///
        /// Re-layering the individual objects rather than adding GhostCamera to the camera mask is
        /// what makes the exclusions possible at all: a mask is all-or-nothing and would drag in the
        /// jumpscare and the invisible doppelganger with everything else. Per object, each one can be
        /// judged on its own. Originals are kept so switching off puts every layer back.
        /// </summary>
        private void ApplyAnomalyMask(bool reveal)
        {
            ResolveLayers();
            if (_ghostLayer < 0 || _visibleLayer < 0) return;

            if (!reveal)
            {
                RestoreAnomalyLayers();
                return;
            }

            Camera main = Camera.main;
            if (main == null) return;
            int mask = main.cullingMask;

            try
            {
                List<StoreBrowseBehaviour> all = Net.FindActive<StoreBrowseBehaviour>();
                for (int i = 0; i < all.Count; i++)
                {
                    StoreBrowseBehaviour b = all[i];
                    if (!Net.Alive(b)) continue;

                    bool doppel = false;
                    try { doppel = b.isDoppelganger; } catch { }
                    if (!doppel) continue;

                    // Once each, and this is not optional. The swap parks the human body on
                    // GhostCamera to hide it, and GhostCamera is precisely what the first loop looks
                    // for - so a second pass over the same doppelganger would promote the body it had
                    // just hidden straight back into view.
                    int id;
                    try { id = b.GetInstanceID(); } catch { continue; }
                    if (!_revealedNpcs.Add(id)) continue;

                    // The jumpscare creature stays where it is.
                    bool scare = false;
                    try { scare = b.GetComponentInChildren<JumpscarePlayer>(true) != null; } catch { }
                    if (scare) continue;

                    // So does the one with no body in the normal view.
                    if (!HasVisibleBody(b, mask)) continue;

                    Transform[] parts = null;
                    try { parts = b.GetComponentsInChildren<Transform>(true); } catch { }
                    if (parts == null) continue;

                    // Bring the creature rig into view.
                    for (int k = 0; k < parts.Length; k++)
                    {
                        Transform t = parts[k];
                        if (t == null) continue;
                        try
                        {
                            if (t.gameObject.layer != _ghostLayer) continue;
                            _relayered.Add(new KeyValuePair<Transform, int>(t, t.gameObject.layer));
                            t.gameObject.layer = _visibleLayer;
                            AnomaliesRevealed++;
                        }
                        catch { }
                    }

                    // And take the human body out of it, which is the half that was missing. The lens
                    // does not draw the person and the creature at once - the layer names say so
                    // outright, NPCNotVisibleInGhostCamera being exactly what the ghost feed leaves
                    // out - so showing both just stacks one inside the other.
                    //
                    // Only the skinned body mesh is hidden. The patience circle and its bar sit on the
                    // same layer and are sprites, not skin; hiding those would take the customer's UI
                    // away along with their face, which is not what looking through a lens does.
                    Renderer[] rends = null;
                    try { rends = b.GetComponentsInChildren<Renderer>(true); } catch { }
                    if (rends == null) continue;

                    for (int k = 0; k < rends.Length; k++)
                    {
                        Renderer r = rends[k];
                        if (r == null) continue;
                        try
                        {
                            if (r.gameObject.layer != _humanLayer) continue;
                            if (r.TryCast<SkinnedMeshRenderer>() == null) continue;
                            _relayered.Add(new KeyValuePair<Transform, int>(r.transform, r.gameObject.layer));
                            r.gameObject.layer = _ghostLayer;   // a layer the player camera does not draw
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex) { Log.Debug("anomaly reveal: " + ex.Message); }
        }

        private void RestoreAnomalyLayers()
        {
            if (_relayered.Count == 0) return;
            for (int i = 0; i < _relayered.Count; i++)
            {
                Transform t = _relayered[i].Key;
                if (t == null) continue;
                try { t.gameObject.layer = _relayered[i].Value; } catch { }
            }
            _relayered.Clear();
            _revealedNpcs.Clear();
            Log.Msg("Anomaly reveal: layers restored.");
        }

        /// <summary>
        /// The jumpscare is the one scan-only thing worth leaving hidden - revealing it means walking
        /// around with it permanently in shot, which is neither useful nor the point. Identified by
        /// its own JumpscarePlayer component, with a name check behind it for anything that carries
        /// the effect without the script.
        /// </summary>
        private static bool IsJumpscare(ScanOnlyObject s)
        {
            try { if (s.GetComponentInParent<JumpscarePlayer>() != null) return true; } catch { }
            try { if (s.GetComponentInChildren<JumpscarePlayer>(true) != null) return true; } catch { }

            try
            {
                string n = s.gameObject.name;
                if (!string.IsNullOrEmpty(n))
                {
                    n = n.ToLowerInvariant();
                    if (n.Contains("jumpscare") || n.Contains("scare")) return true;
                }
            }
            catch { }

            return false;
        }

        // ================================================================ view distance

        /// <summary>
        /// Stops distant geometry being clipped away.
        ///
        /// The far clip plane is per camera and the game sets it low enough that the forest and the
        /// far side of the lot vanish. Raising it is the whole fix, but it has to be done to every
        /// camera and re-done as new ones appear - the ghost feed, the spectator view and the EOD
        /// scene all bring their own. Each camera's own value is kept so switching off restores what
        /// it shipped with rather than a number chosen here.
        /// </summary>
        internal bool NoFarClip;

        /// <summary>How far to see, in metres. Depth precision gets worse the higher this goes.</summary>
        internal float FarClipDistance = 5000f;

        private readonly Dictionary<int, float> _farClips = new Dictionary<int, float>();
        private float _nextClipSweep;
        private bool _clipsRaised;

        private void ApplyFarClip(float now, bool raise)
        {
            if (!raise && !_clipsRaised) return;
            if (now < _nextClipSweep) return;
            _nextClipSweep = now + 2f;

            List<Camera> cams = Net.FindActive<Camera>();
            for (int i = 0; i < cams.Count; i++)
            {
                Camera cam = cams[i];
                if (!Net.Alive(cam)) continue;
                try
                {
                    int id = cam.GetInstanceID();
                    if (raise)
                    {
                        if (!_farClips.ContainsKey(id)) _farClips[id] = cam.farClipPlane;
                        float want = Mathf.Clamp(FarClipDistance, 100f, 20000f);
                        if (cam.farClipPlane < want) cam.farClipPlane = want;
                    }
                    else
                    {
                        float original;
                        if (_farClips.TryGetValue(id, out original)) cam.farClipPlane = original;
                    }
                }
                catch { }
            }

            if (!raise) _farClips.Clear();
            _clipsRaised = raise;
        }

        // ================================================================ tokens

        /// <summary>
        /// Picks the loose arcade tokens up off the floor.
        ///
        /// The coin prefab is called "Coin" and carries a PickupObject, which is an Interactable - so
        /// the pickup already exists and is the same call the game makes when you walk up and press
        /// the key. Going through Interact rather than deleting the coin and adding a token by hand is
        /// the same lesson the spawner and the trash cleaner both taught: the game's own path does the
        /// networking, the balance and the sound, and nothing has to be reimplemented or kept in sync.
        /// </summary>
        internal bool AutoHarvestTokens;

        internal int TokensHarvested;

        private float _nextCoinSweep;
        private readonly HashSet<int> _coinsTaken = new HashSet<int>();

        private void HarvestTokens(float now)
        {
            if (now < _nextCoinSweep) return;
            _nextCoinSweep = now + 1f;

            PlayerManager pm = Net.LocalPlayer;
            if (!Net.Alive(pm)) return;

            try
            {
                List<PickupObject> all = Net.FindActive<PickupObject>();
                int taken = 0;

                for (int i = 0; i < all.Count && taken < 8; i++)
                {
                    PickupObject p = all[i];
                    if (!Net.Alive(p)) continue;

                    try
                    {
                        if (!p.gameObject.activeInHierarchy) continue;
                        if (!p.gameObject.name.StartsWith("Coin", StringComparison.OrdinalIgnoreCase)) continue;

                        // A coin that refuses to go is not worth asking about twice a second.
                        int id = p.GetInstanceID();
                        if (!_coinsTaken.Add(id)) continue;

                        p.Interact(pm);
                        taken++;
                        TokensHarvested++;
                    }
                    catch (Exception ex) { Log.Debug("coin pickup: " + ex.Message); }
                }

                if (taken > 0) Log.Msg("Collected " + taken + " token(s) (" + TokensHarvested + " tonight).");
            }
            catch (Exception ex) { Log.Debug("harvest tokens: " + ex.Message); }
        }

        // ================================================================ vents

        private readonly List<VentTrigger> _vents = new List<VentTrigger>();
        private float _nextVentScan;
        private bool _ventsFreed;

        /// <summary>
        /// Switches off the animator that evicts you. Entering and leaving still work - the trigger
        /// keeps tracking playersInVent either way - the vent just stops throwing you out of it.
        /// </summary>
        private void ApplyVentKick(float now, bool free)
        {
            if (!free && !_ventsFreed) return;
            if (now < _nextVentScan) return;
            _nextVentScan = now + 2f;

            _vents.Clear();
            _vents.AddRange(Net.FindActive<VentTrigger>());
            for (int i = 0; i < _vents.Count; i++)
            {
                VentTrigger v = _vents[i];
                if (!Net.Alive(v)) continue;
                try
                {
                    Animator push = v.ventPushOutAnim;
                    if (push == null) continue;
                    if (push.enabled == free) push.enabled = !free;
                }
                catch { }
            }
            _ventsFreed = free;
        }

        // ================================================================ hints

        /// <summary>Blocks the HUD hint popups. Backed by the Harmony prefix on StoreManager.AddHint.</summary>
        internal bool AutoDismissHints
        {
            get { return HintBlock.Enabled; }
            set { HintBlock.Enabled = value; }
        }

        private float _nextHintSweep;

        /// <summary>Clears anything that was already queued before the block went on.</summary>
        private void DismissHints(float now)
        {
            if (!HintBlock.Enabled || now < _nextHintSweep) return;
            _nextHintSweep = now + 0.2f;
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (!Net.Alive(sm)) return;
                if (Hide(sm.hintCanv)) HintBlock.Blocked++;
            }
            catch (Exception ex) { Log.Debug("hints: " + ex.Message); }
        }

        private static bool Hide(GameObject go)
        {
            try
            {
                if (go == null || !go.activeSelf) return false;
                go.SetActive(false);
                return true;
            }
            catch { return false; }
        }

        // ================================================================ fullbright

        private bool _brightOn;
        private bool _brightCaptured;
        private AmbientMode _savedAmbientMode;
        private Color _savedAmbientLight;
        private float _savedAmbientIntensity;
        private bool _savedFog;
        private float _nextVolumeScan;

        internal bool Fullbright
        {
            get { return _brightOn; }
            set
            {
                if (value == _brightOn) return;
                _brightOn = value;
                if (value) ApplyFullbright();
                else RestoreLighting();
            }
        }

        /// <summary>
        /// Two things at once: a white ambient probe with the fog off, which lifts everything the
        /// lighting rig leaves black, and extra exposure through the scene's own ColorAdjustments -
        /// the same override the game's brightness slider writes - which lifts the baked geometry that
        /// ambient light cannot reach.
        /// </summary>
        private void ApplyFullbright()
        {
            try
            {
                if (!_brightCaptured)
                {
                    _savedAmbientMode = RenderSettings.ambientMode;
                    _savedAmbientLight = RenderSettings.ambientLight;
                    _savedAmbientIntensity = RenderSettings.ambientIntensity;
                    _savedFog = RenderSettings.fog;
                    _brightCaptured = true;
                }

                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = HasPreset ? PresetAmbient : new Color(0.92f, 0.92f, 0.95f, 1f);
                RenderSettings.ambientIntensity = HasPreset ? PresetIntensity : 1f;
                RenderSettings.fog = false;
            }
            catch (Exception ex) { Log.Debug("ambient: " + ex.Message); }

            ApplyExposure();
            LastResult = "Fullbright on (+" + BrightBoost.ToString("0.0") + " EV)";
            Log.Msg(LastResult + ".");
        }

        /// <summary>Re-asserts the values the game overwrites - it drives lighting per event and per day.</summary>
        private void HoldFullbright()
        {
            try
            {
                if (RenderSettings.fog) RenderSettings.fog = false;
                if (RenderSettings.ambientMode != AmbientMode.Flat) RenderSettings.ambientMode = AmbientMode.Flat;

                float wantIntensity = HasPreset ? PresetIntensity : 1f;
                if (Mathf.Abs(RenderSettings.ambientIntensity - wantIntensity) > 0.01f)
                    RenderSettings.ambientIntensity = wantIntensity;

                Color wantAmbient = HasPreset ? PresetAmbient : new Color(0.92f, 0.92f, 0.95f, 1f);
                if (RenderSettings.ambientLight != wantAmbient) RenderSettings.ambientLight = wantAmbient;
            }
            catch { }

            // Re-applied on a timer rather than only when the cached volume died. A volume that has
            // been outvoted by a zone is still perfectly alive, so the old condition never fired and
            // the boost stayed lost until a scene change. Zones also bring their volumes with them, so
            // this doubles as picking up ones that did not exist when fullbright was switched on.
            if (Time.unscaledTime >= _nextVolumeScan)
            {
                _nextVolumeScan = Time.unscaledTime + 0.5f;
                ApplyExposure();
            }
        }

        /// <summary>One volume's grading, and the exposure it had before we touched it.</summary>
        private sealed class Graded
        {
            internal ColorAdjustments Adjustments;
            internal float SavedExposure;
            internal bool SavedOverride;
        }

        private readonly Dictionary<int, Graded> _graded = new Dictionary<int, Graded>();

        /// <summary>
        /// Raises exposure on every grading volume in the scene, not just the winning global one.
        ///
        /// This used to pick the highest-priority volume with isGlobal set, boost that, and cache it.
        /// Zone lighting does not work that way: the store, the forecourt and the forest each have
        /// their own local volume that blends in when you walk into it, at a priority above the global
        /// one. Walk into a zone and its volume simply outvotes the boosted one - which is exactly the
        /// "off inside, on outside, gone in certain places" that made this look random.
        ///
        /// So boost all of them. Whichever one wins where you are standing, it has been raised too.
        /// Each volume's own exposure is remembered the first time it is touched so switching off puts
        /// every one of them back, and re-running this is cheap and idempotent - a volume already at
        /// its target is left alone.
        /// </summary>
        private GameObject _brightGo;
        private Volume _brightVolume;
        private ColorAdjustments _brightAdj;
        private bool _ownVolumeFailed;

        /// <summary>
        /// Our own global grading volume, outranking every volume in the scene.
        ///
        /// Boosting the game's own volumes fixed the outdoors and not the store, which says the store's
        /// volume has no ColorAdjustments to boost - there is nothing there to raise. Owning a volume
        /// sidesteps that entirely: at priority 9999 with the override set, it is the one URP resolves
        /// to wherever you are standing, indoors included, and no longer depends on what the scene
        /// happens to have authored.
        ///
        /// If any of this fails on the Il2Cpp side the flag latches and the old per-volume boost takes
        /// over, so the outdoors keeps working rather than nothing working.
        /// </summary>
        private bool EnsureBrightVolume()
        {
            if (_ownVolumeFailed) return false;
            if (Net.Alive(_brightVolume) && Net.Alive(_brightAdj)) return true;

            try
            {
                if (!Net.Alive(_brightGo))
                {
                    _brightGo = new GameObject("SAM_FullbrightVolume");
                    UnityEngine.Object.DontDestroyOnLoad(_brightGo);
                }

                if (!Net.Alive(_brightVolume))
                {
                    _brightVolume = _brightGo.GetComponent<Volume>();
                    if (!Net.Alive(_brightVolume)) _brightVolume = _brightGo.AddComponent<Volume>();
                }
                if (!Net.Alive(_brightVolume)) { _ownVolumeFailed = true; return false; }

                VolumeProfile profile = UnityEngine.ScriptableObject
                    .CreateInstance(Il2CppInterop.Runtime.Il2CppType.Of<VolumeProfile>()).TryCast<VolumeProfile>();
                ColorAdjustments ca = UnityEngine.ScriptableObject
                    .CreateInstance(Il2CppInterop.Runtime.Il2CppType.Of<ColorAdjustments>()).TryCast<ColorAdjustments>();
                if (profile == null || ca == null) { _ownVolumeFailed = true; return false; }

                ca.active = true;
                ca.postExposure.overrideState = true;
                ca.postExposure.value = HasPreset ? PresetExposure : BrightBoost;
                profile.components.Add(ca);

                _brightVolume.isGlobal = true;
                _brightVolume.priority = 9999f;
                _brightVolume.weight = 1f;
                _brightVolume.sharedProfile = profile;
                _brightAdj = ca;

                Log.Msg("Fullbright: own global volume in place, so the store is covered too.");
                return true;
            }
            catch (Exception ex)
            {
                // Said out loud, not at debug level. This latches after one failure, so it cannot
                // spam - and when it fired silently the only symptom was fullbright quietly not
                // working indoors, with nothing in the log to say why.
                Log.Warn("Fullbright could not create its own volume (" + ex.Message +
                         "); falling back to boosting the scene's own volumes.");
                _ownVolumeFailed = true;
                return false;
            }
        }

        // ---------------------------------------------------------------- captured look

        /// <summary>A lighting setup captured from the game rather than invented here.</summary>
        internal bool HasPreset;
        internal Color PresetAmbient = new Color(0.92f, 0.92f, 0.95f, 1f);
        internal float PresetIntensity = 1f;
        internal float PresetExposure = 2.5f;

        /// <summary>
        /// Takes the lighting exactly as it looks right now and makes it the fullbright setting.
        ///
        /// The built-in values were a guess at "bright" - a flat near-white probe at full intensity -
        /// and a guess is a poor thing to keep re-applying once you have found a look that actually
        /// works in this game. Capturing reads back what is live and stores it, so fullbright stops
        /// being an opinion and starts being the setup you already approved of.
        /// </summary>
        internal void CaptureCurrentLook()
        {
            try
            {
                PresetAmbient = RenderSettings.ambientLight;
                PresetIntensity = RenderSettings.ambientIntensity;

                float exposure = BrightBoost;
                try { if (Net.Alive(_brightAdj)) exposure = _brightAdj.postExposure.value; }
                catch { }
                PresetExposure = exposure;

                HasPreset = true;
                LastResult = "Captured this look for fullbright";
                Log.Msg(LastResult + ": ambient=" + PresetAmbient + " intensity=" +
                        PresetIntensity.ToString("0.00") + " exposure=" + PresetExposure.ToString("0.00") + " EV.");

                if (_brightOn) ApplyFullbright();
            }
            catch (Exception ex) { Log.Ex("capture look", ex); LastResult = "Capture failed"; }
        }

        internal void ClearPreset()
        {
            HasPreset = false;
            LastResult = "Fullbright back to its built-in look";
            Log.Msg(LastResult + ".");
            if (_brightOn) ApplyFullbright();
        }

        private void DestroyBrightVolume()
        {
            try
            {
                if (Net.Alive(_brightGo)) UnityEngine.Object.Destroy(_brightGo);
            }
            catch { }
            _brightGo = null;
            _brightVolume = null;
            _brightAdj = null;
        }

        private void ApplyExposure()
        {
            // Our own volume wins everywhere, so the scene's are handed back rather than left raised.
            if (EnsureBrightVolume())
            {
                try { _brightAdj.postExposure.value = HasPreset ? PresetExposure : BrightBoost; }
                catch { }
                if (_graded.Count > 0) RestoreExposure();
                return;
            }

            try
            {
                List<Volume> volumes = Net.FindActive<Volume>();
                for (int i = 0; i < volumes.Count; i++)
                {
                    Volume v = volumes[i];
                    if (!Net.Alive(v)) continue;
                    try
                    {
                        VolumeProfile profile = v.profileRef;
                        if (profile == null || profile.components == null) continue;

                        int count = profile.components.Count;
                        for (int c = 0; c < count; c++)
                        {
                            VolumeComponent comp = profile.components[c];
                            if (comp == null) continue;
                            ColorAdjustments ca = comp.TryCast<ColorAdjustments>();
                            if (ca == null) continue;

                            int id = ca.GetInstanceID();
                            Graded g;
                            if (!_graded.TryGetValue(id, out g))
                            {
                                g = new Graded
                                {
                                    Adjustments = ca,
                                    SavedExposure = ca.postExposure.value,
                                    SavedOverride = ca.postExposure.overrideState
                                };
                                _graded[id] = g;
                            }

                            ca.postExposure.overrideState = true;
                            float want = g.SavedExposure + BrightBoost;
                            if (Mathf.Abs(ca.postExposure.value - want) > 0.01f) ca.postExposure.value = want;
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex) { Log.Debug("exposure: " + ex.Message); }
        }

        /// <summary>Puts every volume this touched back to the exposure it shipped with.</summary>
        private void RestoreExposure()
        {
            foreach (KeyValuePair<int, Graded> kv in _graded)
            {
                Graded g = kv.Value;
                if (g == null || !Net.Alive(g.Adjustments)) continue;
                try
                {
                    g.Adjustments.postExposure.value = g.SavedExposure;
                    g.Adjustments.postExposure.overrideState = g.SavedOverride;
                }
                catch { }
            }
            _graded.Clear();
        }

        private void RestoreLighting()
        {
            try
            {
                if (_brightCaptured)
                {
                    RenderSettings.ambientMode = _savedAmbientMode;
                    RenderSettings.ambientLight = _savedAmbientLight;
                    RenderSettings.ambientIntensity = _savedAmbientIntensity;
                    RenderSettings.fog = _savedFog;
                    _brightCaptured = false;
                }
            }
            catch (Exception ex) { Log.Debug("restore ambient: " + ex.Message); }

            try
            {
                DestroyBrightVolume();
                RestoreExposure();
            }
            catch (Exception ex) { Log.Debug("restore exposure: " + ex.Message); }

            LastResult = "Fullbright off";
            Log.Msg(LastResult + ".");
        }

        /// <summary>Applied live so the stepper moves the picture while the menu is open.</summary>
        internal void OnBrightBoostChanged()
        {
            if (_brightOn) ApplyExposure();
        }

        internal void Shutdown()
        {
            if (_noFog) { _noFog = false; RestoreFog(); }
            if (_brightOn) { _brightOn = false; RestoreLighting(); }
        }
    }
}
