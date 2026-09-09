using System;
using System.Collections.Generic;
using Il2Cpp;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// Item spawner.
    ///
    /// The old trainer only spawned an item if a PickupObject with that objectIndex already existed
    /// in the loaded scene, so anything not present that night refused to spawn. The real catalogue
    /// is <see cref="StoreManager.pickupObjs"/>, so every id is spawnable - see <see cref="Items"/>.
    ///
    /// Spawn path matters as much as the catalogue. StoreManager keeps two prefab arrays, pickupObjs
    /// and thrownObjs, and <c>ServerThrowObject</c> is the throw path - a thrown explosive is armed,
    /// which is why grenades used to detonate the moment they landed. Drop semantics are the default
    /// now, and explosives are never sent down the throw path at all.
    /// </summary>
    internal sealed class SpawnerModule
    {
        internal enum Method
        {
            Auto = 0,       // drop RPC, then throw (safe items only), then local
            Drop = 1,       // networked drop - inert, carries ammo
            Throw = 2,      // networked throw - arms explosives, do not use for them
            Local = 3       // offline Instantiate from the pickup prefab
        }

        internal int Selected;
        internal int Amount = 1;
        internal int Storage = -1;      // -1 = pick a sensible default per item
        internal int Storage2;
        internal Method SpawnMethod = Method.Auto;
        internal string LastResult = "";

        /// <summary>Ammo a freshly spawned weapon carries when Storage is left on auto.</summary>
        internal const int DefaultWeaponAmmo = 30;

        internal List<Items.Entry> Catalogue { get { return Items.All; } }

        internal Items.Entry SelectedEntry
        {
            get
            {
                List<Items.Entry> all = Items.All;
                if (all.Count == 0) return null;
                if (Selected < 0) Selected = 0;
                if (Selected >= all.Count) Selected = all.Count - 1;
                return all[Selected];
            }
        }

        internal void Move(int delta)
        {
            int count = Items.Count;
            if (count == 0) { Selected = 0; return; }
            Selected = ((Selected + delta) % count + count) % count;
        }

        internal string StorageLabel
        {
            get
            {
                if (Storage >= 0) return Storage.ToString();
                Items.Entry e = SelectedEntry;
                return e != null && e.IsWeapon ? "auto (" + DefaultWeaponAmmo + ")" : "auto (0)";
            }
        }

        /// <summary>Ammo / contents to give the spawned item.</summary>
        private int StorageFor(Items.Entry e)
        {
            if (Storage >= 0) return Storage;
            return e != null && e.IsWeapon ? DefaultWeaponAmmo : 0;
        }

        // ---------------------------------------------------------------- spawn verification

        /// <summary>
        /// A spawn attempt waiting to be confirmed.
        ///
        /// The networked spawn paths are fire-and-forget: Fusion decides whether the call is allowed
        /// based on authority rules that are not knowable from outside, and a refused RPC returns
        /// normally rather than throwing. So "did not throw" is not evidence that anything spawned.
        /// We count the world before and after instead, and escalate to the next method if nothing
        /// actually appeared.
        /// </summary>
        private sealed class Pending
        {
            internal Items.Entry Entry;
            internal int Storage;
            internal Vector3 Pos;
            internal Quaternion Rot;
            internal Vector3 Forward;
            internal int BaselineCount;
            internal float Deadline;
            internal Method Tried;
        }

        private Pending _pending;
        private const float VerifyDelay = 0.7f;

        /// <summary>Cheap world census used only to tell "something appeared" from "nothing did".</summary>
        private static int CountPickups()
        {
            try { return Net.FindActive<PickupObject>().Count; }
            catch { return -1; }
        }

        /// <summary>Called each frame by the mod so a refused spawn can escalate on its own.</summary>
        internal void Tick()
        {
            Pending p = _pending;
            if (p == null || Time.unscaledTime < p.Deadline) return;
            _pending = null;

            int now = CountPickups();
            if (p.BaselineCount < 0 || now < 0) return;
            if (now > p.BaselineCount) return;      // something appeared - all good

            Log.Warn("Spawn via " + p.Tried + " produced nothing (world count stayed at " + now +
                     "). Falling back.");

            // Escalate: drop -> throw (safe items only) -> local.
            string how = "?";
            bool ok = false;
            if (p.Tried == Method.Drop && !p.Entry.IsExplosive)
                ok = TryThrow(p.Entry, p.Pos, p.Rot, p.Forward, ref how);
            if (!ok)
                ok = TryLocal(p.Entry.Id, p.Storage, p.Pos, p.Rot, ref how);

            LastResult = ok
                ? "Spawned " + p.Entry.Name + " (" + how + ", after " + p.Tried + " failed)"
                : "FAILED: " + p.Entry.Name + " - no spawn path worked";
            Log.Msg(LastResult + ".");
        }

        /// <summary>
        /// Drops a flamethrower and switches it into grenade mode. The item itself is the game's own
        /// flamethrower - what makes it "modified" is the firing patch, so picking this one up and
        /// firing it launches grenades until the mode is turned back off.
        /// </summary>
        internal void SpawnGrenadeFlamethrower()
        {
            Items.Entry gun = Items.FindByName("flamethrower");
            if (gun == null)
            {
                LastResult = "No flamethrower in the catalogue";
                Log.Warn(LastResult + ".");
                return;
            }

            Spawn(gun, 1);
            WeaponMods.GrenadeFlamethrower = true;

            // ShootFlamethrower is never called by an empty weapon, and the grenades ride that call -
            // so an unfuelled flamethrower would look like the mod simply did nothing.
            int fuel = 0;
            try
            {
                InventoryManager inv = Net.LocalInventory;
                if (Net.Alive(inv) && inv.flamethrowerAmmo < 100)
                {
                    inv.flamethrowerAmmo = 100;
                    fuel = 100;
                }
            }
            catch (Exception ex) { Log.Debug("flamethrower fuel: " + ex.Message); }

            LastResult = "Grenade launcher spawned";
            Log.Msg(LastResult + " - pick up the flamethrower and hold fire; it lobs impact grenades" +
                    (fuel > 0 ? " (fuel topped up to " + fuel + ")" : "") + ".");
        }

        internal void SpawnSelected()
        {
            Items.Entry e = SelectedEntry;
            if (e == null) { LastResult = "No catalogue yet"; return; }
            Spawn(e, Math.Max(1, Amount));
        }

        internal void Spawn(Items.Entry entry, int amount)
        {
            Transform t = Net.LocalTransform;
            if (t == null)
            {
                LastResult = "Player not ready";
                Log.Warn("Spawn: local player not resolved yet.");
                return;
            }

            int itemId = entry.Id;
            int storage = StorageFor(entry);
            int ok = 0;
            string how = "?";
            int baseline = SpawnMethod == Method.Local ? -1 : CountPickups();
            Vector3 lastPos = Vector3.zero;
            Quaternion lastRot = Quaternion.identity;

            for (int i = 0; i < amount; i++)
            {
                // Fan copies out so they do not all land in one physics singularity.
                float spread = amount > 1 ? (i - (amount - 1) * 0.5f) * 0.45f : 0f;
                Vector3 pos = t.position + t.forward * 2.2f + t.right * spread + Vector3.up * 1.1f;
                Quaternion rot = t.rotation;

                lastPos = pos; lastRot = rot;
                if (SpawnOne(entry, storage, pos, rot, t.forward, ref how)) ok++;
                else break;
            }

            // A networked path returns normally even when Fusion refuses it, so confirm by
            // looking at the world shortly afterwards rather than trusting the call.
            if (ok > 0 && baseline >= 0 && how.IndexOf("drop", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _pending = new Pending
                {
                    Entry = entry,
                    Storage = storage,
                    Pos = lastPos,
                    Rot = lastRot,
                    Forward = t.forward,
                    BaselineCount = baseline,
                    Deadline = Time.unscaledTime + VerifyDelay,
                    Tried = Method.Drop
                };
            }

            string label = entry.Name;
            if (ok == 0)
            {
                LastResult = "FAILED: " + label;
                Log.Warn("Spawn failed for [" + itemId + "] " + label + ".");
            }
            else
            {
                LastResult = "Spawned " + ok + "x " + label + " (" + how + ")";
                Log.Msg("Spawned " + ok + "x [" + itemId + "] " + label + " via " + how +
                        (storage > 0 ? " with storage " + storage : "") + ".");
            }
        }

        private bool SpawnOne(Items.Entry entry, int storage, Vector3 pos, Quaternion rot, Vector3 forward, ref string how)
        {
            switch (SpawnMethod)
            {
                case Method.Drop: return TryDrop(entry.Id, storage, pos, rot, forward, ref how);
                case Method.Throw: return TryThrow(entry, pos, rot, forward, ref how);
                case Method.Local: return TryLocal(entry.Id, storage, pos, rot, ref how);

                default:
                    // Drop first: it is inert and carries ammo. Throw is a fallback and is skipped
                    // outright for explosives, which arm themselves when thrown.
                    if (TryDrop(entry.Id, storage, pos, rot, forward, ref how)) return true;
                    if (!entry.IsExplosive && TryThrow(entry, pos, rot, forward, ref how)) return true;
                    return TryLocal(entry.Id, storage, pos, rot, ref how);
            }
        }

        /// <summary>Fusion mask meaning state + input + proxy, i.e. a local simulation.</summary>
        private const int AuthorityAll = 7;

        /// <summary>
        /// Networked drop. Spawns the inert pickup and carries itemStorage (ammo).
        ///
        /// Rpc_CMD_NetworkDropObject is an input-authority command, and its dispatch refuses the
        /// call outright when the local authority mask is ALL:
        ///     055 Compare rax, 7
        ///     056 JumpIfEqual {130}
        ///     130 Call NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc
        ///     140 Goto {119}            -> returns, nothing spawned, nothing sent
        /// It returns normally, so nothing throws and the spawn silently does not happen.
        ///
        /// The body itself is the real networked spawn and is gated on HasStateAuthority, so on a
        /// host the right move is to run it directly via Fusion's InvokeRpc flag. The object it
        /// creates is a NetworkObject, which replicates on its own - there is no broadcast to miss
        /// here, unlike the plain Rpc_ events.
        /// </summary>
        private bool TryDrop(int itemId, int storage, Vector3 pos, Quaternion rot, Vector3 forward, ref string how)
        {
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) return false;

            InventoryManager inv = Net.LocalInventory;
            if (!Net.Alive(inv)) return false;

            try
            {
                int mask = Rpc.Command(sm,
                    delegate { sm.Rpc_CMD_NetworkDropObject(itemId, pos, rot, storage, Storage2, forward, inv); },
                    "NetworkDropObject");
                how = (Net.IsHost ? "host/drop" : "client/drop") + " (mask " + mask + ")";
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug("Rpc_CMD_NetworkDropObject failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>Networked throw. Never used for explosives - it spawns the armed variant.</summary>
        private bool TryThrow(Items.Entry entry, Vector3 pos, Quaternion rot, Vector3 forward, ref string how)
        {
            if (entry.IsExplosive)
            {
                Log.Debug("Throw path refused for explosive " + entry.Name + ".");
                return false;
            }

            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm) || !Net.IsHost) return false;

            GameObject carrier = null;
            PlayerManager pm = Net.LocalPlayer;
            if (Net.Alive(pm)) carrier = pm.gameObject;

            try
            {
                sm.ServerThrowObject(entry.Id, pos, rot, forward, carrier, 0f);
                how = "host/throw";
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug("ServerThrowObject failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Offline / fallback path. Instantiates the pickup prefab, which is the inert variant, so
        /// explosives stay safe here too.
        /// </summary>
        private bool TryLocal(int itemId, int storage, Vector3 pos, Quaternion rot, ref string how)
        {
            GameObject prefab = null;
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (Net.Alive(sm) && sm.pickupObjs != null && itemId >= 0 && itemId < sm.pickupObjs.Length)
                    prefab = sm.pickupObjs[itemId];
            }
            catch { }

            if (prefab == null) prefab = FindSceneTemplate(itemId);
            if (prefab == null) return false;

            try
            {
                GameObject copy = UnityEngine.Object.Instantiate(prefab, pos, rot);
                if (copy == null) return false;
                copy.SetActive(true);
                PickupObject po = copy.GetComponent<PickupObject>();
                if (po != null)
                {
                    po.objectIndex = itemId;
                    po.itemStorage = storage;
                    po.itemStorage2 = Storage2;
                }
                how = "LOCAL ONLY - other players cannot see this";
                Log.Warn("Spawned " + itemId + " with a local Instantiate. This object is not " +
                         "networked and only exists on this machine.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug("local Instantiate failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>Last resort: an existing instance in the scene, the way the old trainer worked.</summary>
        private GameObject FindSceneTemplate(int itemId)
        {
            List<PickupObject> all = Net.FindAll<PickupObject>();
            GameObject fallback = null;
            for (int i = 0; i < all.Count; i++)
            {
                PickupObject po = all[i];
                if (po == null) continue;
                int idx;
                try { idx = po.objectIndex; } catch { continue; }
                if (idx != itemId) continue;
                GameObject go;
                try { go = po.gameObject; } catch { continue; }
                if (go == null) continue;
                if (go.activeSelf) return go;
                if (fallback == null) fallback = go;
            }
            return fallback;
        }
    }
}
