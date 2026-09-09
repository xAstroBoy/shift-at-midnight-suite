using System;
using System.Collections.Generic;
using Il2Cpp;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite.Modules
{
    /// <summary>
    /// Automatic shelf stocking.
    ///
    /// Two bugs made the old version stall:
    ///
    /// 1. It derived a per-slot cap as maxProductsOnShelf / slotCount. When that does not divide
    ///    evenly (10 products across 3 slots gives a cap of 3, so at most 9) every slot reports
    ///    "full" while the shelf itself is still incomplete, and stocking wedges forever on a shelf
    ///    it will never finish.
    /// 2. It only armed from a Harmony hook on crate pickup, so already holding a crate - after a
    ///    reload, or picking one up before the hook landed - never started anything.
    ///
    /// This version drives off the shelf total (productsOnShelf vs maxProductsOnShelf), always fills
    /// the emptiest slot, and polls for a held crate rather than waiting for an event.
    /// </summary>
    internal sealed class StockModule
    {
        internal bool Enabled;

        /// <summary>
        /// When set, stock is only taken from a crate the player is actually carrying (the old
        /// behaviour). Off by default: shelves fill on their own with no crate handling at all.
        /// </summary>
        internal bool RequireCrate;

        private const float ScanInterval = 0.5f;
        private const int CrateItemId = 0;    // "Crate" is item index 0 in the catalogue.

        private float _nextScan;
        private bool _noShelfLogged;
        private bool _holdLogged;

        internal int Filled;
        internal string Status = "idle";

        internal void OnSceneChanged()
        {
            _noShelfLogged = false;
            _holdLogged = false;
        }

        internal void Tick()
        {
            if (!Enabled) return;
            float now = Time.unscaledTime;
            if (now < _nextScan) return;
            _nextScan = now + ScanInterval;

            try { Run(); }
            catch (Exception ex)
            {
                Status = "error";
                Log.Ex("auto-stock pass", ex);
                _nextScan = now + 1f;
            }
        }

        private void Run()
        {
            InventoryManager inv = Net.LocalInventory;
            int crateUnits = RequireCrate ? HeldCrateUnits(inv) : int.MaxValue;

            if (crateUnits <= 0)
            {
                if (_holdLogged)
                {
                    _holdLogged = false;
                    Log.Msg("Crate empty or put away; auto-stock paused.");
                }
                Status = "no crate held";
                return;
            }

            if (!_holdLogged && RequireCrate)
            {
                _holdLogged = true;
                Log.Msg("Holding crate with " + crateUnits + " unit(s); filling shelves.");
            }

            Target target = FindOpenShelf();
            if (target == null)
            {
                if (!_noShelfLogged)
                {
                    _noShelfLogged = true;
                    Log.Msg("No incomplete shelf right now.");
                }
                Status = "all shelves full";
                return;
            }
            _noShelfLogged = false;

            if (!AddOne(target)) return;

            Filled++;
            Status = "filling " + target.Name;
            if (RequireCrate) ConsumeOne(inv, crateUnits);
        }

        private sealed class Target
        {
            internal RestockShelf Shelf;
            internal ShelfItemManager Slot;
            internal string Name;
        }

        /// <summary>
        /// First shelf whose own total is below its own maximum, filling whichever slot currently
        /// holds the fewest products. No synthetic per-slot cap, so uneven splits still complete.
        /// </summary>
        private Target FindOpenShelf()
        {
            List<RestockShelf> shelves = Net.FindActive<RestockShelf>();
            for (int i = 0; i < shelves.Count; i++)
            {
                RestockShelf shelf = shelves[i];
                if (!Net.Alive(shelf)) continue;

                GameObject go;
                try { go = shelf.gameObject; } catch { continue; }
                if (go == null || !go.activeInHierarchy || !go.scene.IsValid()) continue;

                int max, cur;
                try { max = shelf.maxProductsOnShelf; cur = shelf.productsOnShelf; }
                catch { continue; }
                if (max <= 0 || cur >= max) continue;

                ShelfManager man;
                try { man = shelf.shelfMan; } catch { continue; }
                if (!Net.Alive(man)) continue;

                var slots = man.shelfItemManagers;
                if (slots == null || slots.Length == 0) continue;

                ShelfItemManager best = null;
                int bestCount = int.MaxValue;
                for (int j = 0; j < slots.Length; j++)
                {
                    ShelfItemManager slot = slots[j];
                    if (!Net.Alive(slot)) continue;
                    int have;
                    try { have = slot.amountOfProducts; } catch { continue; }
                    if (have < 0) continue;
                    if (have < bestCount) { bestCount = have; best = slot; }
                }
                if (best == null) continue;

                return new Target { Shelf = shelf, Slot = best, Name = go.name };
            }
            return null;
        }

        private bool AddOne(Target target)
        {
            try
            {
                target.Slot.Rpc_CMD_AddItem(false);
            }
            catch (Exception ex)
            {
                Log.Debug("Rpc_CMD_AddItem failed: " + ex.Message);
                try { target.Slot.LocalAddItem(false); }
                catch (Exception ex2) { Log.Debug("LocalAddItem failed: " + ex2.Message); return false; }
            }

            try { target.Shelf.Rpc_CMD_RecalculateProducts(); }
            catch (Exception ex) { Log.Debug("Rpc_CMD_RecalculateProducts failed: " + ex.Message); }
            try { target.Shelf.AutoUpdateBar(); } catch { }
            return true;
        }

        /// <summary>Units left in the crate the player is holding, or 0 when not holding one.</summary>
        private static int HeldCrateUnits(InventoryManager inv)
        {
            if (!Net.Alive(inv)) return 0;
            try
            {
                int holding = inv.holdingIndex;
                if (holding < 0) return 0;

                var ids = inv.inventoryIds;
                var storages = inv.itemStorages;
                if (ids == null || storages == null) return 0;
                if (holding >= ids.Length || holding >= storages.Length) return 0;
                if (ids[holding] != CrateItemId) return 0;

                return storages[holding];
            }
            catch { return 0; }
        }

        private static void ConsumeOne(InventoryManager inv, int unitsBefore)
        {
            if (!Net.Alive(inv)) return;
            try
            {
                int slot = inv.holdingIndex;
                if (slot < 0) return;
                int left = Math.Max(0, unitsBefore - 1);
                var storages = inv.itemStorages;
                if (storages != null && slot < storages.Length) storages[slot] = left;
                try { inv.Rpc_CMD_ChangeItemStorage(slot, left, 0); }
                catch { inv.ChangePlayerItemStorage(slot, left, 0); }
            }
            catch (Exception ex) { Log.Debug("crate decrement failed: " + ex.Message); }
        }
    }
}
