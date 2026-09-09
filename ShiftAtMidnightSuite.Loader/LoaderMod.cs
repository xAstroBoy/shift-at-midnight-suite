using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using MelonLoader;
using UnityEngine;
using UniverseLib;
using UniverseLib.UI;

[assembly: MelonInfo(typeof(ShiftAtMidnightSuite.Loader.LoaderMod), "Shift At Midnight Suite", "1.0.0", "chadi7bark + xAstroBoy")]
[assembly: MelonGame(null, null)]

namespace ShiftAtMidnightSuite.Loader
{
    /// <summary>
    /// Hot-reload host for the suite.
    ///
    /// MelonLoader loads mods into a context that .NET cannot unload, which is why a normal mod
    /// needs a game restart for every change. This loader is that unloadable, unchanging shell.
    /// The real suite lives in a separate assembly, loaded from bytes into a collectible
    /// AssemblyLoadContext, so the file on disk is never locked and a new build can be swapped in
    /// while the game runs: automatically when the file changes, or on F3.
    ///
    /// The plugin file lives under UserData rather than Mods so MelonLoader never tries to load it
    /// as a mod itself - that would lock it and put its types in the wrong context.
    /// </summary>
    public sealed class LoaderMod : MelonMod, IPluginHost
    {
        public const string UiId = "com.xastroboy.shiftatmidnightsuite";
        private const string P = "[SAM Loader] ";
        private const KeyCode ReloadKey = KeyCode.F3;

