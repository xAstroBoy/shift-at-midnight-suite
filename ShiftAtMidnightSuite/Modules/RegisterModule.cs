using System;
using System.Collections.Generic;
using Il2Cpp;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// The checkout counter.
    ///
    /// A transaction will not complete until every item the customer put down has been bagged - the
    /// game says "bag all items first" and blocks you. Each item on the counter is a
    /// <see cref="ScanItem"/>, and bagging one is <c>ScanItem.Rpc_CMD_Interact()</c>, the same call
    /// the game makes when you click it. So this presses them all rather than faking the counter.
    /// </summary>
    internal sealed class RegisterModule
    {
        /// <summary>Bag items automatically as they appear on the counter.</summary>
        internal bool AutoBag;

        /// <summary>
        /// Let the register complete a transaction with items still unbagged.
        ///
        /// Register.Interact gates on two booleans before it will call CompleteTransaction:
        ///     023 Compare [this+272], 0   -> Register.canCompleteTransaction, false = "Bag all items first!"
        ///     039 Compare [tm+264], 0     -> TransactionManager.canTransact, false = same alert
        /// Holding both true while a transaction is live lets the normal Interact path through, so
        /// the game still runs its own CompleteTransaction rather than us faking the outcome.
        ///
        /// Caveat: revenue is added per item by ItemScanned(cost), so items you never bag are not
        /// paid for. Completing early earns less than bagging everything would.
        /// </summary>
        internal bool AllowUnbagged;

        private const float ScanInterval = 0.4f;
        private const float RetryAfter = 2f;

        private float _nextScan;
        private readonly Dictionary<int, float> _attempted = new Dictionary<int, float>();

        // The register is a fixed scene object; sweeping the scene for it every 0.4s was the single
        // costliest tick in the profile. Find it once, refresh rarely.
        private List<Register> _registers = new List<Register>();
        private float _nextRegisterRefresh;

        private List<Register> Registers()
        {
            float now = Time.unscaledTime;
            if (now >= _nextRegisterRefresh || _registers.Count == 0)
            {
                _nextRegisterRefresh = now + 60f;
                _registers = Net.FindActive<Register>();
            }
            return _registers;
        }

        /// <summary>
        /// The items on the counter, straight from TransactionManager.objectsToScan - the list the
        /// game itself spawned them into. No scene scan, and it is exactly the current transaction.
        /// </summary>
        private static List<ScanItem> CounterItems()
        {
            var result = new List<ScanItem>();
            try
            {
                TransactionManager tm = TransactionManager.Instance;
                if (!Net.Alive(tm) || tm.objectsToScan == null) return result;
                int count = tm.objectsToScan.Count;
                for (int i = 0; i < count; i++)
                {
                    GameObject go = tm.objectsToScan[i];
                    if (go == null) continue;
                    ScanItem item = go.GetComponent<ScanItem>();
                    if (item == null) item = go.GetComponentInChildren<ScanItem>(true);
                    if (item != null) result.Add(item);
                }
            }
            catch (Exception ex) { Log.Debug("objectsToScan: " + ex.Message); }
            return result;
        }

        internal int Bagged;
        internal string Status = "idle";

        private List<Car> _cars = new List<Car>();
        private float _nextCarRefresh;
        private readonly Dictionary<int, float> _carSeen = new Dictionary<int, float>();
        private readonly HashSet<int> _carUnknownLogged = new HashSet<int>();

        internal int CarsSentAway;

        /// <summary>
        /// The same idea at the drive-up window: a car whose occupant is not a doppelganger is waved
        /// through with the game's own CarDone, which is what the "let them go" button calls.
        ///
        /// A car is held back while it still has an unfilled petrol order - CarDone does not care, and
        /// sending them off mid-refuel would throw the sale away. With Auto-Fuel on, that resolves
        /// itself a moment later and the car then leaves on its own.
        /// </summary>
        private void SendCarsAway(float now)
        {
            if (now >= _nextCarRefresh || _cars.Count == 0)
            {
                _nextCarRefresh = now + 5f;
                _cars = Net.FindActive<Car>();
            }

            for (int i = 0; i < _cars.Count; i++)
            {
                Car car = _cars[i];
                if (!Net.Alive(car)) continue;

                int id;
                try { id = car.GetInstanceID(); } catch { continue; }

                try
                {
                    if (car.alreadyFaded) { _carSeen.Remove(id); continue; }

                    DialogueInteractable npc = car.npc;
                    if (!Net.Alive(npc)) { _carSeen.Remove(id); continue; }

                    // Unknown counts as "leave it alone" - never wave through something unidentified.
                    bool? doppel = ReadDoppelFlag(npc);
                    if (!doppel.HasValue)
                    {
                        if (_carUnknownLogged.Add(id))
                            Log.Msg("Car occupant could not be identified (" + npc.name + "); leaving it for you.");
                        _carSeen.Remove(id);
                        continue;
                    }
                    if (doppel.Value) { _carSeen.Remove(id); continue; }

                    // Still owes for petrol; let the pump finish first.
                    PetrolTank tank = car.petrolTank;
                    if (Net.Alive(tank))
                    {
                        try { if (tank.maxMoneySpent > 0f && !tank.petrolFull) { _carSeen.Remove(id); continue; } }
                        catch { }
                    }

                    float since;
                    if (!_carSeen.TryGetValue(id, out since)) { _carSeen[id] = now; continue; }
                    if (now - since < 1.5f) continue;
                    _carSeen[id] = now + 10f;   // do not hammer it if the call does not take

                    car.CarDone();
                    CarsSentAway++;
                    Status = "car waved through";
                    Log.Msg("Car sent on its way (occupant is not a doppelganger).");
                }
                catch (Exception ex) { Log.Debug("send car away: " + ex.Message); }
            }
        }

        /// <summary>
        /// Whether this NPC is a plain customer.
        ///
        /// Two different components carry the flag: shoppers have StoreBrowseBehaviour.isDoppelganger,
        /// while the people in cars carry an Npc component instead - which is why looking only for the
        /// browse script found nothing and, failing safe, left every car for the player. Both are
        /// searched, walking up the hierarchy and then down, the same way the detector overlay does.
        ///
        /// Anything still unidentified is treated as suspicious and left alone.
        /// </summary>
        private static bool IsOrdinary(DialogueInteractable npc)
        {
            bool? verdict = ReadDoppelFlag(npc);
            if (!verdict.HasValue) return false;
            return !verdict.Value;
        }

        /// <summary>True/false if a marker was found, null when the NPC could not be identified.</summary>
        private static bool? ReadDoppelFlag(DialogueInteractable npc)
        {
            Transform t;
            try { t = npc.transform; }
            catch { return null; }

            for (int depth = 0; depth < 10 && t != null; depth++)
            {
                GameObject go;
                try { go = t.gameObject; } catch { break; }
                if (go != null)
                {
                    try
                    {
                        StoreBrowseBehaviour b = go.GetComponent<StoreBrowseBehaviour>();
                        if (b != null) return b.isDoppelganger;
                    }
                    catch { }

                    try
                    {
                        Npc n = go.GetComponent<Npc>();
                        if (n != null) return n.isDoppelganger;
                    }
                    catch { }
                }
                try { t = t.parent; } catch { break; }
            }

            // Nothing on the way up - the marker may sit on a child instead.
            try
            {
                StoreBrowseBehaviour b = npc.GetComponentInChildren<StoreBrowseBehaviour>(true);
                if (b != null) return b.isDoppelganger;
            }
            catch { }
            try
            {
                Npc n = npc.GetComponentInChildren<Npc>(true);
                if (n != null) return n.isDoppelganger;
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Closes the sale for a customer who is not a doppelganger.
        ///
        /// Held back until every item the transaction asked for is actually on the counter: the
        /// completion prefix charges for what it finds there, so completing while the spawn coroutine
        /// is still laying items out would let them walk with the rest.
        /// </summary>
        private void SelfCheckout(float now)
        {
            try
            {
                TransactionManager tm = TransactionManager.Instance;
                if (!Net.Alive(tm) || !tm.transactionInProgress) { _checkoutReadySince = 0f; return; }

                StoreBrowseBehaviour npc = tm.curNpcScript;
                if (!Net.Alive(npc)) { _checkoutReadySince = 0f; return; }

                // The whole point: these are the ones you are meant to look at.
                if (npc.isDoppelganger) { _checkoutReadySince = 0f; return; }

                int want = tm.amountOfItemsToScan;
                if (want <= 0) { _checkoutReadySince = 0f; return; }

                int onCounter = 0;
                try { if (tm.objectsToScan != null) onCounter = tm.objectsToScan.Count; }
                catch { }
                if (onCounter < want) { _checkoutReadySince = 0f; return; }

                // Let the last item settle before ringing it through.
                if (_checkoutReadySince == 0f) { _checkoutReadySince = now; return; }
                if (now - _checkoutReadySince < 0.6f) return;
                _checkoutReadySince = 0f;
            _cars.Clear();
            _carSeen.Clear();
            _carUnknownLogged.Clear();
            _nextCarRefresh = 0f;

                string who = "customer";
                try { who = npc.idDatabaseName; } catch { }

                CompleteNow();
                SelfCheckedOut++;
                Status = "self-checkout: " + who;
                Log.Msg("Self-checkout for " + who + " (" + want + " item(s)); doppelgangers are left alone.");
            }
            catch (Exception ex) { Log.Debug("self checkout: " + ex.Message); }
        }

        /// <summary>
        /// Scans - and therefore charges for - everything still on the counter, called just before the
        /// transaction completes.
        ///
        /// ScanItem.Rpc_Interact is what counts an item: it calls TransactionManager.ItemScanned,
        /// which does revenue += cost and bumps amountOfItemsScanned, and only then destroys the
        /// object. CompleteTransaction hands that accumulated revenue to StoreManager.ChangeRevenue.
        /// So anything still unscanned when the bell rings is never paid for - and deleting it
        /// afterwards, which is what the first version of the leftover sweep did, threw the money
        /// away. Scanning first means the customer is charged for every item they walked in with.
        /// </summary>
        internal void CountRemainingBeforeCompletion()
        {
            if (!ClearLeftovers && !AutoBag) return;
            try { BagAll(false, true); }
            catch (Exception ex) { Log.Debug("count before completion: " + ex.Message); }
        }

        /// <summary>
        /// Clears the counter once the customer is done with it.
        ///
        /// CancelTransaction destroys whatever is still sitting on the counter, but
        /// CompleteTransaction never does - normally you cannot complete with items left, so the game
        /// has no reason to. With unbagged checkout allowed that stops being true, and every item you
        /// waved through stays on the counter for the rest of the night.
        /// </summary>
        internal bool ClearLeftovers = true;

        /// <summary>
        /// Ordinary customers ring themselves up and leave; only doppelgangers stay at the counter.
        ///
        /// StoreBrowseBehaviour carries its own isDoppelganger flag - the same one the end-of-day
        /// report scores you against - so the two can be told apart before the transaction is closed.
        /// Completion waits until the counter is fully laid out, because revenue is banked from what
        /// has been scanned: finishing early would hand them the shopping for free.
        /// </summary>
        internal bool SelfCheckoutNormals;

        internal int SelfCheckedOut;
        private float _checkoutReadySince;

        internal int LeftoversCleared;

        /// <summary>When the counter was first seen holding items with no transaction running.</summary>
        private float _leftoverSince;

        internal void OnSceneChanged()
        {
            _attempted.Clear();
            _leftoverSince = 0f;
            _checkoutReadySince = 0f;
            _registers.Clear();
            _nextRegisterRefresh = 0f;
            Status = "idle";
        }

        internal void Tick()
        {
            if (!AutoBag && !AllowUnbagged && !ClearLeftovers && !SelfCheckoutNormals) return;
            float now = Time.unscaledTime;
            if (now < _nextScan) return;
            _nextScan = now + ScanInterval;

            if (!TransactionActive())
            {
                Status = "no transaction";
                if (ClearLeftovers) SweepLeftovers(now);
                return;
            }
            _leftoverSince = 0f;

            if (AllowUnbagged) OpenTheGate();
            if (AutoBag) BagAll(false);
            if (SelfCheckoutNormals) { SelfCheckout(now); SendCarsAway(now); }
        }

        /// <summary>Hold the two booleans Register.Interact checks before completing.</summary>
        private void OpenTheGate()
        {
            try
            {
                TransactionManager tm = TransactionManager.Instance;
                if (Net.Alive(tm) && !tm.canTransact) tm.canTransact = true;
            }
            catch (Exception ex) { Log.Debug("canTransact: " + ex.Message); }

            List<Register> registers = Registers();
            for (int i = 0; i < registers.Count; i++)
            {
                Register r = registers[i];
                if (!Net.Alive(r)) continue;
                try
                {
                    GameObject go = r.gameObject;
                    if (go == null || !go.scene.IsValid()) continue;
                    if (!r.canCompleteTransaction) r.canCompleteTransaction = true;
                }
                catch { }
            }
        }

        /// <summary>Complete the current transaction outright, bagged or not.</summary>
        internal void CompleteNow()
        {
            TransactionManager tm = null;
            try { tm = TransactionManager.Instance; } catch { }
            if (!Net.Alive(tm)) { Status = "no register"; Log.Warn("Complete transaction: TransactionManager not ready."); return; }

            OpenTheGate();
            try
            {
                if (Net.IsHost) tm.CompleteTransaction();
                else tm.Rpc_CMD_CompleteTransaction();
                Status = "transaction completed";
                Log.Msg("Transaction completed manually (unbagged items are not paid for).");
            }
            catch (Exception ex)
            {
                Log.Ex("complete transaction", ex);
                Status = "could not complete";
            }
        }

        private static bool TransactionActive()
        {
            try
            {
                TransactionManager tm = TransactionManager.Instance;
                if (!Net.Alive(tm)) return false;
                return tm.transactionInProgress;
            }
            catch { return false; }
        }

        /// <summary>Bag every item currently on the counter.</summary>
        internal void BagAll(bool announce) { BagAll(announce, false); }

        /// <param name="force">Ignore the retry throttle - used on the completion path, where there
        /// is no second chance: revenue is banked the moment the transaction completes.</param>
        internal void BagAll(bool announce, bool force)
        {
            float now = Time.unscaledTime;
            int done = 0;
            int remaining = 0;

            List<ScanItem> items = CounterItems();
            for (int i = 0; i < items.Count; i++)
            {
                ScanItem item = items[i];
                if (!Net.Alive(item)) continue;

                GameObject go;
                try { go = item.gameObject; } catch { continue; }
                if (go == null || !go.activeInHierarchy || !go.scene.IsValid()) continue;

                remaining++;

                int id;
                try { id = item.GetInstanceID(); } catch { continue; }

                // A scan that silently did nothing should be retried, not blacklisted for good.
                float last;
                if (!force && _attempted.TryGetValue(id, out last) && now - last < RetryAfter) continue;

                try
                {
                    item.Rpc_CMD_Interact();
                    _attempted[id] = now;
                    Bagged++;
                    done++;
                }
                catch (Exception ex)
                {
                    Log.Debug("ScanItem.Rpc_CMD_Interact failed: " + ex.Message);
                    try { item.Rpc_Interact(); _attempted[id] = now; Bagged++; done++; }
                    catch (Exception ex2) { Log.Debug("ScanItem.Rpc_Interact failed: " + ex2.Message); }
                }
            }

            Prune(items);

            Status = remaining == 0 ? "counter clear" : remaining + " item(s) on counter";
            if (announce || done > 0)
                Log.Msg("Bagged " + done + " item(s); " + Status + ".");
        }

        /// <summary>Report what the game thinks is outstanding, for the status line.</summary>
        internal string TransactionInfo()
        {
            try
            {
                TransactionManager tm = TransactionManager.Instance;
                if (!Net.Alive(tm)) return "no register";
                if (!tm.transactionInProgress) return "no transaction";
                return "scanned " + tm.amountOfItemsScanned + " / " + tm.amountOfItemsToScan;
            }
            catch { return "?"; }
        }

        private void Prune(List<ScanItem> live)
        {
            if (_attempted.Count == 0) return;

            var alive = new HashSet<int>();
            for (int i = 0; i < live.Count; i++)
            {
                if (!Net.Alive(live[i])) continue;
                try { alive.Add(live[i].GetInstanceID()); } catch { }
            }

            List<int> gone = null;
            foreach (KeyValuePair<int, float> kv in _attempted)
            {
                if (alive.Contains(kv.Key)) continue;
                if (gone == null) gone = new List<int>();
                gone.Add(kv.Key);
            }
            if (gone == null) return;
            for (int i = 0; i < gone.Count; i++) _attempted.Remove(gone[i]);
        }        /// <summary>
        /// Destroys anything still on the counter a moment after the customer is finished with it,
        /// the same way CancelTransaction does - disable the behaviour, hand the object to the
        /// network manager, then forget it. Items that were bagged are already gone by this point, so
        /// only what nobody scanned actually gets removed.
        /// </summary>
        private void SweepLeftovers(float now)
        {
            TransactionManager tm;
            try { tm = TransactionManager.Instance; }
            catch { return; }
            if (!Net.Alive(tm)) return;

            Il2CppSystem.Collections.Generic.List<GameObject> objs;
            int count;
            try
            {
                objs = tm.objectsToScan;
                if (objs == null) { _leftoverSince = 0f; return; }
                count = objs.Count;
            }
            catch { return; }

            if (count == 0) { _leftoverSince = 0f; return; }

            // Let the completion animations and the customer's exit play out first.
            if (_leftoverSince == 0f) { _leftoverSince = now; return; }
            if (now - _leftoverSince < 1.5f) return;
            _leftoverSince = 0f;

            int removed = 0;
            for (int i = 0; i < count; i++)
            {
                GameObject go;
                try { go = objs[i]; }
                catch { continue; }
                if (go == null) continue;              // already bagged and consumed

                try { if (!go.scene.IsValid()) continue; }
                catch { continue; }

                if (Destroy(go)) removed++;
            }

            try { objs.Clear(); } catch (Exception ex) { Log.Debug("clear objectsToScan: " + ex.Message); }

            if (removed > 0)
            {
                LeftoversCleared += removed;
                Status = "cleared " + removed + " unbagged item(s)";
                Log.Msg("Counter cleared: " + removed + " unbagged item(s) removed after the customer left.");
            }
        }

        /// <summary>Networked destroy, with the client-side request as a fallback.</summary>
        private static bool Destroy(GameObject go)
        {
            try
            {
                ScanItem si = go.GetComponent<ScanItem>();
                if (si != null) si.enabled = false;
            }
            catch { }

            FusionNetworkManager fnm = null;
            try { fnm = FusionNetworkManager.Instance; } catch { }

            if (Net.Alive(fnm))
            {
                try { fnm.DestroyItem(go); return true; }
                catch (Exception ex) { Log.Debug("DestroyItem: " + ex.Message); }

                try
                {
                    Il2CppFusion.NetworkObject no = go.GetComponent<Il2CppFusion.NetworkObject>();
                    if (no != null) { fnm.Rpc_RequestDestroyItem(no); return true; }
                }
                catch (Exception ex) { Log.Debug("Rpc_RequestDestroyItem: " + ex.Message); }
            }

            try { UnityEngine.Object.Destroy(go); return true; }
            catch (Exception ex) { Log.Debug("Destroy: " + ex.Message); return false; }
        }


    }
}
