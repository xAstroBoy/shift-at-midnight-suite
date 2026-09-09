using System;
using System.Collections.Generic;
using System.Reflection;
using Il2Cpp;
using Il2CppFusion;
using Il2CppInterop.Runtime;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// Every networked call in the game, in one list.
    ///
    /// Rather than hand-listing entry points, this reflects over the whole interop assembly for
    /// NetworkBehaviour types and collects their Rpc_ methods. Anything the game can do over the
    /// network is therefore reachable, including calls nobody thought to expose.
    ///
    /// Invocation uses the same two-step the event menu learned the hard way: call normally first so
    /// the message is broadcast, then - only when the local authority mask is ALL, where the
    /// dispatch skips local execution - run the body via Fusion's own InvokeRpc flag.
    /// </summary>
    internal sealed class RpcModule
    {
        internal sealed class Entry
        {
            internal Type Owner;
            internal MethodInfo Method;
            internal string Label;
            internal bool IsCommand;        // Rpc_CMD_*: client -> host
        }

        private const int AuthorityAll = 7;

        private readonly List<Entry> _all = new List<Entry>();
        private bool _scanned;

        internal string LastResult = "";
        internal int Invoked;

        /// <summary>Skip pure plumbing that corrupts state when fired out of sequence.</summary>
        private static readonly string[] Dangerous =
        {
            "Rpc_UpdateInventory", "Rpc_SetValuesForClients", "Rpc_ActuallySetInventoryValues",
            "Rpc_SendPlayerListToClients", "Rpc_UpdatePlayerCount", "Rpc_CMD_UpdateHoldingIndex",
            "Rpc_UpdateHoldingIndex", "Rpc_CMD_ChangeItemStorage", "Rpc_ChangeItemStorage",
            "Rpc_CMD_RequestIgnoreCollision", "Rpc_DoIgnoreCollision", "Rpc_StopIgnoreCollision",
            "Rpc_SetCost", "Rpc_CMD_AskForCost"
        };

        internal List<Entry> All
        {
            get { Scan(); return _all; }
        }

        internal void Invalidate() { _scanned = false; _all.Clear(); }

        /// <summary>
        /// Collect no-argument Rpc_ methods from every NetworkBehaviour in the game assembly.
        /// Zero-argument only: anything with parameters needs values we cannot invent safely.
        /// </summary>
        private void Scan()
        {
            if (_scanned) return;
            _scanned = true;
            _all.Clear();

            Assembly game = null;
            try
            {
                Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < loaded.Length; i++)
                {
                    string n = loaded[i].GetName().Name;
                    if (string.Equals(n, "Assembly-CSharp", StringComparison.Ordinal)) { game = loaded[i]; break; }
                }
            }
            catch (Exception ex) { Log.Ex("locate game assembly", ex); }

            if (game == null) { Log.Warn("RPC browser: Assembly-CSharp not loaded yet."); _scanned = false; return; }

            Type networkBehaviour = typeof(NetworkBehaviour);
            Type[] types;
            try { types = game.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types; }
            catch (Exception ex) { Log.Ex("enumerate types", ex); return; }

            for (int i = 0; i < types.Length; i++)
            {
                Type t = types[i];
                if (t == null || t.IsAbstract || !networkBehaviour.IsAssignableFrom(t)) continue;

                MethodInfo[] methods;
                try { methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly); }
                catch { continue; }

                for (int m = 0; m < methods.Length; m++)
                {
                    MethodInfo mi = methods[m];
                    if (mi == null || !mi.Name.StartsWith("Rpc_", StringComparison.Ordinal)) continue;
                    if (mi.Name.EndsWith("@Invoker", StringComparison.Ordinal)) continue;
                    if (mi.GetParameters().Length != 0) continue;
                    if (IsDangerous(mi.Name)) continue;

                    bool cmd = mi.Name.StartsWith("Rpc_CMD_", StringComparison.Ordinal);
                    _all.Add(new Entry
                    {
                        Owner = t,
                        Method = mi,
                        IsCommand = cmd,
                        Label = t.Name + "." + mi.Name + (cmd ? "   [cmd]" : "")
                    });
                }
            }

            _all.Sort(delegate (Entry a, Entry b) { return string.CompareOrdinal(a.Label, b.Label); });
            Log.Msg("RPC browser: " + _all.Count + " zero-argument RPCs across the game assembly.");
        }

        private static bool IsDangerous(string name)
        {
            for (int i = 0; i < Dangerous.Length; i++)
                if (name.IndexOf(Dangerous[i], StringComparison.Ordinal) >= 0) return true;
            return false;
        }

        /// <summary>First live scene instance of the owning type, or null.</summary>
        private static NetworkBehaviour FindInstance(Type t)
        {
            try
            {
                var raw = Resources.FindObjectsOfTypeAll(Il2CppType.From(t));
                if (raw == null) return null;

                NetworkBehaviour fallback = null;
                for (int i = 0; i < raw.Length; i++)
                {
                    UnityEngine.Object o = raw[i];
                    if (o == null) continue;
                    NetworkBehaviour nb = o.TryCast<NetworkBehaviour>();
                    if (nb == null) continue;

                    try
                    {
                        GameObject go = nb.gameObject;
                        if (go == null || !go.scene.IsValid()) continue;   // prefab, not in the world
                        if (go.activeInHierarchy) return nb;
                        if (fallback == null) fallback = nb;
                    }
                    catch { }
                }
                return fallback;
            }
            catch (Exception ex)
            {
                Log.Debug("find instance " + t.Name + ": " + ex.Message);
                return null;
            }
        }

        internal void Invoke(Entry entry)
        {
            if (entry == null) return;

            NetworkBehaviour target = FindInstance(entry.Owner);
            if (!Net.Alive(target))
            {
                LastResult = entry.Owner.Name + " has no live instance right now";
                Log.Warn("RPC " + entry.Label + ": " + LastResult + ".");
                return;
            }

            try
            {
                Action call = delegate { entry.Method.Invoke(target, null); };
                int mask = entry.IsCommand
                    ? Rpc.Command(target, call, entry.Label)
                    : Rpc.Call(target, call, entry.Label);

                Invoked++;
                LastResult = "Invoked " + entry.Label + " (mask " + mask + ")";
                Log.Msg(LastResult + ".");
            }
            catch (TargetInvocationException ex)
            {
                Exception inner = ex.InnerException ?? ex;
                LastResult = "Failed: " + entry.Label + " - " + inner.Message;
                Log.Warn(LastResult);
            }
            catch (Exception ex)
            {
                LastResult = "Failed: " + entry.Label + " - " + ex.Message;
                Log.Warn(LastResult);
            }
        }

        /// <summary>Write the full RPC inventory to the log, grouped by owning type.</summary>
        internal void DumpAll()
        {
            Scan();
            Log.Msg("=== RPC inventory: " + _all.Count + " zero-argument RPCs ===");
            string last = null;
            for (int i = 0; i < _all.Count; i++)
            {
                Entry e = _all[i];
                if (e.Owner.Name != last)
                {
                    last = e.Owner.Name;
                    bool live = Net.Alive(FindInstance(e.Owner));
                    Log.Msg("  " + last + (live ? "  [live]" : "  [no instance]"));
                }
                Log.Msg("      " + e.Method.Name + (e.IsCommand ? "   [cmd]" : ""));
            }
            Log.Msg("=== end RPC inventory ===");
            LastResult = "Dumped " + _all.Count + " RPCs to the log";
        }
    }
}
