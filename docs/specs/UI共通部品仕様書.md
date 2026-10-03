# UI 共通部品仕様書

- 最終更新: 2026-10-03
- 文書状態: 運用中
- 位置づけ: プロジェクト内で共通利用する UI 部品の責務と運用方針を定義する。

## 0. サンプル Prefab 資産の管理

外部アセット由来の参考用 Prefab は `Assets/Resources/Prefabs_Sample` に集約する。プロジェクト本体で使用する Prefab は `Assets/Resources/Prefabs` を正とし、サンプル資産をそのまま実装へ使わない。

- `Base`: Button、Frame、Label などの土台 Prefab。
- `UI`: Button、Control、HUD、Popup など画面部品別の Prefab。
- `Demo`: レイアウト例と画面例。
- `Character`: Character 作成用の参考 Prefab。

重複したサンプル Prefab は、内容差分の有無にかかわらず、Scene / Prefab / Addressables から参照されていない複製だけを削除する。移動時は `.meta` を維持して GUID を変えない。

## 1. GameButton

`GameButton` は、プロジェクト内で汎用ボタンを作るときの標準コンポーネントである。

- 実体は `Assets/Scripts/UI/Components/Button/GameButton.cs` に置く。
- `UnityEngine.UI.Button` を継承し、`Button` としてそのまま参照できる。
- 基本 Prefab は `Assets/Resources/Prefabs/GameButton.prefab` に置く。
- Prefab は `CanvasGroup`、`Animator`、`AudioSource` を持ち、`Assets/Resources/Animations/Button/Button.controller` を参照する。
- `Button.controller` は Unity 標準の Button Animation Transition 用 trigger (`Normal` / `Highlighted` / `Pressed` / `Selected` / `Disabled`) を使う。
- Button 系 Sprite は `Assets/Resources/Sprite/Sprite_Common/Button` 配下の project-owned asset を参照する。
- Badge の既定 Sprite は `Assets/Resources/Sprite/Sprite_Dark/HUD/Alert_Dot_01_Red.png` を参照する。
- `GameButton.prefab` は Layer Lab の Prefab / Sprite へ依存しない。Layer Lab は見た目の参考元としてのみ扱う。
- 見た目は `ButtonType`、`FrameType`、`ColorType`、`SizeType`、`ContentType` の enum で切り替える。`ButtonType` は通常の `Normal` と、閉じる操作用の `Close` を持つ。
- `FrameType` は `Button01`、`Button02`、`Button03` を持つ。
- `ColorType` は `Blue`、`Brown`、`Dark`、`Gray`、`Green`、`Mint`、`OffWhite`、`Orange`、`Pink`、`Plum`、`Red`、`Yellow` を持つ。
- `ColorType` ごとの配色は `UIColors.IColorSet` を正本とし、`Background`、`Light`、`Highlight`、`Gradient`、`Shadow` を各 Frame へ適用する。`Button01InnerVisible` も `Button01` の内枠表示に適用する。`GameButton` 内に個別のカラーセットは持たない。
- `SizeType` は `Small`、`Medium`、`Large`、`IconSmall`、`IconMedium`、`Wide`、`TitleWide` を持つ。
- `ContentType` は `None`、`Label`、`Icon`、`IconLabel`、`TitleIconLabel` を持つ。
- スタイルの土台は専用 Inspector の `Apply Style`、または C# の `SetAppearance(...)` / `ApplyStyle()` で明示的に適用する。適用時は Frame、配色、既定サイズ、既定の子要素レイアウトを設定する。
- enum の Inspector 値を変更しただけではスタイルを自動適用しない。スタイル適用後の RectTransform、アンカー、余白、文字位置は Prefab を正本とし、画面・Prefab 固有の調整を維持する。
- `SetLabel(...)`、`SetTitle(...)`、`SetValue(...)`、`SetIcon(...)`、Badge 更新および状態更新は、内容・表示状態だけを更新し、RectTransform やレイアウトを変更しない。
- Icon は `iconSprite` が未設定なら表示しない。
- Label / Title / Value は対応文字列が空なら表示しない。
- `Close` は `FrameType.Button01` 相当の小型ボタンとして BG / Light / Icon のみを表示し、Label / Title / Value / Badge を表示しない。サイズ、配色、閉じるIconは `Dialog-EquipmentDetail` の `Button_Close_02` を基準にする。
- `GameButton` 専用 Inspector は標準 `Button` 設定の下に独自項目を表示する。これは Unity の `ButtonEditor` が `Button` 継承先にも適用されるため、専用 `CustomEditor` で補う。
- Badge、選択表示、無効表示、Loading 表示は任意参照として扱う。
- C# からは `SetAction(...)`、`AddAction(...)`、`RemoveAction(...)`、`ClearAction()` で、ユーザー操作による click / submit 時の追加処理を扱える。
- C# からは `SetAppearance(...)` / `ApplyStyle()` でスタイルを明示適用し、`SetLabel(...)`、`SetTitle(...)`、`SetValue(...)`、`SetIcon(...)` で内容を更新できる。
- Inspector からは継承元 `Button.onClick` を使う。
- 表示状態は `SetVisible(...)`、操作可否は `SetInteractable(...)`、選択状態は `SetSelected(...)`、Loading 状態は `SetLoading(...)` で切り替える。
- Badge 表示は `SetBadgeVisible(...)`、`SetBadgeText(...)` で切り替える。
- Loading 中は内部的に `interactable = false` として扱い、Loading 解除時に Loading 前の操作可否へ戻す。
- Badge は Prefab に常備し、`badgeVisible` または `SetBadgeVisible(...)` で表示を切り替える。
- Badge 背景は `badgeSprite` を使い、内容更新時に `badgeImage` の Sprite へ反映する。
- Badge text は空なら非表示、文字列がある場合のみ表示する。
- SE は `audioSource` が設定されている場合のみ再生する。
- 通常押下と操作不可時の押下は、それぞれ `SoundType` で `Assets/Resources/Sounds` 配下の音を選ぶ。
- `SoundType.Custom` を選んだ場合のみ、個別の `AudioClip` 参照を使う。
- `GameButton.prefab` の初期値は通常押下 `Button1`、操作不可時 `Error` とする。

