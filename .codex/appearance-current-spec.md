# 見た目システム現行仕様メモ

このメモは、このワークスペースでの Codex の作業記憶です。
プロジェクト仕様ではなく、将来見た目システムをリファクタリングする前の現状整理として扱います。

最終更新: 2026-05-08

## 対象範囲

- `StageBattleEnemyMasterData.AppearanceSlots / AppearanceColors` による現在の敵見た目設定
- `PartsManager.Init()` と `ApplyAppearanceData(...)` を使う現在の実行時経路
- 将来見た目システムをリファクタリングする前に把握すべき現行の注意点

## 現在の方向性

- セーブ / 実行時向けの見た目状態は、解決済みデータまたは ID ベースのデータとして保持する。
- 見た目パーツの再利用可能なマスターアセットとして `EquipmentMasterData` を使う。
- マスター側の敵見た目設定は、直接 `partsIndex` を書くより `EquipmentMasterData` 参照を優先する。
- 実行時の戦闘コードは、引き続き解決済みの `AppearanceData` を受け取る。
- `EquipmentId` は意味付き文字列キーではなく `int`。
- 現在の規約では `0` を「なし / 未装備」として扱う。

## 装備見た目アセット生成

- `EquipmentMasterData` は現在、共有の見た目マスターアセットとして使われている。
- 再生成ツールは次の場所にある。
  - `Tools > Original > Rebuild Equipment Database`
- ツールソース:
  - `Assets/Scripts/Editor/EquipmentMasterCatalogBuilder.cs`
- ツールは次を読み取る。
  - `Assets/Resources/Prefabs_Sample/Character/Character.prefab`
- ツールは次を生成または更新する。
  - `Assets/Resources/MasterData/EquipmentDatabase.asset`
  - `Assets/Resources/MasterData/Equipments/<PartsType>/<PartsType>_<index>.asset`
- 生成されるアセット内容は次の通り。
  - `equipmentId`
  - `displayName`
  - `partType`
  - `exclusiveGroup`: `partType` から導出される実行時概念
  - `partsIndex`
  - `icon`
  - `sortOrder`
  - `isDefaultOwned`
- 現在の生成処理は次を意図的に除外する。
  - `Arrow`
  - `HelmetHair`
  - `Skin`
- 現在の再生成時の挙動は次の通り。
  - `partType` は常に同期する。
  - `partsIndex` は常に同期する。
  - `icon` は常に同期する。
  - `sortOrder` が未設定なら `partsIndex` から初期化する。
  - 既存の `equipmentId` がある場合は維持する。
  - 既存の `displayName` がある場合は維持する。
  - 既存の `isDefaultOwned` は維持する。

## 装備 ID ルール

- `EquipmentIdUtility.Build(partType, partsIndex)` は現在 `int` を返す。
- 生成 ID は現在 `partType` ごとのブロック規約を使うが、実行時 / セーブデータは正の `int` であれば有効な装備 ID として扱う。
- `Hair:10` のような旧文字列 ID は、JSON 移行時だけ受け付ける。
- 実行時の見た目解決は、ID をパーツ情報へ逆解析することに依存しない。
- `EquipmentMasterData.OnValidate()` は、`partType + partsIndex` から `equipmentId` を上書きしない。

## 実行時解決ルール

- `PartsManagerAvatarAdapter.ApplyAppearance(...)` は、見た目パーツを `InventoryService -> EquipmentData` 経由でのみ解決する。
- `EquipmentId` から `partsIndex` を再構築する古い代替処理は削除済み。
- `PartsManagerAvatarAdapter.CaptureAppearance(...)` も、`partType:index` から意味付き ID を捏造しない。
- つまり現在は次が正本になる。
  - `EquipmentMasterData` / `EquipmentData`
  - マスター定義が欠けている場合は、隠れた代替挙動ではなく未解決の装備 ID になる。

## 装備マスター出力

- `EquipmentMasterData` は次を公開する。
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
- Inspector 上の作成ガイドは、`EquipmentMasterData` の serialized field 上の `Tooltip` を使う。
- `EquipmentMasterData` にはカスタム Inspector があり、単一装備アセット編集時に検証ヒントを表示する。

## カタログ検証ルール

- `EquipmentMasterCatalog` は次の検証問題を組み立てる。
  - `EquipmentId` 欠落
  - `PartsIndex` 欠落
  - `Icon` 欠落
  - `EquipmentId` 重複
  - `PartType + PartsIndex` 重複
  - catalog list 内の null entry
- 検証は、壊れた装備見た目マスターデータを見つける主な場所として意図されている。
- `EquipmentMasterCatalog` にはカスタム Inspector があり、これらの検証問題を Inspector 上で直接表示する。

## 実行時経路

- `BattleScene` はプレイヤーと敵の character instance を両方生成する。
- 敵描画は現在、次を使う。
  - `PartsManager.Init()`
  - `PartsManager.ApplyAppearanceData(_activeEnemyData.Appearance)`
- 関連ファイル:
  - `Assets/Scripts/Features/Battle/Demo/BattleScene.cs`

## 現在のデータ形状

- 敵マスターは `AppearanceData` を使う。
- `AppearanceData` は次を持つ。
  - `parts: List<PartsEntry>`: 各 entry は `PartsType + index`
  - `colors: List<ColorEntry>`: 各 entry は `ColorTargetType + Color`
  - `visibility: List<VisibilityEntry>`: 各 entry は `PartsType + visible`
- 関連ファイル:
  - `Assets/Scripts/Data/DTO/AppearanceData.cs`

## 敵マスターデータ形状

- `StageBattleEnemyMasterData` は現在次を持つ。
  - `AppearanceSlots`
  - `AppearanceColors`
