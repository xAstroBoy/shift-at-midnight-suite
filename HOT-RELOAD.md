# Hot reload

The suite no longer needs a game restart to pick up a change.

## Layout

| Piece | Where | Loaded by |
|---|---|---|
| `ShiftAtMidnightSuite.Loader.dll` | `Mods/` | MelonLoader, once, at startup. **This is the mod.** |
| `ShiftAtMidnightSuite.Plugin.dll` | `UserData/ShiftAtMidnightSuite/` | The loader, from bytes, into a collectible `AssemblyLoadContext`. **This is the suite.** |

MelonLoader loads mods into a context .NET cannot unload, so anything that is a mod needs a
restart to change. The loader is therefore deliberately tiny and stable: it initialises UniverseLib,
registers the UI id, watches the plugin file, and forwards `OnUpdate` / `OnGUI` / scene events to
whatever plugin is loaded. All the actual features live in the plugin.

The plugin lives under `UserData`, not `Mods`, on purpose. If MelonLoader saw it in `Mods` it would
load it as a mod itself, lock the file, and put its types in the wrong context.

## Workflow

```
cd ModSource/ShiftAtMidnightSuite
dotnet build -c Release
```

The Release build copies the plugin DLL (and PDB) into `UserData/ShiftAtMidnightSuite/`. The
loader checks the file once a second; when the timestamp or size changes it waits 0.8s for the
compiler to finish writing, then:

1. calls `Shutdown()` on the old plugin - saves prefs, destroys the panel, removes its Harmony
   patches, restores movement values, clears noclip;
2. unloads the old context (best effort - see below);
3. reads the new file, loads it, finds the `ISuitePlugin` implementation, calls `Init()`.

**F3** forces the same reload. The log shows `Plugin loaded (generation N, built HH:mm:ss)`.

## What the plugin must do to stay reloadable

- Implement `ISuitePlugin` (in the loader assembly). No `MelonInfo` attribute - it is not a mod.
- Apply its own Harmony patches in `Init` and remove them with `UnpatchSelf()` in `Shutdown`.
  Leaving a detour pointing into an unloaded assembly is a crash, not a leak.
- Destroy its UniverseLib panel in `Shutdown`. The `UIBase` belongs to the loader and is reused.
- Keep no state it expects to survive a reload. Preferences do (they are files); statics do not.

## Honest limits

- The old context may not actually be collected straight away. Delegates handed to Il2Cpp -
  UI button listeners in particular - hold GC handles to plugin methods until the Il2Cpp side
  finalises them. Correctness does not depend on collection: the old plugin has fully detached,
  the new one owns everything. It is memory that lingers, not behaviour.
- Changing the **loader** still needs a restart. It should almost never need changing.
- If `Init()` throws, the loader logs it and leaves you with no plugin rather than a half-loaded
  one. Fix the build, save, and it reloads.

## Building the loader

```
cd ModSource/ShiftAtMidnightSuite.Loader
dotnet build -c Release
copy bin\Release\ShiftAtMidnightSuite.Loader.dll ..\..\Mods\
```

The plugin project references the loader's `bin/Release` output, so build the loader first the
first time.
