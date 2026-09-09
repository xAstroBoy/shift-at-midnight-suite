using System;
using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace ShiftAtMidnightSuite.Util
{
    /// <summary>
    /// The item catalogue, read from <see cref="StoreManager.pickupObjs"/> - the prefab array the
    /// game itself indexes by item id.
    ///
    /// Items are classified from the real prefab name rather than a hardcoded id table, because the
    /// id order is not guaranteed to match the old trainer's list and a wrong guess here is what
    /// makes a grenade behave like a grenade when you did not want it to.
    /// </summary>
    internal static class Items
    {
        internal sealed class Entry
        {
            internal int Id;
            internal string Name;
            internal bool IsExplosive;
            internal bool IsWeapon;
        }

        /// <summary>Fallback labels for the ids the old trainer knew, used only if a prefab has no name.</summary>
        private static readonly string[] FallbackNames =
        {
            "Crate", "Bottle", "Pistol", "Toilet Paper", "Flashlight", "Mop", "Trash", "Bear Trap", "Plank", "Egg",
            "Explosive", "EmotiScope", "Poster", "Rat", "RubberDuck", "Gas Pump", "Potted Plant", "Water Cooler", "Basket Rack", "ATM",
            "Mailbox", "TrashCan", "Banner", "Floor Mat", "SunglassesRack", "Books", "Figurine", "Burger", "Plant1", "Plant2",
            "Plant3", "Plant4", "Robot", "Boombox", "Gumball", "Clock", "Fake Ivy", "String Lights", "Painting 1", "Painting 2",
            "Painting 3", "Deer Head", "Explosive REMOTE", "Pill Bottle", "Medkit", "Shotgun", "Impact Grenade", "BaseballBat", "Sledgehammer", "Brick",
            "Flamethrower", "Molotov", "Landmine", "Stun Mine", "Hose", "SMG", "BucketEmpty", "BucketFilled", "GhostCamera"
        };

        /// <summary>Anything that goes off on impact or arms itself must never take the throw path.</summary>
        private static readonly string[] ExplosiveWords =
        {
            "grenade", "nade", "molotov", "explosive", "landmine", "mine", "beartrap", "bear trap", "bomb", "c4", "dynamite", "petrol"
        };

        /// <summary>
        /// Things that get used up: throwables, one-shot placeables and anything you drink or apply.
        /// Deliberately excludes guns - their magazines are handled by the ammo logic, and pinning a
        /// weapon's stack count is what used to leave them unable to fire.
        /// </summary>
        private static readonly string[] ConsumableWords =
        {
            "nade", "grenade", "molotov", "brick", "flare", "firework", "dynamite", "explosive",
            "landmine", "mine", "beartrap", "bear trap", "trap", "cola", "lemonade", "drink",
            "medkit", "bandage", "syringe", "pill", "battery", "poster", "board", "plank"
        };

        private static readonly string[] WeaponWords =
        {
            "shotgun", "pistol", "revolver", "smg", "rifle", "flamethrower", "gun"
        };

        private static readonly List<Entry> _all = new List<Entry>();
        private static int _builtFor = -1;

        internal static List<Entry> All { get { Refresh(); return _all; } }

        internal static int Count { get { Refresh(); return _all.Count; } }

        internal static Entry Get(int id)
        {
            Refresh();
            for (int i = 0; i < _all.Count; i++) if (_all[i].Id == id) return _all[i];
            return null;
        }

        internal static string NameOf(int id)
        {
            Entry e = Get(id);
            return e != null ? e.Name : "Item " + id;
        }

        internal static bool IsExplosive(int id)
        {
            Entry e = Get(id);
            return e != null && e.IsExplosive;
        }

        /// <summary>Consumable, and not a gun - guns are excluded even if their name matches.</summary>
        internal static bool IsConsumable(int id)
        {
            Entry e = Get(id);
            if (e == null) return false;
            if (e.IsWeapon) return false;
            if (e.IsExplosive) return true;
            return ContainsAny((e.Name ?? "").ToLowerInvariant(), ConsumableWords);
        }

        internal static bool IsWeapon(int id)
        {
            Entry e = Get(id);
            return e != null && e.IsWeapon;
        }

        /// <summary>First catalogue entry whose name contains any of the given words, or null.</summary>
        internal static Entry FindByName(params string[] words)
        {
            Refresh();
            for (int i = 0; i < _all.Count; i++)
            {
                string name = (_all[i].Name ?? "").ToLowerInvariant();
                for (int w = 0; w < words.Length; w++)
                    if (name.IndexOf(words[w], StringComparison.OrdinalIgnoreCase) >= 0) return _all[i];
            }
            return null;
        }

        internal static void Invalidate() { _builtFor = -1; }

        /// <summary>Rebuilt only when the prefab array length changes, never per frame.</summary>
        private static void Refresh()
        {
            int count = 0;
            StoreManager store = null;
            try
            {
                store = StoreManager.Instance;
                if (Net.Alive(store) && store.pickupObjs != null) count = store.pickupObjs.Length;
            }
            catch { }

            if (count == 0)
            {
                // Store not loaded (menu / lobby): offer the known ids so the picker is not empty.
                if (_builtFor == 0 && _all.Count > 0) return;
                _all.Clear();
                for (int i = 0; i < FallbackNames.Length; i++) _all.Add(Build(i, FallbackNames[i]));
                _builtFor = 0;
                return;
            }

            if (count == _builtFor && _all.Count > 0) return;

            _all.Clear();
            _builtFor = count;
            for (int i = 0; i < count; i++)
            {
                string name = null;
                try
                {
                    GameObject prefab = store.pickupObjs[i];
                    if (prefab != null) name = prefab.name;
                }
                catch { }
                if (string.IsNullOrEmpty(name) && i < FallbackNames.Length) name = FallbackNames[i];
                if (string.IsNullOrEmpty(name)) name = "Item " + i;
                _all.Add(Build(i, name));
            }
            Log.Msg("Item catalogue built from StoreManager.pickupObjs: " + count + " item(s).");
        }

        private static Entry Build(int id, string name)
        {
            string probe = name.ToLowerInvariant();
            // Also consider the old label, so classification still works if a prefab is named oddly.
            if (id < FallbackNames.Length) probe += " " + FallbackNames[id].ToLowerInvariant();

            return new Entry
            {
                Id = id,
                Name = name,
                IsExplosive = ContainsAny(probe, ExplosiveWords),
                IsWeapon = ContainsAny(probe, WeaponWords)
            };
        }

        private static bool ContainsAny(string haystack, string[] needles)
        {
            for (int i = 0; i < needles.Length; i++)
                if (haystack.IndexOf(needles[i], StringComparison.Ordinal) >= 0) return true;
            return false;
        }
    }
}