Hierarchy 上では `GameObject/UI/Game Button` から新規作成できる。
このメニューは `GameButton.prefab` が存在する場合は Prefab インスタンスを生成し、Prefab がない場合のみ同等構成の GameObject を作る。

## 2. FeedbackButton

`FeedbackButton` は、既存の UI レイアウトを維持したままクリックSEと追加アニメーションだけを加える軽量コンポーネントである。

- 実体は `Assets/Scripts/UI/Components/Button/FeedbackButton.cs` に置く。
- `UnityEngine.UI.Button` を継承する。通常の `Button` と同じ GameObject に併置せず、必要な場合は通常 `Button` を `FeedbackButton` へ手動で置き換える。
- 見た目、サイズ、ラベル、ゲーム固有のクリック処理は持たない。既存の `Button.onClick` はそのまま利用できる。
- C# 用の追加クリック処理は `SetAction(...)` で設定できる。クリックSE、Animator Trigger、追加クリック処理は有効な click / submit 時だけ実行する。
- `Animator`、`AudioSource`、`AudioClip` は任意参照である。専用Inspectorの `フィードバックをセットアップ` は、必要に応じて `Animator` と `AudioSource` を追加して参照を自動配線し、`Assets/Resources/Animations/Button/Button.controller`、`Assets/Resources/Sounds/Button-1.wav`、2D再生設定を適用する。
- セットアップは Unity 標準の Button Animation Transition を使うよう切り替え、`Normal` / `Highlighted` / `Pressed` / `Selected` / `Disabled` trigger を設定する。追加のクリックTriggerは任意で個別に設定できる。

## 3. Button Transition

`GameButton` は `Button` を継承しているため、Unity 標準の Transition をそのまま使う。

- `None`: 状態変化による見た目変更を行わない。
- `ColorTint`: `targetGraphic` の色を `Normal` / `Highlighted` / `Pressed` / `Selected` / `Disabled` ごとに変える。
- `SpriteSwap`: `targetGraphic` の Sprite を状態ごとに差し替える。
- `Animation`: `Animator` に状態ごとの trigger を送る。`GameButton.prefab` はこの方式を使い、`Button.controller` で `CanvasGroup` の alpha を変える。

`GameButton.SetSelected(...)` の選択表示は、Unity 標準の `Selected` Transition とは別に、任意の `selectedRoot` 表示を切り替えるための補助状態である。

## 4. 移行方針

既存 UI の `Button` 参照は一括置換しない。
新規 UI からは、見た目も含めて新規作成する場合に `GameButton` を優先して使う。既存レイアウトへクリックSEとアニメーションだけを加える場合は `FeedbackButton` を使う。既存 Dialog と主要 UI は触るタイミングで段階的に置き換える。
`Dialog-EquipmentDetail.prefab` の `Button_Equip` と `Button_Close` は `GameButton` へ移行済みである。
