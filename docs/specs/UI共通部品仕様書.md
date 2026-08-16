# UI 共通部品仕様書

- 最終更新: 2026-08-15
- 文書状態: 運用中
- 位置づけ: プロジェクト内で共通利用する UI 部品の責務と運用方針を定義する。

## 1. GameButton

`GameButton` は、プロジェクト内で汎用ボタンを作るときの標準コンポーネントである。

- 実体は `Assets/Scripts/UI/Components/Button/GameButton.cs` に置く。
- `UnityEngine.UI.Button` を継承し、`Button` としてそのまま参照できる。
- 基本 Prefab は `Assets/Resources/Prefabs/GameButton.prefab` に置く。
- Prefab は `CanvasGroup`、`Animator`、`AudioSource` を持ち、`Assets/Resources/Animations/Button/Button.controller` を参照する。
- `Button.controller` は Unity 標準の Button Animation Transition 用 trigger (`Normal` / `Highlighted` / `Pressed` / `Selected` / `Disabled`) を使う。
- Button 系 Sprite は `Assets/Assets/Sprite_Common/Button` 配下の project-owned asset を参照する。
- Badge の既定 Sprite は `Assets/Assets/Sprite_Dark/HUD/Alert_Dot_01_Red.png` を参照する。
- `GameButton.prefab` は Layer Lab の Prefab / Sprite へ依存しない。Layer Lab は見た目の参考元としてのみ扱う。
- 見た目は `FrameType`、`ColorType`、`SizeType`、`ContentType` の enum で切り替える。
- `FrameType` は `Button01`、`Button02`、`Button03` を持つ。
- `ColorType` は `Blue`、`Brown`、`Dark`、`Gray`、`Green`、`Mint`、`OffWhite`、`Orange`、`Pink`、`Plum`、`Red`、`Yellow` を持つ。
- `SizeType` は `Small`、`Medium`、`Large`、`IconSmall`、`IconMedium`、`Wide`、`TitleWide` を持つ。
- `ContentType` は `None`、`Label`、`Icon`、`IconLabel`、`TitleIconLabel` を持つ。
- Icon は `iconSprite` が未設定なら表示しない。
- Label / Title / Value は対応文字列が空なら表示しない。
- `GameButton` 専用 Inspector は標準 `Button` 設定の下に独自項目を表示する。これは Unity の `ButtonEditor` が `Button` 継承先にも適用されるため、専用 `CustomEditor` で補う。
- Badge、選択表示、無効表示、Loading 表示は任意参照として扱う。
- C# からは `SetAction(...)`、`AddAction(...)`、`RemoveAction(...)`、`ClearAction()` で、ユーザー操作による click / submit 時の追加処理を扱える。
- C# からは `SetAppearance(...)`、`SetLabel(...)`、`SetTitle(...)`、`SetValue(...)`、`SetIcon(...)` で見た目と内容を切り替えられる。
- Inspector からは継承元 `Button.onClick` を使う。
- 表示状態は `SetVisible(...)`、操作可否は `SetInteractable(...)`、選択状態は `SetSelected(...)`、Loading 状態は `SetLoading(...)` で切り替える。
- Badge 表示は `SetBadgeVisible(...)`、`SetBadgeText(...)` で切り替える。
- Loading 中は内部的に `interactable = false` として扱い、Loading 解除時に Loading 前の操作可否へ戻す。
- Badge は Prefab に常備し、`badgeVisible` または `SetBadgeVisible(...)` で表示を切り替える。
- Badge 背景は `badgeSprite` を使い、`GameButton` の見た目再適用時に `badgeImage` の Sprite へ反映する。
- Badge text は空なら非表示、文字列がある場合のみ表示する。
- SE は `audioSource` が設定されている場合のみ再生する。
- 通常押下と操作不可時の押下は、それぞれ `SoundType` で `Assets/Resources/Sounds` 配下の音を選ぶ。
- `SoundType.Custom` を選んだ場合のみ、個別の `AudioClip` 参照を使う。
- `GameButton.prefab` の初期値は通常押下 `Button1`、操作不可時 `Error` とする。

Hierarchy 上では `GameObject/UI/Game Button` から新規作成できる。
このメニューは `GameButton.prefab` が存在する場合は Prefab インスタンスを生成し、Prefab がない場合のみ同等構成の GameObject を作る。

## 2. Button Transition

`GameButton` は `Button` を継承しているため、Unity 標準の Transition をそのまま使う。

- `None`: 状態変化による見た目変更を行わない。
- `ColorTint`: `targetGraphic` の色を `Normal` / `Highlighted` / `Pressed` / `Selected` / `Disabled` ごとに変える。
- `SpriteSwap`: `targetGraphic` の Sprite を状態ごとに差し替える。
- `Animation`: `Animator` に状態ごとの trigger を送る。`GameButton.prefab` はこの方式を使い、`Button.controller` で `CanvasGroup` の alpha を変える。

`GameButton.SetSelected(...)` の選択表示は、Unity 標準の `Selected` Transition とは別に、任意の `selectedRoot` 表示を切り替えるための補助状態である。

## 3. 移行方針

既存 UI の `Button` 参照は一括置換しない。
新規 UI から `GameButton` を優先して使い、既存 Dialog と主要 UI は触るタイミングで段階的に置き換える。
`Dialog-StatusEquipmentDetail.prefab` の `Button_Equip` と `Button_Close` は `GameButton` へ移行済みである。
