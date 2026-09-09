using System;
using Il2Cpp;
using Il2CppInterop.Runtime;
using MelonLoader;
using UnityEngine;

namespace ShiftAtMidnightSuite.Util
{
    /// <summary>
    /// Small helpers for reaching the local player and answering "am I the host?".
    /// Every accessor is null-safe: the game tears these down between scenes.
    /// </summary>
    internal static class Net
    {
        private static InventoryManager _inventory;
        private static PlayerManager _player;
        private static float _nextResolve;

        internal static void Invalidate()
        {
            _inventory = null;
            _player = null;
            _nextResolve = 0f;
        }

        internal static InventoryManager LocalInventory
        {
            get { Resolve(); return Alive(_inventory) ? _inventory : null; }
        }

        internal static PlayerManager LocalPlayer
        {
            get { Resolve(); return Alive(_player) ? _player : null; }
        }

        internal static Transform LocalTransform
        {
            get
            {
                PlayerManager pm = LocalPlayer;
                if (Alive(pm)) return pm.transform;
                InventoryManager inv = LocalInventory;
                return Alive(inv) ? inv.transform : null;
            }
        }

        /// <summary>True when this client owns the simulation, i.e. we can spawn networked objects directly.</summary>
        internal static bool IsHost
        {
            get
            {
                try
                {
                    StoreManager sm = StoreManager.Instance;
                    if (Alive(sm)) return sm.HasStateAuthority;
                }
                catch { }
                try
                {
                    InventoryManager inv = LocalInventory;
                    if (Alive(inv)) return inv.HasStateAuthority;
                }
                catch { }
                return false;
            }
        }

        internal static bool Alive(UnityEngine.Object o)
        {
            try { return o != null; }
            catch { return false; }
        }

        private static void Resolve()
        {
            if (Alive(_inventory) && Alive(_player)) return;
            if (Time.unscaledTime < _nextResolve) return;
            _nextResolve = Time.unscaledTime + 1f;

            try
            {
                if (!Alive(_player))
                {
                    var all = UnityEngine.Object.FindObjectsOfType<PlayerManager>();
                    if (all != null)
                    {
                        for (int i = 0; i < all.Length; i++)
                        {
                            PlayerManager pm = all[i];
                            if (!Alive(pm)) continue;
                            if (pm.HasInputAuthority) { _player = pm; break; }
                            if (!Alive(_player)) _player = pm;
                        }
                    }
                }

                if (Alive(_player) && Alive(_player.inventoryMan))
                {
                    _inventory = _player.inventoryMan;
                }
                else if (!Alive(_inventory))
                {
                    var invs = UnityEngine.Object.FindObjectsOfType<InventoryManager>();
                    if (invs != null)
                    {
                        for (int i = 0; i < invs.Length; i++)
                        {
                            InventoryManager im = invs[i];
                            if (!Alive(im)) continue;
                            if (im.HasInputAuthority) { _inventory = im; break; }
                            if (!Alive(_inventory)) _inventory = im;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug("player resolve failed: " + ex.Message);
            }
        }

        /// <summary>
        /// Active scene instances only. Object.FindObjectsOfType walks the live scene; the
        /// FindObjectsOfTypeAll variant below walks every loaded object including prefabs and
        /// assets, which is milliseconds per call and the wrong tool for something that runs
        /// several times a second. Use this for periodic sweeps, FindAll for rosters and prefabs.
        /// </summary>
        internal static System.Collections.Generic.List<T> FindActive<T>() where T : UnityEngine.Object
        {
            var result = new System.Collections.Generic.List<T>();
            try
            {
                var raw = UnityEngine.Object.FindObjectsOfType<T>();
                if (raw == null) return result;
                for (int i = 0; i < raw.Length; i++)
                {
                    T t = raw[i];
                    if (t != null) result.Add(t);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("FindActive<" + typeof(T).Name + "> failed: " + ex.Message);
            }
            return result;
        }

        /// <summary>Every live instance of an Il2Cpp component type, including inactive ones. Never null.</summary>
        internal static System.Collections.Generic.List<T> FindAll<T>() where T : Il2CppSystem.Object
        {
            var result = new System.Collections.Generic.List<T>();
            try
            {
                var raw = Resources.FindObjectsOfTypeAll(Il2CppType.Of<T>());
                if (raw == null) return result;
                for (int i = 0; i < raw.Length; i++)
                {
                    UnityEngine.Object o = raw[i];
                    if (o == null) continue;
                    T t = o.TryCast<T>();
                    if (t != null) result.Add(t);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("FindAll<" + typeof(T).Name + "> failed: " + ex.Message);
            }
            return result;
        }
    }
}
