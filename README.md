# Shift At Midnight Suite

One MelonLoader mod replacing the whole previous stack. Source lives in
`ShiftAtMidnightSuite/`, the built DLL goes in `Mods/`.

**F1** opens the menu. It is a real mouse-driven UniverseLib panel: **click a category in the
sidebar**, click checkboxes, click the -/+ steppers, type in the search box. Drag the title bar to
move it, drag an edge to resize. No TAB cycling, no arrow keys. UnityExplorer keeps F7.

To use a different key, set `MenuKey` under `[ShiftAtMidnightSuite]` in
`UserData/MelonPreferences.cfg` to any `UnityEngine.KeyCode` name (`F4`, `Insert`, `Backspace`, ...).

UniverseLib initialises a second or two after the game loads. If you press F1 before then - or if
UniverseLib is unavailable - a plain keyboard-driven fallback menu opens instead, and the log says
which one you got.

## What it replaces

| Old mod | Where it went |
|---|---|
| `ItemSpawnerTrainer.dll` | SPAWNER page |
| `ShiftAtMidnightModMenu.dll` | the menu itself |
| `AutoShelfFiller.dll` | STORE page - Auto Stock Shelves |
| `ShiftAtMidnightAutoMop.dll` | STORE page - Auto Clean Spills |
| `ShiftAtMidnightAutoLimbCleaner.dll` | STORE page - Auto Clean Trash |
| `DoppelgangerDetector.dll` | EVENTS page - Doppelganger Detector |
| `GunCaseAllowEveryone.dll` (BepInEx) | WORLD page - Gun Case For Everyone |

Originals are kept in `_replaced-mods-backup/`. UnityExplorer is untouched.

## Bugs that were fixed

**Item spawner refused to spawn some items.** The old trainer searched the loaded scene for a
`PickupObject` whose `objectIndex` matched, and gave up if none existed - so anything not already
lying around that night could not be spawned, and its item list was a hardcoded 52 of 59 ids. The
actual catalogue is `StoreManager.pickupObjs`, a prefab array indexed by item id. The spawner now
reads that, so every id in the game is spawnable and the list is built from the real array length.

**Spawned items were local ghosts.** Spawning went through `Object.Instantiate`, which no other
player ever saw. It now goes through the game's own networked path - `Rpc_CMD_NetworkDropObject` -
so everyone sees what you spawn. Direct instantiation is only the offline fallback.

**Spawned grenades exploded on landing.** `StoreManager` keeps two prefab arrays, `pickupObjs`
(inert) and `thrownObjs` (armed), and the first version spawned through `ServerThrowObject` - the
throw path, which arms explosives by design. Drop semantics are the default now and explosives are
never sent down the throw path, not even as a fallback. Items are classified by their real prefab
name rather than a hardcoded id table, since the inherited id list is not guaranteed to match this
build's array order. A *Spawn Method* stepper (Auto / Drop / Throw / Local) can force one path.

**Infinite ammo did nothing for spawned guns.** Two causes. Spawned weapons got `itemStorage = 0`,
so they arrived empty - storage is now auto and gives weapons a full load. And the cheat only held
each slot at the highest value it had ever seen, so a gun that arrives at 0 has a high-water mark of
0 and never refills; weapon slots are now topped up to a target instead. Only slots holding an
actual weapon are touched, because `itemStorages` also carries crate contents and forcing those to
99 would break stocking.

**Auto-clean left most of the mess.** `IsCleanableBloodSpill` required the object to be named
exactly `BloodSpill(Clone)` and tagged `Mess`, so anything else - bathroom blots, vomit, oil - was
skipped, and the `Moppable` component was never touched at all even though it is what most floor
mess actually uses. It now cleans every `Spill` and every `Moppable` regardless of name, plus loose
`Trash`. A failed clean is retried after 3s instead of blacklisting the object for the night.

**Auto-stock wedged on shelves it could not finish.** It derived a per-slot cap of
`maxProductsOnShelf / slotCount`; when that did not divide evenly (10 across 3 slots caps at 9)
every slot read "full" while the shelf was still incomplete, and stocking stalled forever. It now
works off the shelf's own `productsOnShelf` vs `maxProductsOnShelf` and fills the emptiest slot.

**Auto-stock needed a crate in hand.** It only armed from a Harmony hook on crate pickup. It now
polls instead, and by default needs no crate at all - turn on *Only Stock From Held Crate* for the
old behaviour.

**Gun case patch targeted the wrong type.** The BepInEx plugin patched `GunCase.Interact`, but
`GunCase` does not override `Interact` - it inherits it from `Interactable`. The patch now sits on
`Interactable` and filters for gun cases, backed by a once-a-second sweep that also catches cases
spawned later.

## What is new

- **Doppelganger spawner** - 1, 3, or the game's own 8-doppelganger horde.
- **Event menu** - every `EventManager.eventAtlas` entry plus ~35 named set pieces (rats, roaches,
  flickering lights, shrine, trucks, Vanessa, dentist, rake, pets, ...) firable on demand.
- **x3 customers and events** - Harmony postfixes on `EndlessGenerationManager.GenerateNight` and
  `CurrentDayManager.SetUpDay` scale the night's generated lists. Multiplier is 1-10, default 3.
- **Weapon wall always open** - clears `HuntManager.alreadyEnabledWeaponsArsenal` and switches the
  racks on, so the emergency arsenal is usable in the day with no entity to fight.
- **Full arsenal unlock** - marks every `PurchaseManager` weapon and store-upgrade node as bought on
  the save, pushes it live via `SetStoreUpgradesAndWeaponsArsenal` / `Rpc_GetLockedAndLoaded`, then
  opens the wall.
- **Block rake spawns** - prefixes on all five rake entry points (`SpawnRake`, `Rpc_SpawnRake`,
  `SpawnRakeFromPos`, `Rpc_EnableForestRakeRpc`, `InitializeForestRake`).
- **Infinite store refreshes** - keeps `SaveManager.refreshes` topped up.
- **Hat / cosmetic unlocker** - fills `SaveManager.customizablesUnlocked` and saves.
- **Inventory slots** - raise `maxInventorySlots` (capped to what the slot UI can draw).
- **Freeze item stacks** - grenades, molotovs and bricks stop being consumed.
- **Doppelganger radar** - counts every doppelganger nearby, not just the one you look at.

## Host vs client

Rows marked *host only* are greyed out when you do not have state authority. Item spawning works
either way: host spawns directly, client asks the host.

## Building

```
cd ShiftAtMidnightSuite
dotnet build -c Release
```

References resolve from `MelonLoader/net6`, `MelonLoader/Il2CppAssemblies` and
`UserLibs/UniverseLib.ML.IL2CPP.Interop.dll` relative to the game folder, so no NuGet restore is
needed. The game must be closed to copy the DLL into `Mods/`.

Settings persist to `UserData/MelonPreferences.cfg` under `ShiftAtMidnightSuite`.
