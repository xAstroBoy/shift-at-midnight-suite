using System;
using System.Collections.Generic;
using ShiftAtMidnightSuite.Modules;
using ShiftAtMidnightSuite.Util;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.UI.Panels;

namespace ShiftAtMidnightSuite.UI
{
    /// <summary>
    /// The mod menu, built on UniverseLib so it is a real mouse-driven uGUI panel: drag it, resize
    /// it, click a category in the sidebar, click a toggle. No keyboard page cycling.
    /// </summary>
    internal sealed class SuitePanel : PanelBase
    {
        private SuiteMod _mod;

        public override string Name => "Shift At Midnight Suite";
        public override int MinWidth => Ui.S(1280);
        public override int MinHeight => Ui.S(900);

        // Font sizes. The defaults UIFactory uses are tuned for UnityExplorer's dense inspector,
        // which is far too small for a menu you read at a glance while playing.
        private static int FontHeader { get { return Ui.S(23); } }
        private static int FontRow { get { return Ui.S(19); } }
        private static int FontNote { get { return Ui.S(15); } }
        private static int FontNav { get { return Ui.S(20); } }
        private static int FontList { get { return Ui.S(18); } }
        public override Vector2 DefaultAnchorMin => new Vector2(0.5f, 0.5f);
        public override Vector2 DefaultAnchorMax => new Vector2(0.5f, 0.5f);
        public override bool CanDragAndResize => true;

        private readonly List<Category> _categories = new List<Category>();
        private readonly List<ButtonRef> _navButtons = new List<ButtonRef>();
        private GameObject _bodyRoot;
        private Text _statusText;
        private int _active = -1;

        /// <summary>Rows that need their label refreshed each tick (toggle state, live counters).</summary>
        private readonly List<Action> _refreshers = new List<Action>();

        /// <summary>Page index each refresher was built for, parallel to _refreshers.</summary>
        private readonly List<int> _refresherPage = new List<int>();

        /// <summary>Set while a page is being built so its rows can record where they live.</summary>
        private int _buildingPage = -1;

        private void AddRefresher(Action a)
        {
            _refreshers.Add(a);
            _refresherPage.Add(_buildingPage);
        }

        private sealed class Category
        {
            internal string Name;
            internal Action<GameObject> Build;
            internal GameObject Root;
            internal Func<string> Status;
        }

        /// <summary>
        /// Note: PanelBase's constructor calls ConstructUI -> ConstructPanelContent, so the body of
        /// this constructor has not run yet by the time the content is built. Anything the content
        /// needs must come from SuiteMod.Instance, not from a field assigned here.
        /// </summary>
        internal SuitePanel(UIBase owner, SuiteMod mod) : base(owner)
        {
            _mod = mod;
        }

        // ------------------------------------------------------------------ construction

        protected override void ConstructPanelContent()
        {
            DefineCategories();

            UIFactory.SetLayoutGroup<VerticalLayoutGroup>(ContentRoot, true, false, true, true, 4, 4, 4, 4, 4);

            GameObject split = UIFactory.CreateHorizontalGroup(ContentRoot, "Split", true, true, true, true, 6,
                                                               new Vector4(4, 4, 4, 4), new Color(0f, 0f, 0f, 0f));
            UIFactory.SetLayoutElement(split, flexibleWidth: 9999, flexibleHeight: 9999);

            // ---- sidebar: one clickable button per category
            GameObject nav = UIFactory.CreateVerticalGroup(split, "Nav", true, false, true, true, 3,
                                                           new Vector4(4, 4, 4, 4), new Color(0.09f, 0.07f, 0.07f, 1f));
            UIFactory.SetLayoutElement(nav, minWidth: Ui.S(210), flexibleWidth: 0, flexibleHeight: 9999);

            for (int i = 0; i < _categories.Count; i++)
            {
                int index = i;
                ButtonRef btn = UIFactory.CreateButton(nav, "Nav_" + _categories[i].Name, _categories[i].Name);
                btn.ButtonText.fontSize = FontNav;
                UIFactory.SetLayoutElement(btn.GameObject, minHeight: Ui.S(46), flexibleWidth: 9999);
                btn.OnClick += delegate { Show(index); };
                _navButtons.Add(btn);
            }

            GameObject navSpacer = UIFactory.CreateUIObject("NavSpacer", nav);
            UIFactory.SetLayoutElement(navSpacer, flexibleHeight: 9999);

            // ---- body: one scroll view per category, only one active at a time
            GameObject bodyWrap = UIFactory.CreateVerticalGroup(split, "BodyWrap", true, false, true, true, 0,
                                                                default(Vector4), new Color(0.06f, 0.06f, 0.06f, 1f));
            UIFactory.SetLayoutElement(bodyWrap, flexibleWidth: 9999, flexibleHeight: 9999);
            _bodyRoot = bodyWrap;

            for (int i = 0; i < _categories.Count; i++)
            {
                Category cat = _categories[i];
                GameObject scroll = UIFactory.CreateScrollView(bodyWrap, "Page_" + cat.Name, out GameObject content,
                                                               out _, new Color(0.06f, 0.06f, 0.06f, 1f));
                UIFactory.SetLayoutElement(scroll, flexibleWidth: 9999, flexibleHeight: 9999);
                UIFactory.SetLayoutGroup<VerticalLayoutGroup>(content, false, false, true, true, 4, 6, 6, 6, 6);
                cat.Root = scroll;
                _buildingPage = i;
                cat.Build(content);
                _buildingPage = -1;
                scroll.SetActive(false);
            }

            // ---- status strip
            _statusText = UIFactory.CreateLabel(ContentRoot, "Status", "", TextAnchor.MiddleLeft, Ui.Accent);
            _statusText.fontSize = FontNote + 2;
            UIFactory.SetLayoutElement(_statusText.gameObject, minHeight: Ui.S(28), flexibleWidth: 9999);

            Show(0);
        }

        private void Show(int index)
        {
            if (index < 0 || index >= _categories.Count) return;
            _active = index;
            for (int i = 0; i < _categories.Count; i++)
            {
                if (_categories[i].Root != null) _categories[i].Root.SetActive(i == index);
                if (i < _navButtons.Count)
                    _navButtons[i].ButtonText.color = i == index ? Ui.Accent : Ui.Text;
            }
        }

        // ------------------------------------------------------------------ row builders

        private void Header(GameObject parent, string text)
        {
            Text label = UIFactory.CreateLabel(parent, "Header", text, TextAnchor.MiddleLeft, Ui.Accent);
            label.fontStyle = FontStyle.Bold;
            label.fontSize = FontHeader;
            UIFactory.SetLayoutElement(label.gameObject, minHeight: Ui.S(34), flexibleWidth: 9999);
        }

        private void Note(GameObject parent, string text)
        {
            Text label = UIFactory.CreateLabel(parent, "Note", text, TextAnchor.MiddleLeft, Ui.TextDim);
            label.fontSize = FontNote;
            UIFactory.SetLayoutElement(label.gameObject, minHeight: Ui.S(24), flexibleWidth: 9999);
        }

