using System;
using System.Collections.Generic;
using Il2Cpp;
using Il2CppFusion;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// Do things to other players. Host-side control over everyone in the session.
    ///
    /// Every PlayerManager carries its own set of Rpc_ methods (TakeDamage, Downed, Die, Revive,
    /// GetStuck, ChangeScent, SetPlayerName, ...) that the game normally only fires at the local
    /// player. Pointed at someone else's PlayerManager from the host they work just the same. For
    /// movement, RestockShelf.Rpc_CMD_SetPosition(Vector3, NetworkObject) sets the transform of
    /// whatever player object it is handed - the game uses it to park you at a shelf, which makes it
    /// a general-purpose teleport.
    /// </summary>
    internal sealed class PlayersModule
    {
        internal sealed class Target
        {
            internal PlayerManager Player;
            internal string Name;
            internal bool IsLocal;
        }

        private readonly List<Target> _players = new List<Target>();
        private float _nextRefresh;

        internal int Selected;
        internal float DamageAmount = 25f;
        internal string LastResult = "";

        internal List<Target> Players
        {
            get
            {
                if (Time.unscaledTime < _nextRefresh && _players.Count > 0) return _players;
                _nextRefresh = Time.unscaledTime + 1f;
                Refresh();
                return _players;
            }
        }

        internal Target SelectedTarget
        {
            get
            {
                List<Target> all = Players;
                if (all.Count == 0) return null;
                if (Selected < 0) Selected = 0;
                if (Selected >= all.Count) Selected = all.Count - 1;
                return all[Selected];
            }
        }

        private void Refresh()
        {
            _players.Clear();
            PlayerManager local = Net.LocalPlayer;

            List<PlayerManager> found = new List<PlayerManager>();
            try
            {
                StoreManager sm = StoreManager.Instance;
                if (Net.Alive(sm) && sm.playerMans != null)
                    for (int i = 0; i < sm.playerMans.Count; i++)
                        if (Net.Alive(sm.playerMans[i])) found.Add(sm.playerMans[i]);
            }
            catch { }

            // The roster list is only filled once the store has loaded everyone; fall back to a scan.
            if (found.Count == 0) found = Net.FindAll<PlayerManager>();

            for (int i = 0; i < found.Count; i++)
            {
                PlayerManager pm = found[i];
                if (!Net.Alive(pm)) continue;
                try
                {
                    GameObject go = pm.gameObject;
                    if (go == null || !go.scene.IsValid()) continue;
                }
                catch { continue; }

                string name = null;
                try { name = pm.playerName; } catch { }
                if (string.IsNullOrEmpty(name)) { try { name = pm.gameObject.name; } catch { name = "Player"; } }

                bool isLocal = Net.Alive(local) && ReferenceEquals(local.Pointer, pm.Pointer);
                _players.Add(new Target { Player = pm, Name = name, IsLocal = isLocal });
            }
        }

        // ---------------------------------------------------------------- actions

        private bool Ready(out PlayerManager pm, string what)
        {
            Target t = SelectedTarget;
            pm = t != null ? t.Player : null;
            if (!Net.Alive(pm)) { LastResult = "No player selected"; Log.Warn(what + ": no player selected."); return false; }
            return true;
        }

        private void Done(string what)
        {
            Target t = SelectedTarget;
            LastResult = what + " -> " + (t != null ? t.Name : "?");
            Log.Msg(LastResult + ".");
        }

        internal void Damage()
        {
            PlayerManager pm; if (!Ready(out pm, "damage")) return;
            float amount = DamageAmount;
            try { Rpc.Call(pm, delegate { pm.Rpc_TakeDamage(amount, true, ""); }, "TakeDamage"); Done("Damage " + amount); }
            catch (Exception ex) { Log.Ex("damage", ex); }
        }

        internal void Down()
        {
            PlayerManager pm; if (!Ready(out pm, "down")) return;
            try { Rpc.Call(pm, delegate { pm.Rpc_Downed(); }, "Downed"); Done("Downed"); }
            catch (Exception ex) { Log.Ex("down", ex); }
        }

        internal void Kill()
        {
            PlayerManager pm; if (!Ready(out pm, "kill")) return;
            try { Rpc.Call(pm, delegate { pm.Rpc_Die(); }, "Die"); Done("Killed"); }
            catch (Exception ex) { Log.Ex("kill", ex); }
        }

        internal void Revive()
        {
            PlayerManager pm; if (!Ready(out pm, "revive")) return;
            try { Rpc.Call(pm, delegate { pm.Rpc_Revive(); }, "Revive"); Done("Revived"); }
            catch (Exception ex) { Log.Ex("revive", ex); }
        }

        internal void Respawn()
        {
            PlayerManager pm; if (!Ready(out pm, "respawn")) return;
            try { Rpc.Call(pm, delegate { pm.Rpc_Respawn(); }, "Respawn"); Done("Respawned"); }
            catch (Exception ex) { Log.Ex("respawn", ex); }
        }

        internal void Heal()
        {
            PlayerManager pm; if (!Ready(out pm, "heal")) return;
            try { pm.HealToMax(); Rpc.Command(pm, delegate { pm.Rpc_CMD_Heal(999f); }, "Heal"); Done("Healed"); }
            catch (Exception ex) { Log.Ex("heal", ex); }
        }

        /// <summary>The "stuck on a key, wiggle to escape" state the game uses for traps.</summary>
        internal void Stuck()
        {
            PlayerManager pm; if (!Ready(out pm, "stuck")) return;
            try { Rpc.Call(pm, delegate { pm.Rpc_GetStuck(6f); }, "GetStuck"); Done("Stuck"); }
            catch (Exception ex) { Log.Ex("stuck", ex); }
        }

        /// <summary>Scent is what monsters track. Max it and they home in on that player.</summary>
        internal void MaxScent()
        {
            PlayerManager pm; if (!Ready(out pm, "scent")) return;
            try { Rpc.Call(pm, delegate { pm.Rpc_ChangeScent(100f); }, "ChangeScent"); Done("Scent maxed"); }
            catch (Exception ex) { Log.Ex("scent", ex); }
        }

        internal void ClearScent()
        {
            PlayerManager pm; if (!Ready(out pm, "scent")) return;
            try { Rpc.Call(pm, delegate { pm.Rpc_ChangeScent(0f); }, "ChangeScent"); Done("Scent cleared"); }
            catch (Exception ex) { Log.Ex("scent", ex); }
        }

        internal void Rename(string newName)
        {
            PlayerManager pm; if (!Ready(out pm, "rename")) return;
            try { Rpc.Call(pm, delegate { pm.Rpc_SetPlayerName(newName); }, "SetPlayerName"); Done("Renamed to " + newName); }
            catch (Exception ex) { Log.Ex("rename", ex); }
        }

        internal void HuntLight(bool on)
        {
            PlayerManager pm; if (!Ready(out pm, "hunt light")) return;
            try { Rpc.Call(pm, delegate { pm.Rpc_ChangeHuntLight(on); }, "ChangeHuntLight"); Done("Hunt light " + (on ? "on" : "off")); }
            catch (Exception ex) { Log.Ex("hunt light", ex); }
        }

        internal void DropEverything()
        {
            PlayerManager pm; if (!Ready(out pm, "drop")) return;
            try
            {
                InventoryManager inv = pm.inventoryMan;
                if (!Net.Alive(inv)) { LastResult = "No inventory on target"; return; }
                inv.DropEverything(true);
                Done("Dropped everything");
            }
            catch (Exception ex) { Log.Ex("drop everything", ex); }
        }

        internal void Explode()
        {
            PlayerManager pm; if (!Ready(out pm, "explode")) return;
            try
            {
                InventoryManager inv = pm.inventoryMan;
                if (!Net.Alive(inv)) { LastResult = "No inventory on target"; return; }
                inv.Explode();
                Done("Exploded held item");
            }
            catch (Exception ex) { Log.Ex("explode", ex); }
        }

        internal void JamGun(bool jam)
        {
            PlayerManager pm; if (!Ready(out pm, "jam")) return;
            try
            {
                InventoryManager inv = pm.inventoryMan;
                if (!Net.Alive(inv)) { LastResult = "No inventory on target"; return; }
                if (jam) inv.GunJam(); else inv.GunUnjam();
                Done(jam ? "Gun jammed" : "Gun unjammed");
            }
            catch (Exception ex) { Log.Ex("jam", ex); }
        }

        internal void Pulverize()
        {
            PlayerManager pm; if (!Ready(out pm, "pulverize")) return;
            try
            {
                InventoryManager inv = pm.inventoryMan;
                if (!Net.Alive(inv)) { LastResult = "No inventory on target"; return; }
                Rpc.Call(inv, delegate { inv.Rpc_Pulverized(); }, "Pulverized");
                Done("Pulverized");
            }
            catch (Exception ex) { Log.Ex("pulverize", ex); }
        }

        internal void SetSlots(int slots)
        {
            PlayerManager pm; if (!Ready(out pm, "slots")) return;
            try
            {
                InventoryManager inv = pm.inventoryMan;
                if (!Net.Alive(inv)) { LastResult = "No inventory on target"; return; }
                Rpc.Call(inv, delegate { inv.Rpc_SetMaxInventorySlots(slots); }, "SetMaxInventorySlots");
                Done("Inventory slots = " + slots);
            }
            catch (Exception ex) { Log.Ex("slots", ex); }
        }

        // ---------------------------------------------------------------- movement

        /// <summary>
        /// Move a player. RestockShelf.Rpc_CMD_SetPosition takes any player NetworkObject and sets
        /// its transform, so any shelf in the scene can serve as the teleporter.
        /// </summary>
        private bool Teleport(PlayerManager who, Vector3 to, string what)
        {
            NetworkObject obj = null;
            try { obj = who.Object; } catch { }
            if (obj == null) { LastResult = "Target has no NetworkObject"; return false; }

            List<RestockShelf> shelves = Net.FindActive<RestockShelf>();
            RestockShelf shelf = null;
            for (int i = 0; i < shelves.Count; i++)
            {
                try { if (Net.Alive(shelves[i]) && shelves[i].gameObject.scene.IsValid()) { shelf = shelves[i]; break; } }
                catch { }
            }

            if (shelf != null)
            {
                try
                {
                    Rpc.Command(shelf, delegate { shelf.Rpc_CMD_SetPosition(to, obj); }, what);
                    return true;
                }
                catch (Exception ex) { Log.Debug("shelf teleport: " + ex.Message); }
            }

            // No shelf loaded (lobby, forest): move the transform directly. Host-authoritative
            // objects replicate position from here anyway.
            try { who.transform.position = to; return true; }
            catch (Exception ex) { Log.Ex(what, ex); return false; }
        }

        internal void TeleportToMe()
        {
            PlayerManager pm; if (!Ready(out pm, "teleport")) return;
            Transform me = Net.LocalTransform;
            if (me == null) { LastResult = "Local player not ready"; return; }
            if (Teleport(pm, me.position + me.forward * 1.5f, "TeleportToMe")) Done("Teleported to me");
        }

        internal void TeleportMeTo()
        {
            PlayerManager pm; if (!Ready(out pm, "teleport")) return;
            PlayerManager me = Net.LocalPlayer;
            if (!Net.Alive(me)) { LastResult = "Local player not ready"; return; }
            Vector3 there;
            try { there = pm.transform.position + pm.transform.forward * 1.5f; } catch { LastResult = "Target has no transform"; return; }
            if (Teleport(me, there, "TeleportMeTo")) Done("Teleported me to");
        }

        internal void TeleportToStore()
        {
            PlayerManager pm; if (!Ready(out pm, "teleport")) return;
            StoreManager sm = null;
            try { sm = StoreManager.Instance; } catch { }
            if (!Net.Alive(sm)) { LastResult = "StoreManager not ready"; return; }
            Vector3 to;
            try
            {
                Transform spawn = sm.npcSpawnPoints != null && sm.npcSpawnPoints.Length > 0 ? sm.npcSpawnPoints[0] : null;
                to = spawn != null ? spawn.position : sm.transform.position;
            }
            catch { LastResult = "No store spawn point"; return; }
            if (Teleport(pm, to, "TeleportToStore")) Done("Teleported to store");
        }

        // ---------------------------------------------------------------- monsters

        /// <summary>Point every live enemy at the target.</summary>
        internal void SicMonsters()
        {
            PlayerManager pm; if (!Ready(out pm, "sic")) return;
            NetworkObject obj = null;
            try { obj = pm.Object; } catch { }
            if (obj == null) { LastResult = "Target has no NetworkObject"; return; }

            int sent = 0;
            List<ChaseNearestPlayer> chasers = Net.FindActive<ChaseNearestPlayer>();
            for (int i = 0; i < chasers.Count; i++)
            {
                ChaseNearestPlayer c = chasers[i];
                if (!Net.Alive(c)) continue;
                try
                {
                    if (!c.gameObject.activeInHierarchy || !c.gameObject.scene.IsValid()) continue;
                    // Both the retarget and the attack order live on ChaseNearestPlayer.
                    Rpc.Call(c, delegate { c.Rpc_UpdateNearestPlayer(obj); }, "UpdateNearestPlayer");
                    Rpc.Call(c, delegate { c.Rpc_Attack(obj); }, "Attack");
                    sent++;
                }
                catch (Exception ex) { Log.Debug("chase target: " + ex.Message); }
            }

            // Scent is what the pathing monsters actually follow.
            try { Rpc.Call(pm, delegate { pm.Rpc_ChangeScent(100f); }, "ChangeScent"); } catch { }

            Done("Sent " + sent + " monster(s) after");
        }
    }
}