        private static string PluginDir
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "UserData", "ShiftAtMidnightSuite"); }
        }

        private static string PluginPath
        {
            get { return Path.Combine(PluginDir, "ShiftAtMidnightSuite.Plugin.dll"); }
        }

        private UIBase _ui;
        private bool _universeReady;

        private PluginContext _context;
        private ISuitePlugin _plugin;
        private DateTime _loadedStamp;
        private long _loadedLength;
        private int _generation;

        private float _nextFileCheck;
        private float _reloadAt = -1f;
        private bool _reloadRequested;
        private float _nextKey;

        // ---------------------------------------------------------------- IPluginHost

        public UIBase UI { get { return _ui; } }
        string IPluginHost.UiId { get { return UiId; } }
        public bool UniverseReady { get { return _universeReady; } }
        public void RequestReload() { _reloadRequested = true; }

        // ---------------------------------------------------------------- melon lifecycle

        public override void OnInitializeMelon()
        {
            try
            {
                Universe.Init(1f, OnUniverseReady, LogFromUniverse, default(UniverseLib.Config.UniverseLibConfig));
            }
            catch (Exception ex)
            {
                MelonLogger.Error(P + "UniverseLib init failed: " + ex.Message);
            }

            Directory.CreateDirectory(PluginDir);
            MelonLogger.Msg(P + "Plugin path: " + PluginPath);
            MelonLogger.Msg(P + ReloadKey + " reloads the plugin; it also reloads on its own when the file changes.");
            LoadPlugin();
        }

        private void OnUniverseReady()
        {
            try
            {
                _ui = UniversalUI.RegisterUI(UiId, UiTick);
                UniversalUI.SetUIActive(UiId, false);
                _universeReady = true;
                MelonLogger.Msg(P + "UniverseLib ready; UI registered.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error(P + "UI registration failed: " + ex);
            }
        }

        private static void LogFromUniverse(string message, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception) MelonLogger.Warning(P + "[UniverseLib] " + message);
        }

        private void UiTick()
        {
            ISuitePlugin p = _plugin;
            if (p == null) return;
            try { p.UiTick(); }
            catch (Exception ex) { MelonLogger.Warning(P + "plugin UiTick: " + ex.Message); }
        }

        public override void OnUpdate()
        {
            float now = Time.unscaledTime;

            if (Input.GetKeyDown(ReloadKey) && now >= _nextKey)
            {
                _nextKey = now + 0.5f;
                _reloadRequested = true;
            }

            if (now >= _nextFileCheck)
            {
                _nextFileCheck = now + 1f;
                if (FileChanged())
                {
                    // Wait a moment for the compiler to finish writing before reading it.
                    if (_reloadAt < 0f) MelonLogger.Msg(P + "Plugin file changed; reloading shortly.");
                    _reloadAt = now + 0.8f;
                }
            }

            if (_reloadRequested || (_reloadAt >= 0f && now >= _reloadAt))
            {
                _reloadRequested = false;
                _reloadAt = -1f;
                Reload();
            }

            ISuitePlugin p = _plugin;
            if (p == null) return;
            try { p.Update(); }
            catch (Exception ex) { MelonLogger.Warning(P + "plugin Update: " + ex.Message); }
        }

        public override void OnGUI()
        {
            ISuitePlugin p = _plugin;
            if (p == null) return;
            try { p.OnGUI(); }
            catch (Exception ex) { MelonLogger.Warning(P + "plugin OnGUI: " + ex.Message); }
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            ISuitePlugin p = _plugin;
            if (p == null) return;
            try { p.SceneInit(buildIndex, sceneName); }
            catch (Exception ex) { MelonLogger.Warning(P + "plugin SceneInit: " + ex.Message); }
        }

        public override void OnApplicationQuit()
        {
            ISuitePlugin p = _plugin;
            if (p == null) return;
            try { p.Quit(); } catch { }
        }

        // ---------------------------------------------------------------- loading

        private bool FileChanged()
        {
            try
            {
                var fi = new FileInfo(PluginPath);
                if (!fi.Exists) return false;
                return fi.LastWriteTimeUtc != _loadedStamp || fi.Length != _loadedLength;
            }
            catch { return false; }
        }

        private void LoadPlugin()
        {
            var fi = new FileInfo(PluginPath);
            if (!fi.Exists)
            {
                MelonLogger.Warning(P + "No plugin at " + PluginPath + " - build the suite and it will load on its own.");
                return;
            }

            byte[] dll;
            byte[] pdb = null;
            try
            {
                // Read into memory so the file is never locked and can be overwritten by a build.
                dll = File.ReadAllBytes(PluginPath);
                string pdbPath = Path.ChangeExtension(PluginPath, ".pdb");
                if (File.Exists(pdbPath)) pdb = File.ReadAllBytes(pdbPath);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning(P + "Could not read plugin (still being written?): " + ex.Message);
                _reloadAt = Time.unscaledTime + 1f;
                return;
            }

            _generation++;
            var ctx = new PluginContext("SamSuite#" + _generation);
            Assembly asm;
            try
            {
                using (var ms = new MemoryStream(dll))
                {
                    asm = pdb != null ? ctx.LoadFromStream(ms, new MemoryStream(pdb)) : ctx.LoadFromStream(ms);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error(P + "Plugin assembly failed to load: " + ex);
                try { ctx.Unload(); } catch { }
                return;
            }

            Type pluginType = null;
            try
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException rtl) { types = rtl.Types; }
                for (int i = 0; i < types.Length; i++)
                {
                    Type t = types[i];
                    if (t != null && !t.IsAbstract && typeof(ISuitePlugin).IsAssignableFrom(t)) { pluginType = t; break; }
                }
            }
            catch (Exception ex) { MelonLogger.Error(P + "Plugin type scan failed: " + ex); }

            if (pluginType == null)
            {
                MelonLogger.Error(P + "Plugin assembly has no ISuitePlugin implementation.");
                try { ctx.Unload(); } catch { }
                return;
            }

            try
            {
                var plugin = (ISuitePlugin)Activator.CreateInstance(pluginType);
                plugin.Init(this);
                _plugin = plugin;
                _context = ctx;
                _loadedStamp = fi.LastWriteTimeUtc;
                _loadedLength = fi.Length;
                MelonLogger.Msg(P + "Plugin loaded (generation " + _generation + ", " + (dll.Length / 1024) + " KB, built " +
                                fi.LastWriteTime.ToString("HH:mm:ss") + ").");
            }
            catch (Exception ex)
            {
                MelonLogger.Error(P + "Plugin Init threw: " + ex);
                try { ctx.Unload(); } catch { }
            }
        }

        private void Reload()
        {
            MelonLogger.Msg(P + "Reloading plugin...");
            ISuitePlugin old = _plugin;
            PluginContext oldCtx = _context;
            _plugin = null;
            _context = null;

            if (old != null)
            {
                try { old.Shutdown(); }
                catch (Exception ex) { MelonLogger.Warning(P + "old plugin Shutdown threw: " + ex.Message); }
            }

            if (oldCtx != null)
            {
                // Best effort. Delegates handed to Il2Cpp (UI listeners) can pin the old context
                // for a while; correctness does not depend on it actually being collected, only on
                // the old plugin having detached, which Shutdown just did.
                try { oldCtx.Unload(); } catch { }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            LoadPlugin();
        }

        /// <summary>
        /// Collectible context that resolves every dependency to whatever is already loaded in the
        /// process, so the plugin shares the exact MelonLoader / Il2Cpp / UniverseLib types the game
        /// is running, whatever context those happen to live in.
        /// </summary>
        private sealed class PluginContext : AssemblyLoadContext
        {
            public PluginContext(string name) : base(name, isCollectible: true) { }

            protected override Assembly Load(AssemblyName name)
            {
                Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < loaded.Length; i++)
                {
                    try
                    {
                        if (string.Equals(loaded[i].GetName().Name, name.Name, StringComparison.OrdinalIgnoreCase))
                            return loaded[i];
                    }
                    catch { }
                }
                return null;    // fall through to the default context
            }
        }
    }
}
