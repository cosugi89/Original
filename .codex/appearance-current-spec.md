# Appearance Current Spec

This note is for Codex working memory inside this workspace. It is not project documentation.

Last updated: 2026-05-08

## Scope

- Current enemy appearance authoring for `StageBattleEnemyMasterData.AppearanceSlots / AppearanceColors`
- Current runtime path using `PartsManager.Init()` and `ApplyAppearanceData(...)`
- Current caveats before any future appearance-system refactor

## Current Direction

- Keep save/runtime-facing appearance state as resolved or ID-based data.
- Use `EquipmentMasterData` as the reusable appearance master asset for visual parts.
- Master-side enemy authoring now prefers `EquipmentMasterData` references instead of direct `partsIndex` entry authoring.
- Runtime battle code still receives resolved `AppearanceData`.
- `EquipmentId` is now an `int`, not a semantic string key.
- Current convention uses `0` as "none / unequipped".

## Equipment Appearance Asset Generation

- `EquipmentMasterData` is currently the shared appearance master asset.
- A rebuild tool exists at:
  - `Tools > Original > Rebuild Equipment Database`
- Tool source:
  - `Assets/Scripts/Editor/EquipmentMasterCatalogBuilder.cs`
- The tool reads:
  - `Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab`
- It generates or updates:
  - `Assets/Resources/MasterData/EquipmentDatabase.asset`
  - `Assets/Resources/MasterData/Equipments/<PartsType>/<PartsType>_<index>.asset`
- Generated asset content includes:
  - `equipmentId`
  - `displayName`
  - `partType`
  - `exclusiveGroup` as a derived runtime concept from `partType`
  - `partsIndex`
  - `icon`
  - `sortOrder`
  - `isDefaultOwned`
- Current generator intentionally skips:
  - `Arrow`
  - `HelmetHair`
  - `Skin`
- Current rebuild behavior:
  - always syncs `partType`
  - always syncs `partsIndex`
  - always syncs `icon`
  - initializes `sortOrder` from `partsIndex` when unset
  - preserves existing `equipmentId` if already set
  - preserves existing `displayName` if already set
  - preserves existing `isDefaultOwned`

## Equipment ID Rules

- `EquipmentIdUtility.Build(partType, partsIndex)` now returns `int`.
- Generated IDs currently use a `partType` block convention, but runtime/save data should treat any positive `int` as a valid equipment ID.
- Legacy string ids like `Hair:10` are accepted only during JSON migration.
- Runtime appearance resolution should not depend on parsing IDs back into parts.
- `EquipmentMasterData.OnValidate()` no longer overwrites `equipmentId` from `partType + partsIndex`.

## Runtime Resolution Rules

- `PartsManagerAvatarAdapter.ApplyAppearance(...)` now resolves visual parts only through `InventoryService -> EquipmentData`.
- The old fallback of reconstructing `partsIndex` from `EquipmentId` is removed.
- `PartsManagerAvatarAdapter.CaptureAppearance(...)` also no longer fabricates semantic IDs from `partType:index`.
- This means:
  - `EquipmentMasterData` / `EquipmentData` are now the source of truth
  - missing master definitions lead to unresolved equipment ids instead of hidden fallback behavior

## Equipment Master Output

- `EquipmentMasterData` exposes:
  - `EquipmentId`
  - `DisplayName`
  - `PartType`
  - `ExclusiveGroup`
  - `IsRightHandEquipment`
  - `IsLeftHandEquipment`
  - `PartsIndex`
  - `Icon`
  - `SortOrder`
  - `IsDefaultOwned`
- Inspector authoring guidance now uses `Tooltip` on serialized fields in `EquipmentMasterData`.
- A custom inspector exists for `EquipmentMasterData` and shows single-asset validation hints while editing one equipment asset.

## Catalog Validation Rules

- `EquipmentMasterCatalog` now builds validation issues for:
  - missing `EquipmentId`
  - missing `PartsIndex`
  - missing `Icon`
  - duplicate `EquipmentId`
  - duplicate `PartType + PartsIndex`
  - null entries inside the catalog list
- Validation is intended as the main place to catch broken equipment appearance master data.
- A custom inspector exists for `EquipmentMasterCatalog` and shows these validation issues directly in Inspector.

## Runtime Path

- `BattleScene` creates both player and enemy character instances.
- Enemy rendering currently uses:
  - `PartsManager.Init()`
  - `PartsManager.ApplyAppearanceData(_activeEnemyData.Appearance)`
- Relevant file:
  - `Assets/Scripts/Features/Battle/Demo/BattleScene.cs`

## Current Data Shape

- Enemy master uses `AppearanceData`.
- `AppearanceData` has:
  - `parts: List<PartsEntry>` where each entry is `PartsType + index`
  - `colors: List<ColorEntry>` where each entry is `ColorTargetType + Color`
  - `visibility: List<VisibilityEntry>` where each entry is `PartsType + visible`
- Relevant file:
  - `Assets/Scripts/Data/DTO/AppearanceData.cs`

## Enemy Master Data Shape

- `StageBattleEnemyMasterData` now has:
  - `AppearanceSlots`
  - `AppearanceColors`
- `AppearanceSlots` use:
  - `PartsType`
  - `EquipmentMasterData` reference
  - `IsVisible`