        /// <summary>A real checkbox - click it, no key presses.</summary>
        private void Toggle(GameObject parent, string label, string help, Func<bool> get, Action<bool> set)
        {
            GameObject row = UIFactory.CreateHorizontalGroup(parent, "Row", false, false, true, true, 6,
                                                            new Vector4(2, 2, 2, 2), new Color(0f, 0f, 0f, 0f));
            UIFactory.SetLayoutElement(row, minHeight: Ui.S(36), flexibleWidth: 9999);

            UIFactory.CreateToggle(row, "Toggle", out Toggle toggle, out Text text);
            UIFactory.SetLayoutElement(toggle.gameObject, minWidth: Ui.S(30), minHeight: Ui.S(30));
            text.text = label;
            text.color = Ui.Text;
            text.fontSize = FontRow;

            toggle.isOn = Safe(get);
            // Use UniverseLib's own extension: it routes through UnityAction<T>.op_Implicit, which
            // is the conversion Il2CppInterop actually supports for managed delegates.
            toggle.onValueChanged.AddListener(new Action<bool>(delegate (bool v)
            {
                try { set(v); } catch (Exception ex) { Log.Ex("toggle " + label, ex); }
            }));

            // Another code path (a preset button, a config load) can change the value behind us.
            AddRefresher(delegate
            {
                bool now = Safe(get);
                if (toggle.isOn != now) toggle.isOn = now;
            });

            if (!string.IsNullOrEmpty(help)) Note(parent, "     " + help);
        }

        /// <summary>Value row with -/+ buttons and a live readout.</summary>
        private void Stepper(GameObject parent, string label, string help, Func<string> read, Action<int> adjust,
                             int smallStep = 1, int bigStep = 10)
        {
            GameObject row = UIFactory.CreateHorizontalGroup(parent, "Row", false, false, true, true, 6,
                                                            new Vector4(2, 2, 2, 2), new Color(0f, 0f, 0f, 0f));
            UIFactory.SetLayoutElement(row, minHeight: Ui.S(38), flexibleWidth: 9999);

            Text name = UIFactory.CreateLabel(row, "Name", label, TextAnchor.MiddleLeft, Ui.Text);
            name.fontSize = FontRow;
            UIFactory.SetLayoutElement(name.gameObject, minWidth: Ui.S(300), flexibleWidth: 9999);

            AddStep(row, "<<", -bigStep, adjust);
            AddStep(row, "<", -smallStep, adjust);

            Text value = UIFactory.CreateLabel(row, "Value", Safe(read), TextAnchor.MiddleCenter, Ui.Accent);
            value.fontSize = FontRow;
            UIFactory.SetLayoutElement(value.gameObject, minWidth: Ui.S(170), flexibleWidth: 0);

            AddStep(row, ">", smallStep, adjust);
            AddStep(row, ">>", bigStep, adjust);

            AddRefresher(delegate
            {
                string now = Safe(read);
                if (value.text != now) value.text = now;
            });

            if (!string.IsNullOrEmpty(help)) Note(parent, "     " + help);
        }

        private void AddStep(GameObject row, string caption, int delta, Action<int> adjust)
        {
            ButtonRef b = UIFactory.CreateButton(row, "Step" + caption + delta, caption);
            b.ButtonText.fontSize = FontRow;
            UIFactory.SetLayoutElement(b.GameObject, minWidth: Ui.S(48), minHeight: Ui.S(32), flexibleWidth: 0);
            b.OnClick += delegate
            {
                try { adjust(delta); } catch (Exception ex) { Log.Ex("adjust", ex); }
            };
        }

        private void Button(GameObject parent, string label, string help, Action click, bool hostOnly = false)
        {
            ButtonRef b = UIFactory.CreateButton(parent, "Btn_" + label, label);
            b.ButtonText.fontSize = FontRow;
            UIFactory.SetLayoutElement(b.GameObject, minHeight: Ui.S(38), flexibleWidth: 9999);
            b.OnClick += delegate
            {
                try { click(); } catch (Exception ex) { Log.Ex(label, ex); }
            };

            if (hostOnly)
            {
                AddRefresher(delegate
                {
                    bool host = Net.IsHost;
                    b.ButtonText.color = host ? Ui.Text : Ui.TextDim;
                    b.ButtonText.text = host ? label : label + "  (host only)";
                });
            }

            if (!string.IsNullOrEmpty(help)) Note(parent, "     " + help);
        }

