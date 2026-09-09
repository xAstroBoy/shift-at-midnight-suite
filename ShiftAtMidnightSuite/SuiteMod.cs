using System;
using HarmonyLib;
using MelonLoader;
using ShiftAtMidnightSuite.Loader;
using ShiftAtMidnightSuite.Modules;
using ShiftAtMidnightSuite.UI;
using ShiftAtMidnightSuite.Util;
using UnityEngine;

namespace ShiftAtMidnightSuite
{
    /// <summary>
    /// One mod replacing the old split-up set: the mod menu, the item spawner, the auto-mop, the
    /// auto limb cleaner, the auto shelf filler, the doppelganger detector and the BepInEx gun case
    /// plugin. Everything shares one player lookup, one menu and one config file.
    /// </summary>
    /// <remarks>
    /// Not a MelonMod. The loader (ShiftAtMidnightSuite.Loader) is the mod; this is the hot-reloadable
    /// plugin it hosts. Everything that must survive a reload - UniverseLib registration, file
    /// watching - lives over there. Everything here is torn down in Shutdown and rebuilt in Init.
    /// </remarks>
    public sealed class SuiteMod : ISuitePlugin
    {
        internal const string Version = "1.0.8";
        private const string HarmonyId = "com.xastroboy.shiftatmidnightsuite.plugin";

        private IPluginHost _host;
        private HarmonyLib.Harmony _harmony;

        internal static SuiteMod Instance;

        /// <summary>Read by the Harmony gun case prefixes, which cannot see instance state.</summary>
        internal static bool GunCaseOpen;

        internal readonly PlayerModule Player = new PlayerModule();
        internal readonly SpawnerModule Spawner = new SpawnerModule();
        internal readonly WorldModule World = new WorldModule();
        internal readonly CleanModule Clean = new CleanModule();
        internal readonly StockModule Stock = new StockModule();
        internal readonly RegisterModule Counter = new RegisterModule();
        internal readonly RpcModule Rpcs = new RpcModule();
        internal readonly PlayersModule Players = new PlayersModule();
        internal readonly DetectorModule Detector = new DetectorModule();
        internal readonly ComfortModule Comfort = new ComfortModule();

        private Menu _menu;              // simple IMGUI fallback
        private SuiteUI _ui;              // real mouse-driven UniverseLib menu
        private bool _uiRebuildQueued;

        /// <summary>
        /// Ask for the menu to be rebuilt. Deferred to the next frame because the request usually
        /// comes from a button inside the very panel being destroyed.
        /// </summary>
        internal void RequestUiRebuild() { _uiRebuildQueued = true; }
        private float _nextToggle;
        private bool _guiFailed;
        private int _guiErrors;

        private MelonPreferences_Category _cfg;
        private MelonPreferences_Entry<bool> _pGunCase, _pMulCust, _pMulEvent, _pMulNuisance, _pMulDoppel, _pVerbose, _pWeaponWall, _pNoRake, _pRefresh, _pNoHuntEnt, _pNoHunt, _pReviews, _pSkipRead, _pAutoBag, _pUnbagged, _pClearLeftovers, _pSelfCheckout, _pFreezeClock, _pDetector, _pRadar, _pEvidence, _pBadge, _pProfiler,
                                          _pPatience, _pHappy, _pHonestStock, _pInstantTask, _pAutoFuel, _pKillExtras, _pBright,
                                          _pSkipCountdown, _pSkipBriefing, _pFastEod, _pVentsDontKick, _pNoHints, _pEndlessNight, _pInstantScope, _pRevealScan, _pStamina, _pGod, _pMoney, _pAmmo, _pFreezeItems,
                                          _pCleanSpills, _pCleanMop, _pCleanTrash, _pAutoStock, _pStockCrate, _pGrenadeFlamer, _pSilenceBells, _pNoBarriers, _pNoFences, _pNoRoof, _pAutoUnlock, _pHumanShield, _pNoFog, _pWoundFlee;
        private MelonPreferences_Entry<int> _pCustFactor, _pEventFactor, _pMaxSlots, _pNuisanceFactor, _pDoppelCount, _pEndlessTopUp;
        private MelonPreferences_Entry<float> _pEodSpeed;
        private MelonPreferences_Entry<float> _pPatienceFactor, _pBrightBoost, _pSpeedMul, _pJumpMul, _pNoclipSpeed,
                                                 _pGrenadeRate, _pGrenadeForce;
        private MelonPreferences_Entry<int> _pAmmoTarget, _pFundsFloor, _pSpawnAmount, _pGrenadeBurst;
        private MelonPreferences_Entry<string> _pToggleKey, _pUnstickKey, _pHostileNames;
        private MelonPreferences_Entry<float> _pUiScale, _pOverlayScale;

