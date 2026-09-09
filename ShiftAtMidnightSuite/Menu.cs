using System;
using System.Collections.Generic;
using ShiftAtMidnightSuite.Modules;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite
{
    internal sealed class Row
    {
        internal string Label;
        internal string Help;
        internal Func<bool> IsOn;          // toggle rows
        internal Action Activate;          // toggle / action rows
        internal Func<string> Value;       // value rows
        internal Action<int> Adjust;       // value rows
        internal bool HostOnly;

        internal static Row MakeToggle(string label, string help, Func<bool> isOn, Action activate)
        {
            return new Row { Label = label, Help = help, IsOn = isOn, Activate = activate };
        }

        internal static Row MakeValue(string label, string help, Func<string> value, Action<int> adjust)
        {
            return new Row { Label = label, Help = help, Value = value, Adjust = adjust };
        }

        internal static Row MakeAction(string label, string help, Action activate, bool hostOnly = false)
        {
            return new Row { Label = label, Help = help, Activate = activate, HostOnly = hostOnly };
        }
    }

    internal sealed class Page
    {
        internal string Name;
        internal Func<List<Row>> Build;
        internal int Selected;
        internal Action<Rect> DrawExtra;    // picker list drawn below the rows
        internal float ExtraHeight;

        private List<Row> _rows;

        /// <summary>
        /// Rows are built once and reused. Their labels and values are delegates over live module
        /// state, so nothing goes stale - and OnGUI runs several times per frame, so rebuilding the
        /// list and its closures each pass would be pure garbage.
        /// </summary>
        internal List<Row> Rows
        {
            get { return _rows ?? (_rows = Build()); }
        }
    }

    /// <summary>
    /// The whole menu: tabbed pages of rows, keyboard driven, drawn with IMGUI so it works without
    /// touching the game's own canvas.
    /// </summary>
    internal sealed class Menu
    {
        private const float Width = 780f;
        private const float RowHeight = 34f;

        private readonly SuiteMod _mod;
        private readonly List<Page> _pages = new List<Page>();
        private int _page;
        private float _nextKey;

        internal bool Visible;

        internal Menu(SuiteMod mod)
        {
            _mod = mod;
            BuildPages();
        }

        private Page Current { get { return _pages[Mathf.Clamp(_page, 0, _pages.Count - 1)]; } }

        // ---------------------------------------------------------------- pages

        private void BuildPages()
        {
            PlayerModule p = _mod.Player;
            SpawnerModule sp = _mod.Spawner;
            WorldModule w = _mod.World;
            CleanModule c = _mod.Clean;
            StockModule st = _mod.Stock;
            DetectorModule d = _mod.Detector;

            _pages.Add(new Page
            {
                Name = "PLAYER",
                Build = delegate
                {
                    var rows = new List<Row>();
                    rows.Add(Row.MakeToggle("Infinite Stamina", "Never runs out of stamina.",
                        delegate { return p.InfiniteStamina; }, delegate { p.InfiniteStamina = !p.InfiniteStamina; }));
                    rows.Add(Row.MakeToggle("God Mode", "Health is pinned to max; no death, no downed state.",
                        delegate { return p.GodMode; }, delegate { p.GodMode = !p.GodMode; }));
                    rows.Add(Row.MakeToggle("Infinite Money", "Keeps store money and tokens topped up.",
                        delegate { return p.InfiniteMoney; }, delegate { p.InfiniteMoney = !p.InfiniteMoney; }));
                    rows.Add(Row.MakeToggle("Infinite Ammo", "Magazines and reserve never drop below their peak.",
                        delegate { return p.InfiniteAmmo; }, delegate { p.InfiniteAmmo = !p.InfiniteAmmo; }));
                    rows.Add(Row.MakeValue("Ammo Level", "How full Infinite Ammo keeps each weapon slot.",
                        delegate { return p.AmmoTarget.ToString(); },
                        delegate (int dir) { p.AmmoTarget = Mathf.Clamp(p.AmmoTarget + dir, 1, 999); }));
                    rows.Add(Row.MakeToggle("Infinite Store Refreshes", "Keeps the shop refresh counter topped up.",
                        delegate { return p.InfiniteRefreshes; }, delegate { p.InfiniteRefreshes = !p.InfiniteRefreshes; }));
                    rows.Add(Row.MakeToggle("Freeze Item Stacks", "Grenades, molotovs and bricks never get used up.",
                        delegate { return p.FreezeItems; }, delegate { p.FreezeItems = !p.FreezeItems; }));
                    rows.Add(Row.MakeValue("Inventory Slots", "0 keeps the game's own slot count.",
                        delegate { return p.MaxSlots == 0 ? "default" : p.MaxSlots.ToString(); },
                        delegate (int dir) { p.MaxSlots = Mathf.Clamp(p.MaxSlots + dir, 0, 16); }));
                    rows.Add(Row.MakeToggle("No-Clip", "Fly through walls. WASD / SPACE / CTRL, SHIFT to boost.",
                        delegate { return p.Noclip; },
                        delegate { p.Noclip = !p.Noclip; p.OnNoclipChanged(p.Noclip); }));
                    rows.Add(Row.MakeValue("No-Clip Speed", "How fast no-clip moves.",
                        delegate { return p.NoclipSpeed.ToString("0"); },
                        delegate (int dir) { p.NoclipSpeed = Mathf.Clamp(p.NoclipSpeed + dir, 3f, 40f); }));
                    rows.Add(Row.MakeValue("Move Speed", "Multiplies walk, run and crouch speed.",
                        delegate { return p.SpeedMultiplier.ToString("0.0") + "x"; },
                        delegate (int dir) { p.SpeedMultiplier = Mathf.Clamp(p.SpeedMultiplier + 0.25f * dir, 0.25f, 6f); }));
                    rows.Add(Row.MakeValue("Jump Height", "Multiplies jump power.",
                        delegate { return p.JumpMultiplier.ToString("0.0") + "x"; },
                        delegate (int dir) { p.JumpMultiplier = Mathf.Clamp(p.JumpMultiplier + 0.25f * dir, 0.25f, 6f); }));
                    rows.Add(Row.MakeAction("Unlock All Hats / Cosmetics", "Adds every cosmetic id to the save and writes it.",
                        delegate { w.UnlockHats(); }));
                    return rows;
                }
            });

            _pages.Add(new Page
            {
                Name = "SPAWNER",
                ExtraHeight = 300f,
                DrawExtra = DrawSpawnerList,
                Build = delegate
                {
                    var rows = new List<Row>();
                    rows.Add(Row.MakeValue("Item", "Pick an item. LEFT/RIGHT steps, PAGEUP/PAGEDOWN jumps 10.",
                        delegate { Items.Entry e = sp.SelectedEntry; return e == null ? "-" : e.Name; },
                        delegate (int dir) { sp.Move(dir); }));
                    rows.Add(Row.MakeValue("Amount", "How many copies each spawn drops.",
                        delegate { return sp.Amount.ToString(); },
                        delegate (int dir) { sp.Amount = Mathf.Clamp(sp.Amount + dir, 1, 50); }));
                    rows.Add(Row.MakeValue("Item Storage", "Ammo the spawned item carries. Auto gives weapons a full load.",
                        delegate { return sp.StorageLabel; },
                        delegate (int dir) { sp.Storage = Mathf.Clamp(sp.Storage + dir, -1, 999); }));
                    rows.Add(Row.MakeValue("Spawn Method", "Auto = drop (inert), then throw for safe items, then local.",
                        delegate { return sp.SpawnMethod.ToString(); },
                        delegate (int dir)
                        {
                            int v = (int)sp.SpawnMethod + dir;
                            sp.SpawnMethod = (SpawnerModule.Method)Mathf.Clamp(v, 0, 3);
                        }));
                    rows.Add(Row.MakeAction("SPAWN", "Spawns networked when you are host, otherwise asks the host.",
                        delegate { sp.SpawnSelected(); }));
                    return rows;
                }
            });

            _pages.Add(new Page
            {
                Name = "EVENTS",
                ExtraHeight = 300f,
                DrawExtra = DrawEventList,
                Build = delegate
                {
                    var rows = new List<Row>();
                    rows.Add(Row.MakeValue("Event", "Every event the game can run tonight.",
                        delegate { WorldModule.EventEntry e = w.SelectedEntry; return e == null ? "-" : e.Name; },
                        delegate (int dir) { w.MoveEvent(dir); }));
                    rows.Add(Row.MakeAction("FIRE EVENT", "Runs the selected event right now.",
                        delegate { w.FireSelected(); }, true));
                    rows.Add(Row.MakeAction("Spawn 1 Doppelganger", "Spawns a copy of a player.",
                        delegate { w.SpawnDoppelganger(1); }, true));
                    rows.Add(Row.MakeAction("Spawn 3 Doppelgangers", "Three at once.",
                        delegate { w.SpawnDoppelganger(3); }, true));
                    rows.Add(Row.MakeAction("Spawn Doppelganger Horde", "The game's own eight-doppelganger event.",
                        delegate { w.SpawnDoppelgangerHorde(); }, true));
                    rows.Add(Row.MakeToggle("Doppelganger Detector", "Warns when you look at one.",
                        delegate { return d.Enabled; }, delegate { d.Enabled = !d.Enabled; }));
                    rows.Add(Row.MakeToggle("Doppelganger Radar", "Also counts every doppelganger nearby.",
                        delegate { return d.Radar; }, delegate { d.Radar = !d.Radar; }));
                    return rows;
                }
            });

            _pages.Add(new Page
            {
                Name = "STORE",
                Build = delegate
                {
                    var rows = new List<Row>();
                    rows.Add(Row.MakeToggle("Auto Stock Shelves", "Fills every incomplete shelf. No crate needed.",
                        delegate { return st.Enabled; }, delegate { st.Enabled = !st.Enabled; }));
                    rows.Add(Row.MakeToggle("Only Stock From Held Crate", "Old behaviour: consume a crate you are carrying.",
                        delegate { return st.RequireCrate; }, delegate { st.RequireCrate = !st.RequireCrate; }));
                    rows.Add(Row.MakeToggle("Auto Clean Spills", "Blood, vomit, oil - every Spill, not just blood.",
                        delegate { return c.CleanSpills; }, delegate { c.CleanSpills = !c.CleanSpills; }));
                    rows.Add(Row.MakeToggle("Auto Clean Moppables", "Bathroom blots and any other moppable mess.",
                        delegate { return c.CleanMoppables; }, delegate { c.CleanMoppables = !c.CleanMoppables; }));
                    rows.Add(Row.MakeToggle("Auto Clean Trash", "Limbs and junk left on the floor.",
                        delegate { return c.CleanTrash; }, delegate { c.CleanTrash = !c.CleanTrash; }));
                    rows.Add(Row.MakeAction("Clean Everything Now", "Turns all three cleaners on.",
                        delegate { c.CleanSpills = true; c.CleanMoppables = true; c.CleanTrash = true; }));
                    return rows;
                }
            });

            _pages.Add(new Page
            {
                Name = "WORLD",
                Build = delegate
                {
                    var rows = new List<Row>();
                    rows.Add(Row.MakeToggle("Gun Case For Everyone", "Any player can take from any gun case, any number of times.",
                        delegate { return SuiteMod.GunCaseOpen; },
                        delegate { SuiteMod.GunCaseOpen = !SuiteMod.GunCaseOpen; }));
                    rows.Add(Row.MakeToggle("Weapon Wall Always Open", "Emergency arsenal stays usable in the day, no hunt needed.",
                        delegate { return w.WeaponWallAlwaysOpen; },
                        delegate { w.WeaponWallAlwaysOpen = !w.WeaponWallAlwaysOpen; if (w.WeaponWallAlwaysOpen) w.OpenWeaponWall(true); }));
                    rows.Add(Row.MakeAction("Open Weapon Wall Now", "One-shot: unlock the arsenal racks immediately.",
                        delegate { w.OpenWeaponWall(true); }, true));
                    rows.Add(Row.MakeToggle("Block Rake Spawns", "Stops the rake appearing at all.",
                        delegate { return RakeBlock.Enabled; }, delegate { RakeBlock.Enabled = !RakeBlock.Enabled; }));
                    rows.Add(Row.MakeToggle("Multiply Customers", "Scales the customers generated for the night.",
                        delegate { return Multipliers.CustomersEnabled; },
                        delegate { Multipliers.CustomersEnabled = !Multipliers.CustomersEnabled; }));
                    rows.Add(Row.MakeValue("Customer Multiplier", "Applied when the night is generated.",
                        delegate { return "x" + Multipliers.CustomerFactor; },
                        delegate (int dir) { Multipliers.CustomerFactor = Mathf.Clamp(Multipliers.CustomerFactor + dir, 1, 10); }));
                    rows.Add(Row.MakeToggle("Multiply Events", "Scales the events scheduled for the night.",
                        delegate { return Multipliers.EventsEnabled; },
                        delegate { Multipliers.EventsEnabled = !Multipliers.EventsEnabled; }));
                    rows.Add(Row.MakeValue("Event Multiplier", "Applied when the night is generated.",
                        delegate { return "x" + Multipliers.EventFactor; },
                        delegate (int dir) { Multipliers.EventFactor = Mathf.Clamp(Multipliers.EventFactor + dir, 1, 10); }));
                    rows.Add(Row.MakeToggle("Keep Store Busy", "Tops up ambient browsing customers as host.",
                        delegate { return w.ExtraCustomers; }, delegate { w.ExtraCustomers = !w.ExtraCustomers; }));
                    rows.Add(Row.MakeToggle("Verbose Logging", "Writes the per-call detail to the MelonLoader console.",
                        delegate { return Log.Verbose; }, delegate { Log.Verbose = !Log.Verbose; }));
                    return rows;
                }
            });
        }

        // ---------------------------------------------------------------- input

        internal void HandleInput()
        {
            if (!Visible) return;
            Page page = Current;
            List<Row> rows = page.Rows;
            if (rows.Count == 0) return;
            page.Selected = Mathf.Clamp(page.Selected, 0, rows.Count - 1);

            if (Key(KeyCode.Tab)) { _page = (_page + (Input.GetKey(KeyCode.LeftShift) ? _pages.Count - 1 : 1)) % _pages.Count; return; }
            if (Key(KeyCode.UpArrow)) page.Selected = (page.Selected + rows.Count - 1) % rows.Count;
            if (Key(KeyCode.DownArrow)) page.Selected = (page.Selected + 1) % rows.Count;

            Row row = rows[Mathf.Clamp(page.Selected, 0, rows.Count - 1)];

            if (row.Adjust != null)
            {
                if (Key(KeyCode.LeftArrow)) row.Adjust(-1);
                if (Key(KeyCode.RightArrow)) row.Adjust(1);
                if (Key(KeyCode.PageDown)) row.Adjust(-10);
                if (Key(KeyCode.PageUp)) row.Adjust(10);
            }
            if ((Key(KeyCode.Return) || Key(KeyCode.KeypadEnter)) && row.Activate != null) row.Activate();
        }

        /// <summary>Debounced key repeat so holding a key does not fly through the list.</summary>
        private bool Key(KeyCode k)
        {
            if (!Input.GetKeyDown(k)) return false;
            if (Time.unscaledTime < _nextKey) return false;
            _nextKey = Time.unscaledTime + 0.06f;
            return true;
        }

        // ---------------------------------------------------------------- drawing

        internal void Draw()
        {
            if (!Visible) return;
            Ui.EnsureStyles();

            Page page = Current;
            List<Row> rows = page.Rows;

            float height = 150f + rows.Count * RowHeight + 96f + page.ExtraHeight;
            float x = 16f, y = 16f;
            Rect window = new Rect(x, y, Width, height);

            Ui.Box(window, Ui.Panel);
            Ui.Box(new Rect(x, y, Width, 3f), Ui.Accent);
            Ui.Box(new Rect(x, y + height - 3f, Width, 3f), Ui.Accent);
            Ui.Box(new Rect(x, y, 3f, height), Ui.Accent);
            Ui.Box(new Rect(x + Width - 3f, y, 3f, height), Ui.Accent);

            float cx = x + 20f, cw = Width - 40f, cy = y + 14f;

            Ui.Label(new Rect(cx, cy, cw - 130f, 34f), "SHIFT AT MIDNIGHT // SUITE", Ui.Title, Ui.Accent);
            Ui.Label(new Rect(cx + cw - 130f, cy + 6f, 130f, 24f),
                     SuiteMod.Version + (Net.IsHost ? "  HOST" : "  CLIENT"), Ui.Small, Ui.TextDim);
            cy += 38f;
            Ui.Box(new Rect(cx, cy, cw, 2f), Ui.AccentDim);
            cy += 10f;

            // Tab strip
            float tabW = cw / _pages.Count;
            for (int i = 0; i < _pages.Count; i++)
            {
                Rect tr = new Rect(cx + i * tabW, cy, tabW - 4f, 28f);
                bool active = i == _page;
                Ui.Box(tr, active ? Ui.RowSel : new Color(0.08f, 0.07f, 0.07f, 0.9f));
                Ui.Label(tr, _pages[i].Name, active ? Ui.TabActive : Ui.Tab, active ? Ui.Accent : Ui.TextDim);
            }
            cy += 34f;
            Ui.Label(new Rect(cx, cy, cw, 22f),
                     "TAB page   UP/DOWN select   LEFT/RIGHT change   PGUP/PGDN jump 10   ENTER use   F1 close",
                     Ui.Small, Ui.TextDim);
            cy += 28f;
            Ui.Box(new Rect(cx, cy, cw, 2f), Ui.AccentDim);
            cy += 10f;

            page.Selected = rows.Count == 0 ? 0 : Mathf.Clamp(page.Selected, 0, rows.Count - 1);
            for (int i = 0; i < rows.Count; i++)
            {
                DrawRow(cx, cw, cy, rows[i], i == page.Selected);
                cy += RowHeight;
            }

            if (page.DrawExtra != null)
            {
                cy += 6f;
                page.DrawExtra(new Rect(cx, cy, cw, page.ExtraHeight - 10f));
                cy += page.ExtraHeight - 4f;
            }

            // Footer: help for the selected row, then the last thing the mod did.
            Ui.Box(new Rect(cx, cy, cw, 2f), Ui.AccentDim);
            cy += 8f;
            string help = rows.Count > 0 ? rows[page.Selected].Help : "";
            Ui.Label(new Rect(cx + 4f, cy, cw - 8f, 22f), help ?? "", Ui.Small, Ui.Text);
            cy += 24f;
            Ui.Label(new Rect(cx + 4f, cy, cw - 8f, 22f), StatusLine(), Ui.Small, Ui.Accent);
        }

        private void DrawRow(float x, float w, float y, Row row, bool selected)
        {
            if (selected)
            {
                Ui.Box(new Rect(x, y, w, RowHeight - 2f), Ui.RowSel);
                Ui.Box(new Rect(x, y, 5f, RowHeight - 2f), Ui.Accent);
            }

            string marker = selected ? ">>  " : "      ";
            string state;
            if (row.IsOn != null) state = row.IsOn() ? "[ ON ]  " : "[OFF ]  ";
            else if (row.Value != null) state = "";
            else state = "[ GO ]  ";

            string label = marker + state + row.Label;
            if (row.Value != null) label += "   <  " + row.Value() + "  >";

            Color c = selected ? Ui.Accent : Ui.Text;
            if (row.HostOnly && !Net.IsHost) c = Ui.TextDim;

            Ui.Label(new Rect(x + 12f, y + 2f, w - 24f, RowHeight - 4f), label,
                     selected ? Ui.RowSelected : Ui.Row, c);

            if (row.HostOnly && !Net.IsHost)
                Ui.Label(new Rect(x + w - 130f, y + 2f, 120f, RowHeight - 4f), "host only", Ui.Small, Ui.TextDim);
        }

        private void DrawSpawnerList(Rect area)
        {
            SpawnerModule sp = _mod.Spawner;
            List<Items.Entry> items = sp.Catalogue;
            DrawPicker(area, "ITEMS", items.Count, sp.Selected,
                       delegate (int i) { return items[i].Name; });
        }

        private void DrawEventList(Rect area)
        {
            WorldModule w = _mod.World;
            List<WorldModule.EventEntry> events = w.Events;
            DrawPicker(area, "EVENTS", events.Count, w.SelectedEvent,
                       delegate (int i) { return events[i].Name; });
        }

        /// <summary>Windowed list that keeps the selection centred.</summary>
        private static void DrawPicker(Rect area, string title, int count, int selected, Func<int, string> nameAt)
        {
            Ui.Label(new Rect(area.x, area.y, area.width, 22f), title + "  (" + count + ")", Ui.Section, Ui.Accent);
            float top = area.y + 26f;
            float rowH = 24f;
            int visible = Mathf.Max(1, (int)((area.height - 26f) / rowH));

            Ui.Box(new Rect(area.x, top, area.width, visible * rowH), new Color(0.045f, 0.03f, 0.03f, 0.95f));
            if (count == 0)
            {
                Ui.Label(new Rect(area.x + 12f, top + 4f, area.width - 24f, rowH), "(not loaded yet - enter a shift)",
                         Ui.Small, Ui.TextDim);
                return;
            }

            int first = Mathf.Clamp(selected - visible / 2, 0, Mathf.Max(0, count - visible));
            for (int i = 0; i < visible && first + i < count; i++)
            {
                int idx = first + i;
                float ry = top + i * rowH;
                bool sel = idx == selected;
                if (sel)
                {
                    Ui.Box(new Rect(area.x, ry, area.width, rowH), Ui.RowSel);
                    Ui.Box(new Rect(area.x, ry, 4f, rowH), Ui.Accent);
                }
                Ui.Label(new Rect(area.x + 14f, ry, area.width - 28f, rowH),
                         (sel ? ">  " : "   ") + nameAt(idx), Ui.Small, sel ? Ui.Accent : Ui.Text);
            }
        }

        private string StatusLine()
        {
            switch (Current.Name)
            {
                case "SPAWNER": return _mod.Spawner.LastResult;
                case "EVENTS": return _mod.World.LastResult;
                case "STORE":
                    CleanModule c = _mod.Clean;
                    return "stock: " + _mod.Stock.Status + " (" + _mod.Stock.Filled + " added)   |   cleaned  spills " +
                           c.CleanedSpills + "  moppables " + c.CleanedMoppables + "  trash " + c.CleanedTrash;
                case "WORLD":
                    return "last night  customers " + Multipliers.LastCustomerBase + "->" + Multipliers.LastCustomerScaled +
                           "   events " + Multipliers.LastEventBase + "->" + Multipliers.LastEventScaled;
                default: return "";
            }
        }
    }
}
