using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Collections.Generic;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite
{
    /// <summary>
    /// Night-generation multipliers.
    ///
    /// Customers and events are both picked once, when the night is set up, so the cheapest place to
    /// scale them is the moment the game hands back its chosen list. Tripling there means the rest
    /// of the game - pacing, quotas, dialogue - keeps working on a list it built itself.
    /// </summary>
    internal static class Multipliers
    {
        internal static bool CustomersEnabled;
        internal static bool EventsEnabled;
        internal static int CustomerFactor = 3;
        internal static int EventFactor = 3;

        internal static int LastCustomerBase;
        internal static int LastCustomerScaled;
        internal static int LastEventBase;
        internal static int LastEventScaled;
    }

    [HarmonyPatch(typeof(EndlessGenerationManager), nameof(EndlessGenerationManager.GenerateNight))]
    internal static class GenerateNightPatch
    {
        [HarmonyPostfix]
        private static void Postfix(ref List<Npc> __result)
        {
            if (!Multipliers.CustomersEnabled) return;
            int factor = Multipliers.CustomerFactor;
            if (factor <= 1 || __result == null) return;

            try
            {
                int baseCount = __result.Count;
                Multipliers.LastCustomerBase = baseCount;
                if (baseCount == 0) { Multipliers.LastCustomerScaled = 0; return; }

                // Append whole extra passes so the original order and mandatory NPCs stay intact.
                for (int pass = 1; pass < factor; pass++)
                    for (int i = 0; i < baseCount; i++)
                        __result.Add(__result[i]);

                Multipliers.LastCustomerScaled = __result.Count;
                Log.Msg("Night customers " + baseCount + " -> " + __result.Count + " (x" + factor + ").");
            }
            catch (Exception ex) { Log.Ex("customer multiplier", ex); }
        }
    }

    [HarmonyPatch(typeof(CurrentDayManager), nameof(CurrentDayManager.SetUpDay))]
    internal static class SetUpDayPatch
    {
        /// <summary>Size we last left listOfOccurrences at, so a repeated SetUpDay cannot compound.</summary>
        private static int _alreadyScaledTo = -1;

        internal static void Reset() { _alreadyScaledTo = -1; }

        [HarmonyPostfix]
        private static void Postfix(CurrentDayManager __instance)
        {
            if (!Multipliers.EventsEnabled) return;
            int factor = Multipliers.EventFactor;
            if (factor <= 1 || __instance == null) return;

            try
            {
                var occurrences = __instance.listOfOccurrences;
                if (occurrences == null) return;

                int baseCount = occurrences.Count;
                if (baseCount == 0) { Multipliers.LastEventScaled = 0; return; }

                // listOfOccurrences is a persistent field, and SetUpDay runs more than once per
                // night without refilling it. Scaling in place unguarded compounds (8 -> 24 -> 72),
                // so skip a list we already left at exactly this size.
                if (baseCount == _alreadyScaledTo)
                {
                    Log.Debug("Event multiplier skipped: list already scaled to " + baseCount + ".");
                    return;
                }

                // Deliberately no longer padded out.
                //
                // This used to append the same occurrence references again and again - four copies of
                // one object, sharing one object's state. The list did grow (9 -> 36, which is why the
                // setting looked applied) but nothing ever consumed it: TriggerNextEvent was never
                // called once across a whole night. Duplicating shared references to a queue nothing
                // reads is at best useless and at worst what jammed the chain, so the night's own list
                // is left exactly as the game built it and the multiplier drives TriggerNextEvent
                // directly instead - see WorldModule.PumpEvents.
                Multipliers.LastEventBase = baseCount;
                Multipliers.LastEventScaled = baseCount;
                _alreadyScaledTo = baseCount;
            }
            catch (Exception ex) { Log.Ex("event multiplier", ex); }
        }
    }

    /// <summary>
    /// Rake suppression. Every entry point that brings a rake into the world is short-circuited, so
    /// it does not matter which one the night scheduler picks.
    /// </summary>
    internal static class RakeBlock
    {
        internal static bool Enabled;
        internal static int Blocked;

        internal static bool Allow(string where)
        {
            if (!Enabled) return true;
            Blocked++;
            Log.Debug("Rake blocked at " + where + " (total " + Blocked + ").");
            return false;
        }
    }

    /// <summary>
    /// Charge for what is on the counter before the sale closes.
    ///
    /// The bell is gated on every item being scanned, so vanilla never has to think about this. With
    /// unbagged checkout allowed that gate is gone, and revenue is read at completion time - so the
    /// items have to be scanned in the prefix, before CompleteTransaction banks the total.
    /// </summary>
    [HarmonyPatch(typeof(TransactionManager), nameof(TransactionManager.Rpc_CMD_CompleteTransaction))]
    internal static class CompleteTransactionPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            try
            {
                SuiteMod mod = SuiteMod.Instance;
                if (mod != null) mod.Counter.CountRemainingBeforeCompletion();
            }
            catch (Exception ex) { Log.Debug("complete transaction prefix: " + ex.Message); }
        }
    }

    /// <summary>
    /// Modified weapons.
    ///
    /// The flamethrower does not fire projectiles at all - ShootFlamethrower only drives particles,
    /// audio and the ammo counter, and the damage comes from the flame particles colliding. So a
    /// grenade-firing version is built by riding that same call: every shot, on a cooldown, a live
    /// grenade is launched from the weapon's own muzzle transform through StoreManager's throw path,
    /// which is what spawns the armed "thrown" variant rather than the inert pickup.
    /// </summary>
    internal static class WeaponMods
    {
        internal static bool GrenadeFlamethrower;

        /// <summary>Seconds between shots. Zero fires on every single frame the trigger is held.</summary>
        internal static float FireInterval = 0f;

        /// <summary>Grenades per shot. One is a stream; more is a shotgun blast of them.</summary>
        internal static int BurstCount = 1;

        /// <summary>Muzzle velocity, in units per second.</summary>
        internal static float LaunchForce = 45f;

        internal static int Launched;

        private static float _nextShot;
        private static int _grenadeId = -1;
        private static int _flamerId = -1;

        /// <summary>
        /// Colliders of the grenades still in flight.
        ///
        /// An impact grenade goes off on its first contact, and nothing in the game calls
        /// PlayExplosion except Explosion.Spawned - so the blast is a separate prefab the projectile
        /// spawns when it touches something. Fired as a stream, the thing each grenade touches first
        /// is the one launched a frame earlier, which is why they detonated at the muzzle instead of
        /// downrange. Every new grenade is told to ignore the ones already out, and the player.
        /// </summary>
        private static readonly System.Collections.Generic.List<Collider> _inFlight =
            new System.Collections.Generic.List<Collider>();

        private const int TrackedInFlight = 24;

        internal static void Reset()
        {
            _nextShot = 0f;
            _grenadeId = -1;
            _flamerId = -1;
            _inFlight.Clear();
        }

        /// <summary>The impact grenade, looked up by name so a catalogue change does not break it.</summary>
        private static int GrenadeId()
        {
            if (_grenadeId >= 0) return _grenadeId;
            Items.Entry e = Items.FindByName("impact nade", "grenade", "nade");
            _grenadeId = e != null ? e.Id : 46;
            return _grenadeId;
        }

        /// <summary>
        /// Driven from the suite's own per-frame Update rather than from ShootFlamethrower, so the
        /// rate is entirely ours - the weapon just has to be the one in your hands.
        /// </summary>
        internal static void Tick(bool menuOpen)
        {
            // Trigger first, so that every click which does NOT produce a grenade leaves a reason in
            // the log. Checking the mode first meant the silent paths said nothing at all.
            bool held;
            try { held = Input.GetMouseButton(0); }
            catch { return; }
            if (!held) return;

            if (!GrenadeFlamethrower) { Say("grenade launcher mode is off"); return; }
            if (menuOpen) { Say("mod menu is open"); return; }

            InventoryManager inv = Net.LocalInventory;
            if (!Net.Alive(inv)) { Say("no local inventory"); return; }

            if (!CanFire(inv) || !HoldingFlamethrower(inv)) { ReportBlocked(inv); return; }

            SuppressFlame(inv);
            Fire(inv);
        }

        /// <summary>
        /// Kills the flame the weapon would otherwise produce - the first-person ParticleSystem on the
        /// inventory and the third-person one other players see.
        /// </summary>
        internal static void SuppressFlame(InventoryManager inv)
        {
            try
            {
                ParticleSystem ps = inv.flamethrowerFlame;
                if (Net.Alive(ps) && ps.isEmitting) { ps.Stop(); ps.Clear(); }
            }
            catch (Exception ex) { Log.Debug("stop flame: " + ex.Message); }

            try
            {
                ThirdPersonManager tpm = inv.thirdPersonMan;
                if (Net.Alive(tpm) && tpm.flamethrowerFlamePlaying) tpm.ChangeFlamethrowerFire(false);
            }
            catch (Exception ex) { Log.Debug("stop third-person flame: " + ex.Message); }
        }

        private static void Fire(InventoryManager inv)
        {
            // At zero there is no schedule to keep: every Update spawns a grenade, which is what makes
            // it a stream rather than a volley. Any float comparison here would only get in the way.
            if (FireInterval > 0f)
            {
                float now = Time.unscaledTime;
                if (now < _nextShot) return;
                _nextShot = now + FireInterval;
            }

            try
            {
                Transform muzzle = inv.flamethrowerShootPoint;
                if (!Net.Alive(muzzle)) muzzle = inv.playerShootPoint;
                if (!Net.Alive(muzzle)) muzzle = inv.playerCam;
                if (!Net.Alive(muzzle)) return;

                Vector3 forward = muzzle.forward;
                Transform cam = inv.playerCam;
                if (Net.Alive(cam)) forward = cam.forward;

                GameObject prefab = ThrownPrefab();
                if (prefab == null) return;

                int burst = Mathf.Clamp(BurstCount, 1, 10);
                int fired = 0;
                for (int i = 0; i < burst; i++)
                {
                    Vector3 dir = forward;
                    if (i > 0) dir = (forward + UnityEngine.Random.insideUnitSphere * 0.05f).normalized;
                    if (Launch(inv, prefab, muzzle.position + dir * 1.5f, dir)) fired++;
                }

                if (fired == 0) return;
                _lastBlockReason = "";
                bool first = Launched == 0;
                Launched += fired;
                if (first)
                    Log.Msg("Grenade launcher firing " + Items.NameOf(GrenadeId()) + " (item " +
                            GrenadeId() + ").");
            }
            catch (Exception ex) { Log.Debug("grenade launcher: " + ex.Message); }
        }

        /// <summary>The armed, in-flight version of the grenade - not the inert pickup.</summary>
        private static GameObject ThrownPrefab()
        {
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (!Net.Alive(sm) || sm.thrownObjs == null) return null;
                int id = GrenadeId();
                if (id < 0 || id >= sm.thrownObjs.Length) return null;
                return sm.thrownObjs[id];
            }
            catch (Exception ex) { Log.Debug("thrown prefab: " + ex.Message); return null; }
        }

        /// <summary>
        /// Spawns one grenade and throws it. Spawning it here rather than through
        /// StoreManager.ServerThrowObject is what makes the stream possible: that method keeps the
        /// instance to itself, so there is no way to reach the new grenade's collider and stop it
        /// detonating against the rest of the stream.
        /// </summary>
        private static bool Launch(InventoryManager inv, GameObject prefab, Vector3 at, Vector3 dir)
        {
            try
            {
                FusionNetworkManager fnm = FusionNetworkManager.Instance;
                if (!Net.Alive(fnm)) return false;

                Il2CppFusion.NetworkObject prefabNet = prefab.GetComponent<Il2CppFusion.NetworkObject>();
                if (prefabNet == null) return false;

                Quaternion rot = Quaternion.LookRotation(dir);
                Il2CppFusion.NetworkObject spawned = Net.IsHost
                    ? fnm.SpawnItem(prefabNet, at, rot, null)
                    : null;

                if (spawned == null)
                {
                    // Not the host: ask through the manager's own request, and accept that the
                    // collision cleanup below cannot be applied to something we never get back.
                    try { fnm.Rpc_RequestSpawnItem(prefabNet, at, rot, null); return true; }
                    catch (Exception ex) { Log.Debug("Rpc_RequestSpawnItem: " + ex.Message); return false; }
                }

                GameObject go = spawned.gameObject;
                Isolate(inv, go);

                try
                {
                    Rigidbody rb = go.GetComponent<Rigidbody>();
                    if (rb != null) rb.linearVelocity = dir * Mathf.Max(1f, LaunchForce);
                }
                catch (Exception ex) { Log.Debug("grenade velocity: " + ex.Message); }

                return true;
            }
            catch (Exception ex) { Log.Debug("launch grenade: " + ex.Message); return false; }
        }

        /// <summary>Stops a grenade colliding with the player or with the rest of the stream.</summary>
        private static void Isolate(InventoryManager inv, GameObject go)
        {
            Collider mine = null;
            try
            {
                mine = go.GetComponent<Collider>();
                if (mine == null) mine = go.GetComponentInChildren<Collider>(true);
            }
            catch { }
            if (mine == null) return;

            // The player, so it does not go off in your face on the way out.
            try
            {
                Collider pc = inv.playerCol;
                if (Net.Alive(pc)) Physics.IgnoreCollision(mine, pc, true);
            }
            catch (Exception ex) { Log.Debug("ignore player: " + ex.Message); }

            // Everything else still in the air.
            for (int i = _inFlight.Count - 1; i >= 0; i--)
            {
                Collider other = _inFlight[i];
                bool alive = false;
                try { alive = other != null && other.gameObject != null; }
                catch { }
                if (!alive) { _inFlight.RemoveAt(i); continue; }

                try { Physics.IgnoreCollision(mine, other, true); }
                catch { }
            }

            _inFlight.Add(mine);
            while (_inFlight.Count > TrackedInFlight) _inFlight.RemoveAt(0);
        }

        /// <summary>
        /// Whether the player is actually in a position to fire.
        ///
        /// Reading the mouse button directly means reading it everywhere - including while clicking
        /// through a customer's dialogue, the store computer or the pause menu, which is how a
        /// conversation turned into a grenade going off in someone's face. These are the game's own
        /// gates for "can this player use the thing in their hands", plus the cursor: it is locked
        /// during play and released for every menu and dialogue in the game.
        /// </summary>
        private static float _nextBlockLog;
        private static string _lastBlockReason = "";

        /// <summary>Throttled reason, so holding the button cannot flood the log.</summary>
        private static void Say(string why)
        {
            float now = Time.unscaledTime;
            if (now < _nextBlockLog) return;
            _nextBlockLog = now + 2f;
            if (why != _lastBlockReason)
            {
                _lastBlockReason = why;
                Log.Msg("LAUNCHER BLOCKED: " + why + ".");
            }
        }

        /// <summary>Trigger held but nothing came out - say which gate said no.</summary>
        private static void ReportBlocked(InventoryManager inv)
        {
            float now = Time.unscaledTime;
            if (now < _nextBlockLog) return;
            _nextBlockLog = now + 2f;

            // Resolve it here too, or the report shows -1 simply because nothing looked it up yet.
            try
            {
                if (_flamerId < 0)
                {
                    Items.Entry fe = Items.FindByName("flamethrower");
                    _flamerId = fe != null ? fe.Id : -2;
                }
            }
            catch { }

            string why = "";
            try
            {
                why += " canControlItem=" + inv.canControlItem + " inventoryPaused=" + inv.inventoryPaused +
                       " tasking=" + inv.tasking + " canShoot=" + inv.canShoot;
            }
            catch (Exception ex) { why += " inv? " + ex.Message; }

            try { why += " cursor=" + Cursor.lockState; } catch { }

            try
            {
                PlayerManager pm = Net.LocalPlayer;
                if (Net.Alive(pm))
                    {
                    why += " paused=" + pm.paused + " dead=" + pm.dead + " downed=" + pm._downed +
                           " computer=" + pm.lookingAtComputer + " shelf=" + pm.lookingAtShelf;
                    DialogueInteractable d = pm.curNpcScript;
                    if (Net.Alive(d))
                        why += " npcAlive=true interacting=" + d.interacting + " questioning=" + d.inQuestioningMenu;
                    else why += " npcAlive=false";
                }
            }
            catch (Exception ex) { why += " pm? " + ex.Message; }

            try
            {
                var ids = inv.inventoryIds;
                int slot = inv.curInventorySlot;
                why += " slot=" + slot + " id=" + (ids != null && slot >= 0 && slot < ids.Length ? ids[slot] : -99) +
                       " flamerId=" + _flamerId;
            }
            catch { }

            if (why != _lastBlockReason)
            {
                _lastBlockReason = why;
                Log.Msg("LAUNCHER BLOCKED:" + why);
            }
        }

        private static bool CanFire(InventoryManager inv)
        {
            try
            {
                if (!inv.canControlItem || inv.inventoryPaused || inv.tasking) return false;
            }
            catch { return false; }

            // The cursor is the honest signal for "is the player actually playing".
            //
            // Measured in this game: locked during normal play, released for dialogue, the store
            // computer, the pause menu and the mod menu alike. It is deliberately used instead of
            // curNpcScript, which is NOT a "talking right now" flag - the game points it at whoever
            // you last spoke to and never clears it, so gating on that left the launcher dead for the
            // rest of the night after a single conversation.
            try
            {
                if (Cursor.lockState != CursorLockMode.Locked) return false;
            }
            catch { }

            try
            {
                PlayerManager pm = Net.LocalPlayer;
                if (Net.Alive(pm))
                {
                    if (pm.paused || pm.dead || pm._downed) return false;
                    if (pm.lookingAtComputer || pm.lookingAtShelf) return false;
                }
            }
            catch { }

            return true;
        }

        /// <summary>The equipped slot has to actually hold the flamethrower.</summary>
        private static bool HoldingFlamethrower(InventoryManager inv)
        {
            try
            {
                if (_flamerId < 0)
                {
                    Items.Entry e = Items.FindByName("flamethrower");
                    _flamerId = e != null ? e.Id : -2;
                }
                if (_flamerId < 0) return false;

                var ids = inv.inventoryIds;
                int slot = inv.curInventorySlot;
                if (ids == null || slot < 0 || slot >= ids.Length) return false;
                return ids[slot] == _flamerId;
            }
            catch { return false; }
        }
    }

    [HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.ShootFlamethrower))]
    internal static class FlamethrowerGrenadePatch
    {
        /// <summary>
        /// Skipping the original is what turns the flamethrower into a grenade launcher rather than a
        /// flamethrower that also drops grenades: the body is the only thing that spins the flame
        /// particles up, ramps the fire loop and drains the fuel, so with it gone there is no fire,
        /// no roar and no running out - just the stream of grenades this launches instead.
        /// </summary>
        [HarmonyPrefix]
        private static bool Prefix(InventoryManager __instance)
        {
            try
            {
                if (!WeaponMods.GrenadeFlamethrower || !Net.Alive(__instance)) return true;
                if (!ReferenceEquals(__instance, Net.LocalInventory)) return true;

                // Firing is driven from the suite's own Update now; this only kills the flame.
                WeaponMods.SuppressFlame(__instance);
                return false;
            }
            catch (Exception ex)
            {
                Log.Debug("flamethrower prefix: " + ex.Message);
                return true;
            }
        }
    }

    /// <summary>
    /// Keeps people alive when the shooting gets enthusiastic.
    ///
    /// Everything damageable in this game is a Hittable, and the same component sits on customers and
    /// on the things hunting you - so the two are told apart by what else is attached: a person has a
    /// browse, nuisance or dialogue script, while a monster has an Enemy (and the hunt's own creature
    /// is additionally flagged isEntity). Damage aimed at a person is dropped outright, which also
    /// spares you the penalty for killing a customer.
    ///
    /// Monsters, the entity, and anything that is not a person are left exactly as they were.
    /// </summary>
    /// <summary>
    /// Who can be hurt, and how much.
    ///
    /// Three kinds of target, three behaviours:
    ///
    ///  - Bystander (browsing customers, scripted shoppers, anyone you talk to): completely untouched.
    ///    Damage is refused outright, so they never flinch, never bleed and never wander off - shooting
    ///    the shop up simply does not involve them.
    ///  - Troublemaker (nuisance customers): wounded but never killed. They take the hit and react, the
    ///    health floor keeps them alive, and the blow that would have finished them sends them running
    ///    out of the store instead.
    ///  - Everything else - monsters, bosses, the entity, and doppelgangers wearing a customer's
    ///    scripts - dies exactly as the game intends. Nothing here touches them.
    ///
    /// The kind is worked out from the components actually present rather than from Hittable's own
    /// script references: those are only filled in when the prefab wired them, and relying on them let
    /// unwired nuisances through to be blown up.
    /// </summary>
    internal static class HumanShield
    {
        internal static bool Enabled = true;

        /// <summary>
        /// Whether a wounded troublemaker runs for the door.
        ///
        /// Off by default. It was on, and it is why nuisances kept "disappearing" - they were not
        /// dying at all, they were surviving the hit and then being sent out of the store by this,
        /// which looks exactly the same from where you are standing. Wounded now means wounded: they
        /// take it and stay.
        /// </summary>
        internal static bool WoundAndFlee;

        /// <summary>Health a troublemaker is never allowed to fall below.</summary>
        internal static float Floor = 1f;

        internal static int Blocked;

        internal enum Kind
        {
            /// <summary>Monsters, bosses, the entity, doppelgangers - fair game.</summary>
            Fair,
            /// <summary>An ordinary person: never touched at all.</summary>
            Bystander,
            /// <summary>A nuisance: hurt, but never killed.</summary>
            Troublemaker
        }

        internal static Kind Classify(Hittable h)
        {
            if (h == null) return Kind.Fair;

            try
            {
                if (h.isEntity) return Kind.Fair;
                if (h.enemy != null) return Kind.Fair;
            }
            catch { return Kind.Fair; }

            // A doppelganger wears a person's scripts but is the thing you are meant to shoot.
            if (IsDoppelganger(h)) return Kind.Fair;

            // So is a robber. StoreBrowseBehaviour.isThief marks them, and they are the one kind of
            // person the shield deliberately does not cover.
            if (IsThief(h)) return Kind.Fair;

            // Nuisances are settled before hostility is considered. They would trip several of the
            // hostile tests below - a nuisance is not punished for being killed - and making them
            // Fair would quietly undo "nuisances can only be wounded".
            try { if (h.nuisanceNPCScript != null) return Kind.Troublemaker; } catch { }
            try { if (h.GetComponentInParent<NuisanceCustomer>() != null) return Kind.Troublemaker; } catch { }
            try { if (h.GetComponentInChildren<NuisanceCustomer>(true) != null) return Kind.Troublemaker; } catch { }

            // Everything hostile that still wears a customer's scripts: Norbert, the antler man, and
            // the rest of the scripted people the game expects you to fight. Without this they match
            // the browse/dialogue tests below and get shielded along with the actual shoppers.
            if (IsHostile(h)) return Kind.Fair;

            try { if (h.browseScript != null) return Kind.Bystander; } catch { }
            try { if (h.browseNPCScript != null) return Kind.Bystander; } catch { }
            try { if (h.dialogueScript != null) return Kind.Bystander; } catch { }
            try { if (h.GetComponentInParent<RegularBrowsingNPC>() != null) return Kind.Bystander; } catch { }
            try { if (h.GetComponentInParent<StoreBrowseBehaviour>() != null) return Kind.Bystander; } catch { }
            try { if (h.GetComponentInParent<DialogueInteractable>() != null) return Kind.Bystander; } catch { }
            try { if (h.GetComponentInChildren<RegularBrowsingNPC>(true) != null) return Kind.Bystander; } catch { }
            try { if (h.GetComponentInChildren<StoreBrowseBehaviour>(true) != null) return Kind.Bystander; } catch { }
            try { if (h.GetComponentInChildren<DialogueInteractable>(true) != null) return Kind.Bystander; } catch { }

            // There used to be a GetComponentInParent<Npc>/InChildren pair here, meant to catch the
            // scripted roster people. It never matched anything: Npc is a ScriptableObject - the
            // roster asset - not a component, so the lookup can only ever return null. Every
            // UNSHIELDED DEATH line in the log agrees, ordinary customers included: pNpc=False. The
            // browse and dialogue tests above are what actually cover those people.

            return Kind.Fair;
        }

        /// <summary>
        /// Names, lower-case, of people the shield must never cover. Comma-separated and editable in
        /// MelonPreferences as HostileNames, so a character this misses can be added without a build.
        /// Matched as a substring of the object name, which is why "thief" catches "Thief 2(Clone)".
        /// </summary>
        internal static string HostileNames = DefaultHostileNames;

        internal const string DefaultHostileNames = "norbert,thief,robber,antler,reindeer,mugger,attacker,intruder";

        private static string _namesParsedFrom;
        private static string[] _names = new string[0];

        private static string[] Names()
        {
            string src = HostileNames ?? "";
            if (!ReferenceEquals(src, _namesParsedFrom) && src != _namesParsedFrom)
            {
                _namesParsedFrom = src;
                var kept = new System.Collections.Generic.List<string>();
                string[] parts = src.Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    string p = parts[i].Trim().ToLowerInvariant();
                    if (p.Length > 0) kept.Add(p);
                }
                _names = kept.ToArray();
            }
            return _names;
        }

        /// <summary>
        /// Someone the game itself treats as a target rather than a customer.
        ///
        /// Preferring the game's own flags over a name list is the point: Hittable.dontPunishForKilling
        /// and hasBounty are how the game marks "you are meant to kill this", and
        /// StoreBrowseBehaviour.damageToPlayer is only non-zero on people who can hurt you. The name
        /// list is the backstop for characters that carry none of those - it is checked last and it is
        /// editable, so a miss is a config change rather than a rebuild.
        /// </summary>
        private static bool IsHostile(Hittable h)
        {
            try { if (h.dontPunishForKilling) return true; } catch { }
            try { if (h.hasBounty) return true; } catch { }
            try { if (h.hittingCausesFinalSequence) return true; } catch { }

            StoreBrowseBehaviour b = Browse(h);
            if (b != null)
            {
                try { if (b.getAchievementForKilling) return true; } catch { }
                try { if (b.damageToPlayer > 0f) return true; } catch { }
                try { if (b.attacking) return true; } catch { }
                try { if (b.chasingPlayer) return true; } catch { }
                try { if (b.hasStolenItems || b.doesntGiveBackItems) return true; } catch { }
            }

            string[] names = Names();
            if (names.Length == 0) return false;

            string n = null;
            try { n = h.gameObject.name; } catch { }
            if (string.IsNullOrEmpty(n)) return false;
            n = n.ToLowerInvariant();
            for (int i = 0; i < names.Length; i++)
                if (n.Contains(names[i])) return true;

            return false;
        }

        /// <summary>The browse behaviour for this hittable, wherever the prefab hung it.</summary>
        private static StoreBrowseBehaviour Browse(Hittable h)
        {
            StoreBrowseBehaviour b = null;
            try { b = h.browseScript; } catch { }
            if (b == null) { try { b = h.GetComponentInParent<StoreBrowseBehaviour>(); } catch { } }
            if (b == null) { try { b = h.GetComponentInChildren<StoreBrowseBehaviour>(true); } catch { } }
            return b;
        }

        /// <summary>True when this hittable is a person of any kind - used by the health sweep.</summary>
        internal static bool IsPerson(Hittable h)
        {
            Kind k = Classify(h);
            return k == Kind.Bystander || k == Kind.Troublemaker;
        }

        /// <summary>Should a damage call be allowed to run at all? Only bystanders refuse it.</summary>
        internal static bool AllowDamage(Hittable h)
        {
            if (!Enabled) return true;
            if (Classify(h) != Kind.Bystander) return true;
            Blocked++;
            ReportShielded(h);
            return false;
        }

        /// <summary>
        /// The mirror of <see cref="ReportUnshielded"/>: the first time each distinct name is
        /// protected, say so. Norbert, the robbers and the antler man were all invisible bugs
        /// precisely because a shielded thing makes no noise - it just refuses to die. One line per
        /// name per session is enough to tell a customer apart from something that should be a target.
        /// </summary>
        internal static void ReportShielded(Hittable h)
        {
            if (h == null) return;
            string name = "?";
            try { name = h.gameObject.name; } catch { }
            if (!_noted.Add("shielded:" + name)) return;
            Log.Msg("SHIELDED: " + name + " is being protected as a bystander. If it should be " +
                    "killable, add part of that name to HostileNames in MelonPreferences.cfg.");
        }

        /// <summary>Should a death be allowed? Neither kind of person may die.</summary>
        internal static bool AllowDeath(Hittable h)
        {
            if (!Enabled) return true;
            return Classify(h) == Kind.Fair;
        }

        /// <summary>Keeps a troublemaker off the floor after they have been hurt.</summary>
        internal static void Settle(Hittable h)
        {
            if (!Enabled) return;
            try
            {
                if (Classify(h) != Kind.Troublemaker) return;
                if (h.health < Floor) { h.health = Floor; Blocked++; }
            }
            catch { }
        }

        /// <summary>A robber: fair game, however human they look.</summary>
        private static bool IsThief(Hittable h)
        {
            StoreBrowseBehaviour b = Browse(h);
            if (b == null) return false;
            try { return b.isThief; }
            catch { return false; }
        }

        private static bool IsDoppelganger(Hittable h)
        {
            try
            {
                StoreBrowseBehaviour b = h.browseScript;
                if (b != null && b.isDoppelganger) return true;
            }
            catch { }

            Transform t;
            try { t = h.transform; }
            catch { return false; }

            for (int depth = 0; depth < 8 && t != null; depth++)
            {
                try
                {
                    GameObject go = t.gameObject;
                    if (go != null)
                    {
                        StoreBrowseBehaviour b = go.GetComponent<StoreBrowseBehaviour>();
                        if (b != null) return b.isDoppelganger;

                        Npc n = go.GetComponent<Npc>();
                        if (n != null) return n.isDoppelganger;
                    }
                }
                catch { }
                try { t = t.parent; } catch { break; }
            }
            return false;
        }

        private static readonly System.Collections.Generic.HashSet<string> _noted =
            new System.Collections.Generic.HashSet<string>();

        internal static void NoteSaved(string what)
        {
            if (_noted.Add(what)) Log.Msg("Human shield: " + what + ".");
        }

        /// <summary>
        /// Something died that the shield did not class as a person. Print what it actually is, once
        /// per name, so the gap can be closed instead of guessed at.
        /// </summary>
        internal static void ReportUnshielded(Hittable h)
        {
            if (!Enabled || h == null) return;
            string name = "?";
            try { name = h.gameObject.name; } catch { }
            if (!_noted.Add("died:" + name)) return;

            string what = "";
            try { what += " isEntity=" + h.isEntity + " enemy=" + (h.enemy != null); } catch { }
            try { what += " browse=" + (h.browseScript != null) + " browseNPC=" + (h.browseNPCScript != null); } catch { }
            try { what += " nuisance=" + (h.nuisanceNPCScript != null) + " dialogue=" + (h.dialogueScript != null); } catch { }
            try { what += " | pNuisance=" + (h.GetComponentInParent<NuisanceCustomer>() != null); } catch { }
            try { what += " pRegular=" + (h.GetComponentInParent<RegularBrowsingNPC>() != null); } catch { }
            try { what += " pBrowse=" + (h.GetComponentInParent<StoreBrowseBehaviour>() != null); } catch { }
            try { what += " pDialogue=" + (h.GetComponentInParent<DialogueInteractable>() != null); } catch { }
            try { what += " pNpc=" + (h.GetComponentInParent<Npc>() != null); } catch { }
            try { what += " doppel=" + IsDoppelganger(h); } catch { }

            string path = "";
            try
            {
                Transform t = h.transform;
                for (int i = 0; i < 4 && t != null; i++) { path += "/" + t.name; t = t.parent; }
            }
            catch { }

            Log.Msg("UNSHIELDED DEATH: " + name + what + " | path" + path);
        }

        /// <summary>
        /// What a troublemaker does instead of dying: stops whatever they were ruining and runs for
        /// the door. Bystanders never get here - nothing is allowed to hurt them in the first place.
        /// </summary>
        internal static void Wound(Hittable h)
        {
            if (!WoundAndFlee || h == null) return;
            try
            {
                NuisanceCustomer n = h.nuisanceNPCScript;
                if (n == null) n = h.GetComponentInParent<NuisanceCustomer>();
                if (n == null) n = h.GetComponentInChildren<NuisanceCustomer>(true);
                if (n == null) return;

                try { n.Rpc_EndNuisance(); } catch { }
                n.RunOutOfStore();
            }
            catch (Exception ex) { Log.Debug("wound: " + ex.Message); }
        }
    }

    // ---- damage: refused for bystanders, floored for troublemakers -------------------------------

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Hit))]
    internal static class HittableHitPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance) { return HumanShield.AllowDamage(__instance); }

        [HarmonyPostfix]
        private static void Postfix(Hittable __instance) { HumanShield.Settle(__instance); }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Rpc_Hit))]
    internal static class HittableRpcHitPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance) { return HumanShield.AllowDamage(__instance); }

        [HarmonyPostfix]
        private static void Postfix(Hittable __instance) { HumanShield.Settle(__instance); }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Explosion))]
    internal static class HittableExplosionPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance) { return HumanShield.AllowDamage(__instance); }

        [HarmonyPostfix]
        private static void Postfix(Hittable __instance) { HumanShield.Settle(__instance); }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.ChangeHealth))]
    internal static class HittableChangeHealthPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance) { return HumanShield.AllowDamage(__instance); }

        [HarmonyPostfix]
        private static void Postfix(Hittable __instance) { HumanShield.Settle(__instance); }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Rpc_ChangeHealth))]
    internal static class HittableRpcChangeHealthPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance) { return HumanShield.AllowDamage(__instance); }

        [HarmonyPostfix]
        private static void Postfix(Hittable __instance) { HumanShield.Settle(__instance); }
    }

    /// <summary>
    /// The rest of the ways to hurt someone.
    ///
    /// Damage does not only arrive through Hit: Rpc_CMD_Hit is the networked command a gun or a melee
    /// swing actually sends, and fire and stun are separate systems that tick health down inside
    /// FixedUpdateNetwork without ever calling Hit at all - which is how a flamethrower kept killing
    /// people the shield thought it was protecting.
    /// </summary>
    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Rpc_CMD_Hit))]
    internal static class HittableRpcCmdHitPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance) { return HumanShield.AllowDamage(__instance); }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Rpc_FireObj))]
    internal static class HittableFirePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance, bool on)
        {
            // Putting someone out is always allowed; setting them alight is not.
            if (!on) return true;
            return HumanShield.AllowDamage(__instance);
        }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Rpc_StunObj))]
    internal static class HittableStunPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance, bool on)
        {
            if (!on) return true;
            return HumanShield.AllowDamage(__instance);
        }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.DealStunDamage))]
    internal static class HittableStunDamagePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance) { return HumanShield.AllowDamage(__instance); }
    }

    // ---- death: refused for everyone the shield covers ------------------------------------------
    //
    // Rpc_Die is where the kill actually happens - the "Browsing Customer Killed" and
    // "Troublemaker Killed" alerts are raised there - and an explosion reaches it without ever
    // calling Die(), so all three entry points are guarded.

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Die))]
    internal static class HittableDiePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance)
        {
            if (HumanShield.AllowDeath(__instance)) return true;
            HumanShield.NoteSaved("blocked a death (Die)");
            HumanShield.Wound(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Rpc_Die))]
    internal static class HittableRpcDiePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance)
        {
            if (HumanShield.AllowDeath(__instance))
            {
                HumanShield.ReportUnshielded(__instance);
                return true;
            }
            HumanShield.NoteSaved("blocked a death (Rpc_Die)");
            HumanShield.Wound(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.Rpc_CMD_Die))]
    internal static class HittableRpcCmdDiePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance)
        {
            if (HumanShield.AllowDeath(__instance)) return true;
            HumanShield.NoteSaved("blocked a death (Rpc_CMD_Die)");
            return false;
        }
    }

    // ---- the NPCs' own Die(), which the death event reaches directly -----------------------------

    [HarmonyPatch(typeof(NuisanceCustomer), nameof(NuisanceCustomer.Die))]
    internal static class NuisanceDiePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(NuisanceCustomer __instance)
        {
            if (!HumanShield.Enabled) return true;
            HumanShield.Blocked++;
            HumanShield.NoteSaved("kept a troublemaker alive");
            try { HumanShield.Wound(__instance.hittable); } catch { }
            return false;
        }
    }

    [HarmonyPatch(typeof(StoreBrowseBehaviour), nameof(StoreBrowseBehaviour.Die))]
    internal static class BrowserDiePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(StoreBrowseBehaviour __instance)
        {
            if (!HumanShield.Enabled) return true;
            try { if (__instance.isDoppelganger || __instance.isThief) return true; }
            catch { return true; }
            HumanShield.Blocked++;
            HumanShield.NoteSaved("kept a customer alive");
            return false;
        }
    }

    [HarmonyPatch(typeof(RegularBrowsingNPC), nameof(RegularBrowsingNPC.Die))]
    internal static class RegularBrowserDiePatch
    {
        [HarmonyPrefix]
        private static bool Prefix()
        {
            if (!HumanShield.Enabled) return true;
            HumanShield.Blocked++;
            HumanShield.NoteSaved("kept a shopper alive");
            return false;
        }
    }

    /// <summary>
    /// Where the vanishing actually comes from.
    ///
    /// Reading the disassembly only got so far: Explosion.PlayExplosion never touches Hittable, and
    /// blocking every Die entry point still left people shrinking away - so the removal is reached by
    /// some path the static reading did not show. These log the managed stack the first time each
    /// fires, which names the caller outright instead of another guess, and block it for people.
    /// </summary>
    internal static class VanishTrace
    {
        private static readonly System.Collections.Generic.HashSet<string> _seen =
            new System.Collections.Generic.HashSet<string>();

        internal static void Report(string where, Hittable h)
        {
            if (!_seen.Add(where)) return;
            string who = "?";
            try { who = h == null ? "null" : h.gameObject.name; } catch { }
            string kind = "?";
            try { kind = HumanShield.Classify(h).ToString(); } catch { }
            Log.Msg("VANISH [" + where + "] on " + who + " (" + kind + ")");
            Log.Msg(Environment.StackTrace);
        }
    }

    /// <summary>The fade-out coroutine: this is the shrinking away.</summary>
    [HarmonyPatch(typeof(Hittable), nameof(Hittable.LerpAlphaClipping))]
    internal static class HittableFadePatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance)
        {
            try
            {
                VanishTrace.Report("LerpAlphaClipping", __instance);
                if (HumanShield.AllowDeath(__instance)) return true;
                HumanShield.NoteSaved("stopped a fade-out");
                return false;
            }
            catch { return true; }
        }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.SpawnFlesh))]
    internal static class HittableFleshPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance)
        {
            try
            {
                VanishTrace.Report("SpawnFlesh", __instance);
                if (HumanShield.AllowDeath(__instance)) return true;
                HumanShield.NoteSaved("stopped a gib");
                return false;
            }
            catch { return true; }
        }
    }

    [HarmonyPatch(typeof(Hittable), nameof(Hittable.NetworkDestroySelf))]
    internal static class HittableDestroySelfPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Hittable __instance)
        {
            try
            {
                VanishTrace.Report("NetworkDestroySelf", __instance);
                if (HumanShield.AllowDeath(__instance)) return true;
                HumanShield.NoteSaved("stopped a despawn");
                return false;
            }
            catch { return true; }
        }
    }

    [HarmonyPatch(typeof(CurrentDayManager), nameof(CurrentDayManager.SpawnRake))]
    internal static class SpawnRakePatch
    {
        [HarmonyPrefix]
        private static bool Prefix() { return RakeBlock.Allow("CurrentDayManager.SpawnRake"); }
    }

    [HarmonyPatch(typeof(CurrentDayManager), nameof(CurrentDayManager.Rpc_SpawnRake))]
    internal static class RpcSpawnRakePatch
    {
        [HarmonyPrefix]
        private static bool Prefix() { return RakeBlock.Allow("CurrentDayManager.Rpc_SpawnRake"); }
    }

    [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.SpawnRakeFromPos))]
    internal static class SpawnRakeFromPosPatch
    {
        [HarmonyPrefix]
        private static bool Prefix() { return RakeBlock.Allow("StoreManager.SpawnRakeFromPos"); }
    }

    [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.Rpc_EnableForestRakeRpc))]
    internal static class EnableForestRakePatch
    {
        [HarmonyPrefix]
        private static bool Prefix() { return RakeBlock.Allow("StoreManager.Rpc_EnableForestRakeRpc"); }
    }

    [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.InitializeForestRake))]
    internal static class InitForestRakePatch
    {
        [HarmonyPrefix]
        private static bool Prefix() { return RakeBlock.Allow("StoreManager.InitializeForestRake"); }
    }

    /// <summary>
    /// Reviews. Rewrites each review as it is created so customers always leave five stars, and
    /// zeroes the hygiene/stock penalties that drag the overall rating down.
    /// </summary>
    internal static class PerfectReviews
    {
        internal static bool Enabled;
        internal static int Rewritten;
    }

    [HarmonyPatch(typeof(ReviewsManager), nameof(ReviewsManager.Rpc_SpawnReview))]
    internal static class SpawnReviewPatch
    {
        [HarmonyPrefix]
        private static void Prefix(ref int stars_, ref bool perfectReview)
        {
            if (!PerfectReviews.Enabled) return;
            stars_ = 5;
            perfectReview = true;
            PerfectReviews.Rewritten++;
        }
    }

    [HarmonyPatch(typeof(ReviewsManager), nameof(ReviewsManager.CreateReview))]
    internal static class CreateReviewPatch
    {
        [HarmonyPrefix]
        private static void Prefix(ref bool perfectReview)
        {
            if (!PerfectReviews.Enabled) return;
            perfectReview = true;
        }
    }

    /// <summary>
    /// Live customer registry.
    ///
    /// Every scripted customer goes through StoreBrowseBehaviour.Spawned when it enters the world.
    /// Recording it there means the doppelganger radar never has to sweep the scene - which was the
    /// last periodic FindObjectsOfType left in the profile. Dead entries are pruned on read.
    /// </summary>
    internal static class NpcRegistry
    {
        private static readonly System.Collections.Generic.List<StoreBrowseBehaviour> _live =
            new System.Collections.Generic.List<StoreBrowseBehaviour>();

        internal static void Add(StoreBrowseBehaviour b)
        {
            if (b == null) return;
            for (int i = 0; i < _live.Count; i++)
                if (ReferenceEquals(_live[i], b) || (_live[i] != null && _live[i].Pointer == b.Pointer)) return;
            _live.Add(b);
        }

        internal static void Clear() { _live.Clear(); }

        /// <summary>Live entries only; anything destroyed since is dropped as a side effect.</summary>
        internal static System.Collections.Generic.List<StoreBrowseBehaviour> Live()
        {
            for (int i = _live.Count - 1; i >= 0; i--)
            {
                bool alive;
                try { alive = _live[i] != null && _live[i].gameObject != null; }
                catch { alive = false; }
                if (!alive) _live.RemoveAt(i);
            }
            return _live;
        }

        internal static int Count { get { return _live.Count; } }
    }

    [HarmonyPatch(typeof(StoreBrowseBehaviour), nameof(StoreBrowseBehaviour.Spawned))]
    internal static class BrowserSpawnedPatch
    {
        [HarmonyPostfix]
        private static void Postfix(StoreBrowseBehaviour __instance)
        {
            try { NpcRegistry.Add(__instance); } catch { }
        }
    }

    /// <summary>
    /// Skips the forced text reveal.
    ///
    /// Dialogue, chat logs, the shrine and cutscenes print through RevealText, which types the
    /// string out one character at a time. Refresh() already takes an "immediate" flag that fills
    /// the text in at once, so this forces it on rather than reimplementing anything.
    ///
    /// Known hazard, which is why it is off by default: Refresh(immediate: true) returns straight
    /// after SetText and never assigns revealTextRoutine, so anything that infers "still typing"
    /// from that field sees a different picture than normal. An earlier version of this also swept
    /// the scene calling Refresh(true) on every RevealText whose routine was non-null - which, given
    /// the above, was every one of them, forever - and that repeated SetText broke customer
    /// transaction dialogue. The sweep is gone; only the flag flip remains.
    /// </summary>
    internal static class SkipReading
    {
        internal static bool Enabled;
        internal static int Skipped;
    }

    [HarmonyPatch(typeof(RevealText), nameof(RevealText.Refresh))]
    internal static class RevealTextRefreshPatch
    {
        [HarmonyPrefix]
        private static void Prefix(ref bool immediate)
        {
            if (!SkipReading.Enabled || immediate) return;
            immediate = true;
            SkipReading.Skipped++;
        }
    }

    /// <summary>
    /// Hunt control.
    ///
    /// Two separate things, because "no entity hunt" can mean either:
    ///  - NoEntities: the hunt still runs (timer, arsenal, end-of-hunt payout) but no monsters are
    ///    spawned into it. This is the "hunt without fighting anything" option.
    ///  - NoHunt: hunts never start at all.
    /// </summary>
    internal static class HuntBlock
    {
        internal static bool NoEntities;
        internal static bool NoHunt;
        internal static int BlockedSpawns;
        internal static int BlockedHunts;
    }

    /// <summary>
    /// Hunt tracing. Says whether the game even tried to start a hunt and spawn into it, so a night
    /// where nothing turns up can be told apart from a night where the spawn was refused.
    /// </summary>
    [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.StartHunt))]
    internal static class StoreStartHuntTrace
    {
        [HarmonyPostfix]
        private static void Postfix()
        {
            try { Log.Msg("HUNT: StoreManager.StartHunt ran."); } catch { }
        }
    }

    [HarmonyPatch(typeof(HuntManager), nameof(HuntManager.StartHunt))]
    internal static class HuntStartTrace
    {
        [HarmonyPostfix]
        private static void Postfix(HuntManager __instance)
        {
            try
            {
                Log.Msg("HUNT: HuntManager.StartHunt ran - huntInProgress=" + __instance.huntInProgress +
                        " enemiesSpawned=" + __instance.enemiesSpawned +
                        " monsterPrefabs=" + (__instance.monsterObjs == null ? -1 : __instance.monsterObjs.Length));
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(HuntManager), nameof(HuntManager.SpawnEnemy))]
    internal static class SpawnEnemyTrace
    {
        [HarmonyPostfix]
        private static void Postfix(int enemyType)
        {
            try { Log.Msg("HUNT: SpawnEnemy(" + enemyType + ")."); } catch { }
        }
    }

    [HarmonyPatch(typeof(HuntManager), nameof(HuntManager.SpawnEnemies))]
    internal static class SpawnEnemiesPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(HuntManager __instance)
        {
            if (!HuntBlock.NoEntities)
            {
                try
                {
                    Log.Msg("HUNT: SpawnEnemies running - enemiesSpawned=" + __instance.enemiesSpawned +
                            " allEnemies=" + (__instance.allEnemies == null ? -1 : __instance.allEnemies.Count));
                }
                catch { }
                return true;
            }
            HuntBlock.BlockedSpawns++;
            Log.Msg("HUNT: SpawnEnemies BLOCKED by the mod (HuntWithoutEntities is on).");
            return false;
        }

        /// <summary>
        /// With nothing spawned, nothing will ever die, so the hunt's "are we done" check would
        /// never be reached on its own. Poke it once so an empty hunt can still resolve.
        /// </summary>
        [HarmonyPostfix]
        private static void Postfix(HuntManager __instance)
        {
            if (!HuntBlock.NoEntities || __instance == null) return;
            try { __instance.CheckEnemiesLeft(); }
            catch (Exception ex) { Log.Debug("CheckEnemiesLeft after blocked wave: " + ex.Message); }
        }
    }

    [HarmonyPatch(typeof(HuntManager), nameof(HuntManager.SpawnEnemy))]
    internal static class SpawnEnemyPatch
    {
        [HarmonyPrefix]
        private static bool Prefix()
        {
            if (!HuntBlock.NoEntities) return true;
            HuntBlock.BlockedSpawns++;
            return false;
        }
    }

    [HarmonyPatch(typeof(HuntManager), nameof(HuntManager.StartHunt))]
    internal static class StartHuntPatch
    {
        /// <summary>Set when a hunt begins while the player is inside a control-taking screen.</summary>
        internal static bool AutoUnstick = true;

        [HarmonyPrefix]
        private static bool Prefix()
        {
            if (!HuntBlock.NoHunt) return true;
            HuntBlock.BlockedHunts++;
            Log.Debug("Hunt start blocked (total " + HuntBlock.BlockedHunts + ").");
            return false;
        }

        /// <summary>
        /// A hunt starting while you are at the store computer can leave you locked in that screen
        /// with no way out, so step out of it as the hunt begins rather than after you are stuck.
        /// </summary>
        [HarmonyPostfix]
        private static void Postfix()
        {
            if (!AutoUnstick) return;
            try
            {
                SuiteMod mod = SuiteMod.Instance;
                if (mod == null) return;
                if (!mod.World.IsInBlockingScreen()) return;
                Log.Msg("Hunt started while a control-taking screen was open; stepping out of it.");
                mod.World.UnstickPlayer(true);
            }
            catch (Exception ex) { Log.Ex("auto-unstick on hunt", ex); }
        }
    }

    [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.CheckForHunt))]
    internal static class CheckForHuntPatch
    {
        [HarmonyPrefix]
        private static bool Prefix() { return !HuntBlock.NoHunt; }
    }

    [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.StartHuntNoMatterWhat))]
    internal static class StartHuntNoMatterWhatPatch
    {
        [HarmonyPrefix]
        private static bool Prefix()
        {
            if (!HuntBlock.NoHunt) return true;
            HuntBlock.BlockedHunts++;
            return false;
        }
    }

    /// <summary>
    /// Gun cases: the flag is forced on the way in, so even the very first interaction of the night
    /// is already shared. The periodic sweep in WorldModule covers cases that never get interacted
    /// with directly.
    /// </summary>
    [HarmonyPatch(typeof(Interactable), nameof(Interactable.Interact))]
    internal static class InteractPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Interactable __instance)
        {
            if (!SuiteMod.GunCaseOpen) return;
            ForceOpen(__instance);
        }

        internal static void ForceOpen(Interactable target)
        {
            if (target == null) return;
            try
            {
                GunCase gc = target.TryCast<GunCase>();
                if (gc == null) return;
                gc.allowEveryoneToHave = true;
                gc.canPickupItem = true;
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(Interactable), nameof(Interactable.Rpc_Interact))]
    internal static class RpcInteractPatch
    {
        [HarmonyPrefix]
        private static void Prefix(Interactable __instance)
        {
            if (!SuiteMod.GunCaseOpen) return;
            InteractPatch.ForceOpen(__instance);
        }
    }
}