        /// <summary>Menu hotkey. Configurable because F1 is a popular key for other overlays.</summary>
        private KeyCode _toggleKey = KeyCode.F1;

        /// <summary>Panic key. Works without the menu, which is the point: if a screen has trapped
        /// the cursor you may not be able to reach the menu at all.</summary>
        private KeyCode _unstickKey = KeyCode.F2;
        private float _nextUnstick;
        private float _nextPlayerTick;

        public void Init(IPluginHost host)
        {
            _host = host;
            Instance = this;
            _menu = new Menu(this);
            _ui = new SuiteUI(this, host);
            LoadPrefs();

            // As a melon this was automatic; as a plugin we apply (and later remove) our own.
            try
            {
                _harmony = new HarmonyLib.Harmony(HarmonyId);
                _harmony.PatchAll(typeof(SuiteMod).Assembly);
                Log.Msg("Harmony patches applied.");
            }
            catch (Exception ex) { Log.Err("Harmony patching failed: " + ex); }

            Log.Msg("Shift At Midnight Suite v" + Version + " loaded (hot-reloadable plugin; F3 reloads).");
            Log.Msg(_toggleKey + " opens the menu (mouse-driven; click a category in the sidebar). Change it with MenuKey in MelonPreferences.cfg.");
            Log.Msg(_unstickKey + " force-releases control locks if a screen ever traps you (UnstickKey).");
            Log.Msg("Replaces: ItemSpawnerTrainer, ShiftAtMidnightModMenu, AutoShelfFiller, AutoMop, AutoLimbCleaner, DoppelgangerDetector, GunCaseAllowEveryone.");
        }

        /// <summary>
        /// Get-or-create a preference. After a hot reload the category and its entries already
        /// exist (they belong to the process, not the plugin), and MelonPreferences throws on a
        /// duplicate CreateEntry - which silently aborted the whole LoadPrefs and left every
        /// setting at its hard default. Reusing the entry keeps values across reloads too.
        /// </summary>
        private MelonPreferences_Entry<T> Entry<T>(string id, T fallback, string description)
        {
            MelonPreferences_Entry<T> existing = null;
            try { if (_cfg.HasEntry(id)) existing = _cfg.GetEntry<T>(id); } catch { }
            if (existing != null) return existing;
            return _cfg.CreateEntry(id, fallback, description);
        }