- `AppearanceSlots` は次を使う。
  - `PartsType`
  - `EquipmentMasterData` reference
  - `IsVisible`
- `AppearanceColors` は次を使う。
  - `ColorTargetType`
  - `UnityEngine.Color`
- loader はこれらを実行時の `AppearanceData` へ解決する。
- `legacyAppearance` の代替処理は削除済み。

## PartsManager 初期化挙動

- `Init()` はカテゴリマップを構築し、既知の全パーツを次の状態にする。
  - `ActiveIndices[type] = 0`
  - `Visibility[type] = category.DefaultVisible`
- 現在の `Character.prefab` のカテゴリは実質的にすべて `defaultVisible = true`。
- `Init()` は次の色も白に設定する。
  - `Skin`
  - `Hair`
  - `Eye`
  - `Beard`
- `Init()` 内の現在の特殊処理は次の通り。
  - `Beard` は必要に応じて `CanChangeColor = true` と `ColorTarget = Beard` を強制する。
  - `Eye` は `CanChangeColor = false` を強制する。
- 基本セットアップ後、`Init()` は次を実行する。
  - `SyncHelmetHairVisibility()`
  - `SyncArrowVisibility()`
- 関連ファイル:
  - `Assets/Scripts/Core/PartsManager.cs`

## ApplyAppearanceData 挙動

- `parts` entry を先に適用する。
- `index < 0` は `UnequipParts(type)` を意味する。
- `index >= 0` は `EquipParts(type, index)` を意味する。
- その後、`colors` を `SetColor` で適用する。
- 最後に `visibility` を適用する。ただし次は除外する。
  - `Arrow`
  - `HelmetHair`
- この2つは派生パーツなので除外する。
- すべての entry 適用後、再度同期を実行する。
  - `SyncArrowVisibility()`
  - `SyncHelmetHairVisibility()`

## 敵見た目の作成ルール

- 敵見た目マスターの作成は現在、参照ベース。
  - 表示するパーツスロットごとに `EquipmentMasterData` アセットを選ぶ。
  - 色は別途設定する。
- 敵の実行時適用は、loader 解決後も直接 index ベース。
- 有効な index の正本は次にある `PartsManager` のカテゴリデータ。
  - `Assets/Resources/Prefabs_Sample/Character/Character.prefab`
- 次は `AppearanceData.visibility` に直接書かない。
  - `Arrow`
  - `HelmetHair`
- 次も `AppearanceData.parts` に直接書かないことを推奨する。
  - `Arrow`
  - `HelmetHair`
- `Skin` は通常、`parts` ではなく `colors` で扱う。
- パーツが完全に省略された場合は、`Init()` の初期値が残る。
- 現在の初期値は表示かつ index `0` なので、敵の見た目を決定的にするには通常、不要パーツの明示的な非表示または装備解除が必要。

## 敵 Loader 解決ルール

- `AppearanceSlots` が存在する場合:
  - loader は非派生 / 非 skin パーツをすべて非表示にした決定的な非表示状態から開始する。
  - 各 slot は `EquipmentMasterData.PartType + PartsIndex` へ解決される。
  - 参照の不一致や欠落は未装備 / 非表示として解決される。
- slot が作成されていない場合でも:
  - loader は非派生パーツの非表示 / 未装備初期値から開始する。
  - loader は作成済みの色だけを適用する。
- 派生パーツは実行時同期に任せる。
  - `Arrow`
  - `HelmetHair`

## 重要な注意点

- `ApplyAppearanceData` は `SetGroupActiveType(...)` を使わない。
- parts と visibility を直接適用する。
- そのため、右手 / 左手装備の排他性は見た目作成だけでは自動的に保証されない。
- 複数の武器 / 盾 / サブアイテムパーツが表示されたまま残ると、見た目が重なる可能性がある。
- 現在の戦闘敵作成では、不要な装備パーツを明示的に非表示または装備解除すること。

## 派生パーツルール

- `Arrow`
  - 表示状態は `Bow` または `Crossbow` から導出される。
  - `Bolt` renderer は `Crossbow` の場合だけ表示される。
- `HelmetHair`
  - 表示状態は `Hair` の表示状態と `Helmet` の表示状態から導出される。
  - `Hair` が装備されている場合、hair index も `HelmetHair` へ伝播する。

## 有効な PartsType Index 範囲

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
- `Skin`: sprite index は持たない。色駆動として扱う。

## 色対象

- `Skin`
- `Hair`
- `Eye`
- `Beard`

## June 用の実務作成ガイド

- `June.asset` は現在 `appearance.parts/colors/visibility` が空。
- 新システムでは、`June` は主に次で埋める。
  - `AppearanceSlots`
  - `AppearanceColors`
- `June` の見た目を決定的にするには、少なくとも次を定義する。
  - 表示する face / hair / body / weapon の選択
  - 不要な装備グループの明示的な非表示または装備解除
  - skin、hair、必要なら beard と eye の基本色
- 将来、敵見た目を装備 ID ベースの作成へ移行する場合は、このメモと現在の直接 index 前提を見直す。

## 関連ファイル

- `Assets/Scripts/Features/Battle/Demo/BattleScene.cs`
- `Assets/Scripts/Core/PartsManager.cs`
- `Assets/Scripts/Data/DTO/AppearanceData.cs`
- `Assets/Scripts/Data/MasterData/EquipmentMasterData.cs`
- `Assets/Scripts/Data/MasterData/StageBattleEnemyMasterData.cs`
- `Assets/Resources/MasterData/Enemies/June.asset`
- `Assets/Resources/Prefabs_Sample/Character/Character.prefab`
- `Assets/Scripts/Core/Appearance/CharacterEnums.cs`
