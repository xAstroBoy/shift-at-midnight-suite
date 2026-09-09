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

        private const float ForgetAfter = 30f;  // drop bookkeeping for an object nothing has seen since

        /// <summary>What is known about one object we have looked at. One lookup instead of two.</summary>
        private sealed class Track
        {
            internal float FirstSeen;
            internal float LastSeen;
            internal float Attempted;
        }

        private float _nextScan;
        private int _phase;
        private readonly Dictionary<int, Track> _tracked = new Dictionary<int, Track>();
        private readonly List<int> _expired = new List<int>();

        internal int CleanedSpills;
        internal int CleanedMoppables;
        internal int CleanedTrash;

        internal bool AnyActive { get { return CleanSpills || CleanMoppables || CleanTrash; } }

        internal void OnSceneChanged()
        {
            _tracked.Clear();
            _expired.Clear();
            _credited.Clear();
            _phase = 0;
        }

        internal void Tick()
        {
            if (!AnyActive) return;
            float now = Time.unscaledTime;
            if (now < _nextScan) return;
            _nextScan = now + ScanInterval;

            // One type per pass, rotating.
            //
            // All three used to be swept in the same tick, which meant three FindObjectsOfType calls
            // - each one walking the scene - landing in a single frame, twice a second. On a quiet
            // night that is invisible; on a night with a lot of limbs on the floor the Trash sweep
            // alone is large, and putting it in the same frame as the other two is what turned a cost
            // into a spike. Rotating keeps the same throughput and spreads it out.
            int budget = MaxPerPass;
            for (int attempt = 0; attempt < 3; attempt++)
            {
                _phase = (_phase + 1) % 3;
                if (_phase == 0 && CleanSpills) { SweepSpills(now, budget); break; }
                if (_phase == 1 && CleanMoppables) { SweepMoppables(now, budget); break; }
                if (_phase == 2 && CleanTrash) { SweepTrash(now, budget); break; }
            }

            Prune(now);
        }

        private void SweepSpills(float now, int budget)
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
                if (!Eligible(id, now) || budget <= 0) continue;

                if (Invoke(s.Rpc_CMD_Clean, s.RequestClean, "Spill"))
                {
                    MarkAttempted(id, now);
                    CleanedSpills++;
                    budget--;
                }
            }
        }

        private void SweepMoppables(float now, int budget)
        {
            List<Moppable> all = Net.FindActive<Moppable>();
            for (int i = 0; i < all.Count; i++)
            {
                Moppable m = all[i];
                if (!IsLiveInScene(m)) continue;

                int id = IdOf(m);
                if (id == 0) continue;
                if (!Eligible(id, now) || budget <= 0) continue;

                // Whether this mess is a roach has to be read before it is cleaned - cleaning is what
                // tears the object down, and the field is gone with it.
                bool isRoach = false;
                try { isRoach = m.roach; } catch { }

                if (Invoke(m.Rpc_CMD_Clean, m.RequestClean, "Moppable"))
                {
                    MarkAttempted(id, now);
                    CleanedMoppables++;
                    budget--;
                    if (isRoach) CreditRoach(id);
                }
            }
        }

        /// <summary>Loose junk on the floor - limbs, dropped trash - goes through Interactable pickup.</summary>
        private void SweepTrash(float now, int budget)
        {
            List<Trash> all = Net.FindActive<Trash>();
            for (int i = 0; i < all.Count; i++)
            {
                Trash t = all[i];
                if (!IsLiveInScene(t)) continue;

                int id = IdOf(t);
                if (id == 0) continue;

                // Eligible first, then the budget, then the field reads. On a night with a lot of
                // limbs almost every object here is one the last pass already handled, and reading
                // dontCountTowardHygiene and the object name on each of them is an interop call apiece
                // for an answer that changes nothing.
                if (!Eligible(id, now) || budget <= 0) continue;

                try { if (t.dontCountTowardHygiene) continue; }
                catch { }

                bool isRat = false;
                try { isRat = t.gameObject.name.StartsWith("Rat", StringComparison.OrdinalIgnoreCase); }
                catch { }

                bool ok = false;
                try { t.Delete(); ok = true; }
                catch (Exception ex) { Log.Debug("Trash.Delete failed: " + ex.Message); }

                if (ok)
                {
                    MarkAttempted(id, now);
                    CleanedTrash++;
                    budget--;
                    if (isRat) CreditRat(id);
                }
            }
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

        /// <summary>
        /// What a roach actually is.
        ///
        /// Moppable carries a "roach" flag, which is why the crediting was hung off it, but the
        /// infestation is not being picked up - so either the roach is not a Moppable at all, or it is
        /// one the sweep is refusing. This prints the countdown's own state, every cleanable in the
        /// scene, and every object whose name mentions a roach along with the components on it. One of
        /// those three answers it outright.
        /// </summary>
        internal void DumpRoachState()
        {
            try
            {
                List<RoachCountdown> counters = Net.FindActive<RoachCountdown>();
                for (int i = 0; i < counters.Count; i++)
                {
                    RoachCountdown c = counters[i];
                    if (!Net.Alive(c)) continue;
                    try
                    {
                        Log.Msg("ROACH COUNTDOWN: cur=" + c.curRats + " max=" + c.maxRats +
                                 " secondsLeft=" + c.secondsRemaining +
                                " gotObjective=" + c.gotObjective);
                    }
                    catch (Exception ex) { Log.Debug("roach countdown: " + ex.Message); }
                }
                if (counters.Count == 0) Log.Msg("ROACH COUNTDOWN: none active.");

                List<Moppable> mops = Net.FindActive<Moppable>();
                int roachMops = 0;
                for (int i = 0; i < mops.Count; i++)
                {
                    Moppable m = mops[i];
                    if (!Net.Alive(m)) continue;
                    bool isRoach = false;
                    try { isRoach = m.roach; } catch { }
                    if (isRoach) roachMops++;
                    if (i < 15)
                    {
                        string nm = "?";
                        try { nm = m.gameObject.name + " active=" + m.gameObject.activeInHierarchy; } catch { }
                        Log.Msg("   MOPPABLE " + nm + " roach=" + isRoach);
                    }
                }
                Log.Msg("CLEANABLES: spills=" + Net.FindActive<Spill>().Count +
                        " moppables=" + mops.Count + " (roach-flagged " + roachMops + ")" +
                        " trash=" + Net.FindActive<Trash>().Count);

                // Nothing in the scene is called "roach" - only the countdown UI and the spawn points
                // are - so the spawned insects are named after their prefab. EventManager holds that
                // prefab, which gives both the name to hunt for and, from the prefab itself, the
                // components that say what a roach even is.
                string roachName = DumpPrefab("roach");
                string ratName = DumpPrefab("rat");

                HuntFor(roachName);
                HuntFor(ratName);

                DumpCoins();

                LastDump = "Roach and token state dumped to the log";
            }
            catch (Exception ex) { Log.Ex("dump roach state", ex); LastDump = "Dump failed"; }
        }

        internal string LastDump = "";

        /// <summary>
        /// Prints the components on EventManager's roach or rat prefab and returns its name. The
        /// prefab answers "what is a roach" without needing to catch one in the scene first.
        /// </summary>
        private static string DumpPrefab(string which)
        {
            try
            {
                EventManager em = EventManager.Instance;
                if (!Net.Alive(em)) { Log.Msg("   EventManager not ready."); return null; }

                GameObject prefab = string.Equals(which, "roach", StringComparison.Ordinal) ? em.roach : em.rat;
                if (prefab == null) { Log.Msg("   EventManager." + which + " is null."); return null; }

                string name = prefab.name;
                string comps = "";
                try
                {
                    Component[] parts = prefab.GetComponentsInChildren<Component>(true);
                    for (int k = 0; k < parts.Length; k++)
                        if (parts[k] != null) comps += " " + parts[k].GetIl2CppType().Name;
                }
                catch { }
                Log.Msg("   PREFAB " + which + " = \"" + name + "\" |" + comps);
                return name;
            }
            catch (Exception ex) { Log.Debug("dump prefab " + which + ": " + ex.Message); return null; }
        }

        /// <summary>
        /// What a token lying on the floor is, for the auto-harvest.
        ///
        /// StoreManager keeps a coin prefab and an allCoins list, but nothing in the field names says
        /// how one gets picked up - trigger, interactable, or something the player script owns. Same
        /// question as the roach and the same answer: print the prefab's components and a live one,
        /// and then it is known rather than guessed at.
        /// </summary>
        private static void DumpCoins()
        {
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (!Net.Alive(sm)) { Log.Msg("   StoreManager not ready for the coin dump."); return; }

                GameObject prefab = null;
                try { prefab = sm.coin; } catch { }
                if (prefab == null) { Log.Msg("   StoreManager.coin is null."); return; }

                string comps = "";
                try
                {
                    Component[] parts = prefab.GetComponentsInChildren<Component>(true);
                    for (int k = 0; k < parts.Length; k++)
                        if (parts[k] != null) comps += " " + parts[k].GetIl2CppType().Name;
                }
                catch { }
                Log.Msg("   PREFAB coin = \"" + prefab.name + "\" |" + comps);

                HuntFor(prefab.name);
            }
            catch (Exception ex) { Log.Debug("dump coins: " + ex.Message); }
        }

        /// <summary>Finds live instances of a prefab by name and prints what they carry.</summary>
        private static void HuntFor(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName)) return;
            try
            {
                List<Transform> all = Net.FindActive<Transform>();
                int shown = 0;
                for (int i = 0; i < all.Count && shown < 6; i++)
                {
                    Transform t = all[i];
                    if (!Net.Alive(t)) continue;
                    string n;
                    try { n = t.gameObject.name; } catch { continue; }
                    if (string.IsNullOrEmpty(n) ||
                        n.IndexOf(prefabName, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    shown++;

                    string comps = "";
                    try
                    {
                        Component[] parts = t.GetComponents<Component>();
                        for (int k = 0; k < parts.Length; k++)
                            if (parts[k] != null) comps += " " + parts[k].GetIl2CppType().Name;
                    }
                    catch { }
                    Log.Msg("   LIVE \"" + n + "\" active=" + t.gameObject.activeInHierarchy +
                            " layer=" + LayerMask.LayerToName(t.gameObject.layer) + " |" + comps);
                }
                Log.Msg("   " + shown + " live object(s) matching \"" + prefabName + "\".");
            }
            catch (Exception ex) { Log.Debug("hunt " + prefabName + ": " + ex.Message); }
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

        /// <summary>
        /// Notes that this object still exists and says whether it is worth acting on. Marking it
        /// seen is what keeps <see cref="Prune"/> from forgetting it, so this must be called for
        /// every object in the sweep, not only the ones being cleaned.
        /// </summary>
        private bool Eligible(int id, float now)
        {
            Track t;
            if (!_tracked.TryGetValue(id, out t))
            {
                _tracked[id] = new Track { FirstSeen = now, LastSeen = now };
                return false;               // seen for the first time - let it settle a tick
            }

            t.LastSeen = now;
            if (now - t.FirstSeen < MinAge) return false;
            if (t.Attempted > 0f && now - t.Attempted < RetryAfter) return false;
            return true;
        }

        /// <summary>Records that a clean was just asked for, so the retry timer starts.</summary>
        private void MarkAttempted(int id, float now)
        {
            Track t;
            if (_tracked.TryGetValue(id, out t)) t.Attempted = now;
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

        /// <summary>
        /// Forgets objects nothing has seen for a while.
        ///
        /// This used to drop anything missing from the current pass, which only worked while all
        /// three types were swept together - now that they rotate, a Spill would be forgotten on
        /// every Trash pass and immediately re-enter its settling period, so nothing would ever be
        /// cleaned. Age is the right test: a live object is re-seen every pass through the rotation,
        /// well inside the window, and a destroyed one simply stops being seen.
        /// </summary>
        private void Prune(float now)
        {
            if (_tracked.Count == 0) return;

            _expired.Clear();
            foreach (KeyValuePair<int, Track> kv in _tracked)
                if (now - kv.Value.LastSeen > ForgetAfter) _expired.Add(kv.Key);

            for (int i = 0; i < _expired.Count; i++) _tracked.Remove(_expired[i]);
            _expired.Clear();
        }
    }
}