        private void LoadPrefs()
        {
            try
            {
                _cfg = MelonPreferences.CreateCategory("ShiftAtMidnightSuite", "Shift At Midnight Suite");
                _pGunCase = Entry("GunCaseForEveryone", true, "Gun cases usable by everyone");
                _pWeaponWall = Entry("WeaponWallAlwaysOpen", false, "Emergency weapon wall always open");
                _pMulCust = Entry("MultiplyCustomers", true, "Multiply nightly customers");
                _pCustFactor = Entry("CustomerFactor", 3, "Customer multiplier");
                _pMulEvent = Entry("MultiplyEvents", true, "Multiply nightly events");
                _pEventFactor = Entry("EventFactor", 3, "Event multiplier");
                _pMulNuisance = Entry("MultiplyNuisances", false, "Keep extra nuisance customers in the store");
                _pNuisanceFactor = Entry("NuisanceFactor", 2, "How many nuisance customers to hold");
                _pMulDoppel = Entry("MultiplyDoppelgangers", false, "Send extra doppelgangers in each night");
                _pDoppelCount = Entry("DoppelgangerCount", 2, "Extra doppelgangers per night");
                _pMaxSlots = Entry("InventorySlots", 0, "Requested inventory slot count (0 = leave the game alone)");
                _pNoRake = Entry("BlockRakeSpawns", false, "Stop the rake from spawning");
                _pRefresh = Entry("InfiniteStoreRefreshes", false, "Keep store refreshes topped up");
                _pNoHuntEnt = Entry("HuntWithoutEntities", false, "Hunts run but spawn no monsters");
                _pNoHunt = Entry("BlockHunts", false, "No hunt ever starts");
                _pReviews = Entry("AlwaysPerfectReviews", false, "Customers always leave five stars");
                _pSkipRead = Entry("SkipForcedTextReveal", false, "Text appears at once instead of typing out");
                _pAutoBag = Entry("AutoBagItems", false, "Bag customer items at the register automatically");
                _pUnbagged = Entry("AllowUnbaggedCheckout", false, "Register accepts transactions with unbagged items");
                _pFreezeClock = Entry("FreezeShiftClock", false, "Shift countdown never runs out");
                _pDetector = Entry("DoppelgangerDetector", true, "In-world doppelganger warning");
                _pRadar = Entry("DoppelgangerRadar", true, "Count doppelgangers nearby");
                _pEvidence = Entry("DetectorEvidence", true, "Show why they are a doppelganger");
                _pBadge = Entry("DetectorBadge", true, "Corner tag while the detector is armed");
                _pPatience = Entry("MaxCustomerPatience", true, "Customers never run out of patience");
                _pHappy = Entry("HappyCustomers", true, "Customers never leave a complaint on the way out");
                _pHonestStock = Entry("HonestStockRating", true, "Re-derive the stock rating from what is actually on the shelves");
                _pInstantTask = Entry("InstantTasks", true, "No hold timer on traps, disarming and boarding");
                _pAutoFuel = Entry("AutoFuel", true, "Fill a car's petrol order the moment it appears");
                _pKillExtras = Entry("AutoKillHuntExtras", false, "Keep clearing the entity's extra monsters");
                _pNoFog = Entry("NoFog", true, "Remove the fog entirely");
                _pBright = Entry("Fullbright", false, "Flat ambient light, no fog, extra exposure");
                _pPatienceFactor = Entry("PatienceFactor", 3f, "Multiplies each customer's own patience allowance");
                _pBrightBoost = Entry("FullbrightBoost", 2.5f, "Extra exposure in EV while fullbright is on");
                _pSelfCheckout = Entry("NonDoppelsSelfCheckout", false, "Ordinary customers complete their own transaction");
                _pClearLeftovers = Entry("ClearCounterLeftovers", true, "Remove unbagged items from the counter once the customer is done");
                _pSkipCountdown = Entry("AutoSkipHuntCountdown", false, "Skip the warning delay before the entity arrives");
                _pSkipBriefing = Entry("SkipHuntBriefing", false, "Dismiss the hunt explanation panels as soon as they appear");
                _pFastEod = Entry("FastEndOfDay", true, "Run the customer report and the money counter faster");
                _pEodSpeed = Entry("EndOfDaySpeed", 4f, "How much faster the end-of-day report runs");
                _pVentsDontKick = Entry("VentsDontKick", true, "Vents stop throwing you out for staying in them");
                _pNoHints = Entry("AutoDismissHints", true, "Block the HUD hint popups entirely");
                _pInstantScope = Entry("InstantEmotiscope", true, "The emoti-scope finishes its scan at once");
                _pRevealScan = Entry("RevealScanOnly", false, "Show the objects that normally need the anomaly lens");
                _pEndlessNight = Entry("EndlessNight", false, "The shift clock never runs out; call the bus when you want to leave");
                _pEndlessTopUp = Entry("EndlessTopUpMinutes", 10, "Minutes the clock is wound back to when it runs low");

                // Everything below used to live only in memory, so a hot reload - or just restarting
                // the game - quietly turned it all back off again.
                _pStamina = Entry("InfiniteStamina", false, "Never run out of stamina");
                _pGod = Entry("GodMode", false, "Health pinned to max");
                _pMoney = Entry("InfiniteMoney", false, "Top up store money, tokens and story funds");
                _pAmmo = Entry("InfiniteAmmo", false, "Refill a weapon slot when it drops");
                _pFreezeItems = Entry("FreezeItemStacks", false, "Throwables stop being used up");
                _pAmmoTarget = Entry("AmmoCeiling", 0, "0 = only refill to what the gun actually held");
                _pFundsFloor = Entry("WalletFloor", 100000, "Level all three wallets are kept at");
                _pSpeedMul = Entry("MoveSpeedMultiplier", 1f, "Walk, run and crouch speed multiplier");
                _pJumpMul = Entry("JumpMultiplier", 1f, "Jump power multiplier");
                _pNoclipSpeed = Entry("NoclipSpeed", 12f, "No-clip fly speed");
                _pCleanSpills = Entry("AutoCleanSpills", false, "Automatically clean blood and spills");
                _pCleanMop = Entry("AutoCleanMoppables", false, "Automatically clean anything moppable");
                _pCleanTrash = Entry("AutoCleanTrash", false, "Automatically clear loose trash and limbs");
                _pAutoStock = Entry("AutoStockShelves", false, "Automatically restock shelves");
                _pStockCrate = Entry("AutoStockRequiresCrate", false, "Only stock from a crate you are carrying");
                _pSpawnAmount = Entry("SpawnerAmount", 1, "How many copies each spawn drops");
                _pNoBarriers = Entry("RemoveStoreCurbs", false, "Disable the curb colliders in the store");
                _pNoFences = Entry("RemoveChainFences", false, "Disable the chain fence and gate colliders");
                _pNoRoof = Entry("RemoveForestRoof", false, "Disable the ForestRoof colliders");
                _pWoundFlee = Entry("WoundedRunAway", false, "A wounded troublemaker runs out of the store");
                _pHumanShield = Entry("HumansInvincible", true, "Ordinary customers can be hurt but not killed");
                _pHostileNames = Entry("HostileNames", HumanShield.DefaultHostileNames,
                    "Comma-separated name fragments the shield must never protect (Norbert, thieves, the antler man, ...)");
                _pAutoUnlock = Entry("AutoUnlockItemsAfterTalking", true, "Release the item lock a conversation leaves behind");
                _pSilenceBells = Entry("SilenceBells", false, "Mute the door chime and the counter bell");
                _pGrenadeFlamer = Entry("GrenadeFlamethrower", false, "Flamethrower shots also launch grenades");
                // Deliberately new keys: the old GrenadeFlamethrowerRate/Burst entries already exist
                // with the first version's values, and a changed default never reaches an entry that
                // has already been created - which is exactly why it kept firing in pulses.
                _pGrenadeRate = Entry("GrenadeStreamInterval", 0f, "Seconds between shots (0 = every frame)");
                _pGrenadeBurst = Entry("GrenadeStreamCount", 1, "Grenades per shot (1 = a stream)");
                _pGrenadeForce = Entry("GrenadeStreamSpeed", 45f, "Grenade muzzle velocity");

                _pProfiler = Entry("Profiler", true, "Log slow module ticks and a 10s summary");
                _pVerbose = Entry("VerboseLogging", false, "Verbose logging");
                _pToggleKey = Entry("MenuKey", "F1", "Key that opens the menu (any UnityEngine.KeyCode name)");
                _pUnstickKey = Entry("UnstickKey", "F2", "Key that force-releases control locks");
                _pUiScale = Entry("UiScale", 1f, "Menu size multiplier");
                _pOverlayScale = Entry("WarningTextSize", 1f, "Doppelganger warning size multiplier");
                Detector.OverlayScale = Mathf.Clamp(_pOverlayScale.Value, 0.5f, 3f);
                Util.Ui.Scale = Mathf.Clamp(_pUiScale.Value, 0.6f, 2.5f);

                try { _toggleKey = (KeyCode)Enum.Parse(typeof(KeyCode), _pToggleKey.Value, true); }
                catch
                {
                    _toggleKey = KeyCode.F1;
                    Log.Warn("MenuKey '" + _pToggleKey.Value + "' is not a valid KeyCode; falling back to F1.");
                }
                try { _unstickKey = (KeyCode)Enum.Parse(typeof(KeyCode), _pUnstickKey.Value, true); }
                catch
                {
                    _unstickKey = KeyCode.F2;
                    Log.Warn("UnstickKey '" + _pUnstickKey.Value + "' is not a valid KeyCode; falling back to F2.");
                }

                GunCaseOpen = _pGunCase.Value;
                World.GunCaseForEveryone = _pGunCase.Value;
                World.WeaponWallAlwaysOpen = _pWeaponWall.Value;
                Multipliers.CustomersEnabled = _pMulCust.Value;
                Multipliers.CustomerFactor = Mathf.Clamp(_pCustFactor.Value, 1, 10);
                Multipliers.EventsEnabled = _pMulEvent.Value;
                Multipliers.EventFactor = Mathf.Clamp(_pEventFactor.Value, 1, 10);
                Player.MaxSlots = Mathf.Clamp(_pMaxSlots.Value, 0, 8);
                World.CustomerMultiplier = Multipliers.CustomerFactor;
                World.ExtraNuisances = _pMulNuisance.Value;
                World.NuisanceMultiplier = Mathf.Clamp(_pNuisanceFactor.Value, 1, 10);
                World.ExtraDoppelgangers = _pMulDoppel.Value;
                World.DoppelgangerCount = Mathf.Clamp(_pDoppelCount.Value, 1, 10);
                RakeBlock.Enabled = _pNoRake.Value;
                HuntBlock.NoEntities = _pNoHuntEnt.Value;
                HuntBlock.NoHunt = _pNoHunt.Value;
                PerfectReviews.Enabled = _pReviews.Value;
                SkipReading.Enabled = _pSkipRead.Value;
                Counter.AutoBag = _pAutoBag.Value;
                Counter.AllowUnbagged = _pUnbagged.Value;
                World.FreezeClock = _pFreezeClock.Value;
                Detector.Enabled = _pDetector.Value;
                Detector.Radar = _pRadar.Value;
                Detector.ShowEvidence = _pEvidence.Value;
                Detector.ShowBadge = _pBadge.Value;
                Comfort.MaxPatience = _pPatience.Value;
                Comfort.HappyCustomers = _pHappy.Value;
                Comfort.HonestStock = _pHonestStock.Value;
                Comfort.InstantTasks = _pInstantTask.Value;
                Comfort.AutoFuel = _pAutoFuel.Value;
                Comfort.AutoKillExtras = _pKillExtras.Value;
                Comfort.AutoSkipHuntCountdown = _pSkipCountdown.Value;
                Comfort.SkipHuntBriefing = _pSkipBriefing.Value;
                Comfort.FastEndOfDay = _pFastEod.Value;
                Comfort.EndOfDaySpeed = Mathf.Clamp(_pEodSpeed.Value, 1f, 10f);
                Comfort.VentsDontKick = _pVentsDontKick.Value;
                Comfort.AutoDismissHints = _pNoHints.Value;
                Comfort.InstantEmotiscope = _pInstantScope.Value;
                Comfort.RevealScanOnly = _pRevealScan.Value;
                World.EndlessNight = _pEndlessNight.Value;
                World.EndlessTopUpMinutes = Mathf.Clamp(_pEndlessTopUp.Value, 1, 60);

                Player.InfiniteStamina = _pStamina.Value;
                Player.GodMode = _pGod.Value;
                Player.InfiniteMoney = _pMoney.Value;
                Player.InfiniteAmmo = _pAmmo.Value;
                Player.FreezeItems = _pFreezeItems.Value;
                Player.AmmoTarget = Mathf.Clamp(_pAmmoTarget.Value, 0, 999);
                Player.FundsFloor = Mathf.Clamp(_pFundsFloor.Value, 10000, 9000000);
                Player.MoneyFloor = Player.FundsFloor;
                Player.SpeedMultiplier = Mathf.Clamp(_pSpeedMul.Value, 0.25f, 6f);
                Player.JumpMultiplier = Mathf.Clamp(_pJumpMul.Value, 0.25f, 6f);
                Player.NoclipSpeed = Mathf.Clamp(_pNoclipSpeed.Value, 3f, 40f);

                Clean.CleanSpills = _pCleanSpills.Value;
                Clean.CleanMoppables = _pCleanMop.Value;
                Clean.CleanTrash = _pCleanTrash.Value;

                Stock.Enabled = _pAutoStock.Value;
                Stock.RequireCrate = _pStockCrate.Value;

                Spawner.Amount = Mathf.Clamp(_pSpawnAmount.Value, 1, 50);

                Comfort.SilenceBells = _pSilenceBells.Value;
                Comfort.AutoUnlockInventory = _pAutoUnlock.Value;
                HumanShield.Enabled = _pHumanShield.Value;
                HumanShield.WoundAndFlee = _pWoundFlee.Value;
                HumanShield.HostileNames = string.IsNullOrEmpty(_pHostileNames.Value)
                    ? HumanShield.DefaultHostileNames : _pHostileNames.Value;
                Comfort.RemoveCurbs = _pNoBarriers.Value;
                Comfort.RemoveFences = _pNoFences.Value;
                Comfort.RemoveForestRoof = _pNoRoof.Value;
                WeaponMods.GrenadeFlamethrower = _pGrenadeFlamer.Value;
                WeaponMods.FireInterval = Mathf.Clamp(_pGrenadeRate.Value, 0f, 3f);
                WeaponMods.BurstCount = Mathf.Clamp(_pGrenadeBurst.Value, 1, 10);
                WeaponMods.LaunchForce = Mathf.Clamp(_pGrenadeForce.Value, 5f, 120f);
                Comfort.PatienceFactor = Mathf.Clamp(_pPatienceFactor.Value, 1f, 20f);
                Comfort.BrightBoost = Mathf.Clamp(_pBrightBoost.Value, 0f, 8f);
                Comfort.NoFog = _pNoFog.Value;
                Comfort.Fullbright = _pBright.Value;
                Counter.ClearLeftovers = _pClearLeftovers.Value;
                Counter.SelfCheckoutNormals = _pSelfCheckout.Value;
                Profiler.Enabled = _pProfiler.Value;
                Player.InfiniteRefreshes = _pRefresh.Value;
                Log.Verbose = _pVerbose.Value;
            }
            catch (Exception ex)
            {
                Log.Ex("preferences", ex);
                // Sensible defaults if preferences are unavailable.
                GunCaseOpen = true;
                World.GunCaseForEveryone = true;
                Multipliers.CustomersEnabled = true;
                Multipliers.EventsEnabled = true;
            }
        }

