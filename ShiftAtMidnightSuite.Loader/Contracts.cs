using UniverseLib.UI;

namespace ShiftAtMidnightSuite.Loader
{
    /// <summary>
    /// What the loader offers a plugin. The loader owns the things that must survive a reload:
    /// the UniverseLib registration and the file watching.
    /// </summary>
    public interface IPluginHost
    {
        /// <summary>The UniverseLib UI root, or null until UniverseLib has finished initialising.</summary>
        UIBase UI { get; }

        /// <summary>Id the loader registered the UI under; use it with UniversalUI.SetUIActive.</summary>
        string UiId { get; }

        bool UniverseReady { get; }

        /// <summary>Ask the loader to unload this plugin and load the file again.</summary>
        void RequestReload();
    }

    /// <summary>
    /// The plugin's lifecycle. One class in the plugin assembly implements this; the loader finds
    /// it by interface, so the plugin needs no Melon attributes and is not a mod in its own right.
    /// </summary>
    public interface ISuitePlugin
    {
        void Init(IPluginHost host);

        /// <summary>Per-frame, from the loader's OnUpdate.</summary>
        void Update();

        /// <summary>Per-frame from UniverseLib's UI update, for panel refreshers.</summary>
        void UiTick();

        void OnGUI();
        void SceneInit(int buildIndex, string sceneName);
        void Quit();

        /// <summary>
        /// Detach everything: unpatch Harmony, destroy UI, drop references. Called before the
        /// assembly context is unloaded. Must not throw.
        /// </summary>
        void Shutdown();
    }
}