- `AppearanceColors` use:
  - `ColorTargetType`
  - `UnityEngine.Color`
- Loader resolves these into runtime `AppearanceData`.
- `legacyAppearance` fallback is removed.

## PartsManager Init Behavior

- `Init()` builds category maps and sets every known part to:
  - `ActiveIndices[type] = 0`
  - `Visibility[type] = category.DefaultVisible`
- Current `Character.prefab` categories are effectively all `defaultVisible = true`.
- `Init()` also sets:
  - `Skin`, `Hair`, `Eye`, `Beard` colors to white
- Current special handling inside `Init()`:
  - `Beard` is forced to `CanChangeColor = true` and `ColorTarget = Beard` if needed
  - `Eye` is forced to `CanChangeColor = false`
- After base setup, `Init()` runs:
  - `SyncHelmetHairVisibility()`
  - `SyncArrowVisibility()`
- Relevant file:
  - `Assets/Scripts/Core/PartsManager.cs`

## ApplyAppearanceData Behavior

- `parts` entries are applied first.
- `index < 0` means `UnequipParts(type)`.
- `index >= 0` means `EquipParts(type, index)`.
- `colors` are then applied with `SetColor`.
- `visibility` is then applied, except:
  - `Arrow`
  - `HelmetHair`
- Those two are skipped because they are derived parts.
- After all entries are applied, sync runs again:
  - `SyncArrowVisibility()`
  - `SyncHelmetHairVisibility()`

## Authoring Rules For Enemy Appearance

- Enemy appearance master authoring is now reference-based:
  - choose `EquipmentMasterData` assets per visible part slot
  - set colors separately
- Enemy runtime application is still direct-index based after loader resolution.
- Ground truth for valid indices is the `PartsManager` category data in:
  - `Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab`
- Do not author these directly in `AppearanceData.visibility`:
  - `Arrow`
  - `HelmetHair`
- Prefer not to author these directly in `AppearanceData.parts` either:
  - `Arrow`
  - `HelmetHair`
- `Skin` should usually be handled by `colors`, not by `parts`.
- If a part is omitted entirely, its `Init()` default remains.
- Because defaults are currently visible and index `0`, deterministic enemy looks usually require explicitly hiding or unequipping unwanted parts.

## Enemy Loader Resolution Rules

- When `AppearanceSlots` exist:
  - loader starts from a deterministic hidden state for all non-derived, non-skin parts
  - each slot resolves `EquipmentMasterData.PartType + PartsIndex`
  - mismatched or missing references resolve to unequipped/hidden
- Even when no slots are authored:
  - loader starts from hidden / unequipped defaults for non-derived parts
  - loader applies only authored colors
- Derived parts remain runtime-synced:
  - `Arrow`
  - `HelmetHair`

## Important Caveat

- `ApplyAppearanceData` does not use `SetGroupActiveType(...)`.
- It applies parts and visibility directly.
- That means right-hand and left-hand equipment exclusivity is not automatically enforced by appearance authoring alone.
- If multiple weapon/shield/subitem parts remain visible, they can overlap visually.
- Current battle enemy authoring should explicitly hide or unequip unwanted equipment parts.

## Derived-Part Rules

- `Arrow`
  - visibility is derived from `Bow` or `Crossbow`
  - `Bolt` renderer is shown only for `Crossbow`
- `HelmetHair`
  - visibility is derived from `Hair` visibility plus `Helmet` visibility
  - hair index also propagates to `HelmetHair` when `Hair` is equipped

## Valid PartsType Index Ranges

- `Eye`: `0-19`
- `Hair`: `0-25`
- `Helmet`: `0-75`
- `Beard`: `0-14`
- `Chest`: `0-56`
- `Sword`: `0-31`
- `Axe`: `0-13`
- `Bow`: `0-12`
- `Shield`: `0-16`
- `Wand`: `0-9`
- `Staff`: `0-17`
- `Spear`: `0-16`
- `Blunt`: `0-12`
- `Crossbow`: `0-19`
- `SubItem`: `0-51`
- `Arrow`: `0-19`
- `HelmetHair`: `0-25`
- `Skin`: no sprite index set; treat as color-driven

## Color Targets

- `Skin`
- `Hair`
- `Eye`
- `Beard`

## Practical Authoring Guidance For June

- `June.asset` currently has empty `appearance.parts/colors/visibility`.
- Under the new system, `June` should be filled mainly through:
  - `AppearanceSlots`
  - `AppearanceColors`
- To make `June` deterministic, define at least:
  - visible face/hair/body/weapon choices
  - explicit hides or unequips for unwanted equipment groups
  - base colors for skin, hair, and optionally beard and eye
- If a future refactor migrates enemy appearance to equipment-id-based authoring, revisit this note and the current direct-index assumption.

## Source Files

- `Assets/Scripts/Features/Battle/Demo/BattleScene.cs`
- `Assets/Scripts/Core/PartsManager.cs`
- `Assets/Scripts/Data/DTO/AppearanceData.cs`
- `Assets/Scripts/Data/MasterData/EquipmentMasterData.cs`
- `Assets/Scripts/Data/MasterData/StageBattleEnemyMasterData.cs`
- `Assets/Resources/MasterData/Enemies/June.asset`
- `Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab`
- `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Core/CharacterEnums.cs`