        private void SavePrefs()
        {
            if (_cfg == null) return;
            try
            {
                _pGunCase.Value = GunCaseOpen;
                _pWeaponWall.Value = World.WeaponWallAlwaysOpen;
                _pMulCust.Value = Multipliers.CustomersEnabled;
                _pCustFactor.Value = Multipliers.CustomerFactor;
                _pMulNuisance.Value = World.ExtraNuisances;
                _pNuisanceFactor.Value = World.NuisanceMultiplier;
                _pMulDoppel.Value = World.ExtraDoppelgangers;
                _pDoppelCount.Value = World.DoppelgangerCount;
                _pMulEvent.Value = Multipliers.EventsEnabled;
                _pEventFactor.Value = Multipliers.EventFactor;
                _pMaxSlots.Value = Player.MaxSlots;
                _pNoRake.Value = RakeBlock.Enabled;
                _pNoHuntEnt.Value = HuntBlock.NoEntities;
                _pNoHunt.Value = HuntBlock.NoHunt;
                _pReviews.Value = PerfectReviews.Enabled;
                _pSkipRead.Value = SkipReading.Enabled;
                _pAutoBag.Value = Counter.AutoBag;
                _pUnbagged.Value = Counter.AllowUnbagged;
                _pFreezeClock.Value = World.FreezeClock;
                _pDetector.Value = Detector.Enabled;
                _pRadar.Value = Detector.Radar;
                _pEvidence.Value = Detector.ShowEvidence;
                _pBadge.Value = Detector.ShowBadge;
                _pPatience.Value = Comfort.MaxPatience;
                _pHappy.Value = Comfort.HappyCustomers;
                _pHonestStock.Value = Comfort.HonestStock;
                _pInstantTask.Value = Comfort.InstantTasks;
                _pAutoFuel.Value = Comfort.AutoFuel;
                _pKillExtras.Value = Comfort.AutoKillExtras;
                _pSkipCountdown.Value = Comfort.AutoSkipHuntCountdown;
                _pSkipBriefing.Value = Comfort.SkipHuntBriefing;
                _pFastEod.Value = Comfort.FastEndOfDay;
                _pEodSpeed.Value = Comfort.EndOfDaySpeed;
                _pVentsDontKick.Value = Comfort.VentsDontKick;
                _pNoHints.Value = Comfort.AutoDismissHints;
                _pInstantScope.Value = Comfort.InstantEmotiscope;
                _pRevealScan.Value = Comfort.RevealScanOnly;
                _pEndlessNight.Value = World.EndlessNight;
                _pEndlessTopUp.Value = World.EndlessTopUpMinutes;

                _pStamina.Value = Player.InfiniteStamina;
                _pGod.Value = Player.GodMode;
                _pMoney.Value = Player.InfiniteMoney;
                _pAmmo.Value = Player.InfiniteAmmo;
                _pFreezeItems.Value = Player.FreezeItems;
                _pAmmoTarget.Value = Player.AmmoTarget;
                _pFundsFloor.Value = Player.FundsFloor;
                _pSpeedMul.Value = Player.SpeedMultiplier;
                _pJumpMul.Value = Player.JumpMultiplier;
                _pNoclipSpeed.Value = Player.NoclipSpeed;

                _pCleanSpills.Value = Clean.CleanSpills;
                _pCleanMop.Value = Clean.CleanMoppables;
                _pCleanTrash.Value = Clean.CleanTrash;

                _pAutoStock.Value = Stock.Enabled;
                _pStockCrate.Value = Stock.RequireCrate;

                _pSpawnAmount.Value = Spawner.Amount;

                _pSilenceBells.Value = Comfort.SilenceBells;
                _pAutoUnlock.Value = Comfort.AutoUnlockInventory;
                _pHumanShield.Value = HumanShield.Enabled;
                _pWoundFlee.Value = HumanShield.WoundAndFlee;
                _pHostileNames.Value = HumanShield.HostileNames;
                _pNoBarriers.Value = Comfort.RemoveCurbs;
                _pNoFences.Value = Comfort.RemoveFences;
                _pNoRoof.Value = Comfort.RemoveForestRoof;
                _pGrenadeFlamer.Value = WeaponMods.GrenadeFlamethrower;
                _pGrenadeRate.Value = WeaponMods.FireInterval;
                _pGrenadeBurst.Value = WeaponMods.BurstCount;
                _pGrenadeForce.Value = WeaponMods.LaunchForce;
                _pBright.Value = Comfort.Fullbright;
                _pNoFog.Value = Comfort.NoFog;
                _pPatienceFactor.Value = Comfort.PatienceFactor;
                _pBrightBoost.Value = Comfort.BrightBoost;
                _pClearLeftovers.Value = Counter.ClearLeftovers;
                _pSelfCheckout.Value = Counter.SelfCheckoutNormals;
                _pProfiler.Value = Profiler.Enabled;
                _pRefresh.Value = Player.InfiniteRefreshes;
                _pVerbose.Value = Log.Verbose;
                _pUiScale.Value = Util.Ui.Scale;
                _pOverlayScale.Value = Detector.OverlayScale;
            }
            catch (Exception ex) { Log.Warn("save prefs stopped early: " + ex.Message); }

            // Flush whatever did get copied across. This used to sit inside the try above, so a single
            // bad field meant nothing at all reached disk - and a hot reload came back with defaults.
            try { MelonPreferences.Save(); }
            catch (Exception ex) { Log.Debug("MelonPreferences.Save: " + ex.Message); }
        }

