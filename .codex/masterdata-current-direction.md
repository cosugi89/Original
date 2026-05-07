# MasterData Current Direction

This note is for Codex working memory inside this workspace. It is not project documentation.

Last updated: 2026-05-08

## Core Decisions

- Use `Database` as the unified naming for master-data list/search assets.
- Use `int` as the unified ID type for master-data identifiers.
- Keep master asset references as direct `ScriptableObject` references where authoring benefits from it.
- Treat `Database` assets as list/lookup utilities, not as the main authoring relationship when direct references already exist.
- Do not keep `BattleStage*` as a separate long-term line; absorb that role into `StageMaster*`.
- Validation is not a primary design goal for master data. Keep authoring simple by default.
- Reduce `MasterDataResourceLoader` fallback and legacy branches gradually rather than preserving them indefinitely.

## Naming Direction

- Prefer names like:
  - `StageMasterData`
  - `StageMasterDatabase`
  - `StageBattleEnemyMasterData`
  - `StageBattleEnemyMasterDatabase`
  - `StageBattlePatternMasterData`
  - `StageBattlePatternMasterDatabase`
  - `EquipmentMasterData`
  - `EquipmentMasterDatabase`
  - `BattleSkillMasterData`
  - `BattleSkillMasterDatabase`
- Avoid mixing `Catalog` and `Database` for the same kind of role.

## ID Direction

- New master-data IDs should use `int`.
- Existing string IDs should be considered migration targets unless there is a very strong reason to keep them.
- Save/runtime data should also prefer `int` IDs for master-data references.

## Reference Direction

- Authoring-side relationships may use direct `ScriptableObject` references:
  - `Stage -> Enemy`
  - `Enemy -> Pattern`
  - `Equipment -> Skill`
- `Database` assets remain useful for:
  - global lists
  - ID lookup
  - selection UI sources
  - migration or verification helpers

## Stage Direction

- `StageMasterData` should become the single long-term source of truth for stage master data.
- `BattleStageMasterData` / `BattleStageMasterCatalog` are not the desired end state.
- If stage-selection-specific data is needed, prefer extending `StageMasterData` rather than maintaining a second parallel stage master line.

## Validation Direction

- Default stance: no heavy validation-first architecture for master data.
- Keep authoring friction low.
- Small local checks may still be acceptable when they directly prevent obvious breakage, but validation systems should not dominate the design.

## Loader Direction

- `MasterDataResourceLoader` may temporarily keep fallback/legacy branches during migration.
- Long term, remove:
  - legacy duplicate master paths
  - old compatibility-only branches once assets are migrated
  - unnecessary fallback generation that obscures the real source of truth
- Prefer a simpler loader that resolves current master data cleanly over a broad compatibility hub.

## Working Rule For Future Refactors

When touching `Assets/Scripts/Data/MasterData`, prefer this order:

1. Remove parallel naming or duplicate concepts.
2. Normalize ID types to `int`.
3. Keep direct reference authoring where it is clearer.
4. Keep `Database` only as the shared lookup/list layer.
5. Reduce legacy/fallback paths if the current assets are already migrated.