        /// <summary>
        /// A searchable, clickable picker. Rebuilt on demand rather than per frame, and capped so a
        /// long catalogue cannot spawn hundreds of buttons at once.
        /// </summary>
        /// <summary>
        /// Scrollable, searchable list of clickable rows.
        ///
        /// When <paramref name="getSelected"/> is null the list is in immediate mode: clicking a row
        /// performs the action there and then.
        ///
        /// Rows are a pool. The first version destroyed and recreated every button whenever the
        /// source list changed size, which on a 200-row list is close to a full second of frozen
        /// game - and it was doing it for hidden pages too. Now buttons are created once, reused,
        /// retexted, and hidden when there are more of them than rows; a rebuild only happens when
        /// the rendered names actually differ.
        /// </summary>
        private void Picker(GameObject parent, string label, Func<List<string>> items, Func<int> getSelected,
                            Action<int> onPick)
        {
            Header(parent, label);

            InputFieldRef search = UIFactory.CreateInputField(parent, "Search", "Search...");
            UIFactory.SetLayoutElement(search.GameObject, minHeight: Ui.S(34), flexibleWidth: 9999);

            GameObject list = UIFactory.CreateVerticalGroup(parent, "PickerList", true, false, true, true, 3,
                                                            new Vector4(3, 3, 3, 3), new Color(0.04f, 0.04f, 0.04f, 1f));
            UIFactory.SetLayoutElement(list, minHeight: Ui.S(360), flexibleHeight: 9999, flexibleWidth: 9999);
            bool immediate = getSelected == null;

            const int MaxRows = 200;
            var pool = new List<ButtonRef>();      // every button ever created for this list
            var indices = new List<int>();         // source index shown in pool[i], parallel
            var lastNames = new List<string>();    // what is currently rendered, for change detection
            string lastFilter = null;
            bool dirty = true;                     // forces the first fill

            Action fill = delegate
            {
                string filter = (search.Text ?? "").Trim();
                List<string> all = items();

                // Filter into the visible set.
                var names = new List<string>();
                var src = new List<int>();
                for (int i = 0; i < all.Count && names.Count < MaxRows; i++)
                {
                    if (filter.Length > 0 && all[i].IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    names.Add(all[i]);
                    src.Add(i);
                }

                // Nothing changed on screen? Then nothing to do - this is the common case.
                if (!dirty && filter == lastFilter && names.Count == lastNames.Count)
                {
                    bool same = true;
                    for (int i = 0; i < names.Count; i++)
                        if (!string.Equals(names[i], lastNames[i], StringComparison.Ordinal)) { same = false; break; }
                    if (same) return;
                }
                dirty = false;
                lastFilter = filter;

                // Grow the pool only as far as needed; created buttons are never destroyed.
                while (pool.Count < names.Count)
                {
                    int slot = pool.Count;
                    ButtonRef b = UIFactory.CreateButton(list, "Pick" + slot, "");
                    b.ButtonText.fontSize = FontList;
                    UIFactory.SetLayoutElement(b.GameObject, minHeight: Ui.S(32), flexibleWidth: 9999);
                    // Read the current index at click time, since the slot's contents get reused.
                    b.OnClick += delegate
                    {
                        if (slot < indices.Count && indices[slot] >= 0)
                        {
                            try { onPick(indices[slot]); } catch (Exception ex) { Log.Ex("list click", ex); }
                        }
                    };
                    pool.Add(b);
                    indices.Add(-1);
                }

                for (int i = 0; i < pool.Count; i++)
                {
                    if (i < names.Count)
                    {
                        if (pool[i].ButtonText.text != names[i]) pool[i].ButtonText.text = names[i];
                        indices[i] = src[i];
                        if (!pool[i].GameObject.activeSelf) pool[i].GameObject.SetActive(true);
                    }
                    else
                    {
                        indices[i] = -1;
                        if (pool[i].GameObject.activeSelf) pool[i].GameObject.SetActive(false);
                    }
                }

                lastNames = names;
            };

            search.OnValueChanged += delegate { dirty = true; fill(); };
            fill();

            float nextCheck = 0f;
            AddRefresher(delegate
            {
                // Checking the source list every frame means every module getter runs every frame
                // while the menu is open. Twice a second is plenty for a list.
                if (Time.unscaledTime >= nextCheck)
                {
                    nextCheck = Time.unscaledTime + 0.5f;
                    fill();
                }
                if (immediate) return;              // nothing to highlight - every row is a button
                int sel = getSelected();
                for (int i = 0; i < pool.Count && i < lastNames.Count; i++)
                {
                    Color want = indices[i] == sel ? Ui.Accent : Ui.Text;
                    if (pool[i].ButtonText.color != want) pool[i].ButtonText.color = want;
                }
            });
        }

        private static bool Safe(Func<bool> f)
        {
            try { return f(); } catch { return false; }
        }

        private static string Safe(Func<string> f)
        {
            try { return f() ?? ""; } catch { return ""; }
        }

        // ------------------------------------------------------------------ categories

        private void DefineCategories()
        {
            // Runs from the base constructor, before _mod is assigned - see the constructor note.
            SuiteMod mod = _mod ?? SuiteMod.Instance;
            if (mod == null) throw new InvalidOperationException("SuiteMod instance is not available yet.");
            _mod = mod;

            PlayerModule p = mod.Player;
            SpawnerModule sp = mod.Spawner;
            WorldModule w = mod.World;
            CleanModule c = mod.Clean;
            StockModule st = mod.Stock;
            RegisterModule reg = mod.Counter;
            PlayersModule pl = mod.Players;
            DetectorModule d = mod.Detector;
            ComfortModule cm = mod.Comfort;

            _categories.Add(new Category
            {
                Name = "Player",
                Build = delegate (GameObject root)
                {
                    Header(root, "SURVIVAL");
                    Toggle(root, "Infinite Stamina", "Never runs out of stamina.",
                        delegate { return p.InfiniteStamina; }, delegate (bool v) { p.InfiniteStamina = v; });
                    Toggle(root, "God Mode", "Health pinned to max; no death, no downed state.",
                        delegate { return p.GodMode; }, delegate (bool v) { p.GodMode = v; });

                    Header(root, "RESOURCES");
                    Toggle(root, "Infinite Money", "Tops up all three wallets: the store balance, arcade tokens, and story mode's personal funds - the one the vet's bills come out of.",
                        delegate { return p.InfiniteMoney; }, delegate (bool v) { p.InfiniteMoney = v; });
                    Stepper(root, "Wallet Floor", "The level all three wallets are kept at.",
                        delegate { return p.FundsFloor.ToString("N0"); },
                        delegate (int dir)
                        {
                            p.FundsFloor = Mathf.Clamp(p.FundsFloor + dir * 50000, 10000, 9000000);
                            p.MoneyFloor = p.FundsFloor;
                        }, 1, 4);
                    Toggle(root, "Infinite Store Refreshes", "Keeps the shop refresh counter topped up.",
                        delegate { return p.InfiniteRefreshes; }, delegate (bool v) { p.InfiniteRefreshes = v; });
                    Toggle(root, "Infinite Ammo", "Restores a weapon slot whenever it drops below what it held.",
                        delegate { return p.InfiniteAmmo; }, delegate (bool v) { p.InfiniteAmmo = v; });
                    Stepper(root, "Ammo Ceiling", "0 = safest (only refill to what the gun actually held). Higher can break reloads.",
                        delegate { return p.AmmoTarget == 0 ? "auto" : p.AmmoTarget.ToString(); },
                        delegate (int dir) { p.AmmoTarget = Mathf.Clamp(p.AmmoTarget + dir, 0, 999); });
                    Toggle(root, "Freeze Consumables", "Grenades, molotovs, bricks and other single-use items stop being used up. Guns are left alone.",
                        delegate { return p.FreezeItems; }, delegate (bool v) { p.FreezeItems = v; });
                    Stepper(root, "Inventory Slots", "Up to 8. The game only builds 4, so anything above that has extra slots created for it.",
                        delegate
                        {
                            int saved = p.SavedSlots;
                            string s = saved > 0 ? "  (save: " + saved + ")" : "";
                            return (p.MaxSlots == 0 ? "default" : p.MaxSlots.ToString()) + s;
                        },
                        delegate (int dir) { p.MaxSlots = Mathf.Clamp(p.MaxSlots + dir, 0, 8); }, 1, 4);
                    Button(root, "Write Slot Count To Save", "Makes the number above the game's permanent value - no more resync fighting.",
                        delegate { p.WriteSlotsToSave(); });
                    Button(root, "Reset Slots To Save Value", "Stops overriding and re-applies whatever the save says.",
                        delegate { p.ResetSlotsToSave(); });

                    Header(root, "MOVEMENT");
                    Stepper(root, "Move Speed", "Multiplies walk, run and crouch speed.",
                        delegate { return p.SpeedMultiplier.ToString("0.00") + "x"; },
                        delegate (int dir) { p.SpeedMultiplier = Mathf.Clamp(p.SpeedMultiplier + 0.25f * dir, 0.25f, 6f); }, 1, 4);
                    Stepper(root, "Jump Height", "Multiplies jump power.",
                        delegate { return p.JumpMultiplier.ToString("0.00") + "x"; },
                        delegate (int dir) { p.JumpMultiplier = Mathf.Clamp(p.JumpMultiplier + 0.25f * dir, 0.25f, 6f); }, 1, 4);
                    Toggle(root, "No-Clip", "Fly through walls. WASD / SPACE / CTRL, SHIFT to boost.",
                        delegate { return p.Noclip; },
                        delegate (bool v) { p.Noclip = v; p.OnNoclipChanged(v); });
                    Stepper(root, "No-Clip Speed", "",
                        delegate { return p.NoclipSpeed.ToString("0"); },
                        delegate (int dir) { p.NoclipSpeed = Mathf.Clamp(p.NoclipSpeed + dir, 3f, 40f); }, 1, 5);
                },
                Status = delegate { return Net.IsHost ? "HOST" : "CLIENT"; }
            });

            _categories.Add(new Category
            {
                Name = "Comfort",
                Build = delegate (GameObject root)
                {
                    Header(root, "CUSTOMERS");
                    Toggle(root, "Max Customer Patience", "Their patience bar is held full, so nobody walks out on you.",
                        delegate { return cm.MaxPatience; }, delegate (bool v) { cm.MaxPatience = v; });
                    Stepper(root, "Patience Allowance", "Widens each customer's own timer as well as pinning it.",
                        delegate { return cm.PatienceFactor.ToString("0.0") + "x"; },
                        delegate (int dir) { cm.PatienceFactor = Mathf.Clamp(cm.PatienceFactor + dir, 1f, 20f); }, 1, 5);
                    Toggle(root, "Customers Always Happy", "No parting complaint when one does leave. Pair with Always Perfect Reviews on the World page.",
                        delegate { return cm.HappyCustomers; }, delegate (bool v) { cm.HappyCustomers = v; });

                    Toggle(root, "Honest Stock Rating", "Recounts what is really on the shelves and republishes it, so nobody complains about an empty shelf that is full.",
                        delegate { return cm.HonestStock; }, delegate (bool v) { cm.HonestStock = v; });

                    Toggle(root, "Remove Store Curbs", "Turns off the curb colliders you keep catching on - for everything, not just you.",
                        delegate { return cm.RemoveCurbs; }, delegate (bool v) { cm.RemoveCurbs = v; });
                    Toggle(root, "Remove Chain Fences", "Same for the chain fences and their gates.",
                        delegate { return cm.RemoveFences; }, delegate (bool v) { cm.RemoveFences = v; });
                    Toggle(root, "Remove Forest Roof", "Same for the ForestRoof collider overhead.",
                        delegate { return cm.RemoveForestRoof; }, delegate (bool v) { cm.RemoveForestRoof = v; });
                    Toggle(root, "Silence Bells", "Mutes the entry-door chime and the counter bell. The door still works exactly as before, it just shuts up.",
                        delegate { return cm.SilenceBells; }, delegate (bool v) { cm.SilenceBells = v; });

                    Toggle(root, "Auto-Unlock Items After Talking", "A conversation pauses item use and the dialogue never unpauses it. This puts your hands back once you are free.",
                        delegate { return cm.AutoUnlockInventory; }, delegate (bool v) { cm.AutoUnlockInventory = v; });

                    Toggle(root, "Humans Invincible", "Customers are untouchable and troublemakers can only be wounded. Robbers, doppelgangers, monsters and the entity are all still fair game.",
                        delegate { return HumanShield.Enabled; }, delegate (bool v) { HumanShield.Enabled = v; });
                    Toggle(root, "Wounded Run Away", "A blow that would have killed sends them fleeing the store instead of leaving them standing there unharmed.",
                        delegate { return HumanShield.WoundAndFlee; }, delegate (bool v) { HumanShield.WoundAndFlee = v; });

                    Header(root, "END OF DAY");
                    Toggle(root, "Fast Customer Report", "Runs the whole end-of-day screen faster - the customer report and the money counter both. The figures are unchanged, they just stop crawling.",
                        delegate { return cm.FastEndOfDay; }, delegate (bool v) { cm.FastEndOfDay = v; });
                    Stepper(root, "Report Speed", "How much faster, while the report is on screen.",
                        delegate { return cm.EndOfDaySpeed.ToString("0.0") + "x"; },
                        delegate (int dir) { cm.EndOfDaySpeed = Mathf.Clamp(cm.EndOfDaySpeed + dir, 1f, 10f); }, 1, 4);

                    Header(root, "CHORES");
                    Toggle(root, "Vents Don't Kick", "Stops the vent throwing you out for staying in it. Getting in and out still works normally.",
                        delegate { return cm.VentsDontKick; }, delegate (bool v) { cm.VentsDontKick = v; });
                    Toggle(root, "No Hint Popups", "Blocks the HUD nags - \"remember to...\", \"ask the driver...\" - at the queue, so they never appear.",
                        delegate { return cm.AutoDismissHints; }, delegate (bool v) { cm.AutoDismissHints = v; });
                    Toggle(root, "Instant Traps / Boarding", "Removes the hold timer on placing traps, disarming them and boarding doors.",
                        delegate { return cm.InstantTasks; }, delegate (bool v) { cm.InstantTasks = v; });
                    Toggle(root, "Auto-Fuel Cars", "Opens the fuel flap and fills a car's petrol order as soon as it is asked for.",
                        delegate { return cm.AutoFuel; }, delegate (bool v) { cm.AutoFuel = v; });

                    Header(root, "SCANNING");
                    Toggle(root, "Instant Emoti-Scope", "The scan bar fills at once. The game's own threshold still fires, so the emotion reads out the usual way, just sooner.",
                        delegate { return cm.InstantEmotiscope; }, delegate (bool v) { cm.InstantEmotiscope = v; });
                    Toggle(root, "Reveal Anomaly Forms", "Shows the second figure the anomaly lens reveals, without holding the lens. It is a whole duplicate rig sat on the GhostCamera layer, which the player camera does not draw; this moves it onto a layer that is drawn. The jumpscare creature and the doppelganger with no visible body are both left alone.",
                        delegate { return cm.RevealScanOnly; }, delegate (bool v) { cm.RevealScanOnly = v; });
                    Button(root, "Dump Doppelganger Rig To Log", "The lens only affects doppelgangers, so the hidden figure is on them. This prints every child object and renderer of the doppelgangers in the store, with their on/off state. Press it with one in the shop and send the output - that is what makes the reveal work.",
                        delegate { cm.DumpDoppelgangerRig(); });
                    Button(root, "Dump Anomaly Lens To Log", "The lens object itself, its components and the camera masks. Secondary - the doppelganger dump above is the useful one.",
                        delegate { cm.DumpAnomalyLens(); });

                    Header(root, "VISION");
                    Toggle(root, "No Fog", "Removes the fog entirely. Independent of fullbright.",
                        delegate { return cm.NoFog; }, delegate (bool v) { cm.NoFog = v; });
                    Toggle(root, "Fullbright", "Flat white ambient, fog off, extra exposure. Restored when switched off.",
                        delegate { return cm.Fullbright; }, delegate (bool v) { cm.Fullbright = v; });
                    Stepper(root, "Brightness Boost", "Extra exposure in EV while fullbright is on.",
                        delegate { return "+" + cm.BrightBoost.ToString("0.0") + " EV"; },
                        delegate (int dir)
                        {
                            cm.BrightBoost = Mathf.Clamp(cm.BrightBoost + 0.5f * dir, 0f, 8f);
                            cm.OnBrightBoostChanged();
                        }, 1, 2);

                    Header(root, "HUNT");
                    Button(root, "Kill Extra Monsters", "Kills everything the hunt spawned except the entity itself.",
                        delegate { cm.KillHuntExtras(true, false); }, true);
                    Button(root, "Kill Baby Dolls Only", "Clears just the baby dolls and leaves the rest of the hunt alone.",
                        delegate { cm.KillHuntExtras(true, true); }, true);
                    Toggle(root, "Auto-Kill Extras", "Keeps clearing them as the entity spawns more.",
                        delegate { return cm.AutoKillExtras; }, delegate (bool v) { cm.AutoKillExtras = v; });
                    Button(root, "Skip Entity Countdown", "Brings the entity in now instead of waiting out the warning delay.",
                        delegate { cm.SkipHuntCountdown(true); }, true);
                    Toggle(root, "Auto-Skip Countdown", "Never wait for the warning: the entity arrives as soon as the hunt starts.",
                        delegate { return cm.AutoSkipHuntCountdown; }, delegate (bool v) { cm.AutoSkipHuntCountdown = v; });
                    Toggle(root, "No Hunt Briefing", "Closes the hunt explanation panels the moment they appear, so there is no timer to sit through once a hunt is on.",
                        delegate { return cm.SkipHuntBriefing; }, delegate (bool v) { cm.SkipHuntBriefing = v; });
                    Note(root, "The entity is identified by its own Hittable flag, so ending the hunt still needs the World page.");
                },
                Status = delegate { return cm.LastResult; }
            });

            _categories.Add(new Category
            {
                Name = "Spawner",
                Build = delegate (GameObject root)
                {
                    Header(root, "SPAWN");
                    Stepper(root, "Amount", "How many copies each spawn drops.",
                        delegate { return sp.Amount.ToString(); },
                        delegate (int dir) { sp.Amount = Mathf.Clamp(sp.Amount + dir, 1, 50); }, 1, 5);
                    Stepper(root, "Item Storage", "Ammo the spawned item carries. Auto gives weapons a full load.",
                        delegate { return sp.StorageLabel; },
                        delegate (int dir) { sp.Storage = Mathf.Clamp(sp.Storage + dir, -1, 999); }, 1, 10);
                    Stepper(root, "Spawn Method", "Auto = drop (inert), then throw for safe items, then local.",
                        delegate { return sp.SpawnMethod.ToString(); },
                        delegate (int dir)
                        {
                            sp.SpawnMethod = (SpawnerModule.Method)Mathf.Clamp((int)sp.SpawnMethod + dir, 0, 3);
                        }, 1, 1);
                    Button(root, "SPAWN AGAIN", "Re-spawns the last item you clicked, using the settings above.",
                        delegate { sp.SpawnSelected(); });

                    // Same rule as the event list: clicking a row does the thing. Amount, storage
                    // and method are set above, so a click is unambiguous.
                    Picker(root, "ITEMS  -  click one to spawn it",
                        delegate
                        {
                            var names = new List<string>();
                            List<Items.Entry> all = sp.Catalogue;
                            for (int i = 0; i < all.Count; i++)
                            {
                                string tag = all[i].IsExplosive ? "  [explosive]" : (all[i].IsWeapon ? "  [weapon]" : "");
                                names.Add(all[i].Name + tag);
                            }
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            List<Items.Entry> all = sp.Catalogue;
                            if (i < 0 || i >= all.Count) return;
                            sp.Selected = i;
                            sp.Spawn(all[i], Math.Max(1, sp.Amount));
                        });
                },
                Status = delegate { return sp.LastResult; }
            });

            _categories.Add(new Category
            {
                Name = "Weapons",
                Build = delegate (GameObject root)
                {
                    Header(root, "MODIFIED WEAPONS");
                    Button(root, "Spawn Grenade Launcher", "Drops a flamethrower, fuels it and switches it to lobbing impact grenades. Pick it up and hold fire.",
                        delegate { sp.SpawnGrenadeFlamethrower(); });
                    Toggle(root, "Grenade Launcher Mode", "While on, holding fire on the flamethrower lobs live grenades from the muzzle.",
                        delegate { return WeaponMods.GrenadeFlamethrower; },
                        delegate (bool v) { WeaponMods.GrenadeFlamethrower = v; });
                    Stepper(root, "Grenade Fire Rate", "Seconds between bursts. 0 fires on every single frame.",
                        delegate { return WeaponMods.FireInterval <= 0f ? "every frame" : WeaponMods.FireInterval.ToString("0.00") + "s"; },
                        delegate (int dir) { WeaponMods.FireInterval = Mathf.Clamp(WeaponMods.FireInterval + 0.05f * dir, 0f, 3f); }, 1, 4);
                    Stepper(root, "Grenades Per Burst", "How many go out at once, fanned slightly apart.",
                        delegate { return WeaponMods.BurstCount.ToString(); },
                        delegate (int dir) { WeaponMods.BurstCount = Mathf.Clamp(WeaponMods.BurstCount + dir, 1, 10); }, 1, 2);
                    Note(root, "Hold fire with the flamethrower equipped. No flame, no fuel drain.");
                    Stepper(root, "Grenade Speed", "Muzzle velocity. Higher clears the barrel faster.",
                        delegate { return WeaponMods.LaunchForce.ToString("0"); },
                        delegate (int dir) { WeaponMods.LaunchForce = Mathf.Clamp(WeaponMods.LaunchForce + dir, 5f, 120f); }, 1, 5);

                    Header(root, "WEAPONS  -  spawned loaded, networked");
                    Stepper(root, "Amount", "How many per click.",
                        delegate { return sp.Amount.ToString(); },
                        delegate (int dir) { sp.Amount = Mathf.Clamp(sp.Amount + dir, 1, 50); }, 1, 5);
                    Stepper(root, "Ammo", "Auto = full load.",
                        delegate { return sp.StorageLabel; },
                        delegate (int dir) { sp.Storage = Mathf.Clamp(sp.Storage + dir, -1, 999); }, 1, 10);

                    Picker(root, "WEAPONS  -  click one to spawn it",
                        delegate
                        {
                            var names = new List<string>();
                            List<Items.Entry> all = sp.Catalogue;
                            for (int i = 0; i < all.Count; i++)
                                if (all[i].IsWeapon || all[i].IsExplosive)
                                    names.Add(all[i].Name + (all[i].IsExplosive ? "   [explosive]" : ""));
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            var picks = new List<Items.Entry>();
                            List<Items.Entry> all = sp.Catalogue;
                            for (int k = 0; k < all.Count; k++) if (all[k].IsWeapon || all[k].IsExplosive) picks.Add(all[k]);
                            if (i < 0 || i >= picks.Count) return;
                            sp.Selected = picks[i].Id;
                            sp.Spawn(picks[i], Math.Max(1, sp.Amount));
                        });
                },
                Status = delegate { return sp.LastResult; }
            });

            _categories.Add(new Category
            {
                Name = "Players",
                Build = delegate (GameObject root)
                {
                    Header(root, "TARGET");
                    Picker(root, "PLAYERS  -  click one to target it",
                        delegate
                        {
                            var names = new List<string>();
                            List<PlayersModule.Target> all = pl.Players;
                            for (int i = 0; i < all.Count; i++)
                                names.Add((i == pl.Selected ? ">> " : "    ") + all[i].Name + (all[i].IsLocal ? "   (you)" : ""));
                            if (names.Count == 0) names.Add("(no players yet - enter a shift)");
                            return names;
                        },
                        delegate { return pl.Selected; },
                        delegate (int i) { pl.Selected = i; });

                    Header(root, "HURT");
                    Stepper(root, "Damage Amount", "",
                        delegate { return pl.DamageAmount.ToString("0"); },
                        delegate (int dir) { pl.DamageAmount = Mathf.Clamp(pl.DamageAmount + 5f * dir, 5f, 500f); }, 1, 5);
                    Button(root, "Damage", "Rpc_TakeDamage on the target.", delegate { pl.Damage(); }, true);
                    Button(root, "Down", "Puts them in the downed state.", delegate { pl.Down(); }, true);
                    Button(root, "Kill", "Rpc_Die.", delegate { pl.Kill(); }, true);
                    Button(root, "Stuck (wiggle to escape)", "The trap-stuck state.", delegate { pl.Stuck(); }, true);
                    Button(root, "Sic Monsters On Them", "Every live enemy retargets them; scent maxed.", delegate { pl.SicMonsters(); }, true);
                    Button(root, "Max Scent", "Monsters home in on them.", delegate { pl.MaxScent(); }, true);

                    Header(root, "HELP");
                    Button(root, "Heal To Max", "", delegate { pl.Heal(); }, true);
                    Button(root, "Revive", "", delegate { pl.Revive(); }, true);
                    Button(root, "Respawn", "", delegate { pl.Respawn(); }, true);
                    Button(root, "Clear Scent", "", delegate { pl.ClearScent(); }, true);

                    Header(root, "MOVE");
                    Button(root, "Teleport Them To Me", "", delegate { pl.TeleportToMe(); }, true);
                    Button(root, "Teleport Me To Them", "", delegate { pl.TeleportMeTo(); }, true);
                    Button(root, "Teleport Them To Store", "", delegate { pl.TeleportToStore(); }, true);

                    Header(root, "INVENTORY");
                    Button(root, "Drop Everything", "", delegate { pl.DropEverything(); }, true);
                    Button(root, "Explode Held Item", "", delegate { pl.Explode(); }, true);
                    Button(root, "Jam Gun", "", delegate { pl.JamGun(true); }, true);
                    Button(root, "Unjam Gun", "", delegate { pl.JamGun(false); }, true);
                    Button(root, "Pulverize", "", delegate { pl.Pulverize(); }, true);
                    Button(root, "Inventory Slots = 1", "", delegate { pl.SetSlots(1); }, true);
                    Button(root, "Inventory Slots = 6", "", delegate { pl.SetSlots(6); }, true);

                    Header(root, "MISC");
                    Button(root, "Rename To \"Doppelganger\"", "", delegate { pl.Rename("Doppelganger"); }, true);
                    Button(root, "Hunt Light On", "", delegate { pl.HuntLight(true); }, true);
                    Button(root, "Hunt Light Off", "", delegate { pl.HuntLight(false); }, true);
                },
                Status = delegate { return pl.LastResult; }
            });

            _categories.Add(new Category
            {
                Name = "Doppels",
                Build = delegate (GameObject root)
                {
                    Header(root, "SPAWN");
                    Button(root, "Spawn 1 Doppelganger", "A copy of a player.", delegate { w.SpawnDoppelganger(1); }, true);
                    Button(root, "Spawn 3 Doppelgangers", "", delegate { w.SpawnDoppelganger(3); }, true);
                    Button(root, "Spawn Doppelganger Horde", "The eight-doppelganger event.",
                        delegate { w.SpawnDoppelgangerHorde(); }, true);

                    Header(root, "DETECTOR");
                    Toggle(root, "Doppelganger Detector", "Warns when you look at one, with the authored reason text.",
                        delegate { return d.Enabled; }, delegate (bool v) { d.Enabled = v; });
                    Toggle(root, "Doppelganger Radar", "Also counts every doppelganger nearby.",
                        delegate { return d.Radar; }, delegate (bool v) { d.Radar = v; });
                    Toggle(root, "Show Detection Evidence", "Lists what identified them - ID name, flags, source.",
                        delegate { return d.ShowEvidence; }, delegate (bool v) { d.ShowEvidence = v; });
                    Toggle(root, "Corner Badge", "Top-right tag while the detector is armed.",
                        delegate { return d.ShowBadge; }, delegate (bool v) { d.ShowBadge = v; });
                    Stepper(root, "Warning Text Size", "Size of the in-world doppelganger warning.",
                        delegate { return d.OverlayScale.ToString("0.0") + "x"; },
                        delegate (int dir) { d.OverlayScale = Mathf.Clamp(d.OverlayScale + 0.1f * dir, 0.5f, 3f); }, 1, 3);

                    Picker(root, "DOPPELGANGERS BY NAME  -  click one to spawn it (networked)",
                        delegate
                        {
                            var names = new List<string>();
                            List<WorldModule.NpcAsset> all = w.NpcAssets;
                            for (int i = 0; i < all.Count; i++)
                                if (all[i].IsDoppelganger) names.Add(all[i].Name);
                            if (names.Count == 0) names.Add("(roster not loaded yet - enter a shift)");
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            var picks = new List<WorldModule.NpcAsset>();
                            List<WorldModule.NpcAsset> all = w.NpcAssets;
                            for (int k = 0; k < all.Count; k++) if (all[k].IsDoppelganger) picks.Add(all[k]);
                            if (i >= 0 && i < picks.Count) w.SpawnNpcAsset(picks[i]);
                        });

                },
                Status = delegate { return w.LastResult; }
            });

            _categories.Add(new Category
            {
                Name = "Creatures",
                Build = delegate (GameObject root)
                {
                    Picker(root, "MONSTERS / BOSSES  -  click one to spawn it",
                        delegate
                        {
                            var names = new List<string>();
                            List<WorldModule.NpcChoice> all = w.Monsters;
                            for (int i = 0; i < all.Count; i++) names.Add(all[i].Name);
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            List<WorldModule.NpcChoice> all = w.Monsters;
                            if (i >= 0 && i < all.Count) w.SpawnMonster(all[i]);
                        });

                    Picker(root, "SCRIPTED CUSTOMERS (with ID cards, no doppelgangers)  -  click one to spawn it",
                        delegate
                        {
                            var names = new List<string>();
                            List<WorldModule.NpcAsset> all = w.NpcAssets;
                            for (int i = 0; i < all.Count; i++)
                                if (!all[i].IsDoppelganger) names.Add(all[i].Name);
                            if (names.Count == 0) names.Add("(roster not loaded yet - enter a shift)");
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            var picks = new List<WorldModule.NpcAsset>();
                            List<WorldModule.NpcAsset> all = w.NpcAssets;
                            for (int k = 0; k < all.Count; k++) if (!all[k].IsDoppelganger) picks.Add(all[k]);
                            if (i >= 0 && i < picks.Count) w.SpawnNpcAsset(picks[i]);
                        });

                    Picker(root, "AMBIENT SHOPPERS (no ID, never doppelgangers)  -  click one to spawn it",
                        delegate
                        {
                            var names = new List<string>();
                            List<WorldModule.NpcChoice> all = w.NpcChoices;
                            for (int i = 0; i < all.Count; i++)
                                names.Add(all[i].Name + (all[i].IsDoppelganger ? "   [DOPPELGANGER]" : ""));
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            List<WorldModule.NpcChoice> all = w.NpcChoices;
                            if (i >= 0 && i < all.Count) w.SpawnSpecificNpc(all[i]);
                        });

                    Picker(root, "PETS  -  click one to spawn it",
                        delegate
                        {
                            var names = new List<string>();
                            List<WorldModule.NpcChoice> all = w.Pets;
                            for (int i = 0; i < all.Count; i++) names.Add(all[i].Name);
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            List<WorldModule.NpcChoice> all = w.Pets;
                            if (i >= 0 && i < all.Count) w.SpawnPet(all[i]);
                        });

                    Picker(root, "SET PIECES  -  click to switch on / off (cow, dentists, Pubert...)",
                        delegate
                        {
                            var names = new List<string>();
                            List<WorldModule.SetPiece> all = w.SetPieces;
                            for (int i = 0; i < all.Count; i++) names.Add(all[i].Name);
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            List<WorldModule.SetPiece> all = w.SetPieces;
                            if (i >= 0 && i < all.Count) w.ToggleSetPiece(all[i]);
                        });
                },
                Status = delegate { return w.LastResult; }
            });

            _categories.Add(new Category
            {
                Name = "Events",
                Build = delegate (GameObject root)
                {
                    Picker(root, "EVENTS  -  click one to run it",
                        delegate
                        {
                            var names = new List<string>();
                            List<WorldModule.EventEntry> all = w.Events;
                            for (int i = 0; i < all.Count; i++) names.Add(all[i].Name);
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            List<WorldModule.EventEntry> all = w.Events;
                            if (i >= 0 && i < all.Count) w.Fire(all[i]);
                        });
                },
                Status = delegate { return w.LastResult; }
            });

            _categories.Add(new Category
            {
                Name = "RPC",
                Build = delegate (GameObject root)
                {
                    RpcModule rpc = mod.Rpcs;

                    Header(root, "EVERY NETWORKED CALL IN THE GAME");
                    Note(root, "     Reflected from every NetworkBehaviour. Click to invoke.");
                    Note(root, "     [cmd] entries are client-to-host commands; the rest need state authority.");
                    Button(root, "Dump RPC Inventory To Log", "Every RPC by type, and whether it has a live instance.",
                        delegate { rpc.DumpAll(); });

                    Picker(root, "RPCs  -  search, then click to invoke",
                        delegate
                        {
                            var names = new List<string>();
                            List<RpcModule.Entry> all = rpc.All;
                            for (int i = 0; i < all.Count; i++) names.Add(all[i].Label);
                            return names;
                        },
                        null,
                        delegate (int i)
                        {
                            List<RpcModule.Entry> all = rpc.All;
                            if (i >= 0 && i < all.Count) rpc.Invoke(all[i]);
                        });
                },
                Status = delegate { return mod.Rpcs.LastResult; }
            });

            _categories.Add(new Category
            {
                Name = "Store",
                Build = delegate (GameObject root)
                {
                    Header(root, "REGISTER");
                    Toggle(root, "Auto Bag Items", "Bags everything the customer puts on the counter, so the transaction can complete.",
                        delegate { return reg.AutoBag; }, delegate (bool v) { reg.AutoBag = v; });
                    Button(root, "Bag All Items Now", "One-shot: bag whatever is on the counter right now.",
                        delegate { reg.BagAll(true); });
                    Toggle(root, "Allow Unbagged Checkout", "Register accepts the transaction with items left unbagged. Those items are not paid for.",
                        delegate { return reg.AllowUnbagged; }, delegate (bool v) { reg.AllowUnbagged = v; });
                    Toggle(root, "Non-Doppels Self-Checkout", "Ordinary customers ring themselves up and drive off - register and cars both. Only doppelgangers stay for you.",
                        delegate { return reg.SelfCheckoutNormals; }, delegate (bool v) { reg.SelfCheckoutNormals = v; });
                    Toggle(root, "Clear Counter Leftovers", "Removes whatever is still on the counter once the customer is done. Completing a sale never cleans it up - only cancelling does.",
                        delegate { return reg.ClearLeftovers; }, delegate (bool v) { reg.ClearLeftovers = v; });
                    Button(root, "Complete Transaction Now", "Ends the current transaction immediately, bagged or not.",
                        delegate { reg.CompleteNow(); });

                    Header(root, "STOCKING");
                    Toggle(root, "Auto Stock Shelves", "Fills every incomplete shelf. No crate needed.",
                        delegate { return st.Enabled; }, delegate (bool v) { st.Enabled = v; });
                    Toggle(root, "Only Stock From Held Crate", "Old behaviour: consume a crate you are carrying.",
                        delegate { return st.RequireCrate; }, delegate (bool v) { st.RequireCrate = v; });

                    Header(root, "CLEANING");
                    Toggle(root, "Auto Clean Spills", "Blood, vomit, oil - every Spill, not just blood.",
                        delegate { return c.CleanSpills; }, delegate (bool v) { c.CleanSpills = v; });
                    Toggle(root, "Auto Clean Moppables", "Bathroom blots and any other moppable mess.",
                        delegate { return c.CleanMoppables; }, delegate (bool v) { c.CleanMoppables = v; });
                    Toggle(root, "Auto Clean Trash", "Limbs and junk left on the floor.",
                        delegate { return c.CleanTrash; }, delegate (bool v) { c.CleanTrash = v; });
                    Button(root, "Dump Roach & Token State To Log", "Press this while the roach infestation is running. Prints the countdown, every cleanable in the scene, what a roach actually is, and what a floor token is - which is what the auto-cleaner and the token harvest need in order to pick them up.",
                        delegate { c.DumpRoachState(); });
                    Button(root, "Turn On All Cleaners", "",
                        delegate { c.CleanSpills = true; c.CleanMoppables = true; c.CleanTrash = true; });
                },
                Status = delegate
                {
                    return "register: " + reg.TransactionInfo() + " (" + reg.Bagged + " bagged)   |   stock: " +
                           st.Status + " (" + st.Filled + " added)   |   cleaned  spills " +
                           c.CleanedSpills + "  moppables " + c.CleanedMoppables + "  trash " + c.CleanedTrash;
                }
            });

            _categories.Add(new Category
            {
                Name = "World",
                Build = delegate (GameObject root)
                {
                    Header(root, "ARSENAL");
                    Button(root, "UNLOCK FULL ARSENAL", "Marks every weapon and upgrade purchased, then opens the wall.",
                        delegate { w.UnlockArsenal(); }, true);
                    Toggle(root, "Weapon Wall Always Open", "Emergency arsenal usable in the day, no hunt needed.",
                        delegate { return w.WeaponWallAlwaysOpen; },
                        delegate (bool v) { w.WeaponWallAlwaysOpen = v; if (v) w.OpenWeaponWall(true); });
                    Button(root, "Open Weapon Wall Now", "One-shot: unlock the racks immediately.",
                        delegate { w.OpenWeaponWall(true); }, true);
                    Toggle(root, "Gun Case For Everyone", "Any player can take from any case, any number of times.",
                        delegate { return SuiteMod.GunCaseOpen; }, delegate (bool v) { SuiteMod.GunCaseOpen = v; });

                    Header(root, "PACING");
                    Toggle(root, "Skip Forced Text Reveal", "Text appears at once. EXPERIMENTAL - turn off if customer dialogue stops advancing.",
                        delegate { return SkipReading.Enabled; }, delegate (bool v) { SkipReading.Enabled = v; });
                    Button(root, "Finish Current Objective", "Marks the store objective complete.",
                        delegate { w.FinishObjectiveNow(); }, true);

                    Header(root, "STUCK?");
                    Button(root, "UNSTICK ME", "Closes the computer/shelf screen and releases all control locks.",
                        delegate { w.UnstickPlayer(true); });
                    Toggle(root, "Auto-Unstick When A Hunt Starts", "Steps out of the computer screen as the hunt begins.",
                        delegate { return StartHuntPatch.AutoUnstick; },
                        delegate (bool v) { StartHuntPatch.AutoUnstick = v; });

                    Header(root, "SHIFT CLOCK");
                    Toggle(root, "Endless Night", "The night never ends on its own - no \"your shift is done\", no store shutting down around you. The clock keeps running and winds back before it can expire, so customers and events keep coming. Call The Bus when you actually want to leave.",
                        delegate { return w.EndlessNight; },
                        delegate (bool v) { w.EndlessNight = v; if (v) w.FreezeClock = false; });
                    Stepper(root, "Wind Back To", "How much time the clock is given each time it runs low.",
                        delegate { return w.EndlessTopUpMinutes + " min"; },
                        delegate (int dir) { w.EndlessTopUpMinutes = Mathf.Clamp(w.EndlessTopUpMinutes + dir, 1, 60); }, 1, 5);
                    Button(root, "CALL THE BUS", "Brings the end-of-day bus in now. Board it and the night ends the usual way.",
                        delegate { w.CallBus(); }, true);
                    Toggle(root, "Freeze Shift Clock", "Pins the countdown where it stands. Endless Night is usually what you want instead - a frozen clock also freezes the generation that is scheduled against it.",
                        delegate { return w.FreezeClock; },
                        delegate (bool v) { w.FreezeClock = v; if (v) w.EndlessNight = false; });
                    Button(root, "+ 5 Minutes", "", delegate { w.AddSeconds(300); }, true);
                    Button(root, "+ 15 Minutes", "", delegate { w.AddSeconds(900); }, true);
                    Button(root, "- 1 Minute", "", delegate { w.AddSeconds(-60); }, true);

                    Header(root, "HUNT");
                    Toggle(root, "Hunt Without Entities", "Hunt still runs and pays out, but no monsters are spawned into it.",
                        delegate { return HuntBlock.NoEntities; }, delegate (bool v) { HuntBlock.NoEntities = v; });
                    Toggle(root, "Block Hunts Entirely", "No hunt ever starts.",
                        delegate { return HuntBlock.NoHunt; }, delegate (bool v) { HuntBlock.NoHunt = v; });
                    Button(root, "Send Entities Away Now", "Tells every entity in the world to leave.",
                        delegate { w.ClearHuntEntities(); }, true);
                    Button(root, "Force End Hunt", "Use if an entity-less hunt ever sits waiting.",
                        delegate { w.ForceEndHunt(); });

                    Header(root, "NIGHT");
                    Toggle(root, "Block Rake Spawns", "Stops the rake appearing at all.",
                        delegate { return RakeBlock.Enabled; }, delegate (bool v) { RakeBlock.Enabled = v; });
                    Toggle(root, "Multiply Customers", "Scales the customers scheduled for the night and keeps the store topped up as host.",
                        delegate { return Multipliers.CustomersEnabled; },
                        delegate (bool v) { Multipliers.CustomersEnabled = v; });
                    Stepper(root, "Customer Multiplier", "Applied when the night is generated.",
                        delegate { return "x" + Multipliers.CustomerFactor; },
                        delegate (int dir) { Multipliers.CustomerFactor = Mathf.Clamp(Multipliers.CustomerFactor + dir, 1, 10); }, 1, 3);
                    Toggle(root, "Multiply Events", "Scales the events scheduled for the night.",
                        delegate { return Multipliers.EventsEnabled; },
                        delegate (bool v) { Multipliers.EventsEnabled = v; });
                    Stepper(root, "Event Multiplier", "Applied when the night is generated.",
                        delegate { return "x" + Multipliers.EventFactor; },
                        delegate (int dir) { Multipliers.EventFactor = Mathf.Clamp(Multipliers.EventFactor + dir, 1, 10); }, 1, 3);
                    Stepper(root, "Customer Multiplier", "How busy the store is kept.",
                        delegate { return w.CustomerMultiplier.ToString() + "x"; },
                        delegate (int dir) { w.CustomerMultiplier = Mathf.Clamp(w.CustomerMultiplier + dir, 1, 10); }, 1, 2);
                    Toggle(root, "Extra Nuisance Customers", "Keeps troublemakers in the store instead of the occasional one.",
                        delegate { return w.ExtraNuisances; }, delegate (bool v) { w.ExtraNuisances = v; });
                    Stepper(root, "Nuisances Held", "How many nuisance customers to keep around.",
                        delegate { return w.NuisanceMultiplier.ToString(); },
                        delegate (int dir) { w.NuisanceMultiplier = Mathf.Clamp(w.NuisanceMultiplier + dir, 1, 10); }, 1, 2);
                    Toggle(root, "Extra Doppelgangers", "Sends more doppelgangers in each night, on top of the ones the night scheduled.",
                        delegate { return w.ExtraDoppelgangers; }, delegate (bool v) { w.ExtraDoppelgangers = v; });
                    Stepper(root, "Doppelgangers Per Night", "How many extra to send in.",
                        delegate { return w.DoppelgangerCount.ToString(); },
                        delegate (int dir) { w.DoppelgangerCount = Mathf.Clamp(w.DoppelgangerCount + dir, 1, 10); }, 1, 2);

                    Header(root, "REPUTATION");
                    Toggle(root, "Always Perfect Reviews", "Every customer leaves five stars; hygiene and stock penalties are cleared.",
                        delegate { return PerfectReviews.Enabled; }, delegate (bool v) { PerfectReviews.Enabled = v; });

                    Header(root, "COSMETICS");
                    Button(root, "Unlock All Hats / Cosmetics", "Adds every cosmetic id to the save and writes it.",
                        delegate { w.UnlockHats(); });

                    Header(root, "INTERFACE");
                    Stepper(root, "UI Scale", "Rebuilds the menu at a new size immediately - no game restart.",
                        delegate { return Ui.Scale.ToString("0.00") + "x"; },
                        delegate (int dir)
                        {
                            Ui.Scale = Mathf.Clamp(Ui.Scale + 0.1f * dir, 0.6f, 2.5f);
                            SuiteMod.Instance.RequestUiRebuild();
                        }, 1, 3);

                    Header(root, "DIAGNOSTICS");
                    Button(root, "Dump Network State To Log", "Real Fusion authority: game mode, masks, state/input authority.",
                        delegate { w.DumpNetworkState(); });
                    Button(root, "Dump Item Catalogue To Log", "Prints pickupObjs vs thrownObjs per slot, with components.",
                        delegate { w.DumpItemCatalogue(); });
                    Button(root, "Dump Customer Roster To Log", "Every customer with their real name, id and report text.",
                        delegate { w.DumpNpcRoster(); });
                    Toggle(root, "Profiler", "Logs any module tick over 6 ms and a 10-second summary of where time goes.",
                        delegate { return Profiler.Enabled; }, delegate (bool v) { Profiler.Enabled = v; });
                    Toggle(root, "Verbose Logging", "Writes per-call detail to the MelonLoader console.",
                        delegate { return Log.Verbose; }, delegate (bool v) { Log.Verbose = v; });
                },
                Status = delegate
                {
                    return "clock " + WorldModule.Fmt(w.SecondsLeft) + (w.FreezeClock ? " (frozen)" : "") +
                           "   |   last night  customers " + Multipliers.LastCustomerBase + "->" + Multipliers.LastCustomerScaled +
                           "   events " + Multipliers.LastEventBase + "->" + Multipliers.LastEventScaled +
                           "   rakes blocked " + RakeBlock.Blocked +
                           "   hunt waves blocked " + HuntBlock.BlockedSpawns +
                           "   reviews fixed " + PerfectReviews.Rewritten;
                }
            });
        }

        // ------------------------------------------------------------------ per-frame

        /// <summary>Called from the UIBase update; only refreshes while the panel is visible.</summary>
        internal void Tick()
        {
            if (UIRoot == null || !UIRoot.activeInHierarchy) return;

            for (int i = 0; i < _refreshers.Count; i++)
            {
                // Hidden pages do no work. Before this, every page's lists were being refreshed -
                // and rebuilt - every tick regardless of which one was showing.
                int page = i < _refresherPage.Count ? _refresherPage[i] : -1;
                if (page >= 0 && page != _active) continue;
                try { _refreshers[i](); }
                catch (Exception ex) { Log.Debug("row refresh: " + ex.Message); }
            }

            if (_statusText != null && _active >= 0 && _active < _categories.Count)
            {
                Func<string> s = _categories[_active].Status;
                if (s != null)
                {
                    string now = Safe(s);
                    if (_statusText.text != now) _statusText.text = now;
                }
            }
        }
    }
}