        public void SceneInit(int buildIndex, string sceneName)
        {
            Net.Invalidate();
            Items.Invalidate();
            Player.OnSceneChanged();
            Clean.OnSceneChanged();
            Stock.OnSceneChanged();
            Counter.OnSceneChanged();
            Rpcs.Invalidate();
            World.OnSceneChanged();
            Detector.OnSceneChanged();
            Comfort.OnSceneChanged();
            SetUpDayPatch.Reset();
            NpcRegistry.Clear();
            WeaponMods.Reset();
            Log.Debug("scene initialised: " + sceneName + " - caches cleared.");
        }

        public void Quit()
        {
            SavePrefs();
        }

        public void UiTick()
        {
            if (_ui != null) using (Profiler.Begin("UI.Panel")) _ui.Update();
        }

        /// <summary>Detach completely so the loader can drop this assembly and load a new one.</summary>
        public void Shutdown()
        {
            try { Comfort.Shutdown(); } catch { }
            try { SavePrefs(); } catch { }
            try { if (_ui != null) _ui.Shutdown(); } catch (Exception ex) { Log.Debug("ui shutdown: " + ex.Message); }
            try { if (_menu != null) _menu.Visible = false; } catch { }
            try { if (Player != null && Player.Noclip) Player.OnNoclipChanged(false); } catch { }
            try { if (Player != null) Player.RestoreMovement(); } catch { }
            try
            {
                if (_harmony != null) { _harmony.UnpatchSelf(); Log.Msg("Harmony patches removed."); }
            }
            catch (Exception ex) { Log.Warn("Unpatch failed: " + ex.Message); }
            _harmony = null;
            if (ReferenceEquals(Instance, this)) Instance = null;
        }

