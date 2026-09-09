using System;
using System.Collections.Generic;
using Il2Cpp;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// Automatic cleaning.
    ///
    /// The old auto-mop only touched objects literally named "BloodSpill(Clone)" and tagged "Mess",
    /// which is why most of the mess on the floor was left behind: bathroom blots, vomit, oil and
    /// every other <see cref="Moppable"/> use a different prefab (and Moppable is a completely
    /// separate component from Spill). This cleans every Spill and every Moppable, whatever it is
    /// called, plus loose Trash.
    ///
    /// A cleaned object is remembered only for a short while - if the RPC silently did nothing the
    /// object becomes eligible again instead of being blacklisted for the rest of the night.
    /// </summary>
    internal sealed class CleanModule
    {
        internal bool CleanSpills;
        internal bool CleanMoppables;
        internal bool CleanTrash;

        private const float ScanInterval = 0.5f;
        private const float RetryAfter = 3f;   // re-attempt an object that survived its clean call
        private const float MinAge = 0.2f;     // let a fresh spill finish spawning first
        private const int MaxPerPass = 40;

        private float _nextScan;
        private readonly Dictionary<int, float> _firstSeen = new Dictionary<int, float>();
        private readonly Dictionary<int, float> _attempted = new Dictionary<int, float>();

        internal int CleanedSpills;
        internal int CleanedMoppables;
        internal int CleanedTrash;

        internal bool AnyActive { get { return CleanSpills || CleanMoppables || CleanTrash; } }

        internal void OnSceneChanged()
        {
            _firstSeen.Clear();
            _attempted.Clear();
            _credited.Clear();
        }

        internal void Tick()
        {
            if (!AnyActive) return;
            float now = Time.unscaledTime;
            if (now < _nextScan) return;
            _nextScan = now + ScanInterval;

            var seen = new HashSet<int>();
            int budget = MaxPerPass;

            if (CleanSpills) budget = SweepSpills(now, seen, budget);
            if (CleanMoppables) budget = SweepMoppables(now, seen, budget);
            if (CleanTrash) budget = SweepTrash(now, seen, budget);

            Prune(seen);
        }

        private int SweepSpills(float now, HashSet<int> seen, int budget)
        {
            List<Spill> all = Net.FindActive<Spill>();
            for (int i = 0; i < all.Count; i++)
            {
                Spill s = all[i];
                if (!IsLiveInScene(s)) continue;

                bool cleaned, cantClean;
                try { cleaned = s.cleaned; cantClean = s.cantClean; }
                catch { continue; }
                if (cleaned || cantClean) continue;

                int id = IdOf(s);
                if (id == 0) continue;
                seen.Add(id);
                if (budget <= 0 || !Eligible(id, now)) continue;

                if (Invoke(s.Rpc_CMD_Clean, s.RequestClean, "Spill"))
                {
                    _attempted[id] = now;
                    CleanedSpills++;
                    budget--;
                }
            }
            return budget;
        }

        private int SweepMoppables(float now, HashSet<int> seen, int budget)
        {
            List<Moppable> all = Net.FindActive<Moppable>();
            for (int i = 0; i < all.Count; i++)
            {
                Moppable m = all[i];
                if (!IsLiveInScene(m)) continue;

                int id = IdOf(m);
                if (id == 0) continue;
                seen.Add(id);
                if (budget <= 0 || !Eligible(id, now)) continue;

                // Whether this mess is a roach has to be read before it is cleaned - cleaning is what
                // tears the object down, and the field is gone with it.
                bool isRoach = false;
                try { isRoach = m.roach; } catch { }

                if (Invoke(m.Rpc_CMD_Clean, m.RequestClean, "Moppable"))
                {
                    _attempted[id] = now;
                    CleanedMoppables++;
                    budget--;
                    if (isRoach) CreditRoach(id);
                }
            }
            return budget;
        }

        /// <summary>Loose junk on the floor - limbs, dropped trash - goes through Interactable pickup.</summary>
        private int SweepTrash(float now, HashSet<int> seen, int budget)
        {
            List<Trash> all = Net.FindActive<Trash>();
            for (int i = 0; i < all.Count; i++)
            {
                Trash t = all[i];
                if (!IsLiveInScene(t)) continue;

                try { if (t.dontCountTowardHygiene) continue; }
                catch { }

                int id = IdOf(t);
                if (id == 0) continue;
                seen.Add(id);
                if (budget <= 0 || !Eligible(id, now)) continue;

                bool isRat = false;
                try { isRat = t.gameObject.name.StartsWith("Rat", StringComparison.OrdinalIgnoreCase); }
                catch { }

                bool ok = false;
                try { t.Delete(); ok = true; }
                catch (Exception ex) { Log.Debug("Trash.Delete failed: " + ex.Message); }

                if (ok)
                {
                    _attempted[id] = now;
                    CleanedTrash++;
                    budget--;
                    if (isRat) CreditRat(id);
                }
            }
            return budget;
        }

        // ---------------------------------------------------------------- event objectives

        /// <summary>
        /// Objects already credited to an event counter, so a retry cannot count the same roach twice.
        /// The clean is retried after 3s if the object survived it, and without this that retry would
        /// award the objective again.
        /// </summary>
        private readonly HashSet<int> _credited = new HashSet<int>();

        internal int CreditedRoaches;
        internal int CreditedRats;

        /// <summary>
        /// The roach and rat infestations are objectives, not just mess: the game counts them as they
        /// are dealt with and pays out when the count is met. Cleaning a Moppable through
        /// Rpc_CMD_Clean removes the roach but never tells the counter, which is why the objective sat
        /// unfinished while the floor came up spotless. The count is a separate call, so make it.
        /// </summary>
        private void CreditRoach(int id)
        {
            if (!_credited.Add(id)) return;
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (!Net.Alive(sm)) return;
                if (Net.IsHost) sm.Rpc_RoachKilled();
                else sm.Rpc_CMD_RoachKilled();
                CreditedRoaches++;
            }
            catch (Exception ex) { Log.Debug("credit roach: " + ex.Message); }
        }

        /// <summary>The rat infestation counterpart; the counter lives on the local inventory.</summary>
        private void CreditRat(int id)
        {
            if (!_credited.Add(id)) return;
            try
            {
                InventoryManager inv = Net.LocalInventory;
                if (!Net.Alive(inv)) return;
                if (Net.IsHost) inv.Rpc_GotARat();
                else inv.Rpc_CMD_GotARat();
                CreditedRats++;
            }
            catch (Exception ex) { Log.Debug("credit rat: " + ex.Message); }
        }

        /// <summary>Prefer the networked command; fall back to the local request path.</summary>
        private static bool Invoke(Action networked, Action local, string what)
        {
            try { networked(); return true; }
            catch (Exception ex) { Log.Debug(what + ".Rpc_CMD_Clean failed: " + ex.Message); }
            try { local(); return true; }
            catch (Exception ex) { Log.Debug(what + ".RequestClean failed: " + ex.Message); }
            return false;
        }

        private bool Eligible(int id, float now)
        {
            float first;
            if (!_firstSeen.TryGetValue(id, out first))
            {
                _firstSeen[id] = now;
                return false;               // seen for the first time - let it settle a tick
            }
            if (now - first < MinAge) return false;

            float last;
            if (_attempted.TryGetValue(id, out last) && now - last < RetryAfter) return false;
            return true;
        }

        private static bool IsLiveInScene(Component c)
        {
            if (!Net.Alive(c)) return false;
            try
            {
                GameObject go = c.gameObject;
                if (go == null || !go.activeInHierarchy) return false;
                // Prefab assets live in no scene; only clean things that are actually in the world.
                return go.scene.IsValid();
            }
            catch { return false; }
        }

        private static int IdOf(UnityEngine.Object o)
        {
            try { return o.GetInstanceID(); }
            catch { return 0; }
        }

        private void Prune(HashSet<int> seen)
        {
            if (_firstSeen.Count > 0) PruneMap(_firstSeen, seen);
            if (_attempted.Count > 0) PruneMap(_attempted, seen);
        }

        private static void PruneMap(Dictionary<int, float> map, HashSet<int> seen)
        {
            List<int> gone = null;
            foreach (KeyValuePair<int, float> kv in map)
            {
                if (seen.Contains(kv.Key)) continue;
                if (gone == null) gone = new List<int>();
                gone.Add(kv.Key);
            }
            if (gone == null) return;
            for (int i = 0; i < gone.Count; i++) map.Remove(gone[i]);
        }
    }
}
