using System;
using ShiftAtMidnightSuite.Loader;
using ShiftAtMidnightSuite.Util;
using UniverseLib.UI;

namespace ShiftAtMidnightSuite.UI
{
    /// <summary>
    /// Owns the suite's panel. The UniverseLib registration itself belongs to the loader, because
    /// it has to outlive a plugin reload; this only creates and destroys the panel inside it.
    /// </summary>
    internal sealed class SuiteUI
    {
        private readonly SuiteMod _mod;
        private readonly IPluginHost _host;
        private SuitePanel _panel;
        private bool _visible;

        internal bool Ready { get { return _panel != null; } }
        internal bool Failed { get; private set; }

        internal SuiteUI(SuiteMod mod, IPluginHost host)
        {
            _mod = mod;
            _host = host;
        }

        /// <summary>Create the panel once UniverseLib is up. Cheap to call every frame until then.</summary>
        internal void EnsurePanel()
        {
            if (_panel != null || Failed) return;
            if (!_host.UniverseReady || _host.UI == null) return;

            try
            {
                _panel = new SuitePanel(_host.UI, _mod);
                _panel.SetActive(false);
                UniversalUI.SetUIActive(_host.UiId, false);
                Log.Msg("Mod menu UI created. Click the sidebar to switch category.");
            }
            catch (Exception ex)
            {
                Failed = true;
                Log.Err("Could not build the menu, falling back to the simple menu: " + ex);
            }
        }

        /// <summary>Driven by UniverseLib's UI update, forwarded through the loader.</summary>
        internal void Update()
        {
            if (_panel == null) return;
            try { _panel.Tick(); }
            catch (Exception ex) { Log.Debug("panel tick: " + ex.Message); }
        }

        internal bool Visible
        {
            get { return _visible; }
            set
            {
                if (_panel == null) return;
                _visible = value;
                try
                {
                    UniversalUI.SetUIActive(_host.UiId, value);
                    _panel.SetActive(value);
                }
                catch (Exception ex) { Log.Ex("toggle menu", ex); }
            }
        }

        internal void Toggle() { Visible = !_visible; }

        /// <summary>Tear the panel down and rebuild it from current settings (UI scale etc.).</summary>
        internal void Rebuild()
        {
            if (_host.UI == null) return;
            bool wasVisible = _visible;
            try
            {
                if (_panel != null) { _panel.Destroy(); _panel = null; }
                _panel = new SuitePanel(_host.UI, _mod);
                _panel.SetActive(wasVisible);
                UniversalUI.SetUIActive(_host.UiId, wasVisible);
                Log.Msg("Mod menu rebuilt at scale " + Ui.Scale.ToString("0.00") + ".");
            }
            catch (Exception ex)
            {
                Log.Err("Menu rebuild failed: " + ex);
                Failed = true;
            }
        }

        /// <summary>Remove the panel from UniverseLib so a reloaded plugin can register its own.</summary>
        internal void Shutdown()
        {
            try
            {
                if (_panel != null)
                {
                    _panel.SetActive(false);
                    _panel.Destroy();
                    _panel = null;
                }
                UniversalUI.SetUIActive(_host.UiId, false);
            }
            catch (Exception ex) { Log.Debug("ui shutdown: " + ex.Message); }
            _visible = false;
        }
    }
}