        public void Update()
        {
            try
            {
                _ui.EnsurePanel();

                if (Input.GetKeyDown(_toggleKey) && Time.unscaledTime >= _nextToggle)
                {
                    _nextToggle = Time.unscaledTime + 0.2f;
                    if (_ui.Ready)
                    {
                        _ui.Toggle();
                        if (!_ui.Visible) SavePrefs();
                    }
                    else
                    {
                        // UniverseLib is still starting up (or unavailable) - use the simple menu.
                        _menu.Visible = !_menu.Visible;
                        _guiFailed = false;
                        _guiErrors = 0;
                        if (!_menu.Visible) SavePrefs();
                    }
                }

                if (Input.GetKeyDown(_unstickKey) && Time.unscaledTime >= _nextUnstick)
                {
                    _nextUnstick = Time.unscaledTime + 0.5f;
                    World.UnstickPlayer(true);
                }

                // Keep the two gun case paths - the Harmony prefix and the sweep - agreeing.
                World.GunCaseForEveryone = GunCaseOpen;
                World.CustomerMultiplier = Multipliers.CustomerFactor;

                if (_uiRebuildQueued)
                {
                    _uiRebuildQueued = false;
                    _ui.Rebuild();
                    SavePrefs();
                }

                using (Profiler.Begin("Menu.Input")) _menu.HandleInput();

                // Settled before the modules run, so Comfort holds the item lock on the same frame
                // the menu appears rather than one behind it.
                bool menuOpen = (_ui != null && _ui.Visible) || (_menu != null && _menu.Visible);
                Comfort.MenuOpen = menuOpen;

                // The cheats only need to re-assert a few times a second; per-frame was pure waste.
                if (Player.AnyActive && Time.unscaledTime >= _nextPlayerTick)
                {
                    _nextPlayerTick = Time.unscaledTime + 0.1f;
                    using (Profiler.Begin("Player.Tick")) Player.Tick();
                }
                if (Player.Noclip) Player.RunNoclip();

                using (Profiler.Begin("Spawner.Tick")) Spawner.Tick();
                using (Profiler.Begin("Clean.Tick")) Clean.Tick();
                using (Profiler.Begin("Stock.Tick")) Stock.Tick();
                using (Profiler.Begin("Counter.Tick")) Counter.Tick();
                using (Profiler.Begin("World.Tick")) World.Tick();
                using (Profiler.Begin("Detector.Tick")) Detector.Tick();
                using (Profiler.Begin("Comfort.Tick")) Comfort.Tick();

                // Reads the trigger itself, so the fire rate is not the flamethrower's to decide.
                WeaponMods.Tick(menuOpen);
                Profiler.Tick();
            }
            catch (Exception ex)
            {
                Log.Ex("update", ex);
            }
        }

        public void OnGUI()
        {
            if (_guiFailed) return;
            try
            {
                using (Profiler.Begin("GUI.Overlay")) Detector.DrawOverlay();
                using (Profiler.Begin("GUI.Fallback")) _menu.Draw();
                _guiErrors = 0;
            }
            catch (Exception ex)
            {
                // One bad frame should not cost the menu for the rest of the session; only give up
                // if it keeps throwing, and say so once rather than every repaint.
                _guiErrors++;
                if (_guiErrors == 1) Log.Err("GUI draw error: " + ex);
                if (_guiErrors >= 30)
                {
                    _guiFailed = true;
                    _menu.Visible = false;
                    Log.Err("GUI disabled after " + _guiErrors + " consecutive draw errors.");
                }
            }
        }
    }
}
