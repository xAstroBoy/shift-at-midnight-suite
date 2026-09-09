using System;
using System.Collections.Generic;
using System.Reflection;
using Il2Cpp;
using Il2CppFusion;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// Host-side world control: fire events on demand, spawn doppelgangers and customers, keep gun
    /// cases open to everyone, unlock cosmetics.
    /// </summary>
    internal sealed class WorldModule
    {
        internal sealed class EventEntry
        {
            internal string Name;
            internal Action Fire;
            internal bool FromAtlas;
        }

        private readonly List<EventEntry> _events = new List<EventEntry>();
        private int _atlasCount = -1;

        internal int SelectedEvent;
        internal string LastResult = "";

        /// <summary>Sweep gun cases so every player can take a weapon, however many times.</summary>
        internal bool GunCaseForEveryone;

        /// <summary>
        /// Keep the emergency weapon arsenal open all the time, so it is available during the day
        /// without waiting for a hunt to start.
        /// </summary>
        internal bool WeaponWallAlwaysOpen;

        /// <summary>Extra browsing/nuisance customers on top of whatever the night generated.</summary>
        /// <summary>
        /// Whether the store is kept busy.
        ///
        /// This is deliberately not a field of its own. There were two bools for one idea - this, and
        /// Multipliers.CustomersEnabled, which scales the night's scheduled list - and a per-frame
        /// line copying one into the other. Whichever the menu wrote got overwritten on the next
        /// frame, so the checkbox appeared to untick itself the instant it was clicked.
        /// </summary>
        internal bool ExtraCustomers
        {
            get { return Multipliers.CustomersEnabled; }
            set { Multipliers.CustomersEnabled = value; }
        }

        /// <summary>Keep a few nuisance customers in the store at all times.</summary>
        internal bool ExtraNuisances;
        internal int NuisanceMultiplier = 2;

        /// <summary>Send extra doppelgangers in over the course of the night.</summary>
        internal bool ExtraDoppelgangers;
        internal int DoppelgangerCount = 2;

        internal int NuisancesSpawned;
        internal int DoppelsSpawned;
        internal int CustomerMultiplier = 3;

        private float _nextSweep;
        private float _nextCustomerTopUp;

        // Gun cases are fixed scene objects. Two FindObjectsOfType sweeps every 2s for them was the
        // worst single tick in the profile (11 ms); find them once and refresh rarely.
        private List<GunCase> _gunCases = new List<GunCase>();
        private float _nextGunCaseRefresh;

        /// <summary>The panel cover only needs revealing once per scene; its animation replays otherwise.</summary>
        private bool _panelRevealed;

        private List<GunCase> GunCases()
        {
            float now = Time.unscaledTime;
            if (now >= _nextGunCaseRefresh || _gunCases.Count == 0)
            {
                _nextGunCaseRefresh = now + 60f;
                _gunCases = Net.FindActive<GunCase>();
            }
            return _gunCases;
        }
        private float _nextReviewTick;
        private int _gunCasesPatched;

        // ---------------------------------------------------------------- events

        internal List<EventEntry> Events
        {
            get { RefreshEvents(); return _events; }
        }

        internal EventEntry SelectedEntry
        {
            get
            {
                RefreshEvents();
                if (_events.Count == 0) return null;
                if (SelectedEvent < 0) SelectedEvent = 0;
                if (SelectedEvent >= _events.Count) SelectedEvent = _events.Count - 1;
                return _events[SelectedEvent];
            }
        }

        internal void MoveEvent(int delta)
        {
            RefreshEvents();
            if (_events.Count == 0) { SelectedEvent = 0; return; }
            SelectedEvent = ((SelectedEvent + delta) % _events.Count + _events.Count) % _events.Count;
        }

        /// <summary>
        /// The menu is built from two sources: the designer-authored UnityEvent list on EventManager
        /// (eventAtlas, the same list the night scheduler picks from) and the named no-argument RPC
        /// entry points for the set pieces.
        /// </summary>
        private void RefreshEvents()
        {
            EventManager em = null;
            try { em = EventManager.Instance; } catch { }

            int atlas = 0;
            if (Net.Alive(em))
            {
                try { if (em.eventAtlas != null) atlas = em.eventAtlas.Length; } catch { }
            }

            if (atlas == _atlasCount && _events.Count > 0) return;
            _atlasCount = atlas;
            _events.Clear();

            if (!Net.Alive(em)) return;

            EventManager e = em;
            for (int i = 0; i < atlas; i++)
            {
                int index = i;
                _events.Add(new EventEntry
                {
                    Name = "Atlas Event " + index,
                    FromAtlas = true,
                    Fire = delegate
                    {
                        var a = e.eventAtlas;
                        if (a != null && index < a.Length && a[index] != null) a[index].Invoke();
                    }
                });
            }

            Add("Spawn Player Doppelganger", delegate { e.SpawnPlayerDoppelganger(); });
            Add("Spawn 8 Doppelgangers", delegate { e.Spawn8PlayerDoppelgangers(); });
            Add("Aggro All Employee Copies", delegate { e.AggroAllOtherEmployeeCopies(); });
            AddRpc("Rat Infestation", e, delegate { e.Rpc_SpawnRatInfestation(); });
            AddRpc("Roach Infestation", e, delegate { e.Rpc_SpawnRoachInfestation(); });
            Add("Spawn One Rat", delegate { e.SpawnRat(); });
            Add("Spawn One Roach", delegate { e.SpawnRoach(); });
            AddRpc("Flickering Lights", e, delegate { e.Rpc_FlickeringLightsEvent(); });
            AddRpc("Spawn Truck", e, delegate { e.Rpc_SpawnTruck(); });
            AddRpc("Chasing Nathan", e, delegate { e.Rpc_SpawnChasingNathan(); });
            AddRpc("Forest Limbs", e, delegate { e.Rpc_SpawnForestLimbs(); });
            AddRpc("Blood Moon Explanation", e, delegate { e.Rpc_BloodMoonExplanation(); });
            AddRpc("Shrine Event", e, delegate { e.Rpc_StartShrineEvent(); });
            AddRpc("Corpse Night Call", e, delegate { e.Rpc_CallOnCorpseNightRpc(); });
            AddRpc("Suspicious Customer", e, delegate { e.Rpc_SuspiciousCustomerRpc(); });
            Add("Vanessa Dead 1", delegate { e.VanessaDeadEvent1(); });
            Add("Vanessa Dead 2", delegate { e.VanessaDeadEvent2(); });
            AddRpc("End Of Day Bus", e, delegate { e.Rpc_EODBus(); });
            AddRpc("End Of Day Fake Bus", e, delegate { e.Rpc_EODFakeBus(); });
            AddRpc("Disable End Of Day Bus", e, delegate { e.Rpc_DisableEODBus(); });
            AddRpc("Night 1 Truck", e, delegate { e.Rpc_SpawnNight1TruckRpc(); });
            Add("No Event Tonight", delegate { e.NoEventOccursTonight(); });

            CurrentDayManager dm = null;
            try { dm = CurrentDayManager.Instance; } catch { }
            if (Net.Alive(dm))
            {
                CurrentDayManager d = dm;
                AddRpc("Nuisance Event", d, delegate { d.Rpc_NuisanceEvent(); });
                Add("Spawn Rake", delegate { d.SpawnRake(); });
                AddRpc("Rake Intro", d, delegate { d.Rpc_RakeIntro(); });
                AddRpc("Rake Graffiti", d, delegate { d.Rpc_RakeGraffiti(); });
                AddRpc("Scary Truck", d, delegate { d.Rpc_ScaryTruck(); });
                AddRpc("Enable Ambulance", d, delegate { d.Rpc_EnableAmbulance(); });
                AddRpc("Bathroom Guy", d, delegate { d.Rpc_EnableBathroomGuy(); });
                AddRpc("Dentist In Storage", d, delegate { d.Rpc_EnableDentistInStorage(); });
                AddRpc("Dentist Near Dumpster", d, delegate { d.Rpc_SpawnDentistNearDumpster(); });
                AddRpc("Dentist On Roof", d, delegate { d.Rpc_EnableDentistOnRoof(); });
                AddRpc("Dentist On Bus", d, delegate { d.Rpc_EnableDentistOnBus(); });
                AddRpc("Crawling Dentist", d, delegate { d.Rpc_EnableCrawlingDentist(); });
                AddRpc("Doppel Drinking Petrol", d, delegate { d.Rpc_EnableDoppelDrinkingPetrol(); });
                AddRpc("Ten Antlers", d, delegate { d.Rpc_Enable10Antlers(); });
                AddRpc("Creepy Painting", d, delegate { d.Rpc_EnableCreepyPainting(); });
                AddRpc("Anomaly Lens", d, delegate { d.Rpc_EnableAnomalyLens(); });
                AddRpc("Pubert (today)", d, delegate { d.Rpc_EnablePubert(d.curDay); });
                AddRpc("Day 7 Setup", d, delegate { d.Rpc_Day7(); });
                AddRpc("Purchase Traps Hint", d, delegate { d.Rpc_PurchaseTrapsHint(); });
                AddRpc("Disable Quota Note", d, delegate { d.Rpc_DisableQuotaNote(); });
                AddRpc("Turn Off Day-1 Objects", d, delegate { d.Rpc_TurnOffDay1Objs(); });
                AddRpc("Turn Off Not-Day-1 Objects", d, delegate { d.Rpc_TurnOffNotDay1Objs(); });
                AddRpc("Turn On Before-Car Objects", d, delegate { d.Rpc_TurnOnBeforeCarObjs(); });
                AddRpc("Turn On After-Car Objects", d, delegate { d.Rpc_TurnOnAfterCarObjs(); });
                Add("Spawn Pet", delegate { d.SpawnPet(); });
                Add("Next Occurrence", delegate { d.PlayNextOccurence(); });
            }

            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (Net.Alive(sm))
            {
                StoreManager s = sm;
                Add("Spawn Browsing Customer", delegate { s.SpawnBrowsingNPC(); });
                Add("Spawn Nuisance Customer", delegate { s.SpawnNuisanceNPC(); });
                Add("Spawn Poster", delegate { s.SpawnPoster(); });
                Add("All Customers Run Away", delegate { s.AllBrowsingNPCsRunAway(); });
                Add("All Customers Walk Away", delegate { s.AllBrowsingNPCsWalkAway(); });
                AddRpc("Disable Dumpster Monster", s, delegate { s.Rpc_DisableDumpsterMonster(); });
                AddRpc("Destroy Tutorial Stuff", s, delegate { s.Rpc_DestroyTutorialStuff(); });
                AddRpc("Start Hazard Lights", s, delegate { s.Rpc_StartHazardLights(); });
                AddRpc("Teleport Me To Store", s, delegate { s.Rpc_TeleportPlayerToStore(); });
                AddRpc("Exit Cutscene", s, delegate { s.Rpc_ExitCutscene(); });
                AddRpc("End Hunt", s, delegate { s.Rpc_EndHunt(); });
                Add("Start Hunt (no matter what)", delegate { s.StartHuntNoMatterWhat(); });
                // Ends the run - kept last and marked, because every row here fires on one click.
                AddRpc("[!] Start Final Sequence (ends the game)", s, delegate { s.Rpc_StartFinalSequence(); });
            }

            Log.Msg("Event menu built: " + _events.Count + " entries (" + atlas + " from eventAtlas).");
        }

        private void Add(string name, Action fire)
        {
            _events.Add(new EventEntry { Name = name, Fire = fire, FromAtlas = false });
        }

        /// <summary>
        /// An event backed by an Rpc_ method. Invocation goes through Util.Rpc, which applies the
        /// measured dispatch rules (a STATE-only host is refused by the plain call, so the body is
        /// run directly as the authority).
        /// </summary>
        private static void AddRpc(List<EventEntry> into, string name, NetworkBehaviour owner, Action call)
        {
            into.Add(new EventEntry
            {
                Name = name,
                FromAtlas = false,
                Fire = delegate { Rpc.Call(owner, call, name); }
            });
        }

        private void AddRpc(string name, NetworkBehaviour owner, Action call)
        {
            AddRpc(_events, name, owner, call);
        }

        internal void FireSelected()
        {
            EventEntry entry = SelectedEntry;
            if (entry == null) { LastResult = "No events available"; return; }
            Fire(entry);
        }

        internal void Fire(EventEntry entry)
        {
            if (entry == null) return;
            try
            {
                entry.Fire();
                LastResult = "Fired: " + entry.Name;
                Log.Msg("Fired event: " + entry.Name);
            }
            catch (Exception ex)
            {
                LastResult = "Failed: " + entry.Name;
                Log.Ex("event " + entry.Name, ex);
            }
        }

        // ---------------------------------------------------------------- doppelgangers

        internal void SpawnDoppelganger(int count)
        {
            EventManager em = null;
            try { em = EventManager.Instance; } catch { }
            if (!Net.Alive(em)) { LastResult = "EventManager not ready"; return; }

            int ok = 0;
            for (int i = 0; i < count; i++)
            {
                try { em.SpawnPlayerDoppelganger(); ok++; }
                catch (Exception ex) { Log.Ex("SpawnPlayerDoppelganger", ex); break; }
            }
            LastResult = "Spawned " + ok + " doppelganger(s)";
            Log.Msg(LastResult + ".");
        }

        internal void SpawnDoppelgangerHorde()
        {
            EventManager em = null;
            try { em = EventManager.Instance; } catch { }
            if (!Net.Alive(em)) { LastResult = "EventManager not ready"; return; }
            try
            {
                em.Spawn8PlayerDoppelgangers();
                LastResult = "Spawned doppelganger horde";
                Log.Msg(LastResult + ".");
            }
            catch (Exception ex) { Log.Ex("Spawn8PlayerDoppelgangers", ex); }
        }

        // ---------------------------------------------------------------- specific customers

        internal sealed class NpcChoice
        {
            internal int Index;          // index into StoreManager.allBrowsingNpcs
            internal string Name;
            internal bool IsDoppelganger;
        }

        private readonly List<NpcChoice> _npcChoices = new List<NpcChoice>();
        private int _npcChoiceCount = -1;

        /// <summary>
        /// Every customer prefab the store can spawn, flagged by whether it is a doppelganger.
        /// Read from StoreManager.allBrowsingNpcs, the list SpawnBrowsingNPC actually picks from.
        /// </summary>
        internal List<NpcChoice> NpcChoices
        {
            get
            {
                StoreManager sm = null;
                try { sm = StoreManager.Instance; } catch { }

                int count = 0;
                try { if (Net.Alive(sm) && sm.allBrowsingNpcs != null) count = sm.allBrowsingNpcs.Count; }
                catch { }

                if (count == _npcChoiceCount && _npcChoices.Count > 0) return _npcChoices;
                _npcChoices.Clear();
                _npcChoiceCount = count;

                for (int i = 0; i < count; i++)
                {
                    string name = "NPC " + i;
                    bool doppel = false;
                    try
                    {
                        GameObject prefab = sm.allBrowsingNpcs[i];
                        if (prefab != null)
                        {
                            name = prefab.name;

                            StoreBrowseBehaviour b = prefab.GetComponentInChildren<StoreBrowseBehaviour>(true);
                            if (b != null) doppel = b.isDoppelganger;

                            // Prefer the character's real name over the prefab name. The localisation
                            // tables are shipped encrypted, so this only resolves at runtime through
                            // JSONAccess - which is also where the end-of-day report gets it.
                            string real = ReportName(prefab);
                            if (!string.IsNullOrEmpty(real)) name = real + "   (" + prefab.name + ")";
                        }
                    }
                    catch { }
                    _npcChoices.Add(new NpcChoice { Index = i, Name = name, IsDoppelganger = doppel });
                }

                if (count > 0) Log.Msg("Customer roster read: " + count + " prefab(s).");
                return _npcChoices;
            }
        }

        /// <summary>The npc id a prefab reports under, or null.</summary>
        private static string DialogueIdOf(GameObject prefab)
        {
            // includeInactive must be true: roster entries are inactive prefabs, and the default
            // overload skips inactive children, so this silently returned null for every one of them.
            try
            {
                DialogueInteractable di = prefab.GetComponentInChildren<DialogueInteractable>(true);
                if (di == null) return null;
                string id = di.dialogueId;
                return string.IsNullOrEmpty(id) ? null : id;
            }
            catch { return null; }
        }

        /// <summary>Look a localisation entry up, treating the not-found marker as absent.</summary>
        private static string Misc(string category, string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            try
            {
                JSONAccess json = JSONAccess.Instance;
                if (!Net.Alive(json)) return null;
                string v = json.GetMiscText(category, key);
                if (string.IsNullOrEmpty(v)) return null;
                if (v.IndexOf("not found", StringComparison.OrdinalIgnoreCase) >= 0) return null;
                if (v.IndexOf("TNF", StringComparison.Ordinal) >= 0) return null;
                return v;
            }
            catch { return null; }
        }

        private static string ReportName(GameObject prefab)
        {
            return Misc("EOD Report Names", DialogueIdOf(prefab));
        }

        /// <summary>
        /// Print the whole customer roster with real names and report text. The localisation files
        /// ship encrypted, so this is the only way to see who actually exists in this build.
        /// </summary>
        internal void DumpNpcRoster()
        {
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm) || sm.allBrowsingNpcs == null)
            {
                LastResult = "StoreManager not ready";
                Log.Warn("Roster dump: StoreManager not ready (enter a shift first).");
                return;
            }

            int count = sm.allBrowsingNpcs.Count;
            Log.Msg("=== Customer roster: " + count + " prefab(s) ===");
            for (int i = 0; i < count; i++)
            {
                string prefabName = "?", id = "-", realName = "-", desc = "-";
                bool doppel = false;
                try
                {
                    GameObject prefab = sm.allBrowsingNpcs[i];
                    if (prefab != null)
                    {
                        prefabName = prefab.name;
                        id = DialogueIdOf(prefab) ?? "-";
                        realName = Misc("EOD Report Names", id) ?? "-";
                        desc = Misc("EOD Report Descs", id) ?? "-";
                        StoreBrowseBehaviour b = prefab.GetComponentInChildren<StoreBrowseBehaviour>(true);
                        if (b != null) doppel = b.isDoppelganger;
                    }
                }
                catch { }

                Log.Msg("[" + i.ToString().PadLeft(3) + "] " + (doppel ? "DOPPEL " : "       ") +
                        "id=" + id + "  name=" + realName + "  prefab=" + prefabName);
                if (desc != "-") Log.Msg("        why: " + desc);
            }
            Log.Msg("=== end roster ===");
            LastResult = "Dumped " + count + " customers to the log";
        }

        /// <summary>
        /// Spawn one specific customer.
        ///
        /// SpawnBrowsingNPC increments curBrowsingNpcIndex and then uses it (wrapping at the end of
        /// the list):
        ///     117 Move rax, [this+1304]   ; curBrowsingNpcIndex
        ///     118 Add  rax, rax, 1
        ///     119 Move [this+1304], rax
        ///     124 Compare rax, [list+24]  ; wrap when past the end
        /// So setting the index to (target - 1) and calling it makes the game spawn exactly the one
        /// we want, through its own spawn path.
        /// </summary>
        internal void SpawnSpecificNpc(NpcChoice choice)
        {
            if (choice == null) return;

            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) { LastResult = "StoreManager not ready"; return; }

            try
            {
                int count = sm.allBrowsingNpcs != null ? sm.allBrowsingNpcs.Count : 0;
                if (count <= 0) { LastResult = "No customer roster"; return; }

                int previous = sm.curBrowsingNpcIndex;
                sm.curBrowsingNpcIndex = choice.Index - 1;      // the call increments before use
                if (sm.curBrowsingNpcIndex < -1) sm.curBrowsingNpcIndex = count - 1;

                // The spawner refuses to run while it thinks browsing NPCs are not allowed.
                bool wasAllowed = sm.allowedToSpawnBrowsingNPCs;
                if (!wasAllowed) sm.allowedToSpawnBrowsingNPCs = true;

                sm.SpawnBrowsingNPC();

                if (!wasAllowed) sm.allowedToSpawnBrowsingNPCs = wasAllowed;

                LastResult = "Spawned " + choice.Name + (choice.IsDoppelganger ? " (doppelganger)" : "");
                Log.Msg(LastResult + " [roster index " + choice.Index + ", was " + previous + "].");
            }
            catch (Exception ex)
            {
                Log.Ex("spawn specific npc", ex);
                LastResult = "Failed to spawn " + choice.Name;
            }
        }

        // ---------------------------------------------------------------- scripted customers (Npc assets)

        internal sealed class NpcAsset
        {
            internal Npc Asset;
            internal string Id;
            internal string Name;
            internal string Why;
            internal bool IsDoppelganger;
        }

        private readonly List<NpcAsset> _npcAssets = new List<NpcAsset>();
        private float _nextNpcAssetScan;

        /// <summary>
        /// Every scripted customer the game can put in the queue - the ones with an ID card and an
        /// end-of-day report entry, doppelgangers included.
        ///
        /// These are Npc ScriptableObjects, not shopper prefabs: allBrowsingNpcs is the ambient
        /// crowd and never contains them, which is why the earlier roster came up empty. The night
        /// generator picks from EndlessGenerationManager's Npc arrays; we read those and also sweep
        /// every loaded Npc asset so nothing is missed.
        /// </summary>
        internal List<NpcAsset> NpcAssets
        {
            get
            {
                // Once per scene. Recomputing every 5s meant a scene scan plus two localisation
                // lookups per asset, on the UI thread, while the menu was open.
                if (_npcAssets.Count > 0 || Time.unscaledTime < _nextNpcAssetScan) return _npcAssets;
                _nextNpcAssetScan = Time.unscaledTime + 3f;   // retry cadence only while still empty

                var seen = new HashSet<string>();
                var found = new List<Npc>();

                try
                {
                    CurrentDayManager dm = CurrentDayManager.Instance;
                    EndlessGenerationManager gen = Net.Alive(dm) ? dm.customerGenManager : null;
                    if (Net.Alive(gen))
                    {
                        AddAll(found, gen.allRandomSpawningNpcs);
                        AddAll(found, gen.endlessGenSpawningNpcs);
                        AddAll(found, gen.demoRandomSpawningNpcs);
                    }
                }
                catch (Exception ex) { Log.Debug("gen manager npcs: " + ex.Message); }

                // Anything loaded that the generator arrays do not reference (story-only characters).
                List<Npc> loaded = Net.FindAll<Npc>();
                for (int i = 0; i < loaded.Count; i++) found.Add(loaded[i]);

                _npcAssets.Clear();
                for (int i = 0; i < found.Count; i++)
                {
                    Npc npc = found[i];
                    if (!Net.Alive(npc)) continue;

                    string id = null, assetName = null;
                    bool doppel = false;
                    try { id = npc.id; assetName = npc.name; doppel = npc.isDoppelganger; } catch { continue; }

                    string key = !string.IsNullOrEmpty(id) ? id : assetName;
                    if (string.IsNullOrEmpty(key) || !seen.Add(key)) continue;

                    string real = Misc("EOD Report Names", id);
                    string why = doppel ? Misc("EOD Report Descs", id) : null;
                    string name = !string.IsNullOrEmpty(real) ? real : (assetName ?? "Npc " + id);
                    if (!string.IsNullOrEmpty(id)) name += "   [#" + id + "]";

                    _npcAssets.Add(new NpcAsset { Asset = npc, Id = id, Name = name, Why = why, IsDoppelganger = doppel });
                }

                _npcAssets.Sort(delegate (NpcAsset a, NpcAsset b) { return string.CompareOrdinal(a.Name, b.Name); });
                if (_npcAssets.Count > 0) Log.Debug("Scripted customer roster: " + _npcAssets.Count + " Npc asset(s).");
                return _npcAssets;
            }
        }

        private static void AddAll(List<Npc> into, Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Npc> arr)
        {
            if (arr == null) return;
            for (int i = 0; i < arr.Length; i++) if (arr[i] != null) into.Add(arr[i]);
        }

        /// <summary>
        /// Spawn one scripted customer through the same call the night scheduler uses:
        /// FusionNetworkManager.SpawnItem(prefab's NetworkObject, position, rotation). Networked, so
        /// every player sees them arrive.
        /// </summary>
        internal void SpawnNpcAsset(NpcAsset choice)
        {
            if (choice == null || !Net.Alive(choice.Asset)) return;

            FusionNetworkManager fnm = null;
            try { fnm = FusionNetworkManager.Instance; } catch { }
            if (!Net.Alive(fnm)) { LastResult = "FusionNetworkManager not ready"; Log.Warn(LastResult + "."); return; }

            NetworkObject prefab = null;
            try
            {
                GameObject go = choice.Asset.prefab;
                if (go != null)
                {
                    prefab = go.GetComponent<NetworkObject>();
                    if (prefab == null) prefab = go.GetComponentInChildren<NetworkObject>(true);
                }
            }
            catch (Exception ex) { Log.Debug("npc prefab: " + ex.Message); }
            if (prefab == null) { LastResult = choice.Name + " has no networked prefab"; Log.Warn(LastResult + "."); return; }

            Vector3 pos; Quaternion rot;
            if (!StoreSpawnPoint(out pos, out rot))
            {
                Transform me = Net.LocalTransform;
                if (me == null) { LastResult = "No spawn point"; return; }
                pos = me.position + me.forward * 3f; rot = me.rotation;
            }

            try
            {
                // FusionNetworkManager is not a NetworkBehaviour, so no authority-mask dance here:
                // host spawns directly, a client asks the host through the manager's own request RPC.
                if (Net.IsHost) fnm.SpawnItem(prefab, pos, rot, null);
                else fnm.Rpc_RequestSpawnItem(prefab, pos, rot, null);
                LastResult = "Spawned " + choice.Name + (choice.IsDoppelganger ? "  (doppelganger)" : "");
                Log.Msg(LastResult + (string.IsNullOrEmpty(choice.Why) ? "" : "  -  " + choice.Why) + ".");
            }
            catch (Exception ex)
            {
                Log.Ex("spawn npc asset", ex);
                LastResult = "Failed to spawn " + choice.Name;
            }
        }

        private static bool StoreSpawnPoint(out Vector3 pos, out Quaternion rot)
        {
            pos = Vector3.zero; rot = Quaternion.identity;
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (!Net.Alive(sm) || sm.npcSpawnPoints == null || sm.npcSpawnPoints.Length == 0) return false;
                Transform t = sm.npcSpawnPoints[UnityEngine.Random.Range(0, sm.npcSpawnPoints.Length)];
                if (t == null) return false;
                pos = t.position; rot = t.rotation;
                return true;
            }
            catch { return false; }
        }

        // ---------------------------------------------------------------- monsters and pets

        private readonly List<NpcChoice> _monsters = new List<NpcChoice>();
        private int _monsterCount = -1;
        private readonly List<NpcChoice> _pets = new List<NpcChoice>();
        private int _petCount = -1;

        /// <summary>
        /// The hunt roster. HuntManager.SpawnEnemy(int) indexes monsterObjs and spawns through
        /// FusionNetworkManager.SpawnItem, so this is the game's own networked monster spawn - not
        /// a local instantiate.
        /// </summary>
        internal List<NpcChoice> Monsters
        {
            get
            {
                HuntManager hm = null;
                try { hm = HuntManager.Instance; } catch { }

                int count = 0;
                try { if (Net.Alive(hm) && hm.monsterObjs != null) count = hm.monsterObjs.Length; }
                catch { }

                if (count == _monsterCount && _monsters.Count > 0) return _monsters;
                _monsters.Clear();
                _monsterCount = count;

                for (int i = 0; i < count; i++)
                {
                    string name = "Monster " + i;
                    try
                    {
                        GameObject prefab = hm.monsterObjs[i];
                        if (prefab != null) name = prefab.name;
                    }
                    catch { }

                    // The difficulty weighting doubles as a rough "how nasty is this" hint.
                    string suffix = "";
                    try
                    {
                        var pts = hm.monsterDifficultyPoints;
                        if (pts != null && i < pts.Length) suffix = "   (difficulty " + pts[i] + ")";
                    }
                    catch { }

                    _monsters.Add(new NpcChoice { Index = i, Name = name + suffix });
                }

                if (count > 0) Log.Msg("Monster roster read: " + count + " prefab(s).");
                return _monsters;
            }
        }

        internal void SpawnMonster(NpcChoice choice)
        {
            if (choice == null) return;

            HuntManager hm = null;
            try { hm = HuntManager.Instance; } catch { }
            if (!Net.Alive(hm)) { LastResult = "HuntManager not ready (enter a shift first)"; return; }

            try
            {
                hm.SpawnEnemy(choice.Index);
                LastResult = "Spawned monster: " + choice.Name;
                Log.Msg(LastResult + " [roster index " + choice.Index + "].");
            }
            catch (Exception ex)
            {
                Log.Ex("spawn monster", ex);
                LastResult = "Failed to spawn " + choice.Name;
            }
        }

        /// <summary>Pet prefabs from CurrentDayManager.pets, spawned through the game's pet RPC.</summary>
        internal List<NpcChoice> Pets
        {
            get
            {
                CurrentDayManager dm = null;
                try { dm = CurrentDayManager.Instance; } catch { }

                int count = 0;
                try { if (Net.Alive(dm) && dm.pets != null) count = dm.pets.Length; }
                catch { }

                if (count == _petCount && _pets.Count > 0) return _pets;
                _pets.Clear();
                _petCount = count;

                for (int i = 0; i < count; i++)
                {
                    string name = "Pet " + i;
                    try
                    {
                        GameObject prefab = dm.pets[i];
                        if (prefab != null) name = prefab.name;
                    }
                    catch { }
                    _pets.Add(new NpcChoice { Index = i, Name = name });
                }
                return _pets;
            }
        }

        internal void SpawnPet(NpcChoice choice)
        {
            if (choice == null) return;

            CurrentDayManager dm = null;
            try { dm = CurrentDayManager.Instance; } catch { }
            if (!Net.Alive(dm)) { LastResult = "CurrentDayManager not ready"; return; }

            try
            {
                dm.Rpc_CMD_SpawnPet(choice.Index, choice.Name);
                LastResult = "Spawned pet: " + choice.Name;
                Log.Msg(LastResult + ".");
            }
            catch (Exception ex)
            {
                Log.Ex("spawn pet", ex);
                LastResult = "Failed to spawn " + choice.Name;
            }
        }

        // ---------------------------------------------------------------- shift clock

        /// <summary>Hold the shift countdown at whatever it is now; the day never ends on its own.</summary>
        internal bool FreezeClock;

        /// <summary>Extra seconds to keep the clock above while frozen (0 = just hold current).</summary>
        private int _frozenAt = -1;
        private float _nextClockTick;

        /// <summary>
        /// The shift is StoreManager.secondsLeft, an int that CountDownSeconds decrements once per
        /// tick ("Subtract [this+520], [this+520], 1"). Holding or raising it is all "make the shift
        /// last longer" needs; the countdown text re-reads the field every tick.
        /// </summary>
        internal int SecondsLeft
        {
            get
            {
                try { StoreManager sm = StoreManager.Instance; return Net.Alive(sm) ? sm.secondsLeft : -1; }
                catch { return -1; }
            }
        }

        internal void AddSeconds(int seconds)
        {
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) { LastResult = "StoreManager not ready"; return; }
            try
            {
                int now = sm.secondsLeft;
                int next = Math.Max(0, now + seconds);
                sm.secondsLeft = next;
                if (FreezeClock) _frozenAt = next;
                LastResult = "Shift clock " + Fmt(now) + " -> " + Fmt(next);
                Log.Msg(LastResult + ".");
            }
            catch (Exception ex) { Log.Ex("add seconds", ex); }
        }

        /// <summary>
        /// The night never runs out, but the bus is still yours to call.
        ///
        /// "Your shift is done, head to the bus" is what the clock reaching zero buys you: the store
        /// stops generating, the customers walk out, and you are stood in a finished night waiting to
        /// be allowed to leave. Freezing the clock stops that, but it also stops time moving, and the
        /// night's generation is scheduled against the clock - a frozen night is just as dead, only
        /// quieter.
        ///
        /// So let the clock run and top it back up before it can expire. Time keeps moving, the store
        /// keeps generating, and the shift simply never reaches its end. Nothing here blocks the day
        /// from completing: boarding the bus still ends the night exactly as it always did, which is
        /// the whole point of leaving that path alone. Use Call The Bus when you want to go.
        /// </summary>
        internal bool EndlessNight;

        /// <summary>Minutes the clock is wound back to when it runs low.</summary>
        internal int EndlessTopUpMinutes = 10;

        internal int NightsExtended;
        private float _nextEndlessCustomers;
        private float _nextEndlessEvent;

        /// <summary>
        /// Ends the night on purpose, which a blocked ending still has to allow.
        ///
        /// The block is lifted for exactly this call and put straight back, so the shift finishes down
        /// the game's own path - the report, the payout, the next day - rather than through anything
        /// reimplemented here.
        /// </summary>
        internal void EndNightNow()
        {
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) { LastResult = "StoreManager not ready"; Log.Warn(LastResult + "."); return; }

            try
            {
                // Held open until the scene changes, not just for this call: the ending continues into
                // EODScene a second later, and closing the gate behind CompleteDay is what left the
                // screen black with the report never loading.
                DayEndBlock.EndingInProgress = true;
                sm.CompleteDay();
                LastResult = "Night ended";
                Log.Msg(LastResult + " on request - the ending will run through to the report.");
            }
            catch (Exception ex)
            {
                DayEndBlock.EndingInProgress = false;
                Log.Ex("end night", ex);
                LastResult = "Could not end the night";
            }
        }

        private void TickEndlessNight()
        {
            float now = Time.unscaledTime;
            // Winding the clock back re-crosses whatever the night schedules against it, so the
            // once-a-night spawns need the game's own "already done" flags enforced against us.
            RepeatSpawnBlock.Enabled = true;

            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) return;

            try
            {
                // This used to wind StoreManager.secondsLeft back to ten minutes, on the inherited
                // claim that it is the shift clock. It is not - it is the entity countdown, the one
                // behind "the entity will arrive in N seconds". Winding it back did not lengthen a
                // single night; all it did was push the hunt ten minutes away, which is why a hunt
                // announced itself as 580 seconds out. It also explains why the wind-back only ever
                // fired once a night: nothing decrements that field outside a hunt, so once it had
                // been set to 600 it simply stayed there.
                //
                // So it is left alone. The real shift clock has not been identified yet - Dump Shift
                // Clock To Log is what finds it - and until it is, Endless Night does the half it can
                // honestly do rather than breaking the hunt for the half it cannot.
                //
                // The shift ending is what closes the doors to new shoppers, and holding that open
                // does keep a night busier for as long as it lasts.
                if (!sm.allowedToSpawnBrowsingNPCs)
                {
                    sm.allowedToSpawnBrowsingNPCs = true;
                    NightsExtended++;
                }

                // Rewinding curOccurrence was the first idea here and it was the wrong one: it replays
                // a list this codebase has already established nothing consumes - TriggerNextEvent was
                // never called from it once across a whole night - so it would have re-run the same
                // entries out of a queue that was not driving anything anyway.
                //
                // New content instead, through the two paths that demonstrably do fire: the game's own
                // SpawnBrowsingNPC for shoppers, and the event pump for atmosphere. Both already exist
                // and are already the mechanism behind Keep Store Busy and Multiply Events; a held-open
                // night just needs them running whether or not those toggles are on.
                if (Net.IsHost)
                {
                    if (now >= _nextEndlessCustomers)
                    {
                        _nextEndlessCustomers = now + 6f;
                        TopUpCustomers();
                    }
                    if (now >= _nextEndlessEvent)
                    {
                        _nextEndlessEvent = now + EventInterval();
                        PumpEvents();
                        NightsExtended++;
                    }
                }
            }
            catch (Exception ex) { Log.Debug("endless night: " + ex.Message); }
        }

        /// <summary>
        /// Finds the widget the shift timer is actually drawn into.
        ///
        /// Endless Night wound the clock back once and then never again, which is only possible if
        /// nothing is decrementing StoreManager.secondsLeft - so that field is not the shift clock,
        /// whatever the inherited comment on it says, and the real one ran out underneath it. Freeze
        /// Shift Clock and the +/- minute buttons all write to the same field, so they will have been
        /// no-ops too.
        ///
        /// Every clock on screen is text in the end. This prints secondsLeft next to what its own
        /// label reads, then every text in the scene that looks like a timer, with the path to it -
        /// and the object holding the real one names the system that drives it.
        /// </summary>
        internal void DumpShiftClock()
        {
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (Net.Alive(sm))
                {
                    string label = "?";
                    try { label = sm.secondsLeftText == null ? "(no text)" : sm.secondsLeftText.text; } catch { }
                    string extra = "";
                    try { extra = " inHunt=" + sm.inHunt + " alreadyCompleted=" + sm.alreadyCompleted; } catch { }
                    Log.Msg("SHIFT CLOCK: StoreManager.secondsLeft=" + sm.secondsLeft +
                            " secondsLeftText=\"" + label + "\"" + extra);
                }

                CurrentDayManager dm = CurrentDayManager.Instance;
                if (Net.Alive(dm))
                {
                    try { Log.Msg("   CurrentDayManager: curDay=" + dm.curDay + " curOccurrence=" + dm.curOccurrence +
                                  " occurrencesCompleted=" + dm.occurrencesCompleted + " startedDay=" + dm.startedDay); }
                    catch { }
                }

                // Anything reading like mm:ss is a candidate for the real timer.
                List<Il2CppTMPro.TextMeshProUGUI> texts = Net.FindActive<Il2CppTMPro.TextMeshProUGUI>();
                int shown = 0;
                for (int i = 0; i < texts.Count && shown < 20; i++)
                {
                    Il2CppTMPro.TextMeshProUGUI t = texts[i];
                    if (!Net.Alive(t)) continue;

                    string s;
                    try { s = t.text; } catch { continue; }
                    if (string.IsNullOrEmpty(s) || s.Length > 12 || s.IndexOf(':') < 0) continue;

                    bool digits = false;
                    for (int k = 0; k < s.Length; k++) if (s[k] >= '0' && s[k] <= '9') { digits = true; break; }
                    if (!digits) continue;

                    shown++;
                    string path = "";
                    try
                    {
                        Transform tr = t.transform;
                        for (int d = 0; d < 5 && tr != null; d++) { path = "/" + tr.name + path; tr = tr.parent; }
                    }
                    catch { }
                    Log.Msg("   CLOCK TEXT \"" + s + "\" at" + path);
                }
                if (shown == 0) Log.Msg("   No timer-looking text on screen right now.");

                LastResult = "Shift clock dumped to the log";
            }
            catch (Exception ex) { Log.Ex("dump shift clock", ex); LastResult = "Dump failed"; }
        }

        /// <summary>Brings the end-of-day bus in on demand. Board it and the night ends normally.</summary>
        internal void CallBus()
        {
            EventManager em = null;
            try { em = EventManager.Instance; } catch { }
            if (!Net.Alive(em)) { LastResult = "EventManager not ready"; Log.Warn(LastResult + "."); return; }

            try
            {
                em.Rpc_EODBus();
                LastResult = "Bus called";
                Log.Msg(LastResult + " - board it when you are ready to end the night.");
            }
            catch (Exception ex) { Log.Ex("call bus", ex); LastResult = "Could not call the bus"; }
        }

        private void TickClock()
        {
            if (!FreezeClock) { _frozenAt = -1; return; }
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) return;
            try
            {
                int now = sm.secondsLeft;
                if (_frozenAt < 0) _frozenAt = now;          // capture on first frozen tick
                if (now < _frozenAt) sm.secondsLeft = _frozenAt;
            }
            catch (Exception ex) { Log.Debug("freeze clock: " + ex.Message); }
        }

        internal static string Fmt(int s)
        {
            if (s < 0) return "--:--";
            return (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
        }

        // ---------------------------------------------------------------- network diagnostics

        /// <summary>
        /// Print the actual Fusion authority state.
        ///
        /// Everything the spawner and the event menu do depends on the local authority mask, and up
        /// to now that has been reasoned about rather than read. This prints what the runtime really
        /// reports, so the behaviour can be matched against the dispatch instead of assumed.
        /// </summary>
        internal void DumpNetworkState()
        {
            Log.Msg("=== Fusion network state ===");

            NetworkRunner runner = null;
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (Net.Alive(sm)) runner = sm.Runner;
            }
            catch (Exception ex) { Log.Debug("runner: " + ex.Message); }

            if (runner == null)
            {
                Log.Warn("No NetworkRunner - not in a session yet.");
            }
            else
            {
                Report("Runner.IsRunning", delegate { return runner.IsRunning.ToString(); });
                Report("Runner.GameMode", delegate { return runner.GameMode.ToString(); });
                Report("Runner.IsServer", delegate { return runner.IsServer.ToString(); });
                Report("Runner.IsClient", delegate { return runner.IsClient.ToString(); });
                Report("Runner.IsSinglePlayer", delegate { return runner.IsSinglePlayer.ToString(); });
                Report("Runner.IsSharedModeMasterClient", delegate { return runner.IsSharedModeMasterClient.ToString(); });
                Report("Runner.Stage", delegate { return runner.Stage.ToString(); });

                // Il2Cpp enumerators do not expose MoveNext through the interop interface, so count
                // the live PlayerManagers instead - same thing for our purposes.
                int players = -1;
                try { players = Net.FindAll<PlayerManager>().Count; } catch { }
                Log.Msg("  PlayerManagers in scene  = " + players);
            }

            DumpBehaviour("StoreManager", TryGet(delegate { return (NetworkBehaviour)StoreManager.Instance; }));
            DumpBehaviour("EventManager", TryGet(delegate { return (NetworkBehaviour)EventManager.Instance; }));
            DumpBehaviour("CurrentDayManager", TryGet(delegate { return (NetworkBehaviour)CurrentDayManager.Instance; }));
            DumpBehaviour("HuntManager", TryGet(delegate { return (NetworkBehaviour)HuntManager.Instance; }));
            DumpBehaviour("local InventoryManager", TryGet(delegate { return (NetworkBehaviour)Net.LocalInventory; }));

            Log.Msg("  (STATE=1 INPUT=2 PROXY=4. A plain Rpc_ call is REFUSED at mask 1; the suite runs the body as authority instead.)");
            Log.Msg("=== end network state ===");
            LastResult = "Network state written to the log";
        }

        private static NetworkBehaviour TryGet(Func<NetworkBehaviour> get)
        {
            try { return get(); } catch { return null; }
        }

        private static void Report(string label, Func<string> read)
        {
            string v;
            try { v = read(); } catch (Exception ex) { v = "<" + ex.Message + ">"; }
            Log.Msg("  " + label.PadRight(24) + " = " + v);
        }

        private static void DumpBehaviour(string label, NetworkBehaviour nb)
        {
            if (!Net.Alive(nb)) { Log.Msg("  " + label.PadRight(24) + " = <not present>"); return; }

            string mask = "?", state = "?", input = "?";
            try { mask = nb.GetLocalAuthorityMask().ToString(); } catch (Exception ex) { mask = "<" + ex.Message + ">"; }
            try { state = nb.HasStateAuthority.ToString(); } catch { }
            try { input = nb.HasInputAuthority.ToString(); } catch { }

            Log.Msg("  " + label.PadRight(24) + " mask=" + mask + "  stateAuth=" + state + "  inputAuth=" + input);
        }

        // ---------------------------------------------------------------- scene set-pieces

        internal sealed class SetPiece
        {
            internal string Name;
            internal Func<GameObject> Get;
        }

        private readonly List<SetPiece> _setPieces = new List<SetPiece>();

        /// <summary>
        /// Pre-placed scene objects the night scheduler switches on.
        ///
        /// Not everything has an Rpc_ entry point. The cow, for instance, is only ever touched
        /// inside PlayNextOccurence - there is no Rpc_EnableCow - so it can never appear in an event
        /// list built from RPC names. These are switched on directly instead.
        ///
        /// Caveat: this activates an object rather than spawning one. On the host that is how the
        /// game does it too, but whether a client sees it depends on the object replicating its own
        /// active state, which not all of them do.
        /// </summary>
        internal List<SetPiece> SetPieces
        {
            get
            {
                CurrentDayManager dm = null;
                try { dm = CurrentDayManager.Instance; } catch { }
                if (!Net.Alive(dm)) { _setPieces.Clear(); return _setPieces; }
                if (_setPieces.Count > 0) return _setPieces;

                CurrentDayManager d = dm;
                Piece("Cow", delegate { return d.cow; });
                Piece("Camouflaged Monster", delegate { return d.camouflagedMonster; });
                Piece("Forest Limbs", delegate { return d.forestLimbs; });
                Piece("Night 4 Dentist", delegate { return d.night4Dentist; });
                Piece("Dentist On Bus", delegate { return d.dentistOnBus; });
                Piece("Dentist On Roof", delegate { return d.dentistOnRoof; });
                Piece("Dentist In Storage", delegate { return d.dentistInStorage; });
                Piece("Crawling Dentist", delegate { return d.crawlingDentist; });
                Piece("Ambulance", delegate { return d.ambulance; });
                Piece("Bathroom Guy", delegate { return d.bathroomGuy; });
                Piece("Doppel Drinking Petrol", delegate { return d.doppelDrinkingPetrol; });
                Piece("Ten Antlers", delegate { return d.tenAntlers; });
                Piece("Creepy Painting", delegate { return d.creepyPainting; });
                Piece("Anomaly Lens", delegate { return d.anomalyLens; });
                Piece("Lemonade Stand Pubert", delegate { return d.lemonadeStandPubert; });
                Piece("Motorcycle Pubert", delegate { return d.motorcyclePubert; });
                Piece("Car Pubert", delegate { return d.carPubert; });
                Piece("Quota Note", delegate { return d.quotaNote; });
                Piece("Weapons Arsenal Icon", delegate { return d.weaponsArsenalIcon; });
                Piece("Truck Holder", delegate { return d.truckHolder; });
                Piece("Today's Day-Object NPC", delegate { return d.todaysDayObjNpc; });
                Piece("Today's Day-Object Car", delegate { return d.todaysDayObjCar; });

                // Hunt-side pieces that sit outside monsterObjs and so are not in the monster roster.
                HuntManager hm = null;
                try { hm = HuntManager.Instance; } catch { }
                if (Net.Alive(hm))
                {
                    HuntManager h = hm;
                    Piece("Spider (level 1)", delegate { return h.spiderLvl1; });
                    Piece("Spider (level 2)", delegate { return h.spiderLvl2; });
                    Piece("Spider (level 3)", delegate { return h.spiderLvl3; });
                    Piece("Jack In The Box", delegate { return h.jackInTheBox; });
                }

                Log.Msg("Set-piece list built: " + _setPieces.Count + " entries.");
                return _setPieces;
            }
        }

        private void Piece(string name, Func<GameObject> get)
        {
            _setPieces.Add(new SetPiece { Name = name, Get = get });
        }

        internal void ToggleSetPiece(SetPiece piece)
        {
            if (piece == null) return;
            try
            {
                GameObject go = piece.Get();
                if (go == null) { LastResult = piece.Name + " is not present in this scene"; Log.Warn(LastResult + "."); return; }

                bool now = !go.activeSelf;
                go.SetActive(now);
                LastResult = piece.Name + (now ? " enabled" : " disabled");
                Log.Msg(LastResult + ".");
            }
            catch (Exception ex)
            {
                Log.Ex("set-piece " + piece.Name, ex);
                LastResult = "Failed on " + piece.Name;
            }
        }

        // ---------------------------------------------------------------- per-frame upkeep

        internal void Tick()
        {
            float now = Time.unscaledTime;

            if ((GunCaseForEveryone || WeaponWallAlwaysOpen) && now >= _nextSweep)
            {
                _nextSweep = now + 2f;
                if (GunCaseForEveryone) SweepGunCases();
                if (WeaponWallAlwaysOpen) OpenWeaponWall(false);
            }

            // Only Endless Night moves the clock backwards, so only it needs the repeat guard.
            if (RepeatSpawnBlock.Enabled != EndlessNight) RepeatSpawnBlock.Enabled = EndlessNight;
            if (DayEndBlock.Enabled != EndlessNight) DayEndBlock.Enabled = EndlessNight;
            if (ObjectiveMute.Enabled != EndlessNight) ObjectiveMute.Enabled = EndlessNight;

            if ((FreezeClock || EndlessNight) && now >= _nextClockTick)
            {
                _nextClockTick = now + 0.25f;
                // Freezing pins the clock; endless lets it run and winds it back. Doing both would
                // pin it at the floor and never wind it, so the explicit freeze wins and says so.
                if (FreezeClock) { RepeatSpawnBlock.Enabled = false; TickClock(); }
                else TickEndlessNight();
            }

            if (PerfectReviews.Enabled && now >= _nextReviewTick)
            {
                _nextReviewTick = now + 1f;
                KeepReviewsPerfect();
            }

            if (ExtraCustomers && Net.IsHost && now >= _nextCustomerTopUp)
            {
                _nextCustomerTopUp = now + 6f;
                TopUpCustomers();
            }

            if (Multipliers.EventsEnabled && Net.IsHost && now >= _nextEventPump)
            {
                // First one comes a little after the night starts, then on the interval.
                _nextEventPump = now + EventInterval();
                PumpEvents();
            }

            if (ExtraNuisances && Net.IsHost && now >= _nextNuisanceTopUp)
            {
                _nextNuisanceTopUp = now + 8f;
                TopUpNuisances();
            }

            if (ExtraDoppelgangers && Net.IsHost && now >= _nextDoppelCheck)
            {
                _nextDoppelCheck = now + 20f;
                TopUpDoppelgangers();
            }
        }

        /// <summary>
        /// GunCase does not override Interact, so patching typeof(GunCase).Interact resolves to the
        /// Interactable base and cannot be filtered per case. A cheap sweep is both simpler and more
        /// reliable, and it also covers cases spawned after the patch.
        /// </summary>
        private void SweepGunCases()
        {
            List<GunCase> cases = GunCases();
            int touched = 0;
            for (int i = 0; i < cases.Count; i++)
            {
                GunCase gc = cases[i];
                if (!Net.Alive(gc)) continue;
                try
                {
                    if (!gc.allowEveryoneToHave) { gc.allowEveryoneToHave = true; touched++; }
                    gc.canPickupItem = true;
                }
                catch { }
            }
            if (touched > 0)
            {
                _gunCasesPatched += touched;
                Log.Debug("Gun cases opened to everyone: " + touched + " (total " + _gunCasesPatched + ").");
            }
        }

        /// <summary>
        /// Keeps the shop busier than the night generator intended. The generator multiplier handles
        /// scheduled NPCs; this tops up ambient browsers so the multiplier is actually felt in-store.
        /// </summary>
        private float _nextEventPump;
        internal int EventsPumped;

        /// <summary>Seconds between forced events: more often the higher the multiplier.</summary>
        private float EventInterval()
        {
            int factor = Mathf.Clamp(Multipliers.EventFactor, 1, 10);
            return Mathf.Max(20f, 240f / factor);
        }

        /// <summary>
        /// The only events the multiplier is allowed to roll.
        ///
        /// An allowlist, not a blocklist: the night's event table is full of things that are not
        /// "an event" at all - scene setup, day teardown, the end-of-day bus - plus the beats that
        /// end a run. A blocklist kept missing one (Start Hazard Lights is the entity's own warning,
        /// which is how a hunt got tripped), so this names what may fire instead and everything else
        /// is left to the night. All of these are atmosphere: a scare, a mess or a visitor.
        /// </summary>
        private static readonly string[] PumpableEvents =
        {
            // Every event in the menu was read in the dump before this list was written; the note on
            // each line is what its RPC actually does, not what its name suggests.
            //
            // Phone-call events. These are the game's own event flow: NewObjective("Answer the Phone")
            // plus a scenario key, so they announce themselves properly and clear when answered.
            "Rat Infestation",              // NewObjective RatInfestation
            "Roach Infestation",            // NewObjective RoachInfestation
            "Suspicious Customer",          // NewObjective SuspiciousCustomer

            // Spawns. Both routes end in CurrentDayManager.Rpc_SpawnTruck / SpawnNuisanceNPC.
            "Nuisance Event",               // StoreManager.SpawnNuisanceNPC
            "Scary Truck",                  // -> Rpc_SpawnTruck
            "Spawn Truck",                  // GameObject.SetActive only

            // Pure GameObject.SetActive: a prop or a figure switched on. No objective, no spawn, no
            // side effects of any kind - these are the safest things in the whole table.
            "Enable Ambulance",
            "Bathroom Guy",
            "Creepy Painting",
            "Anomaly Lens",
            "Doppel Drinking Petrol",
            "Dentist In Storage",
            "Dentist Near Dumpster",
            "Dentist On Roof",
            "Dentist On Bus",
            "Crawling Dentist"              // SetActive, self-gated on the Epilepsy pref
        };

        // Deliberately left out, with the reason each was checked against:
        //
        //   Flickering Lights   calls StoreManager.AllBrowsingNPCsRunAway - it empties your store
        //   Chasing Nathan      NewObjective CarBrokenDown, sends someone after you
        //   Forest Limbs        NewObjective ForestLimbs - forest
        //   Corpse Night Call   NewObjective CorpseNight
        //   Shrine Event        reads the KilledDumpsterMonster pref - boss follow-up
        //   Blood Moon          NewObjective BloodMoonExplanation
        //   Rake Intro/Graffiti the rake lives in the forest
        //   Ten Antlers         a creature prop, left out with the other forest things
        //   Pubert              FusionNetworkManager.SpawnItem - spawns a character outright
        //   EOD Bus / Fake Bus  both call NewObjective("EOD Bus") - they retarget your objective
        //   Start Hazard Lights the entity's own warning ("Hunt Warning 2/3") - starts the hunt
        //   End Hunt            destroys every enemy, BreachCompensation, CompleteOccurrence
        //   Final Sequence      ends the run
        //   Teleport / Exit Cutscene / Destroy Tutorial Stuff / Turn On|Off * / Day 7 Setup /
        //   Disable Quota Note / Purchase Traps Hint / Disable Dumpster Monster
        //                       scene setup, teardown and hints - not events at all

        private static bool IsPumpable(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < PumpableEvents.Length; i++)
                if (string.Equals(name, PumpableEvents[i], StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>
        /// Makes extra events actually happen.
        ///
        /// The old multiplier padded CurrentDayManager.listOfOccurrences, which grew the list and
        /// changed nothing - across a whole night StoreManager.TriggerNextEvent was never called once,
        /// so whatever that list is, it is not what fires the night's events.
        ///
        /// The menu's own event list is used instead - those entries demonstrably do fire - narrowed
        /// to the atmospheric ones named above. Nothing here starts a hunt, ends a day or advances the
        /// story; the night still does all of that itself.
        /// </summary>
        private void PumpEvents()
        {
            List<EventEntry> all = Events;
            if (all == null || all.Count == 0) return;

            var safe = new List<EventEntry>();
            for (int i = 0; i < all.Count; i++)
            {
                EventEntry e = all[i];
                if (e == null || e.Fire == null) continue;
                if (!IsPumpable(e.Name)) continue;
                safe.Add(e);
            }

            if (safe.Count == 0)
            {
                if (!_noSafeEventLogged) { _noSafeEventLogged = true; Log.Warn("Event multiplier: no safe events to fire."); }
                return;
            }

            EventEntry pick = safe[UnityEngine.Random.Range(0, safe.Count)];
            try
            {
                pick.Fire();
                EventsPumped++;
                LastResult = "Event fired: " + pick.Name;
                Log.Msg("Event multiplier fired " + pick.Name + " (" + EventsPumped +
                        " tonight, one every " + EventInterval().ToString("0") + "s).");
            }
            catch (Exception ex) { Log.Debug("pump event " + pick.Name + ": " + ex.Message); }
        }

        private bool _noSafeEventLogged;

        private float _nextNuisanceTopUp;
        private float _nextDoppelCheck;
        private int _doppelsSentTonight;

        /// <summary>
        /// Holds a few nuisance customers in the store, the same way the ambient shoppers are topped
        /// up: SpawnNuisanceNPC is the game's own call and currentNuisanceNPCs is its own tally, so
        /// this only ever asks for more of what the night was already going to produce.
        /// </summary>
        private void TopUpNuisances()
        {
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) return;

            try
            {
                if (!sm.allowedToSpawnBrowsingNPCs) return;

                int pool = sm.allNuisanceNpcs != null ? sm.allNuisanceNpcs.Count : 0;
                if (pool <= 0) return;

                int current = sm.currentNuisanceNPCs != null ? sm.currentNuisanceNPCs.Count : 0;
                int want = Math.Min(pool, Math.Max(1, NuisanceMultiplier));
                for (int i = current; i < want; i++)
                {
                    sm.SpawnNuisanceNPC();
                    NuisancesSpawned++;
                }
                if (want > current)
                    Log.Debug("Nuisance top-up: " + current + " -> " + want + ".");
            }
            catch (Exception ex) { Log.Debug("nuisance top-up: " + ex.Message); }
        }

        /// <summary>
        /// Sends extra doppelgangers in on top of whatever the night scheduled.
        ///
        /// They are ordinary Npc assets flagged isDoppelganger, spawned through the same call the
        /// scheduler uses, so they behave exactly like the ones the game sends - they just arrive
        /// because you asked for them. The count is per night, not per check, so the store does not
        /// slowly fill up with them.
        /// </summary>
        private void TopUpDoppelgangers()
        {
            int want = Math.Max(1, DoppelgangerCount);
            if (_doppelsSentTonight >= want) return;

            List<NpcAsset> all = NpcAssets;
            var doppels = new List<NpcAsset>();
            for (int i = 0; i < all.Count; i++)
                if (all[i] != null && all[i].IsDoppelganger && Net.Alive(all[i].Asset)) doppels.Add(all[i]);

            if (doppels.Count == 0)
            {
                if (!_noDoppelLogged) { _noDoppelLogged = true; Log.Warn("Doppelganger multiplier: no doppelganger assets found."); }
                return;
            }

            NpcAsset pick = doppels[UnityEngine.Random.Range(0, doppels.Count)];
            SpawnNpcAsset(pick);
            _doppelsSentTonight++;
            DoppelsSpawned++;
            Log.Msg("Extra doppelganger sent in: " + pick.Name + " (" + _doppelsSentTonight + " of " + want + " tonight).");
        }

        private bool _noDoppelLogged;

        private void TopUpCustomers()
        {
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) return;

            try
            {
                if (!sm.allowedToSpawnBrowsingNPCs) return;

                int current = sm.currentBrowsingNPCs != null ? sm.currentBrowsingNPCs.Count : 0;
                int pool = sm.allBrowsingNpcs != null ? sm.allBrowsingNpcs.Count : 0;
                if (pool <= 0) return;

                // Aim for multiplier x the base ambient level, capped by the available NPC pool.
                int want = Math.Min(pool, Math.Max(1, CustomerMultiplier) * 2);
                for (int i = current; i < want; i++) sm.SpawnBrowsingNPC();
            }
            catch (Exception ex) { Log.Debug("customer top-up: " + ex.Message); }
        }

        /// <summary>
        /// The emergency arsenal normally only comes up part-way into a hunt, gated by
        /// HuntManager.alreadyEnabledWeaponsArsenal. Setting that flag and switching the racks on
        /// directly makes the wall usable during the day, with no entity to fight for it.
        /// </summary>
        internal void OpenWeaponWall(bool announce)
        {
            int opened = 0;

            try
            {
                HuntManager hm = HuntManager.Instance;
                if (Net.Alive(hm)) hm.alreadyEnabledWeaponsArsenal = true;
            }
            catch (Exception ex) { Log.Debug("hunt arsenal flag: " + ex.Message); }

            // The wall is covered by HuntPanel, and the hunt is what normally reveals it - which is
            // why setting the arsenal flags alone left the door shut outside a hunt.
            try
            {
                HuntPanel hp = HuntPanel.Instance;
                if (Net.Alive(hp) && !_panelRevealed)
                {
                    _panelRevealed = true;
                    Rpc.Call(hp, delegate { hp.Rpc_RevealPanel(); }, "RevealPanel");
                    opened++;
                }
            }
            catch (Exception ex) { Log.Debug("arsenal panel: " + ex.Message); }

            try
            {
                CurrentDayManager dm = CurrentDayManager.Instance;
                if (Net.Alive(dm))
                {
                    GameObject icon = dm.weaponsArsenalIcon;
                    if (icon != null && !icon.activeSelf) { icon.SetActive(true); opened++; }
                }
            }
            catch (Exception ex) { Log.Debug("arsenal icon: " + ex.Message); }

            List<GunCase> cases = GunCases();
            for (int i = 0; i < cases.Count; i++)
            {
                GunCase gc = cases[i];
                if (!Net.Alive(gc)) continue;
                try
                {
                    GameObject go = gc.gameObject;
                    if (go == null || !go.scene.IsValid()) continue;

                    if (!go.activeSelf) { go.SetActive(true); opened++; }
                    // The rack usually hangs off a parent that gets switched on with the arsenal.
                    Transform parent = go.transform.parent;
                    if (parent != null && parent.gameObject != null && !parent.gameObject.activeSelf)
                    {
                        parent.gameObject.SetActive(true);
                        opened++;
                    }

                    if (!gc.interactable)
                    {
                        gc.interactable = true;
                        try { gc.ChangeInteractableStatusLocalOnly(true); } catch { }
                        try { if (Net.IsHost) gc.ChangeInteractableStatus(true); } catch { }
                        opened++;
                    }
                    gc.canPickupItem = true;
                    gc.allowEveryoneToHave = true;
                }
                catch { }
            }

            if (announce || opened > 0)
            {
                LastResult = "Weapon wall open (" + cases.Count + " rack(s), " + opened + " change(s))";
                if (announce || opened > 0) Log.Msg(LastResult + ".");
            }
        }

        /// <summary>
        /// Marks every weapon (and store upgrade) node as purchased on the save and pushes it into
        /// the live arsenal, so the emergency wall is fully stocked rather than just openable.
        /// </summary>
        internal void UnlockArsenal()
        {
            SaveManager sm = null;
            try { sm = SaveManager.Instance; } catch { }
            if (!Net.Alive(sm)) { LastResult = "SaveManager not ready"; Log.Warn("Arsenal unlock: SaveManager not ready."); return; }

            int weaponNodes = 0, upgradeNodes = 0;
            try
            {
                PurchaseManager pm = PurchaseManager.Instance;
                if (Net.Alive(pm))
                {
                    if (pm.weaponNodesUnlimited != null) weaponNodes += pm.weaponNodesUnlimited.Length;
                    if (pm.weaponNodesLimited != null) weaponNodes += pm.weaponNodesLimited.Length;
                    if (pm.storeUpgradeNodes != null) upgradeNodes = pm.storeUpgradeNodes.Length;
                }
            }
            catch (Exception ex) { Log.Debug("purchase nodes: " + ex.Message); }

            // PurchaseManager only exists in the store scene; be generous when it is not up yet.
            if (weaponNodes <= 0) weaponNodes = 32;
            if (upgradeNodes <= 0) upgradeNodes = 32;

            int addedWeapons = Fill(sm.weaponsPurchased, weaponNodes, "weapons");
            int addedUpgrades = Fill(sm.storeUpgradesPurchased, upgradeNodes, "store upgrades");

            // Push the save state into the live arsenal.
            try { sm.SetStoreUpgradesAndWeaponsArsenal(); }
            catch (Exception ex)
            {
                Log.Debug("SetStoreUpgradesAndWeaponsArsenal: " + ex.Message);
                try { sm.Rpc_CMD_SetStoreUpgradesAndWeaponsArsenal(); } catch { }
            }
            try { sm.Rpc_GetLockedAndLoaded(); } catch (Exception ex) { Log.Debug("Rpc_GetLockedAndLoaded: " + ex.Message); }
            try { sm.Save(); } catch { }

            OpenWeaponWall(false);

            LastResult = "Arsenal unlocked (+" + addedWeapons + " weapons, +" + addedUpgrades + " upgrades)";
            Log.Msg(LastResult + ".");
        }

        private static int Fill(Il2CppSystem.Collections.Generic.List<int> list, int count, string what)
        {
            if (list == null) { Log.Debug("arsenal: " + what + " list is null."); return 0; }
            int added = 0;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    if (list.Contains(i)) continue;
                    list.Add(i);
                    added++;
                }
            }
            catch (Exception ex) { Log.Debug("arsenal fill " + what + ": " + ex.Message); }
            return added;
        }

        /// <summary>
        /// Keeps the rating inputs perfect. The review text itself is rewritten by the Harmony
        /// patch; this clears the penalties that would otherwise pull the star average down.
        /// </summary>
        /// <summary>Cleared per scene so a fresh store gets one repaint even if nothing was wrong.</summary>
        private bool _reviewPainted;

        private void KeepReviewsPerfect()
        {
            try
            {
                ReviewsManager rm = ReviewsManager.Instance;
                if (!Net.Alive(rm)) return;

                bool changed = false;
                if (rm.hygienePenalty != 0f) { rm.hygienePenalty = 0f; changed = true; }

                // stockPenalty is deliberately NOT written here. Zero does not mean "no penalty" for
                // that field - it counts -1 per item sitting on a shelf, so zero reads as a completely
                // empty store and pinned the stock rating to 0%. ComfortModule owns it and publishes
                // minus the real product count instead.

                float targetDecor = 0f;
                try { targetDecor = rm.GetTargDecorPoints(); } catch { }
                if (targetDecor > 0f && rm.decorPoints < targetDecor) { rm.decorPoints = targetDecor; changed = true; }

                if (rm.overallRating < 5f) { rm.overallRating = 5f; changed = true; }

                // The store's mood signs and the review bars are only repainted inside
                // UpdateReviewUI, which the game calls from its own Update*Penalty methods. Writing
                // the fields directly - which is all this did before - left an angry "shelves are
                // empty" face hanging over a store that was fully stocked. Repaint when something was
                // actually wrong, and once after a load, rather than every second: UpdateReviewUI
                // rewrites every bar, label and font on the board.
                if (changed || !_reviewPainted)
                {
                    _reviewPainted = true;
                    try { rm.UpdateReviewUI(); } catch (Exception ex) { Log.Debug("review UI: " + ex.Message); }
                }
            }
            catch (Exception ex) { Log.Debug("perfect reviews: " + ex.Message); }

            try
            {
                StoreManager sm = StoreManager.Instance;
                if (Net.Alive(sm) && sm.storeRating < 5f) sm.storeRating = 5f;
            }
            catch { }
        }

        // ---------------------------------------------------------------- forced reading

        /// <summary>Marks the current store objective complete, for the look-at style gates.</summary>
        internal void FinishObjectiveNow()
        {
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) { LastResult = "StoreManager not ready"; return; }
            try
            {
                sm.FinishObjective();
                LastResult = "Objective finished";
                Log.Msg("Objective force-finished.");
            }
            catch (Exception ex) { Log.Ex("finish objective", ex); LastResult = "Could not finish objective"; }
        }

        // ---------------------------------------------------------------- diagnostics

        /// <summary>
        /// Dumps both spawn prefab arrays side by side to the log.
        ///
        /// StoreManager keeps pickupObjs and thrownObjs in parallel, and which one a given spawn
        /// path uses decides whether a grenade arrives inert or already armed. This prints what is
        /// actually in each slot so that is a fact rather than an assumption.
        /// </summary>
        internal void DumpItemCatalogue()
        {
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) { LastResult = "StoreManager not ready"; Log.Warn("Catalogue dump: StoreManager not ready."); return; }

            int pickups = 0, thrown = 0;
            try { if (sm.pickupObjs != null) pickups = sm.pickupObjs.Length; } catch { }
            try { if (sm.thrownObjs != null) thrown = sm.thrownObjs.Length; } catch { }

            Log.Msg("=== Item catalogue: pickupObjs=" + pickups + "  thrownObjs=" + thrown + " ===");
            int max = Math.Max(pickups, thrown);
            for (int i = 0; i < max; i++)
            {
                string pick = Describe(sm.pickupObjs, pickups, i);
                string thr = Describe(sm.thrownObjs, thrown, i);
                Log.Msg("[" + i.ToString().PadLeft(2) + "] pickup=" + pick + "   |   thrown=" + thr);
            }
            Log.Msg("=== end catalogue ===");

            LastResult = "Dumped " + max + " item slots to the log";
        }

        private static string Describe(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<GameObject> arr,
                                       int length, int index)
        {
            if (arr == null || index >= length) return "-";
            try
            {
                GameObject go = arr[index];
                if (go == null) return "(null)";

                string extra = "";
                try { if (go.GetComponent<PickupObject>() != null) extra += " +PickupObject"; } catch { }
                try { if (go.GetComponent<Explosion>() != null) extra += " +Explosion"; } catch { }
                try { if (go.GetComponent<ThrownObject>() != null) extra += " +ThrownObject"; } catch { }
                try { if (go.GetComponent<Landmine>() != null) extra += " +Landmine"; } catch { }
                try { if (go.GetComponent<BearTrap>() != null) extra += " +BearTrap"; } catch { }
                return go.name + extra;
            }
            catch { return "(error)"; }
        }

        // ---------------------------------------------------------------- unstick

        /// <summary>
        /// Force the local player back into normal control.
        ///
        /// Being at the store computer (or a stocking shelf) hands control to that screen; if a hunt
        /// starts while you are in there the handover can be left half-done and you are stuck in the
        /// screen with no way out. This walks every holder of control back to a sane state, using
        /// the game's own exit paths first so nothing is left inconsistent.
        /// </summary>
        internal void UnstickPlayer(bool announce)
        {
            int freed = 0;

            // 1. The game's own "get me out of this screen" paths.
            List<Computer> computers = Net.FindActive<Computer>();
            for (int i = 0; i < computers.Count; i++)
            {
                Computer pc = computers[i];
                if (!Net.Alive(pc)) continue;
                try
                {
                    if (!pc.interacting) continue;
                    pc.LeaveComputerImmediate();
                    freed++;
                }
                catch (Exception ex)
                {
                    Log.Debug("LeaveComputerImmediate: " + ex.Message);
                    try { pc.StopInteract(); freed++; } catch { }
                }
            }

            List<RestockShelf> shelves = Net.FindActive<RestockShelf>();
            for (int i = 0; i < shelves.Count; i++)
            {
                RestockShelf shelf = shelves[i];
                if (!Net.Alive(shelf)) continue;
                try
                {
                    if (!shelf.interacting) continue;
                    shelf.StopInteract();
                    freed++;
                }
                catch (Exception ex) { Log.Debug("RestockShelf.StopInteract: " + ex.Message); }
            }

            // 2. Leave any cutscene state that never got exited.
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (Net.Alive(sm))
                {
                    if (Net.IsHost) sm.ExitCutscene();
                    else sm.Rpc_CMD_ExitCutscene();
                }
            }
            catch (Exception ex) { Log.Debug("ExitCutscene: " + ex.Message); }

            // 3. Release the direct control locks.
            PlayerManager pm = Net.LocalPlayer;
            if (Net.Alive(pm))
            {
                try
                {
                    pm.paused = false;
                    pm.canPause = true;
                    pm.dontAllowLockCursor = false;
                    pm.lookingAtComputer = false;
                    pm.lookingAtShelf = false;
                }
                catch (Exception ex) { Log.Debug("player flags: " + ex.Message); }

                try
                {
                    FPSController fps = pm.fpsScript;
                    if (Net.Alive(fps))
                    {
                        fps.lockMove = false;
                        fps.lockCam = false;
                    }
                }
                catch (Exception ex) { Log.Debug("fps locks: " + ex.Message); }
            }

            try
            {
                InventoryManager inv = Net.LocalInventory;
                if (Net.Alive(inv)) inv.UnpauseInventory();
            }
            catch (Exception ex) { Log.Debug("UnpauseInventory: " + ex.Message); }

            LastResult = "Unstuck (" + freed + " screen(s) closed)";
            if (announce || freed > 0) Log.Msg("Unstick: closed " + freed + " screen(s) and released control locks.");
        }

        /// <summary>True when the local player is inside a screen that takes control away.</summary>
        internal bool IsInBlockingScreen()
        {
            try
            {
                List<Computer> computers = Net.FindActive<Computer>();
                for (int i = 0; i < computers.Count; i++)
                    if (Net.Alive(computers[i]) && computers[i].interacting) return true;

                List<RestockShelf> shelves = Net.FindActive<RestockShelf>();
                for (int i = 0; i < shelves.Count; i++)
                    if (Net.Alive(shelves[i]) && shelves[i].interacting) return true;
            }
            catch { }
            return false;
        }

        // ---------------------------------------------------------------- hunt

        /// <summary>
        /// Sends every hunt entity currently in the world away. Uses the game's own Leave() rather
        /// than destroying networked objects, so nothing is left half-torn-down.
        /// </summary>
        internal void ClearHuntEntities()
        {
            int sent = 0;

            try
            {
                HuntManager hm = HuntManager.Instance;
                if (Net.Alive(hm) && hm.allEnemies != null)
                {
                    for (int i = 0; i < hm.allEnemies.Count; i++)
                    {
                        Enemy e = hm.allEnemies[i];
                        if (!Net.Alive(e)) continue;
                        try { if (!e.leaving) { e.Leave(); sent++; } }
                        catch (Exception ex) { Log.Debug("Enemy.Leave: " + ex.Message); }
                    }
                }
            }
            catch (Exception ex) { Log.Debug("hunt roster: " + ex.Message); }

            // Anything not on the roster (spawned by an event rather than the hunt) still counts.
            List<Enemy> loose = Net.FindActive<Enemy>();
            for (int i = 0; i < loose.Count; i++)
            {
                Enemy e = loose[i];
                if (!Net.Alive(e)) continue;
                try
                {
                    GameObject go = e.gameObject;
                    if (go == null || !go.activeInHierarchy || !go.scene.IsValid()) continue;
                    if (e.leaving) continue;
                    e.Leave();
                    sent++;
                }
                catch { }
            }

            LastResult = "Sent " + sent + " entit" + (sent == 1 ? "y" : "ies") + " away";
            Log.Msg(LastResult + ".");
        }

        /// <summary>
        /// Escape hatch: an empty hunt can in principle sit waiting on enemies that were never
        /// spawned, so there is an explicit way to end one.
        /// </summary>
        internal void ForceEndHunt()
        {
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) { LastResult = "StoreManager not ready"; return; }

            try
            {
                if (Net.IsHost) sm.EndHunt();
                else sm.Rpc_CMD_EndHunt();
                LastResult = "Hunt ended";
                Log.Msg("Hunt force-ended.");
            }
            catch (Exception ex)
            {
                Log.Ex("force end hunt", ex);
                LastResult = "Could not end hunt";
            }
        }

        // ---------------------------------------------------------------- cosmetics

        internal void UnlockHats()
        {
            SaveManager sm = null;
            try { sm = SaveManager.Instance; } catch { }
            if (!Net.Alive(sm)) { LastResult = "SaveManager not ready"; Log.Warn("Hat unlock: SaveManager not ready."); return; }

            int total = 0;
            try
            {
                HatsRenderer hr = HatsRenderer.Instance;
                if (Net.Alive(hr) && hr.hatObjs != null) total = hr.hatObjs.Length;
            }
            catch { }
            if (total <= 0) total = 64;   // HatsRenderer only exists in the store scene; be generous.

            int added = 0;
            try
            {
                var unlocked = sm.customizablesUnlocked;
                if (unlocked == null) { LastResult = "No customizables list"; return; }
                for (int i = 0; i < total; i++)
                {
                    if (unlocked.Contains(i)) continue;
                    unlocked.Add(i);
                    added++;
                }
            }
            catch (Exception ex) { Log.Ex("hat unlock", ex); LastResult = "Hat unlock failed"; return; }

            try { sm.Save(); } catch (Exception ex) { Log.Debug("save after hat unlock: " + ex.Message); }

            LastResult = "Unlocked " + added + " new cosmetic(s) of " + total;
            Log.Msg(LastResult + ".");
        }

        internal void OnSceneChanged()
        {
            // The night that was ending is gone; the next one is held open again.
            DayEndBlock.EndingInProgress = false;
            _doppelsSentTonight = 0;
            _nextEventPump = 0f;
            EventsPumped = 0;
            _noSafeEventLogged = false;
            _noDoppelLogged = false;
            _nextNuisanceTopUp = 0f;
            _nextDoppelCheck = 0f;
            _events.Clear();
            _atlasCount = -1;
            _gunCases.Clear();
            _nextGunCaseRefresh = 0f;
            _panelRevealed = false;
            _reviewPainted = false;
            _npcChoices.Clear();
            _npcChoiceCount = -1;
            _monsters.Clear();
            _monsterCount = -1;
            _npcAssets.Clear();
            _nextNpcAssetScan = 0f;
            _pets.Clear();
            _petCount = -1;
            _setPieces.Clear();
            _gunCasesPatched = 0;
        }
    }
}
