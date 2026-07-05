# C# メソッド棚卸し

## Summary

- Generated on: 2026-05-18
- Enabled build scenes: Assets/Scenes/SampleScene.unity
- Main code methods: 672 (`必要 628 / 保留 41 / 不要候補 3`)
- External sample methods: 159 (`不要候補 159`)
- Scripts with no methods: 25

## Criteria

- `必要`: コード参照、Unity ライフサイクル、Editor 属性/override、UI イベント登録、asset 側イベント文字列参照のいずれかを確認できたもの。
- `保留`: スクリプト自体は scene/prefab/asset に接続されている、または公開 API だが、静的に呼び出し元を断定できなかったもの。
- `不要候補`: private かつ参照痕跡が見当たらない、または外部サンプル/デモ配下で本体運用から切り離せるもの。
- Note: Unity の inspector 接続、アニメーションイベント、未ビルド scene、将来利用予定 API は静的解析だけでは断定しきれないため、`保留` を残しています。

## Main Code

### `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Camera/CameraControl.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `CameraControl.public void Init(Transform followTarget)` | 対象を初期化する | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:30, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:35, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:440 |
| 23 | `CameraControl.private void LateUpdate()` | 描画直前の更新を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |

### `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Core/CharacterEnums.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 96 | `UICategoryConfig.public static PartsType[] GetSubTypes(UICategory category)` | 指定したカテゴリに含まれるPartsTypeの配列を返す | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:74, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:114, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:133 |
| 102 | `UICategoryConfig.public static bool IsGroup(UICategory category)` | 指定したカテゴリが複数のPartsTypeを持つカテゴリかどうか判定する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:88, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:112, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:69 |

### `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Core/Player.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 34 | `Player.private void Awake()` | 参照取得とイベント登録を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 46 | `Player.public void Init()` | 対象を初期化する | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:30, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:35, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:440 |
| 55 | `Player.public void SetMode(GameMode mode)` | modeを設定する | 必要 | コード参照あり: Assets/Layer Lab/Scripts/PanelControl.cs:168, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Core/Player.cs:52 |
| 71 | `Player.private void Update()` | 毎フレームの更新を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 88 | `Player.private void HandleClick()` | クリックを処理する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Core/Player.cs:77 |
| 103 | `Player.private void UpdateMovement()` | movementを更新する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Core/Player.cs:82 |

### `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/AnimationControl.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 26 | `AnimationControl.public void Init(PartsManager pm)` | Initializes the animation panel with the given . Populates button labels from av | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:30, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:35, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:440 |
| 60 | `AnimationControl.private void OnClickAnimation(int index)` | クリックアニメーション時の処理を行う | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/AnimationControl.cs:51, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/AnimationControl.cs:57 |
| 69 | `AnimationControl.private void SetSelectedButton(int index)` | selectedボタンを設定する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/AnimationControl.cs:66 |
| 84 | `AnimationControl.private void OnDestroy()` | 破棄時の後始末を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |

### `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 45 | `ColorSelectScrollUIScript.private void OnEnable()` | 有効化時の購読や初期化を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 52 | `ColorSelectScrollUIScript.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 62 | `ColorSelectScrollUIScript.public void Init(PartsManager pm, AvatarEditorPresenter presenter = null)` | Initializes the color picker with the given PartsManager.  The PartsManager used | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:30, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:35, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:440 |
| 75 | `ColorSelectScrollUIScript.public void SetTarget(ColorTargetType target)` | Sets the current color target type and syncs the preset selection to the target' | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:158, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:417 |
| 82 | `ColorSelectScrollUIScript.private void RefreshPresetButtons()` | UIイベントの購読を設定する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:48, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:55, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:67 |
| 125 | `ColorSelectScrollUIScript.private void OnPresetButtonClicked(Button button, Color color)` | presetボタンclicked時の処理を行う | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:119 |
| 146 | `ColorSelectScrollUIScript.private void SetPresetButtonColor(Button button, Color color)` | presetボタン色を設定する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:112 |
| 153 | `ColorSelectScrollUIScript.private Image FindPresetColorImage(Button button)` | preset色imageを探す | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:197 |
| 171 | `ColorSelectScrollUIScript.private void ResolveContentParent()` | content親を解決する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:85 |
| 183 | `ColorSelectScrollUIScript.private Transform FindPresetContentParent(Transform root)` | presetcontent親を探す | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:176, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:178, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:180 |
| 215 | `ColorSelectScrollUIScript.private bool IsValidPresetContentParent(Transform candidate)` | validpresetcontent親か判定する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:87, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:173, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:177 |
| 220 | `ColorSelectScrollUIScript.private void ResolveCheck()` | checkを解決する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:47, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:54, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:66 |
| 230 | `ColorSelectScrollUIScript.private void SyncSelectionToCurrentTarget()` | selectionto現在値targetを同期する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:49, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:68, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:79 |
| 265 | `ColorSelectScrollUIScript.private int FindMatchingPresetIndex(Color color)` | matchingpresetindexを探す | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:253 |
| 281 | `ColorSelectScrollUIScript.private void SelectPresetButton(Button button)` | selectpresetボタンを処理する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:143, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:262 |
| 297 | `ColorSelectScrollUIScript.private void ClearPresetSelection()` | clearpresetselectionを処理する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:234, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:258 |
| 303 | `ColorSelectScrollUIScript.private void ClearPresetButtonListeners()` | イベント購読を解除する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:84, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:318, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:323 |
| 316 | `ColorSelectScrollUIScript.private void OnDisable()` | 無効化時の購読解除や後始末を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 321 | `ColorSelectScrollUIScript.private void OnDestroy()` | 破棄時の後始末を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |

### `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 37 | `PanelPartsListControl.public void Init(PartsManager pm, AvatarEditorPresenter presenter = null)` | 指定された  を使ってパーツ一覧パネルを初期化します。 パーツ変更イベントと色変更イベントを購読します。  パーツデータを提供する PartsManager。 | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:30, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:35, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:440 |
| 58 | `PanelPartsListControl.public void Show(UICategory category)` | 指定された UI カテゴリのパーツ一覧を表示します。 カテゴリに応じて、グループ表示または単一タイプ表示の処理へ振り分けます。  表示する UI カテゴリ。 | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:54, Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:82 |
| 79 | `PanelPartsListControl.public void RefreshSelectionFrame()` | selectionframeを更新する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:76, Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:77, Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:115 |
| 89 | `PanelPartsListControl.private void ShowGroup(UICategory category)` | オブジェクトを生成する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:71 |
| 129 | `PanelPartsListControl.private void ShowPartsType(PartsType type)` | オブジェクトを生成する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:76 |
| 184 | `PanelPartsListControl.public void SelectSlot(PartsListSlot slot)` | パーツ一覧スロットの選択を処理します。グループでは選択されたサブタイプのみ表示し、 その他は非表示にします。単一タイプでは装備前に表示状態を整えます。  クリッ | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsListSlot.cs:64, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:222 |
| 223 | `PanelPartsListControl.public void OnClickReset()` | 現在アクティブなパーツ種別、またはグループ内のすべてのサブタイプをリセット （装備解除）します。 | 必要 | イベント購読で登録される: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:44, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:447 / デリゲート/識別子として参照される: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:44, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:447 |
| 244 | `PanelPartsListControl.private void HideAllSlots()` | hideallslotsを処理する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:387 |
| 250 | `PanelPartsListControl.private void UpdateSelectFrame()` | selectframeを更新する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:86, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:126, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:176 |
| 317 | `PanelPartsListControl.private void MoveFrameTo(RectTransform target)` | moveframetoを処理する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:262, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:287, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:308 |
| 325 | `PanelPartsListControl.private void ChangeColorList(Color color)` | change色listを処理する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:115, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:154, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:163 |
| 334 | `PanelPartsListControl.private void OnPartsChanged(PartsType type, int index)` | パーツchanged時の処理を行う | 必要 | イベント購読で登録される: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:48, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:38 / デリゲート/識別子として参照される: Assets/Scripts/Core/PartsManager.cs:49, Assets/Scripts/Core/PartsManager.cs:256, Assets/Scripts/Core/PartsManager.cs:288 |
| 356 | `PanelPartsListControl.private void OnColorChanged(ColorTargetType target, Color color)` | 色changed時の処理を行う | 必要 | イベント購読で登録される: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:49, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:39 / デリゲート/識別子として参照される: Assets/Scripts/Core/PartsManager.cs:54, Assets/Scripts/Core/PartsManager.cs:368, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:49 |
| 377 | `PanelPartsListControl.private void ShowPresenterSection(UICategory category)` | オブジェクトを生成する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:65, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:190, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:228 |
| 430 | `PanelPartsListControl.private bool IsRelevantToActiveCategory(PartsType type)` | relevanttoactivecategoryか判定する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:338 |
| 444 | `PanelPartsListControl.private void OnDestroy()` | 破棄時の後始末を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |

### `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsListSlot.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 39 | `PartsListSlot.public void SetSlot(PanelPartsListControl parent, Sprite sprite, int index, PartsType type = default, int equipmentId = 0)` | Configures this slot with display data and associates it with the parent list co | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:108, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:146, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:398 |
| 54 | `PartsListSlot.public void ChangeColor(Color color)` | Applies a tint color to the item thumbnail image.  The color to apply. | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:330 |
| 62 | `PartsListSlot.public void OnPointerClick(PointerEventData eventData)` | Handles pointer click to select this slot, triggering part equipping via the par | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity / UI イベントインターフェースから呼ばれる。参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |

### `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 23 | `PartsSlot.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab |
| 30 | `PartsSlot.public void Init(PartsManager pm, Sprite[] bgSprites, AvatarEditorPresenter presenter = null)` | 対象を初期化する | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:30, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:35, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:440 |
| 44 | `PartsSlot.private void OnDestroy()` | 破棄時の後始末を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab |
| 51 | `PartsSlot.private void OnPartsChanged(PartsType type, int index)` | パーツchanged時の処理を行う | 必要 | イベント購読で登録される: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:48, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:38 / デリゲート/識別子として参照される: Assets/Scripts/Core/PartsManager.cs:49, Assets/Scripts/Core/PartsManager.cs:256, Assets/Scripts/Core/PartsManager.cs:288 |
| 57 | `PartsSlot.private void OnColorChanged(ColorTargetType target, Color color)` | 色changed時の処理を行う | 必要 | イベント購読で登録される: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:49, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:39 / デリゲート/識別子として参照される: Assets/Scripts/Core/PartsManager.cs:54, Assets/Scripts/Core/PartsManager.cs:368, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:49 |
| 79 | `PartsSlot.private bool IsRelevantType(PartsType type)` | relevanttypeか判定する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:53 |
| 89 | `PartsSlot.private void RefreshDisplay()` | displayを更新する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:41, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:54, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:61 |
| 164 | `PartsSlot.private void RefreshDisplayFromPresenter()` | displayfrompresenterを更新する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:93 |
| 198 | `PartsSlot.private void ApplyItemColor(Color color)` | アイテム色を適用する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:67, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:75, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:122 |
| 205 | `PartsSlot.private void UpdateBg()` | bgを更新する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:160, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:194 |
| 212 | `PartsSlot.private void UpdateItemAlpha()` | アイテムalphaを更新する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:161, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:195 |
| 220 | `PartsSlot.public void OnPointerClick(PointerEventData eventData)` | pointerクリック時の処理を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab / UI イベントインターフェースから呼ばれる。参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab |
| 225 | `PartsSlot.public void OnPointerExit(PointerEventData eventData)` | pointerexit時の処理を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab / UI イベントインターフェースから呼ばれる。参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab |

### `Assets/Layer Lab/Scripts/PanelControl.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 27 | `PanelControl.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Layer Lab/GUI Pro-MinimalGame/Theme_Dark/Scene/DemoScene_MinimalGame(Dark).unity |
| 36 | `PanelControl.private void Reset()` | 初期状態へ戻す | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Layer Lab/GUI Pro-MinimalGame/Theme_Dark/Scene/DemoScene_MinimalGame(Dark).unity |
| 41 | `PanelControl.private void Start()` | 起動直後の初期化を行う | 保留 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Layer Lab/GUI Pro-MinimalGame/Theme_Dark/Scene/DemoScene_MinimalGame(Dark).unity / 接続先 scene はあるが Build Settings では未有効 |
| 73 | `PanelControl.private void Update()` | 毎フレームの更新を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Layer Lab/GUI Pro-MinimalGame/Theme_Dark/Scene/DemoScene_MinimalGame(Dark).unity |
| 100 | `PanelControl.private void Click_Prev()` | クリックprevを処理する | 必要 | イベント購読で登録される: Assets/Layer Lab/Scripts/PanelControl.cs:44 / コード参照あり: Assets/Layer Lab/Scripts/PanelControl.cs:80, Assets/Layer Lab/Scripts/PanelControl.cs:89 |
| 127 | `PanelControl.private void Click_Next()` | クリックnextを処理する | 必要 | イベント購読で登録される: Assets/Layer Lab/Scripts/PanelControl.cs:45 / コード参照あり: Assets/Layer Lab/Scripts/PanelControl.cs:84, Assets/Layer Lab/Scripts/PanelControl.cs:93 |
| 141 | `PanelControl.private void SetArrowActive()` | arrowactiveを設定する | 必要 | コード参照あり: Assets/Layer Lab/Scripts/PanelControl.cs:162 |
| 148 | `PanelControl.private void CheckControl()` | checkcontrolを処理する | 必要 | コード参照あり: Assets/Layer Lab/Scripts/PanelControl.cs:70, Assets/Layer Lab/Scripts/PanelControl.cs:123, Assets/Layer Lab/Scripts/PanelControl.cs:137 |
| 165 | `PanelControl.public void Click_Mode()` | クリックmodeを処理する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Layer Lab/GUI Pro-MinimalGame/Theme_Dark/Scene/DemoScene_MinimalGame(Dark).unity |
| 172 | `PanelControl.private void SetMode()` | modeを設定する | 必要 | コード参照あり: Assets/Layer Lab/Scripts/PanelControl.cs:168, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Core/Player.cs:52 |

### `Assets/Layer Lab/Scripts/PanelView.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 9 | `PanelView.public void OnEnable()` | 有効化時の購読や初期化を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Layer Lab/GUI Pro-MinimalGame/Theme_Dark/Scene/DemoScene_MinimalGame(Dark).unity |
| 14 | `PanelView.public void OnDisable()` | 無効化時の購読解除や後始末を行う | 保留 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Layer Lab/GUI Pro-MinimalGame/Theme_Dark/Scene/DemoScene_MinimalGame(Dark).unity / 接続先 scene はあるが Build Settings では未有効 |

### `Assets/Scripts/Core/PartsManager.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 68 | `PartsManager.public void Init()` | すべてのカテゴリ、インデックス、表示状態、色を初期化し、デフォルトの Sprite を適用する | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:30, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:35, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:440 |
| 140 | `PartsManager.private void AutoMapColorRenderers()` | 子オブジェクトから Skin、Hair、Eye、Beard 用の SpriteRenderer を自動検出して割り当てます。 Inspector で未設定の場合 | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:117 |
| 165 | `PartsManager.public int GetPartsCount(PartsType type)` | 指定した PartsType で利用可能な Sprite 数を返す | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:242, Assets/Scripts/Core/PartsManager.cs:331, Assets/Scripts/Core/PartsManager.cs:346 |
| 176 | `PartsManager.public int GetActiveIndex(PartsType type)` | 指定した PartsType の Active な Sprite Index を返す なければ0 (未装備状態ではない) | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:334, Assets/Scripts/Core/PartsManager.cs:349, Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:28 |
| 185 | `PartsManager.public Sprite GetThumbnail(PartsType type, int index)` | 指定した PartsType Index に対応する Thumbnail Sprite を返す なければ最初の Renderers のスプライトを返す | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:71, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:108, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:146 |
| 203 | `PartsManager.public bool IsPartsVisible(PartsType type)` | 指定した PartsType が現在表示されているかどうか | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:42, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:207, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:278 |
| 211 | `PartsManager.public void SetPartsVisible(PartsType type, bool isVisible)` | 指定した PartsType の表示状態を変更し、連動パーツも必要に応じて同期します。 | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:103, Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:108, Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:113 |
| 228 | `PartsManager.public bool IsEquipped(PartsType type)` | 指定した PartsType が現在装備されているかどうか | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:26, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:276, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:299 |
| 237 | `PartsManager.public void EquipParts(PartsType type, int index)` | 指定した PartsType に特定の Sprite Index を装備させる OnPartsChanged を発火させる | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:318, Assets/Scripts/Core/PartsManager.cs:336, Assets/Scripts/Core/PartsManager.cs:351 |
| 279 | `PartsManager.public void UnequipParts(PartsType type)` | 指定した PartsType を未装備状態にする | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:638, Assets/Scripts/Core/PartsManager.cs:705, Assets/Scripts/Core/PartsManager.cs:738 |
| 300 | `PartsManager.public void SetGroupActiveType(UICategory category, PartsType visibleType)` | UICategory 内で表示する PartsType を設定し、他を非表示にさせる | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:201 |
| 329 | `PartsManager.public void NextParts(PartsType type)` | nextパーツを処理する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 344 | `PartsManager.public void PrevParts(PartsType type)` | prevパーツを処理する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 355 | `PartsManager.public Color GetColor(ColorTargetType target)` | 指定したカラー対象の現在の色を返す | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:51, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:253, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:153 |
| 364 | `PartsManager.public void SetColor(ColorTargetType target, Color color)` | 指定したカラー対象の色を設定し、対応する Renderer に適用する OnColorChangedを発火させる | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:690, Assets/Scripts/Core/PartsManager.cs:720, Assets/Scripts/Core/PartsManager.cs:744 |
| 378 | `PartsManager.private SpriteRenderer[] FindRenderersByName(SpriteRenderer[] allRenderers, params string[] names)` | 指定した名前のいずれかに一致する GameObject を持つ SpriteRenderer を検索します。 | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:151, Assets/Scripts/Core/PartsManager.cs:153, Assets/Scripts/Core/PartsManager.cs:155 |
| 396 | `PartsManager.private PartsCategory GetCategory(PartsType type)` | PartsType に対応する PartsCategory を返す | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:167, Assets/Scripts/Core/PartsManager.cs:187, Assets/Scripts/Core/PartsManager.cs:213 |
| 403 | `PartsManager.private void ApplyColor(ColorTargetType target, Color color)` | 指定した色を、対応するカラー対象のすべての SpriteRenderer に適用する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:367 |
| 424 | `PartsManager.private static bool IsHandRightWeapon(PartsType type)` | 指定した PartsType が右手武器（Sword、Axe、Bow など）かどうかを返す | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:220, Assets/Scripts/Core/PartsManager.cs:259 |
| 436 | `PartsManager.private void SyncArrowVisibility()` | Bow / Crossbow の表示状態に応じて Arrow / Bolt の表示を切り替えます。 Arrow は Bow 表示時のみ、Bolt は Cross | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:133, Assets/Scripts/Core/PartsManager.cs:221, Assets/Scripts/Core/PartsManager.cs:260 |
| 466 | `PartsManager.private void SyncHelmetHairVisibility()` | Helmet の表示状態と Hair の表示希望状態に応じて、 HelmetHair と Hair の表示を切り替えます。 Helmet が表示中で Hair | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:130, Assets/Scripts/Core/PartsManager.cs:224, Assets/Scripts/Core/PartsManager.cs:275 |
| 503 | `PartsManager.private void SetRenderersActive(PartsCategory cat, bool active)` | 指定したカテゴリに含まれるすべてのレンダラーの GameObject を有効 / 無効化します。 | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:126, Assets/Scripts/Core/PartsManager.cs:218, Assets/Scripts/Core/PartsManager.cs:253 |
| 516 | `PartsManager.private void ApplySprites(PartsCategory cat, int index)` | 指定したインデックスのスプライトを、カテゴリ内のすべてのレンダラーへ割り当てます。 | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:122, Assets/Scripts/Core/PartsManager.cs:247, Assets/Scripts/Core/PartsManager.cs:269 |
| 534 | `PartsManager.public void PlayAnimation(string animName)` | アニメーションを再生する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1272, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1286, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Core/Player.cs:66 |
| 542 | `PartsManager.public string GetCurrentAnimation()` | 現在値アニメーションを取得する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 556 | `PartsManager.public string[] GetAnimationNames()` | アニメーションnamesを取得する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/AnimationControl.cs:29 |
| 567 | `PartsManager.public bool HasAnimation(string animName)` | 指定した Animation Clip が Animator Controller に存在するかを返す | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1267, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1284 |
| 573 | `PartsManager.public float GetAnimationLength(string animName)` | 指定した Animation Clip の再生時間を秒で返す。未検出時は 0 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1277 |
| 579 | `PartsManager.private AnimationClip FindAnimationClip(string animName)` | アニメーションclipを探す | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:569, Assets/Scripts/Core/PartsManager.cs:575 |
| 598 | `PartsManager.public void RandomizeAll()` | すべてのパーツをランダムに装備します。 | 必要 | asset 側のイベント文字列参照あり: Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 681 | `PartsManager.private void RandomizeColors()` | カラー対象の色をランダム化します。 Eye は対象外です。 | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:674 |
| 698 | `PartsManager.public void CopyFrom(PartsManager other)` | 別の PartsManager から、すべてのパーツインデックス、表示状態、色をコピーします。  コピー元の PartsManager。 | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 731 | `PartsManager.public void ApplyAppearanceData(AppearanceData appearanceData)` | 保存済みのアバター状態をこのキャラクターに適用し、 パーツ、色、表示状態を復元します。  適用するアバター状態データ。 | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:31, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:449, Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:66 |
| 767 | `PartsManager.public AppearanceData CreateAppearanceData()` | 現在のキャラクター状態をアバター状態データとしてシリアライズします。  現在の状態を格納した新しい 。 | 必要 | コード参照あり: Assets/Scripts/Editor/CharacterPrefabSaver.cs:31 |
| 789 | `PartsManager.public PartsType[] GetAllPartsTypes()` | categories 配列に登録されているすべてのパーツタイプを返します。   の配列。 | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:39, Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:20, Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:68 |
| 800 | `PartsManager.public bool CanChangeColor(PartsType type)` | 指定したパーツタイプがカラー変更に対応しているかを返します。  確認対象のパーツタイプ。 カラー変更可能なら true。 | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:150, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:370, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:72 |
| 811 | `PartsManager.public ColorTargetType GetColorTarget(PartsType type)` | 指定したパーツタイプに対応するカラー対象タイプを返します。  取得対象のパーツタイプ。 対応する 。見つからない場合は Skin。 | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:152, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:371, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:73 |
| 822 | `PartsManager.public string GetDisplayName(PartsType type)` | 指定したパーツタイプの表示名を返します。  取得対象のパーツタイプ。 表示名。未設定の場合は enum 名。 | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 830 | `PartsManager.private PartsType[] GetPartsTypesByCategory(UICategory category)` | パーツtypesbycategoryを取得する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:842 |
| 840 | `PartsManager.private bool IsGroupCategory(UICategory category)` | groupcategoryか判定する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 845 | `PartsManager.private PartsCategory[] GetExclusiveGroupMembers(PartsExclusiveGroup group)` | exclusivegroupmembersを取得する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:103, Assets/Scripts/Core/PartsManager.cs:305, Assets/Scripts/Core/PartsManager.cs:609 |
| 855 | `PartsManager.private PartsExclusiveGroup ToExclusiveGroup(UICategory category)` | toexclusivegroupを処理する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:302 |
| 865 | `PartsManager.private bool IsExclusivePartsType(PartsType type)` | exclusiveパーツtypeか判定する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Scenes/Home.unity, Assets/Layer Lab/2D Minimal-CharacterMaker/Prefabs/Character.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |

### `Assets/Scripts/Data/CharacterPrefabData.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 19 | `CharacterPrefabData.public void SetData(AppearanceData data)` | Sets the character data from avatar appearance data.  The appearance data contai | 必要 | コード参照あり: Assets/Scripts/Editor/CharacterPrefabSaver.cs:40 |
| 24 | `CharacterPrefabData.private void Start()` | 起動直後の初期化を行う | 必要 | 型が動的生成され、そのライフサイクルで呼ばれる: Assets/Scripts/Editor/CharacterPrefabSaver.cs:39 |

### `Assets/Scripts/Data/MasterData/AttributeMasterData.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 47 | `AttributeMasterData.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/Attributes/Earth.asset, Assets/Resources/MasterData/Attributes/Fire.asset, Assets/Resources/MasterData/Attributes/Water.asset |

### `Assets/Scripts/Data/MasterData/AttributeMasterDatabase.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `AttributeMasterDatabase.public bool TryGetById(int attributeId, out AttributeMasterData attributeData)` | 取得するbyidを試みる | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:75, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:144 |

### `Assets/Scripts/Data/MasterData/BattleNodeVisualDatabase.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 24 | `BattleNodeVisualDatabase.private void OnEnable()` | 有効化時の購読や初期化を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/BattleNodeVisualDatabase.asset |
| 29 | `BattleNodeVisualDatabase.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/BattleNodeVisualDatabase.asset |
| 34 | `BattleNodeVisualDatabase.public BattleNodeVisualEntry GetEntry(BattleNodeType nodeType)` | entryを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:719, Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs:202 |
| 42 | `BattleNodeVisualDatabase.private void EnsureLookup()` | lookupを確保する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleNodeVisualDatabase.cs:36 |
| 50 | `BattleNodeVisualDatabase.private void RebuildLookup()` | rebuildlookupを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleNodeVisualDatabase.cs:26, Assets/Scripts/Data/MasterData/BattleNodeVisualDatabase.cs:31, Assets/Scripts/Data/MasterData/BattleNodeVisualDatabase.cs:46 |

### `Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 19 | `ValidationIssue.public ValidationIssue(string code, string message, ValidationSeverity severity)` | validationissueを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:117, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:125, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:133 |
| 111 | `BattleSkillMasterData.public IReadOnlyList<ValidationIssue> GetValidationIssues()` | validationissuesを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseEditor.cs:20, Assets/Scripts/Editor/EquipmentMasterDataEditor.cs:19 |
| 182 | `BattleSkillMasterData.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/Skills/arc-flare.asset, Assets/Resources/MasterData/Skills/mana-bloom.asset, Assets/Resources/MasterData/Skills/pierce-volley.asset |

### `Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 19 | `BattleSkillMasterDatabase.private void OnEnable()` | 有効化時の購読や初期化を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/BattleSkillDatabase.asset |
| 24 | `BattleSkillMasterDatabase.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/BattleSkillDatabase.asset |
| 29 | `BattleSkillMasterDatabase.public bool TryGetById(int skillId, out BattleSkillMasterData definition)` | 取得するbyidを試みる | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:75, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:144 |
| 35 | `BattleSkillMasterDatabase.private void EnsureLookups()` | lookupsを確保する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:31, Assets/Scripts/Data/MasterData/EquipmentMasterDatabase.cs:50, Assets/Scripts/Data/MasterData/EquipmentMasterDatabase.cs:56 |
| 43 | `BattleSkillMasterDatabase.private void RebuildLookups()` | rebuildlookupsを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:21, Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:26, Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:39 |

### `Assets/Scripts/Data/MasterData/EffectAnimationMasterData.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 53 | `EffectAnimationMasterData.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/EffectAnimations/arc-flare-effect.asset, Assets/Resources/MasterData/EffectAnimations/mana-bloom-effect.asset, Assets/Resources/MasterData/EffectAnimations/pierce-volley-effect.asset |

### `Assets/Scripts/Data/MasterData/EquipmentIdUtility.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 10 | `EquipmentIdUtility.public static int Build(PartsType partType, int partsIndex)` | 対象を構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/EquipmentIdUtility.cs:52, Assets/Scripts/Data/MasterData/EquipmentMasterData.cs:117, Assets/Scripts/Data/MasterData/EquipmentMasterDatabase.cs:57 |
| 18 | `EquipmentIdUtility.public static bool TryParse(int equipmentId, out PartsType partType, out int partsIndex)` | parseを試みる | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/EquipmentIdUtility.cs:42, Assets/Scripts/Data/MasterData/EquipmentIdUtility.cs:71, Assets/Scripts/Data/MasterData/EquipmentIdUtility.cs:72 |
| 36 | `EquipmentIdUtility.public static bool TryConvertLegacyStringToId(string legacyEquipmentId, out int equipmentId)` | convert旧形式stringtoidを試みる | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveJsonSerializer.cs:145 |
| 56 | `EquipmentIdUtility.public static bool TryParseLegacyString(string legacyEquipmentId, out PartsType partType, out int partsIndex)` | parse旧形式stringを試みる | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/EquipmentIdUtility.cs:49 |

### `Assets/Scripts/Data/MasterData/EquipmentMasterData.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 20 | `ValidationIssue.public ValidationIssue(string code, string message, ValidationSeverity severity)` | validationissueを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:117, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:125, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:133 |
| 119 | `EquipmentMasterData.public IReadOnlyList<ValidationIssue> GetValidationIssues()` | validationissuesを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseEditor.cs:20, Assets/Scripts/Editor/EquipmentMasterDataEditor.cs:19 |
| 204 | `EquipmentMasterData.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/Equipments/Chest/Chest_012.asset, Assets/Resources/MasterData/Equipments/Eye/Eye_004.asset, Assets/Resources/MasterData/Equipments/Hair/Hair_010.asset |
| 211 | `EquipmentMasterData.private static PartsExclusiveGroup ResolveExclusiveGroup(PartsType type)` | exclusivegroupを解決する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/EquipmentMasterData.cs:93 |

### `Assets/Scripts/Data/MasterData/EquipmentMasterDatabase.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `ValidationIssue.public ValidationIssue(string code, string message, EquipmentMasterData definition = null)` | validationissueを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:117, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:125, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:133 |
| 38 | `EquipmentMasterDatabase.private void OnEnable()` | 有効化時の購読や初期化を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/EquipmentDatabase.asset |
| 43 | `EquipmentMasterDatabase.private void OnValidate()` | インスペクタ変更時の整合を取る | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/MasterData/EquipmentDatabase.asset |
| 48 | `EquipmentMasterDatabase.public bool TryGetById(int equipmentId, out EquipmentMasterData definition)` | 取得するbyidを試みる | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:75, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:144 |
| 54 | `EquipmentMasterDatabase.internal bool TryGetByPartsIndex(PartsType partType, int partsIndex, out EquipmentMasterData definition)` | 取得するbyパーツindexを試みる | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:30 |
| 60 | `EquipmentMasterDatabase.public IReadOnlyList<ValidationIssue> GetValidationIssues()` | validationissuesを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseEditor.cs:20, Assets/Scripts/Editor/EquipmentMasterDataEditor.cs:19 |
| 68 | `EquipmentMasterDatabase.public IReadOnlyList<EquipmentMasterData> GetByPartType(PartsType partType)` | byparttypeを取得する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Resources/MasterData/EquipmentDatabase.asset |
| 76 | `EquipmentMasterDatabase.public IReadOnlyList<EquipmentMasterData> GetDefaultOwned()` | 既定ownedを取得する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Resources/MasterData/EquipmentDatabase.asset |
| 86 | `EquipmentMasterDatabase.private void EnsureLookups()` | lookupsを確保する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:31, Assets/Scripts/Data/MasterData/EquipmentMasterDatabase.cs:50, Assets/Scripts/Data/MasterData/EquipmentMasterDatabase.cs:56 |
| 94 | `EquipmentMasterDatabase.private void RebuildLookups()` | rebuildlookupsを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:21, Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:26, Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:39 |

### `Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 28 | `MasterDataResourceLoader.public static IReadOnlyList<EquipmentData> LoadEquipmentData()` | 装備マスタを読み込み、DTO リストに変換して返す。 | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/InventoryService.cs:42 |
| 57 | `MasterDataResourceLoader.public static IReadOnlyList<BattleSkillData> LoadBattleSkillData()` | バトルスキルマスタを読み込み、DTO リストに変換して返す。 | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 66 | `MasterDataResourceLoader.public static bool TryLoadBattleSkillData(int skillId, out BattleSkillData skillData)` | 読み込むバトルskilldataを試みる | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1158 |
| 93 | `MasterDataResourceLoader.public static IReadOnlyList<AttributeData> LoadAttributeData()` | 属性マスタを読み込み、DTO リストに変換して返す。 | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 112 | `MasterDataResourceLoader.public static IReadOnlyList<BattleStageData> LoadBattleStageData()` | バトルステージマスタを読み込み、DTO リストに変換して返す。 | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:39 |
| 137 | `MasterDataResourceLoader.public static bool TryLoadStageData(int stageId, out StageData stageData)` | stageId に対応するステージデータを DTO に変換して返す | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:370 |
| 161 | `MasterDataResourceLoader.private static EquipmentMasterDatabase LoadEquipmentMasterDatabase()` | Resources から読み込む | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:30 |
| 172 | `MasterDataResourceLoader.private static BattleSkillMasterDatabase LoadBattleSkillMasterDatabase()` | Resources から読み込む | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:59, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:74 |
| 183 | `MasterDataResourceLoader.private static AttributeMasterDatabase LoadAttributeMasterDatabase()` | Resources から読み込む | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:95 |
| 194 | `MasterDataResourceLoader.private static StageMasterDatabase LoadStageMasterDatabase()` | Resources から読み込む | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:114, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:140 |
| 205 | `MasterDataResourceLoader.private static StageBattleBoardData BuildBattleBoardData(StageBattlePatternMasterData patternMaster)` | バトル盤面dataを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:499 |
| 225 | `MasterDataResourceLoader.private static IReadOnlyList<BattleSkillData> BuildBattleSkillData(IReadOnlyList<BattleSkillMasterData> skillMasters)` | バトルskilldataを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:46, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:63, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:80 |
| 253 | `MasterDataResourceLoader.private static AttributeData BuildAttributeData(AttributeMasterData attribute)` | attributedataを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:47, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:245, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:283 |
| 271 | `MasterDataResourceLoader.private static IReadOnlyList<EquipmentAttributeModifierData> BuildEquipmentAttributeModifierData(             IReadOnlyList<EquipmentAttributeModifierEntry> modifiers)` | 装備attributemodifierdataを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:48, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:341 |
| 289 | `MasterDataResourceLoader.private static IReadOnlyList<string> BuildCategoryTags(IReadOnlyList<string> categoryTags)` | categorytagsを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:49 |
| 302 | `MasterDataResourceLoader.private static EffectAnimationData BuildEffectAnimationData(EffectAnimationMasterData effectAnimation)` | effectアニメーションdataを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:247 |
| 321 | `MasterDataResourceLoader.private static StageBattleEnemyData BuildBattleEnemyData(StageMasterData master)` | バトルenemydataを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:154 |
| 360 | `MasterDataResourceLoader.private static AttributeMasterData ResolveEnemyRightHandWeaponAttribute(StageBattleEnemyMasterData enemyMaster)` | enemyrighthandweaponattributeを解決する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:340 |
| 380 | `MasterDataResourceLoader.private static IReadOnlyList<EquipmentAttributeModifierEntry> ResolveEnemyChestAttributeModifiers(             StageBattleEnemyMasterData enemyMaster)` | enemychestattributemodifiersを解決する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:341 |
| 401 | `MasterDataResourceLoader.private static AppearanceData BuildResolvedEnemyAppearanceData(StageBattleEnemyMasterData enemyMaster)` | resolvedenemyappearancedataを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:342 |
| 480 | `MasterDataResourceLoader.private static IReadOnlyList<StageBattlePatternData> BuildPatterns(StageBattleEnemyMasterData enemyMaster)` | patternsを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:343 |
| 512 | `MasterDataResourceLoader.private static IReadOnlyList<StageTurnCellData> BuildCellPlacements(             IReadOnlyList<StageTurnCellMasterData> cellMasters)` | セルplacementsを構築する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:505 |
| 534 | `MasterDataResourceLoader.private static BattleNodeType NormalizeNodeType(BattleNodeType nodeType)` | normalizenodetypeを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:529 |
| 541 | `MasterDataResourceLoader.private static bool IsDerivedEnemyAppearancePart(PartsType partType)` | derivedenemyappearancepartか判定する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:416, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:438 |
| 546 | `MasterDataResourceLoader.private static void ApplyResolvedEnemyColors(             AppearanceData appearanceData,             IReadOnlyList<StageBattleEnemyAppearanceColorMasterData> colorMasters)` | resolvedenemycolorsを適用する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:475 |

### `Assets/Scripts/Data/MasterData/StageBattleEnemyMasterDatabase.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `StageBattleEnemyMasterDatabase.public bool TryGetById(int enemyId, out StageBattleEnemyMasterData enemyData)` | 取得するbyidを試みる | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:75, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:144 |

### `Assets/Scripts/Data/MasterData/StageBattlePatternMasterDatabase.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `StageBattlePatternMasterDatabase.public bool TryGetById(int patternId, out StageBattlePatternMasterData patternData)` | 取得するbyidを試みる | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:75, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:144 |

### `Assets/Scripts/Data/MasterData/StageMasterDatabase.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 19 | `StageMasterDatabase.public bool TryGetById(int stageId, out StageMasterData stageData)` | 指定した StageId に一致する StageMasterData を検索する。 | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:75, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:144 |

### `Assets/Scripts/Editor/CharacterPrefabSaver.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 23 | `CharacterPrefabSaver.public static void Save(PartsManager partsManager, float thumbnailCameraSize, Vector3 thumbnailCameraOffset)` | Saves the current character state as a prefab asset with an optional thumbnail. | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:87, Assets/Scripts/Systems/Save/GameSaveService.cs:94, Assets/Scripts/Systems/Save/GameSaveService.cs:103 |
| 83 | `CharacterPrefabSaver.private static void CleanupComponents(GameObject prefabObj)` | 不要オブジェクトを破棄する | 必要 | コード参照あり: Assets/Scripts/Editor/CharacterPrefabSaver.cs:37 |
| 95 | `CharacterPrefabSaver.private static Texture2D CaptureThumbnail(GameObject characterObj, float cameraSize, Vector3 cameraOffset)` | 不要オブジェクトを破棄する | 必要 | コード参照あり: Assets/Scripts/Editor/CharacterPrefabSaver.cs:47 |
| 138 | `CharacterPrefabSaver.private static void SaveThumbnail(Texture2D thumbnail, string prefabPath)` | サムネイルを保存する | 必要 | コード参照あり: Assets/Scripts/Editor/CharacterPrefabSaver.cs:65 |

### `Assets/Scripts/Editor/ClearButtonEditor.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 11 | `ClearButtonEditor.protected override void OnEnable()` | 有効化時の購読や初期化を行う | 必要 | override 実装でランタイム/Editor から呼ばれる / Editor のコールバック名として呼ばれる |
| 19 | `ClearButtonEditor.public override void OnInspectorGUI()` | inspectorgui時の処理を行う | 必要 | override 実装でランタイム/Editor から呼ばれる / Editor のコールバック名として呼ばれる |

### `Assets/Scripts/Editor/EquipmentMasterDataEditor.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 10 | `EquipmentMasterDataEditor.public override void OnInspectorGUI()` | inspectorgui時の処理を行う | 必要 | override 実装でランタイム/Editor から呼ばれる / Editor のコールバック名として呼ばれる |
| 37 | `EquipmentMasterDataEditor.private static MessageType ToMessageType(EquipmentMasterData.ValidationSeverity severity)` | tomessagetypeを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDataEditor.cs:32 |

### `Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 20 | `EquipmentMasterDatabaseBuilder.[MenuItem(                                           )]         public static void Rebuild()` | menuアイテムを処理する | 必要 | [MenuItem] 属性で Unity/Editor から呼ばれる |
| 106 | `EquipmentMasterDatabaseBuilder.private static bool IsAuthorableType(PartsType partType)` | authorabletypeか判定する | 必要 | デリゲート/識別子として参照される: Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:40 |
| 113 | `EquipmentMasterDatabaseBuilder.private static string BuildAssetName(PartsType partType, int index)` | assetnameを構築する | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:69, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:158 |
| 118 | `EquipmentMasterDatabaseBuilder.private static EquipmentMasterData LoadOrCreateEquipmentAsset(string assetPath)` | or生成する装備assetを読み込む | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:70 |
| 132 | `EquipmentMasterDatabaseBuilder.private static EquipmentMasterDatabase LoadOrCreateDatabase()` | or生成するデータベースを読み込む | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:88 |
| 147 | `EquipmentMasterDatabaseBuilder.private static void UpdateEquipmentAsset(             EquipmentMasterData asset,             PartsType partType,             int partsIndex,             Sprite icon)` | 装備assetを更新する | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:71 |
| 184 | `EquipmentMasterDatabaseBuilder.private static void UpdateDatabase(EquipmentMasterDatabase database, IReadOnlyList<EquipmentMasterData> orderedAssets)` | データベースを更新する | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:89 |
| 203 | `EquipmentMasterDatabaseBuilder.private static void EnsureFolderExists(string assetFolderPath)` | folderexistsを確保する | 必要 | コード参照あり: Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:36, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:59, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:134 |

### `Assets/Scripts/Editor/EquipmentMasterDatabaseEditor.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 11 | `EquipmentMasterDatabaseEditor.public override void OnInspectorGUI()` | inspectorgui時の処理を行う | 必要 | override 実装でランタイム/Editor から呼ばれる / Editor のコールバック名として呼ばれる |

### `Assets/Scripts/Editor/PartsManagerEditor.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 94 | `PartsManagerEditor.private void OnEnable()` | 有効化時の購読や初期化を行う | 必要 | Editor のコールバック名として呼ばれる |
| 107 | `PartsManagerEditor.private void SetSpriteFolderPath(string path)` | spritefolder経路を設定する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:160, Assets/Scripts/Editor/PartsManagerEditor.cs:172, Assets/Scripts/Editor/PartsManagerEditor.cs:181 |
| 116 | `PartsManagerEditor.public override void OnInspectorGUI()` | Draws the custom inspector GUI for PartsManager. | 必要 | override 実装でランタイム/Editor から呼ばれる / Editor のコールバック名として呼ばれる |
| 131 | `PartsManagerEditor.private void DrawAnimatorField()` | drawanimatorfieldを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:120 |
| 139 | `PartsManagerEditor.private void DrawAutoSetupSection()` | drawautoセットアップするsectionを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:122 |
| 197 | `PartsManagerEditor.private bool IsThemeSelected(ThemeType theme)` | themeselectedか判定する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:209 |
| 207 | `PartsManagerEditor.private void AddTheme(ThemeType theme)` | themeを追加する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:185 |
| 214 | `PartsManagerEditor.private void RemoveTheme(ThemeType theme)` | themeを削除する | 不要候補 | private メソッドだがコード/asset 参照を確認できない |
| 226 | `PartsManagerEditor.private void DrawColorTargetSection()` | draw色targetsectionを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:124 |
| 245 | `PartsManagerEditor.private void DrawCategoriesSection()` | drawcategoriessectionを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:126 |
| 284 | `PartsManagerEditor.private void DrawCategoryElement(int index)` | drawcategoryelementを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:269 |
| 347 | `PartsManagerEditor.private void DrawPreviewSection(int catIndex, SerializedProperty renderersProp)` | drawプレビューするsectionを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:338 |
| 391 | `PartsManagerEditor.private void ApplyPreviewInEditor(SerializedProperty renderersProp, int spriteIndex)` | プレビューするineditorを適用する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:367, Assets/Scripts/Editor/PartsManagerEditor.cs:374 |
| 417 | `PartsManagerEditor.private int GetSpriteCount(SerializedProperty renderersProp)` | spritecountを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:298 |
| 426 | `PartsManagerEditor.private void AutoSetupRenderers()` | autoセットアップするrenderersを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:189 |
| 509 | `PartsManagerEditor.private UICategory GetUICategory(PartsType pType)` | uicategoryを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:444 |
| 541 | `PartsManagerEditor.private PartsExclusiveGroup GetExclusiveGroup(PartsType pType)` | exclusivegroupを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:445 |
| 561 | `PartsManagerEditor.private bool GetDefaultVisible(PartsType pType)` | 既定表示状態を取得する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:446 |
| 571 | `PartsManagerEditor.private bool MatchRendererName(string objName, string searchName, PartsType pType)` | matchrenderernameを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:470 |
| 590 | `PartsManagerEditor.private void AutoMapColors()` | automapcolorsを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:506 |
| 625 | `PartsManagerEditor.private void SetRendererArray(SerializedProperty arrayProp, List<SpriteRenderer> renderers)` | rendererarrayを設定する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:616, Assets/Scripts/Editor/PartsManagerEditor.cs:617, Assets/Scripts/Editor/PartsManagerEditor.cs:618 |
| 632 | `PartsManagerEditor.private void LoadSpritesFromFolder()` | spritesfromfolderを読み込む | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:190 |
| 732 | `PartsManagerEditor.private void SetSpritesToCategory(PartsType type, Sprite[] sprites)` | spritestocategoryを設定する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:667 |
| 753 | `PartsManagerEditor.private void ScanThemeFolder(string folderPath, Dictionary<PartsType, List<Sprite>> spritesByType)` | scanthemefolderを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:686, Assets/Scripts/Editor/PartsManagerEditor.cs:775 |
| 780 | `PartsManagerEditor.private Sprite[] FilterSpritesForRenderer(PartsType type, string rendererName, Sprite[] allSprites)` | filterspritesforrendererを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:718 |
| 837 | `PartsManagerEditor.private Sprite[] LoadSpritesFromPath(string folderPath)` | spritesfrom経路を読み込む | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:664, Assets/Scripts/Editor/PartsManagerEditor.cs:763, Assets/Scripts/Editor/PartsManagerEditor.cs:884 |
| 858 | `PartsManagerEditor.private void LoadThumbnails()` | thumbnailsを読み込む | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:191 |
| 897 | `PartsManagerEditor.private int FindCategoryIndex(PartsType type)` | categoryindexを探す | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:734 |

### `Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 43 | `PatternDraft.public void Normalize()` | normalizeを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:201, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:419, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:431 |
| 82 | `ValidationIssue.public ValidationIssue(ValidationSeverity severity, string message)` | validationissueを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:117, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:125, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:133 |
| 105 | `StageBattlePatternBoardEditorWindow.[MenuItem(MenuPath)]         private static void OpenWindow()` | menuアイテムを処理する | 必要 | [MenuItem] 属性で Unity/Editor から呼ばれる |
| 114 | `StageBattlePatternBoardEditorWindow.private void OnEnable()` | 有効化時の購読や初期化を行う | 必要 | Editor のコールバック名として呼ばれる / 型が動的生成され、そのライフサイクルで呼ばれる: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:108 |
| 120 | `StageBattlePatternBoardEditorWindow.private void OnSelectionChange()` | selectionchange時の処理を行う | 必要 | Editor のコールバック名として呼ばれる |
| 137 | `StageBattlePatternBoardEditorWindow.private void OnGUI()` | gui時の処理を行う | 必要 | Editor のコールバック名として呼ばれる / 型が動的生成され、そのライフサイクルで呼ばれる: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:108 |
| 208 | `StageBattlePatternBoardEditorWindow.private void DrawHeader()` | drawheaderを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:139 |
| 248 | `StageBattlePatternBoardEditorWindow.private void DrawPaintToolbar()` | drawpainttoolbarを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:172 |
| 254 | `StageBattlePatternBoardEditorWindow.private void DrawPaintToolbarRow(params PaintTool[] tools)` | drawpainttoolbarrowを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:250, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:251 |
| 275 | `StageBattlePatternBoardEditorWindow.private void DrawBoard(             int width,             int height,             int startX,             int startY,             Dictionary<Vector2Int, BattleNodeType> placements)` | draw盤面を処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:203 |
| 330 | `StageBattlePatternBoardEditorWindow.private void ApplyPaint(int width, int height, int startX, int startY, int x, int y)` | paintを適用する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:311 |
| 366 | `StageBattlePatternBoardEditorWindow.private void AssignPattern(StageBattlePatternMasterData pattern)` | assignpatternを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:133, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:222, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:610 |
| 372 | `StageBattlePatternBoardEditorWindow.private void ReloadDraftFromPattern()` | reloaddraftfrompatternを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:184, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:369 |
| 424 | `StageBattlePatternBoardEditorWindow.private void SaveDraftToAsset()` | drafttoassetを保存する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:195 |
| 466 | `StageBattlePatternBoardEditorWindow.private void MarkDraftChanged()` | markdraftchangedを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:165, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:178, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:344 |
| 478 | `StageBattlePatternBoardEditorWindow.private void DrawValidationSummary(PatternDraft draft)` | drawvalidationsummaryを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:202 |
| 501 | `StageBattlePatternBoardEditorWindow.private bool TryConfirmDiscardDraft()` | 確定するdiscarddraftを試みる | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:216, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:234 |
| 511 | `StageBattlePatternBoardEditorWindow.private static List<ValidationIssue> BuildValidationIssues(PatternDraft draft)` | validationissuesを構築する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:432, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:480 |
| 524 | `StageBattlePatternBoardEditorWindow.private static void AddGoalValidationIssue(PatternDraft draft, List<ValidationIssue> issues)` | goalvalidationissueを追加する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:520 |
| 541 | `StageBattlePatternBoardEditorWindow.private static bool HasBlockingValidationIssue(List<ValidationIssue> issues)` | blockingvalidationissueがあるか判定する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:433 |
| 559 | `StageBattlePatternBoardEditorWindow.BattleNodeType> BuildPlacementMap(             SerializedProperty cellPlacementsProperty,             int width,             int height,             int startX,             int startY)` | 配置mapを構築する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:407 |
| 589 | `StageBattlePatternBoardEditorWindow.private static void RewritePlacements(SerializedProperty cellPlacementsProperty, Dictionary<Vector2Int, BattleNodeType> placements)` | rewriteplacementsを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:456 |
| 606 | `StageBattlePatternBoardEditorWindow.private void TryAssignFromSelection()` | assignfromselectionを試みる | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:111, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:117, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:236 |
| 614 | `StageBattlePatternBoardEditorWindow.private BattleNodeVisualDatabase ResolveVisualDatabase()` | Resources から読み込む | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:116, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:716 |
| 624 | `StageBattlePatternBoardEditorWindow.private void DrawCellContent(Rect rect, CellVisual visual)` | drawセルcontentを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:315 |
| 654 | `StageBattlePatternBoardEditorWindow.private static void DrawSprite(Rect rect, Sprite sprite, Color tint)` | drawspriteを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:634 |
| 675 | `StageBattlePatternBoardEditorWindow.private static BattleNodeType ToNodeType(PaintTool paintTool)` | tonodetypeを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:360 |
| 690 | `StageBattlePatternBoardEditorWindow.private CellVisual GetToolVisual(PaintTool tool)` | toolvisualを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:260, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:694, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:696 |
| 707 | `StageBattlePatternBoardEditorWindow.private CellVisual GetToolVisual(string label, BattleNodeType nodeType, bool isStart)` | toolvisualを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:260, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:694, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:696 |
| 713 | `StageBattlePatternBoardEditorWindow.private CellVisual GetNodeVisual(BattleNodeType nodeType, bool isStart)` | nodevisualを取得する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:305, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:709 |
| 751 | `CellVisual.public CellVisual(                 string label,                 Sprite icon,                 Color backgroundColor,                 Color outlineColor,                 Color iconColor)` | セルvisualを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:695, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:710, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:722 |

### `Assets/Scripts/Features/Battle/AnimationEventReceiver.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 23 | `AnimationEventReceiver.public void OnAttackHit()` | attackhit時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/PlayerAttack.cs:14 / asset 側のイベント文字列参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Animations/Attack.anim, Assets/Layer Lab/2D Minimal-CharacterMaker/Animations/Jump Attack.anim, Assets/Layer Lab/2D Minimal-CharacterMaker/Animations/Jump Attack.anim / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/PlayerAttack.cs:14, Assets/Scripts/Features/Battle/PlayerAttack.cs:19 |
| 29 | `AnimationEventReceiver.public void OnSkillHit()` | skillhit時の処理を行う | 必要 | asset 側のイベント文字列参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Animations/Skill.anim |

### `Assets/Scripts/Features/Battle/Core/BattleGridPosition.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 10 | `BattleGridPosition.public BattleGridPosition(int x, int y)` | バトルgridpositionを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:57, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:170, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:181 |
| 20 | `BattleGridPosition.public bool IsAdjacent4Way(BattleGridPosition other)` | adjacent4wayか判定する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:75 |
| 27 | `BattleGridPosition.public bool Equals(BattleGridPosition other)` | equalsを処理する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:590, Assets/Scripts/Editor/PartsManagerEditor.cs:655, Assets/Scripts/Features/Battle/Core/BattleGridPosition.cs:34 |
| 32 | `BattleGridPosition.public override bool Equals(object obj)` | equalsを処理する | 必要 | override 実装でランタイム/Editor から呼ばれる / コード参照あり: Assets/Scripts/Core/PartsManager.cs:590, Assets/Scripts/Editor/PartsManagerEditor.cs:655, Assets/Scripts/Features/Battle/Core/BattleGridPosition.cs:34 |
| 37 | `BattleGridPosition.public override int GetHashCode()` | hashcodeを取得する | 必要 | override 実装でランタイム/Editor から呼ばれる |
| 45 | `BattleGridPosition.public override string ToString()` | tostringを処理する | 必要 | override 実装でランタイム/Editor から呼ばれる / コード参照あり: Assets/Scripts/Core/PartsManager.cs:825, Assets/Scripts/Editor/CharacterPrefabSaver.cs:57, Assets/Scripts/Editor/PartsManagerEditor.cs:297 |

### `Assets/Scripts/Features/Battle/Core/BattlePathSegment.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 8 | `BattlePathSegment.public BattlePathSegment(BattleGridPosition from, BattleGridPosition to)` | バトル経路segmentを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:85, Assets/Scripts/Features/Battle/Runtime/BattlePathDraft.cs:71 |
| 18 | `BattlePathSegment.public bool HasEndpoint(BattleGridPosition position)` | endpointがあるか判定する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Core/BattlePathSegment.cs:25, Assets/Scripts/Features/Battle/Core/BattlePathSegment.cs:25 |
| 23 | `BattlePathSegment.public bool SharesEndpointWith(BattlePathSegment other)` | sharesendpointwithを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:100 |
| 28 | `BattlePathSegment.public override string ToString()` | tostringを処理する | 必要 | override 実装でランタイム/Editor から呼ばれる / コード参照あり: Assets/Scripts/Core/PartsManager.cs:825, Assets/Scripts/Editor/CharacterPrefabSaver.cs:57, Assets/Scripts/Editor/PartsManagerEditor.cs:297 |

### `Assets/Scripts/Features/Battle/Core/Definitions/BattleSharedDefinitions.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 21 | `BattleAnimationCatalog.public static string GetClipName(BattlePlayerAnimationCue cue)` | clipnameを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1266 |

### `Assets/Scripts/Features/Battle/Demo/BattleScene.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 92 | `BattleScene.private void Awake()` | 参照取得とイベント登録を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/BattleScene.unity |
| 100 | `BattleScene.private void Start()` | 起動直後の初期化を行う | 保留 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/BattleScene.unity / 接続先 scene はあるが Build Settings では未有効 |
| 105 | `BattleScene.private void OnDestroy()` | 破棄時の後始末を行う | 保留 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/BattleScene.unity / 接続先 scene はあるが Build Settings では未有効 |
| 118 | `BattleScene.public void Initialize()` | ステージ、戦闘状態、表示を初期化し、最初のターン入力を開始する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:102, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:154, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:243 |
| 137 | `BattleScene.public void ExecuteTurn()` | 現在確定しているプレイヤーパスをもとにターン実行を開始する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:926 |
| 145 | `BattleScene.private async UniTaskVoid ExecuteTurnAsync()` | パス解決、アニメーション再生、HP反映、次ターン準備までを 1 ターンとして進める。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:139 |
| 224 | `BattleScene.public void ClearSelectedSkill()` | 選択中のスキルを解除する。 | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Scenes/BattleScene.unity |
| 239 | `BattleScene.public void SelectSkillSlot(int slotIndex)` | 指定スロットのスキル選択状態を更新する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:268, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:270, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:272 |
| 268 | `BattleScene.private void OnClickSkillButton1()` | クリックskillボタン1時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:94, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:107 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:94, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:107 |
| 270 | `BattleScene.private void OnClickSkillButton2()` | クリックskillボタン2時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:95, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:108 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:95, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:108 |
| 272 | `BattleScene.private void OnClickSkillButton3()` | クリックskillボタン3時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:96, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:109 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:96, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:109 |
| 274 | `BattleScene.private void OnClickSkillButton4()` | クリックskillボタン4時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:97, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:110 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:97, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:110 |
| 276 | `BattleScene.private void ResolvePlayerBattleProfile()` | プレイヤーバトルprofileを解決する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:400 |
| 325 | `BattleScene.private void RefreshSkillButtonVisuals()` | skillボタンvisualsを更新する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:128, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:232, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:264 |
| 354 | `BattleScene.private bool TryResolveStageData(out StageData stageData)` | 解決するステージdataを試みる | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:121 |
| 379 | `BattleScene.private void ApplyStageSceneState(StageData stageData)` | ステージシーンstateを適用する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:123 |
| 398 | `BattleScene.private void ResetRuntimeState(StageData stageData)` | runtimestateをリセットする | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:124 |
| 429 | `BattleScene.private void SpawnPreviewCharacters()` | オブジェクトを生成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:131 |
| 454 | `BattleScene.private void GainTurnChargeToAllSkills()` | gainターンchargetoallskillsを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:643, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:688 |
| 465 | `BattleScene.private void ApplyResolutionResult(BattleTurnResolutionReport result, StageBattlePatternData pattern)` | resolver の結果を session の HP と勝敗状態へ反映する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:195 |
| 494 | `BattleScene.private void ApplyPostTurnEnemyState(StageBattlePatternData pattern)` | ターン終了後に敵行動由来の継続状態を次ターンへ持ち越す。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:198 |
| 507 | `BattleScene.private void RefreshHud()` | HUDを更新する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:127, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:200 |
| 528 | `BattleScene.private void PrepareUpcomingTurnPresentation()` | 次ターンの pattern を選び、盤面生成、入力待機、HUD 文言更新までを行う。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:126, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:199 |
| 623 | `BattleScene.private bool TryResolveInteractiveTurn(             StageBattlePatternData pattern,              out BattleTurnResolutionReport result,              out IReadOnlyList<BattleCellState> executedCellPath)` | 確定済みのプレイヤーパスを論理セル列へ変換し、実際のターン解決を行う。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:180 |
| 670 | `BattleScene.private bool TryResolveFallbackTurn(              StageBattlePatternData pattern,              out BattleTurnResolutionReport result,              out IReadOnlyList<BattleCellState> executedCellPath)` | 解決するfallbackターンを試みる | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:187 |
| 712 | `BattleScene.private void BootstrapPresentationIfNeeded()` | bootstrappresentationifneededを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:125 |
| 775 | `BattleScene.private void OnTraceUpdated(BattlePathTraceResult result)` | トレースupdated時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:766 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:763, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:766 |
| 815 | `BattleScene.private void OnTraceRejected(BattlePathTraceResult result)` | トレースrejected時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:767 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:764, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:767 |
| 833 | `BattleScene.private void OnPathConfirmed(IReadOnlyList<Assets.Scripts.Features.Battle.Core.BattleGridPosition> _)` | 経路confirmed時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:768 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:765, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:768 |
| 843 | `BattleScene.private void ConsumeSelectedSkillIfNeeded(BattleTurnResolutionReport result)` | consumeselectedskillifneededを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:196 |
| 863 | `BattleScene.private void BeginAwaitNodePathInput(StageBattlePatternData pattern)` | 現在の pattern に対するパス確定待機 UniTask を開始する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:617 |
| 880 | `BattleScene.private void CancelAwaitNodePathInput()` | cancelawaitnode経路inputを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:111, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:533, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:865 |
| 892 | `BattleScene.private void CancelTurnExecution()` | cancelターンexecutionを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:112, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:170, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:217 |
| 907 | `BattleScene.private async UniTaskVoid AwaitNodePathInputAsync(StageBattlePatternData pattern, CancellationToken cancellationToken)` | パス確定を待ち、条件が変わっていなければ自動で ExecuteTurn へ進める。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:876 |
| 937 | `BattleScene.private void UpdateInteractiveTurnPreview()` | 入力中または確定済みのパスを先読み解決し、HUD にターン予測を表示する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:233, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:265, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:794 |
| 976 | `BattleScene.private bool TryBuildInteractiveTurnPreview(             IReadOnlyList<BattleGridPosition> positions,              out BattleTurnResolutionReport preview,              out List<BattleNodeType> path)` | 現在のパスから preview 用の node/cell 列を組み、resolver で結果を先読みする。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:963 |
| 1004 | `BattleScene.private BattleTurnContext CreateCurrentTurnContext(StageBattlePatternData pattern, bool hazardBoosted)` | 現在の battle session と選択スキルを元に、このターン専用の解決コンテキストを作る。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:997 |
| 1022 | `BattleScene.private string BuildTurnPrompt(StageBattlePatternData pattern)` | ターンpromptを構築する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:537, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:594, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:636 |
| 1042 | `BattleScene.private string BuildTurnPreviewMessage(BattleTurnResolutionReport preview)` | ターンプレビューするmessageを構築する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:969, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:970 |
| 1101 | `BattleScene.private void HandleBattleClear()` | バトルclearを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:481 |
| 1121 | `BattleScene.private BattleSkillSlotRuntime GetSelectedSkill()` | selectedskillを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1051, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1053, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1132 |
| 1130 | `BattleScene.private BattleSkillSlotRuntime CreateSelectedSkillRuntime()` | selectedskillruntimeを生成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:650, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:695, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1010 |
| 1136 | `BattleScene.private static List<BattleSkillSlotRuntime> CreateRuntimeSkillSlots(UserBattleProfileData battleProfile)` | runtimeskillslotsを生成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:288 |
| 1153 | `BattleScene.private static BattleSkillSlotRuntime CreateRuntimeSkillSlot(UserBattleSkillSlotData slotData)` | runtimeskillslotを生成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1147 |
| 1196 | `BattleScene.private static BattleSkillSlotRuntime CloneSkillSlotRuntime(BattleSkillSlotRuntime source)` | cloneskillslotruntimeを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1133 |
| 1224 | `BattleScene.private void LogResolutionReport(StageBattlePatternData pattern, BattleTurnResolutionReport result)` | logresolutionreportを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:201 |
| 1237 | `BattleScene.private static string GetPatternLabel(StageBattlePatternData pattern)` | patternlabelを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:470, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:476, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:598 |
| 1247 | `BattleScene.private async UniTask PlayPlayerActionAnimationsAsync(              BattleTurnResolutionReport result,              CancellationToken cancellationToken)` | プレイヤーactionanimationsを再生する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:193 |
| 1290 | `BattleScene.private static List<BattlePlayerAnimationCue> BuildPlayerAnimationCueSequence(BattleTurnResolutionReport result)` | プレイヤーアニメーションcuesequenceを構築する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1256 |
| 1313 | `BattleScene.private static string FormatPath(IReadOnlyList<BattleGridPosition> positions)` | format経路を処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:598, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:663, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:912 |
| 1323 | `BattleScene.private static string FormatNodePath(IReadOnlyList<BattleNodeType> nodeTypes)` | formatnode経路を処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:663, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:970 |

### `Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 14 | `BattlePathRuleEvaluator.public BattlePathValidationError ValidateStart(BattleBoardState board, BattleGridPosition position)` | トレース開始位置が有効な Start マスかを判定する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:56, Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:91 |
| 34 | `BattlePathRuleEvaluator.public bool IsBacktrackCandidate(BattlePathDraft draft, BattleGridPosition candidate)` | backtrackcandidateか判定する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:45 |
| 44 | `BattlePathRuleEvaluator.public BattlePathValidationError ValidateAppend(             BattleBoardState board,             BattlePathDraft draft,             BattleGridPosition candidate)` | 次に追加しようとしているマスが隣接・未再訪・非交差などの条件を満たすか判定する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:51 |
| 94 | `BattlePathRuleEvaluator.private static bool CrossesExistingSegments(BattlePathDraft draft, BattlePathSegment newSegment)` | crossesexistingsegmentsを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:86 |
| 114 | `BattlePathRuleEvaluator.private static bool SegmentsIntersect(BattlePathSegment left, BattlePathSegment right)` | segmentsintersectを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:105 |
| 154 | `BattlePathRuleEvaluator.private static int Orientation(BattleGridPosition from, BattleGridPosition via, BattleGridPosition to)` | orientationを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:121, Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:122, Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:123 |
| 166 | `BattlePathRuleEvaluator.private static bool OnSegment(BattleGridPosition from, BattleGridPosition candidate, BattleGridPosition to)` | segment時の処理を行う | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:131, Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:136, Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:141 |
| 174 | `BattlePathRuleEvaluator.private static int Min(int left, int right)` | minを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:169, Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:171, Assets/Scripts/Features/Battle/Runtime/BattleSkillSlotRuntime.cs:154 |
| 179 | `BattlePathRuleEvaluator.private static int Max(int left, int right)` | maxを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/AttributeMasterData.cs:49, Assets/Scripts/Data/MasterData/AttributeMasterData.cs:50, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:139 |

### `Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 12 | `BattlePathTracer.public BattlePathTracer(BattleBoardState board, BattlePathRuleEvaluator ruleEvaluator = null)` | バトル経路tracerを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:609, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:137 |
| 28 | `BattlePathTracer.public BattlePathTraceResult TryTrace(BattleGridPosition position)` | 1 マス分の入力を仮パスへ反映し、開始/追加/巻き戻し/拒否の結果を返す。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:336, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:193 |
| 65 | `BattlePathTracer.public BattlePathTraceResult TryRelease()` | 入力終了時に仮パスを確定し、Goal 未到達なら破棄する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:344, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:218 |
| 84 | `BattlePathTracer.public void Reset()` | 現在の仮パスを完全にリセットする。 | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 89 | `BattlePathTracer.private BattlePathTraceResult TryStart(BattleGridPosition position)` | startを試みる | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:32 |
| 102 | `BattlePathTracer.private BattlePathTraceResult CreateResult(             BattlePathTraceStatus status,             BattlePathValidationError validationError,             bool goalReachedNow)` | resultを生成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:37, Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:42, Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:48 |

### `Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 17 | `AttributeDamageResolution.public AttributeDamageResolution(                 int damage,                 int appliedPercent,                 BattleAttributeEffectiveness effectiveness,                 string summary)` | attributedamageresolutionを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:378, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:383, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:389 |
| 38 | `BattleTurnResolver.public static BattleTurnResolutionReport Resolve(IReadOnlyList<BattleCellState> path, BattleTurnContext context)` | 確定パスを先頭から順に解釈し、攻撃・回避・被弾・Goal 到達を 1 ターン結果へ解決する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:664, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:707, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:995 |
| 192 | `BattleTurnResolver.public static BattleTurnResolutionReport Resolve(IReadOnlyList<BattleNodeType> path, BattleTurnContext context)` | ノード種別列を簡易セル列へ変換してターン解決する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:664, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:707, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:995 |
| 215 | `BattleTurnResolver.private static int ResolveAttackDamage(             BattleTurnContext context,             BattleSkillSlotRuntime selectedSkill,             int normalAttackDamage,             int doubleAttackFollowUpDamage,             int jumpAttackDamage,             ref bool jumpAttackPrimed,             ref int basicAttackCount,             BattleTurnResolutionReport report)` | 通常攻撃、連撃、Jump 攻撃、または選択スキルを判定して敵ダメージを算出する。 | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:105 / コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:105 |
| 275 | `BattleTurnResolver.private static void FinalizeOutcomeFlags(BattleTurnResolutionReport report, BattleSkillSlotRuntime selectedSkill)` | finalizeoutcomeflagsを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:122, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:170, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:179 |
| 281 | `BattleTurnResolver.private static int ResolveHazardGroupKey(BattleCellState cell, int stepIndex)` | hazardgroupkeyを解決する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:132 |
| 294 | `BattleTurnResolver.private static int ResolveHazardDamage(BattleTurnContext context, BattleTurnResolutionReport report)` | Hazard 接触時の被ダメージを、危険強化と属性相性込みで計算する。 | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:160 / コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:160 |
| 311 | `BattleTurnResolver.private static void ResolvePendingEnemyActionIfNeeded(             BattleTurnResolutionReport report,             BattleTurnContext context,             int enemyActionDamage,             int currentPlayerHp)` | Goal 到達後、盤面上の Hazard 解決が無かった場合に敵行動の直ダメージを解決する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:178, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:184 |
| 370 | `BattleTurnResolver.private static AttributeDamageResolution ResolveAttributeDamage(             int baseDamage,             AttributeData attackAttribute,             IReadOnlyList<EquipmentAttributeModifierData> defenseModifiers)` | attributedamageを解決する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:229, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:243, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:256 |
| 396 | `BattleTurnResolver.private static int ResolveAttributePercent(             int attributeId,             IReadOnlyList<EquipmentAttributeModifierData> defenseModifiers)` | attributepercentを解決する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:386 |
| 417 | `BattleTurnResolver.private static BattleAttributeEffectiveness ResolveEffectiveness(int appliedPercent)` | effectivenessを解決する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:387 |
| 437 | `BattleTurnResolver.private static string BuildAttributeSummary(string attributeName, BattleAttributeEffectiveness effectiveness)` | attributesummaryを構築する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:393 |
| 456 | `BattleTurnResolver.private static string BuildAttributeLogSuffix(AttributeDamageResolution resolution)` | attributelogsuffixを構築する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:234, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:248, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:261 |
| 466 | `BattleTurnResolver.private static void ApplyEnemyDamageAttributeSummary(             BattleTurnResolutionReport report,             AttributeDamageResolution resolution)` | enemydamageattributesummaryを適用する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:233, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:247, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:260 |
| 479 | `BattleTurnResolver.private static void ApplyPlayerDamageAttributeSummary(             BattleTurnResolutionReport report,             AttributeDamageResolution resolution)` | プレイヤーdamageattributesummaryを適用する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:304, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:345, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:358 |
| 492 | `BattleTurnResolver.private static int Max(int left, int right)` | maxを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/AttributeMasterData.cs:49, Assets/Scripts/Data/MasterData/AttributeMasterData.cs:50, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:139 |
| 497 | `BattleTurnResolver.private static int RoundToInt(float value)` | roundtointを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:298, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:388, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:368 |

### `Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 16 | `Layout.public Layout(BattleBoardState board, IReadOnlyList<BattleGridPosition> fallbackTracePositions)` | layoutを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:80 |
| 30 | `StageBattleRuntimeAdapter.public static Layout CreateBoardLayout(StageBattlePatternData patternData)` | Pattern master の定義から、1 ターンぶんの論理盤面と fallback ルートを構築する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:596, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:678, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:136 |
| 86 | `StageBattleRuntimeAdapter.public static BattleTurnContext CreateTurnContext(             StageBattlePatternData patternData,             int enemyDamage,             bool hazardBoosted,             BattleSkillSlotRuntime selectedSkill,             int currentEnemyHp,             int currentPlayerHp,             int normalAttackDamage,             int doubleAttackFollowUpDamage,             int jumpAttackDamage,             AttributeData playerAttackAttribute,             AttributeData enemyAttackAttribute,             IReadOnlyList<EquipmentAttributeModifierData> enemyDefenseAttributeModifiers,             IReadOnlyList<EquipmentAttributeModifierData> playerDefenseAttributeModifiers)` | 現在の player/enemy 状態と pattern 情報から、ターン解決用コンテキストを組み立てる。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:646, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:691, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1006 |
| 122 | `StageBattleRuntimeAdapter.public static BattleTurnResolutionReport ResolveWithRuntime(             StageBattlePatternData patternData,             bool hazardBoosted,             BattleSkillSlotRuntime selectedSkill,             int currentEnemyHp,             int currentPlayerHp,             int normalAttackDamage,             int doubleAttackFollowUpDamage,             int jumpAttackDamage,             AttributeData playerAttackAttribute,             AttributeData enemyAttackAttribute,             IReadOnlyList<EquipmentAttributeModifierData> enemyDefenseAttributeModifiers,             IReadOnlyList<EquipmentAttributeModifierData> playerDefenseAttributeModifiers)` | Pattern から fallback ルートを自動生成し、そのまま 1 ターン分の解決まで行う。 | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 165 | `StageBattleRuntimeAdapter.private static BattleGridPosition ResolveStartPosition(StageBattlePatternData patternData, int width, int height)` | startpositionを解決する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:34 |
| 173 | `StageBattleRuntimeAdapter.private static void FillEmptyBoard(BattleBoardState board)` | 満たすempty盤面を処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:37 |
| 189 | `StageBattleRuntimeAdapter.private static IReadOnlyList<BattleGridPosition> BuildFallbackTracePositions(             BattleBoardState board,             HashSet<BattleGridPosition> explicitPositions)` | fallbackトレースpositionsを構築する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:80 |
| 230 | `StageBattleRuntimeAdapter.private static IEnumerable<BattleGridPosition> GetNeighbors(BattleGridPosition origin)` | neighborsを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:212 |
| 238 | `StageBattleRuntimeAdapter.private static void AssignConnectedHazardGroups(BattleBoardState board)` | assignconnectedhazardgroupsを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:79 |
| 308 | `StageBattleRuntimeAdapter.private static IReadOnlyList<BattleGridPosition> ReconstructPath(             BattleGridPosition start,             BattleGridPosition goal,             IReadOnlyDictionary<BattleGridPosition, BattleGridPosition> previous)` | reconstruct経路を処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:209 |
| 325 | `StageBattleRuntimeAdapter.private static BattleTurnResolutionReport TraceFallbackPath(             BattlePathTracer tracer,             IReadOnlyList<BattleGridPosition> tracePositions)` | トレースfallback経路を処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:138 |
| 353 | `StageBattleRuntimeAdapter.private static BattleTurnResolutionReport CreateTraceFailureReport(string message)` | トレースfailurereportを生成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:331, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:339, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:347 |
| 360 | `StageBattleRuntimeAdapter.private static int ResolveEnemyActionDamage(StageBattlePatternData patternData, int enemyDamage)` | enemyactiondamageを解決する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:108 |
| 371 | `StageBattleRuntimeAdapter.private static int Clamp(int value, int min, int max)` | clampを処理する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:245, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:186, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:210 |
| 386 | `StageBattleRuntimeAdapter.private static int RoundToInt(float value)` | roundtointを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:298, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:388, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:368 |

### `Assets/Scripts/Features/Battle/PlayerAttack.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 12 | `PlayerAttack.private void Awake()` | 参照取得とイベント登録を行う | 不要候補 | private メソッドだがコード/asset 参照を確認できない |
| 17 | `PlayerAttack.private void OnDestroy()` | 破棄時の後始末を行う | 不要候補 | private メソッドだがコード/asset 参照を確認できない |
| 22 | `PlayerAttack.private void OnAttackHit()` | attackhit時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/PlayerAttack.cs:14 / asset 側のイベント文字列参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Animations/Attack.anim, Assets/Layer Lab/2D Minimal-CharacterMaker/Animations/Jump Attack.anim, Assets/Layer Lab/2D Minimal-CharacterMaker/Animations/Jump Attack.anim / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/PlayerAttack.cs:14, Assets/Scripts/Features/Battle/PlayerAttack.cs:19 |

### `Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 39 | `BattleBoardController.public void ConfigureGeneratedCells(RectTransform root, GridLayoutGroup layoutGroup = null, BattleCellView prefab = null)` | generatedcellsを構成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:734 |
| 54 | `BattleBoardController.public void RenderBoard(BattleBoardState board)` | 論理盤面をもとにセル prefab 群を生成し、入力可能な盤面 View を構築する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:602 |
| 115 | `BattleBoardController.public void ClearBoard()` | 不要オブジェクトを破棄する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:57, Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:200 |
| 144 | `BattleBoardController.public void ClearTraceVisual()` | clearトレースvisualを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:535, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:546, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:566 |
| 152 | `BattleBoardController.public void SetTraceVisual(IReadOnlyList<BattleGridPosition> positions, bool canConfirm)` | パス中セルのハイライト状態を更新する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:146, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:256 |
| 179 | `BattleBoardController.public List<Vector3> BuildWorldPointPath(IReadOnlyList<BattleGridPosition> positions)` | 論理パスを軌跡線描画用のワールド座標列へ変換する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:257 |
| 198 | `BattleBoardController.private void OnDestroy()` | 破棄時の後始末を行う | 必要 | 型が動的生成され、そのライフサイクルで呼ばれる: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:730 |
| 203 | `BattleBoardController.private void SuppressLegacyChildrenIfNeeded()` | suppress旧形式childrenifneededを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:48 |
| 229 | `BattleBoardController.private BattleGridPosition GetPositionForLayoutIndex(int index)` | GridLayoutGroup の並び順に従って、生成順インデックスを論理座標へ変換する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:98 |
| 267 | `BattleBoardController.private void OnCellPointerDown(BattleCellView view, PointerEventData eventData)` | セルpointerdown時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:106 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:106, Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:125 |
| 272 | `BattleBoardController.private void OnCellPointerEnter(BattleCellView view, PointerEventData eventData)` | セルpointerenter時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:107 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:107, Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:126 |
| 277 | `BattleBoardController.private void OnCellPointerUp(BattleCellView view, PointerEventData eventData)` | セルpointerup時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:108 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:108, Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:127 |

### `Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 41 | `BattleCellView.public void Bind(BattleCellState cell, BattleNodeVisualDatabase nodeVisualDatabase = null)` | 論理セルの状態とノード見た目定義を、この prefab 表示へ結び付ける。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:607, Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:105 |
| 49 | `BattleCellView.private void Reset()` | 初期状態へ戻す | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/Prefabs/Battle/BattleCellView.prefab |
| 73 | `BattleCellView.public void SetTraceState(bool isInPath, bool isCurrent, bool canConfirm)` | パス上か、現在位置か、確定可能かに応じてセル見た目を更新する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:156, Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:172, Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs:46 |
| 135 | `BattleCellView.public Vector3 GetWorldCenter()` | 軌跡線描画に使う、このセル中央のワールド座標を返す。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:191 |
| 142 | `BattleCellView.public void OnPointerDown(PointerEventData eventData)` | pointerdown時の処理を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/Prefabs/Battle/BattleCellView.prefab / UI イベントインターフェースから呼ばれる。参照: Assets/Resources/Prefabs/Battle/BattleCellView.prefab |
| 147 | `BattleCellView.public void OnPointerEnter(PointerEventData eventData)` | pointerenter時の処理を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/Prefabs/Battle/BattleCellView.prefab / UI イベントインターフェースから呼ばれる。参照: Assets/Resources/Prefabs/Battle/BattleCellView.prefab |
| 152 | `BattleCellView.public void OnPointerUp(PointerEventData eventData)` | pointerup時の処理を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Resources/Prefabs/Battle/BattleCellView.prefab / UI イベントインターフェースから呼ばれる。参照: Assets/Resources/Prefabs/Battle/BattleCellView.prefab |
| 157 | `BattleCellView.private void EnsureRuntimeVisuals()` | runtimevisualsを確保する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs:43 |
| 187 | `BattleCellView.private void ApplyIcon(NodeVisualStyle style)` | iconを適用する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs:105 |
| 200 | `BattleCellView.private NodeVisualStyle GetStyle(BattleNodeType nodeType)` | styleを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs:80, Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextInfoDebugTool.cs:200 |
| 237 | `NodeVisualStyle.public NodeVisualStyle(                 Sprite icon,                 Color baseColor,                 Color accentColor,                 Color iconColor)` | nodevisualstyleを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs:205, Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs:215, Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs:217 |

### `Assets/Scripts/Features/Battle/Presentation/BattleHudController.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 20 | `BattleHudController.public void ConfigureLegacyReferences(             TMP_Text turnText,             TMP_Text waveText,             TMP_Text enemyName = null,             TMP_Text playerHp = null,             TMP_Text enemyHp = null,             TMP_Text confirm = null,             GameObject nextHazardBoost = null)` | 旧形式referencesを構成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:759 |
| 38 | `BattleHudController.public void ApplySession(BattleSessionState session)` | セッションを適用する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:511 |
| 76 | `BattleHudController.public void SetConfirmText(string message)` | 確定するtextを設定する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:537, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:545, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:565 |

### `Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 64 | `BattleTraceInputHandler.public UniTask<IReadOnlyList<BattleGridPosition>> WaitForConfirmedPathAsync(CancellationToken cancellationToken = default)` | Goal 到達済みの確定パスが得られるまで待機する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:911 |
| 80 | `BattleTraceInputHandler.public void Bind(BattleBoardController boardController, BattlePathTracer tracer, BattleTraceLineView traceLineView = null)` | 盤面 View と tracer を接続し、以後の pointer 入力をパス追跡へ流す。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:607, Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs:105 |
| 99 | `BattleTraceInputHandler.public void Unbind()` | unbindを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:82, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:139 |
| 116 | `BattleTraceInputHandler.public void ResetState(bool clearConfirmedPath)` | stateをリセットする | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:534, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:95 |
| 129 | `BattleTraceInputHandler.private void Update()` | 毎フレームの更新を行う | 必要 | 型が動的生成され、そのライフサイクルで呼ばれる: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:746 |
| 137 | `BattleTraceInputHandler.private void OnDestroy()` | 破棄時の後始末を行う | 必要 | 型が動的生成され、そのライフサイクルで呼ばれる: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:746 |
| 145 | `BattleTraceInputHandler.private void HandleCellPointerDown(BattleGridPosition position, UnityEngine.EventSystems.PointerEventData _)` | Start 入力で新しいトレースを開始し、最初のマスを path draft に反映する。 | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:90 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:90, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:103 |
| 167 | `BattleTraceInputHandler.private void HandleCellPointerEnter(BattleGridPosition position, UnityEngine.EventSystems.PointerEventData _)` | ドラッグ中に入った隣接マスを順次 tracer へ渡す。 | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:91 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:91, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:104 |
| 180 | `BattleTraceInputHandler.private void HandleCellPointerUp(BattleGridPosition _, UnityEngine.EventSystems.PointerEventData __)` | ドラッグ終了時に現在の仮パスを確定判定へ進める。 | 必要 | イベント購読で登録される: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:92 / デリゲート/識別子として参照される: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:92, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:105 |
| 191 | `BattleTraceInputHandler.private void ProcessTrace(BattleGridPosition position)` | 1 マス分の入力を tracer に適用し、受理/拒否結果をイベントと見た目へ反映する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:161, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:174 |
| 210 | `BattleTraceInputHandler.private void FinalizeTrace()` | 指離し時に Goal 到達済みなら確定し、未到達なら仮パスを破棄する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:133, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:184 |
| 240 | `BattleTraceInputHandler.private void RefreshVisuals()` | 現在のドラフトまたは確定パスを、盤面ハイライトと軌跡線へ反映する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:126, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:198, Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:204 |
| 260 | `BattleTraceInputHandler.private void CancelConfirmedPathAwait()` | cancelconfirmed経路awaitを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:113 |
| 266 | `BattleTraceInputHandler.private static string FormatPath(IReadOnlyList<BattleGridPosition> positions)` | format経路を処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:598, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:663, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:912 |

### `Assets/Scripts/Features/Battle/Presentation/BattleTraceLineView.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 22 | `BattleTraceLineView.public void Clear()` | 不要オブジェクトを破棄する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:72, Assets/Scripts/Core/PartsManager.cs:73, Assets/Scripts/Core/PartsManager.cs:74 |
| 35 | `BattleTraceLineView.public void SetLine(IReadOnlyList<Vector3> worldPoints, bool canConfirm)` | オブジェクトを生成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:257 |
| 67 | `BattleTraceLineView.private bool TryWorldToLocal(Vector3 worldPoint, out Vector2 localPoint)` | worldtolocalを試みる | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleTraceLineView.cs:46, Assets/Scripts/Features/Battle/Presentation/BattleTraceLineView.cs:47 |

### `Assets/Scripts/Features/Battle/Runtime/BattleBoardState.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `BattleBoardState.public BattleBoardState(int width, int height)` | バトル盤面stateを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:36 |
| 39 | `BattleBoardState.public void SetCell(BattleCellState cell)` | セルを設定する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:38, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:69, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:179 |
| 58 | `BattleBoardState.public bool IsInside(BattleGridPosition position)` | insideか判定する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:16, Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:59, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:58 |
| 66 | `BattleBoardState.public bool TryGetCell(BattleGridPosition position, out BattleCellState cell)` | 取得するセルを試みる | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:21, Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:64, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:273 |
| 71 | `BattleBoardState.public BattleCellState GetCell(BattleGridPosition position)` | セルを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Runtime/BattleBoardState.cs:113, Assets/Scripts/Features/Battle/Runtime/BattleBoardState.cs:132 |
| 81 | `BattleBoardState.public bool IsGoal(BattleGridPosition position)` | goalか判定する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:57, Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:97, Assets/Scripts/Features/Battle/Logic/StageBattleRuntimeAdapter.cs:207 |
| 86 | `BattleBoardState.public List<BattleGridPosition> GetGoalPositions()` | goalpositionsを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:598 |
| 103 | `BattleBoardState.public List<BattleNodeType> BuildNodePath(IReadOnlyList<BattleGridPosition> positions)` | 座標列をノード種別列へ変換する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:662, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:989 |
| 122 | `BattleBoardState.public List<BattleCellState> BuildCellPath(IReadOnlyList<BattleGridPosition> positions)` | 座標列をセル状態列へ変換する。 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:661, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:706, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:996 |

### `Assets/Scripts/Features/Battle/Runtime/BattlePathDraft.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 38 | `BattlePathDraft.public bool Contains(BattleGridPosition position)` | containsを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:447, Assets/Scripts/Editor/PartsManagerEditor.cs:789, Assets/Scripts/Editor/PartsManagerEditor.cs:795 |
| 43 | `BattlePathDraft.public void Reset()` | 初期状態へ戻す | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 51 | `BattlePathDraft.public void StartAt(BattleGridPosition startPosition, bool isGoal)` | startatを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:98, Assets/Scripts/Features/Battle/Runtime/BattlePathDraft.cs:66 |
| 62 | `BattlePathDraft.public void Append(BattleGridPosition position, bool isGoal)` | appendを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:58 |
| 81 | `BattlePathDraft.public bool Backtrack()` | backtrackを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs:47 |
| 105 | `BattlePathDraft.public List<BattleGridPosition> CreateSnapshot()` | snapshotを生成する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs:222 |

### `Assets/Scripts/Features/Battle/Runtime/BattleSessionState.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 31 | `BattleSessionState.public void ResetForBattle(string enemyName, int initialPlayerHp, int initialEnemyHp)` | forバトルをリセットする | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:413 |
| 46 | `BattleSessionState.public void AdvanceTurn()` | advanceターンを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:642, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:687 |
| 51 | `BattleSessionState.public void ApplyEnemyDamage(int damage)` | enemydamageを適用する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:469 |
| 61 | `BattleSessionState.public void ApplyPlayerDamage(int damage)` | プレイヤーdamageを適用する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:475 |
| 75 | `BattleSessionState.public bool ConsumeHazardBoostFlag()` | consumehazardboostflagを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:641, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:686 |
| 82 | `BattleSessionState.public void ReserveNextTurnHazardBoost()` | reservenextターンhazardboostを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:503 |
| 87 | `BattleSessionState.private static int Max(int left, int right)` | maxを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/AttributeMasterData.cs:49, Assets/Scripts/Data/MasterData/AttributeMasterData.cs:50, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:139 |

### `Assets/Scripts/Features/Battle/Runtime/BattleSkillSlotRuntime.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 76 | `BattleSkillSlotRuntime.public void ResetRuntime()` | runtimeをリセットする | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:425 |
| 81 | `BattleSkillSlotRuntime.public void SetCurrentCharge(int currentCharge)` | 現在値chargeを設定する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1220 |
| 86 | `BattleSkillSlotRuntime.public void GainTurnCharge()` | gainターンchargeを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:458 |
| 91 | `BattleSkillSlotRuntime.public void GainAttackCharge(int resolvedAttackCount = 1)` | gainattackchargeを処理する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 101 | `BattleSkillSlotRuntime.public void Consume()` | consumeを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:855 |
| 106 | `BattleSkillSlotRuntime.public int GetRemainingTurnsUntilReady()` | remainingturnsuntilreadyを取得する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 122 | `BattleSkillSlotRuntime.public string GetStateLabel()` | statelabelを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:259 |
| 137 | `BattleSkillSlotRuntime.public string GetChargeLabel()` | chargelabelを取得する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 147 | `BattleSkillSlotRuntime.private void GainCharge(int delta)` | gainchargeを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Runtime/BattleSkillSlotRuntime.cs:88, Assets/Scripts/Features/Battle/Runtime/BattleSkillSlotRuntime.cs:98 |
| 157 | `BattleSkillSlotRuntime.private int GetRequiredCharge()` | requiredchargeを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Runtime/BattleSkillSlotRuntime.cs:70, Assets/Scripts/Features/Battle/Runtime/BattleSkillSlotRuntime.cs:78, Assets/Scripts/Features/Battle/Runtime/BattleSkillSlotRuntime.cs:83 |
| 162 | `BattleSkillSlotRuntime.private static int Clamp(int value, int min, int max)` | clampを処理する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:245, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:186, Assets/Scripts/Data/MasterData/MasterDataResourceLoader.cs:210 |
| 167 | `BattleSkillSlotRuntime.private static int Max(int left, int right)` | maxを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/AttributeMasterData.cs:49, Assets/Scripts/Data/MasterData/AttributeMasterData.cs:50, Assets/Scripts/Data/MasterData/BattleSkillMasterData.cs:139 |
| 172 | `BattleSkillSlotRuntime.private static int Min(int left, int right)` | minを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:169, Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs:171, Assets/Scripts/Features/Battle/Runtime/BattleSkillSlotRuntime.cs:154 |

### `Assets/Scripts/Features/Battle/Runtime/BattleTurnContext.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 42 | `BattleTurnContext.public BattleTurnContext Clone()` | cloneを処理する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |

### `Assets/Scripts/Features/Battle/Runtime/BattleTurnResolutionReport.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 59 | `BattleTurnResolutionReport.public void AddLog(string message)` | logを追加する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:666, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:708, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:82 |
| 69 | `BattleTurnResolutionReport.public void AddPlayerAnimationCue(BattlePlayerAnimationCue cue)` | プレイヤーアニメーションcueを追加する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:81, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:89, Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs:98 |

### `Assets/Scripts/Features/Home/HomeUIScript.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 32 | `HomeUIScript.private void Awake()` | 参照取得とイベント登録を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/Home.unity |
| 43 | `HomeUIScript.private void Start()` | 起動直後の初期化を行う | 保留 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/Home.unity / 接続先 scene はあるが Build Settings では未有効 |
| 54 | `HomeUIScript.public void OnClickAvatarButton()` | クリックアバターボタン時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Home/HomeUIScript.cs:39 / デリゲート/識別子として参照される: Assets/Scripts/Features/Home/HomeUIScript.cs:39 |
| 59 | `HomeUIScript.public void OnClickItemButton()` | クリックアイテムボタン時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Home/HomeUIScript.cs:40 / デリゲート/識別子として参照される: Assets/Scripts/Features/Home/HomeUIScript.cs:40 |
| 64 | `HomeUIScript.private async UniTaskVoid OpenAvatarDialogAsync()` | アバターダイアログを開く | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:56 |
| 75 | `HomeUIScript.private async UniTaskVoid OpenItemDialogAsync()` | アイテムダイアログを開く | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:61 |
| 116 | `HomeUIScript.private void ConfigureModalHierarchy()` | modalhierarchyを構成する | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:36, Assets/Scripts/Features/Home/HomeUIScript.cs:89 |
| 144 | `HomeUIScript.private static void StretchToFillParent(RectTransform rectTransform)` | to満たす親へ伸ばす | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:118, Assets/Scripts/Features/Home/HomeUIScript.cs:130, Assets/Scripts/Features/Home/HomeUIScript.cs:131 |

### `Assets/Scripts/Features/Main/HomeScreen.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 24 | `HomeScreen.public void Init()` | イベント購読を解除する | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:30, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:35, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:440 |
| 34 | `HomeScreen.private void OnDestroy()` | 破棄時の後始末を行う | 保留 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/MainScene.unity / 接続先 scene はあるが Build Settings では未有効 |
| 39 | `HomeScreen.private void OnClickBattle()` | シーン遷移を行う | 必要 | イベント購読で登録される: Assets/Scripts/Features/Main/HomeScreen.cs:29, Assets/Scripts/Features/Main/HomeScreen.cs:30, Assets/Scripts/Features/Main/HomeScreen.cs:36 / デリゲート/識別子として参照される: Assets/Scripts/Features/Main/HomeScreen.cs:29, Assets/Scripts/Features/Main/HomeScreen.cs:30, Assets/Scripts/Features/Main/HomeScreen.cs:36 |
| 60 | `HomeScreen.private int GetNextStageId()` | nextステージidを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1114, Assets/Scripts/Features/Main/HomeScreen.cs:47 |

### `Assets/Scripts/Features/Main/MainScene.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 30 | `MainScene.private void Awake()` | 参照取得とイベント登録を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/MainScene.unity |
| 35 | `MainScene.private void Start()` | 起動直後の初期化を行う | 保留 | Unity ライフサイクルで実行される。スクリプト参照: Assets/Scenes/MainScene.unity / 接続先 scene はあるが Build Settings では未有効 |
| 44 | `MainScene.public void ChangeScreen(Assets.Scripts.Features.Main.Screen screen)` | changescreenを処理する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/Scenes/MainScene.unity |
| 54 | `MainScene.private static void BootstrapServices()` | 静的サービスの初期化。各 EnsureInitialized() は冪等なので ここで一度呼んでおくだけで後続のアクセスが安全になる。 | 必要 | コード参照あり: Assets/Scripts/Features/Main/MainScene.cs:32 |

### `Assets/Scripts/Systems/GameData/AvatarRenderService.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 26 | `AvatarRenderService.[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]         private static void Bootstrap()` | runtime初期化するon読み込むmethodを処理する | 必要 | [RuntimeInitializeOnLoadMethod] 属性で Unity/Editor から呼ばれる |
| 32 | `AvatarRenderService.public static AvatarRenderService EnsureInitialized()` | initializedを確保する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:278, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:290, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:291 |
| 45 | `AvatarRenderService.public AvatarRenderService(             GameSaveService saveService,             AvatarService avatarService,             InventoryService inventoryService)` | アバター描画するサービスを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:41 |
| 56 | `AvatarRenderService.public bool SyncSessionFromRendererIfNeeded(PartsManager partsManager, bool saveAfterSync = false)` | セッションfromrendererifneededを同期する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:444, Assets/Scripts/Features/Home/HomeUIScript.cs:49 |
| 65 | `AvatarRenderService.public void ApplyTo(PartsManager partsManager)` | toを適用する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:445, Assets/Scripts/Features/Home/HomeUIScript.cs:50, Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:91 |
| 77 | `AvatarRenderService.public AvatarAppearanceData CaptureFrom(PartsManager partsManager, bool saveAfterCapture = false)` | capturefromを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:61, Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:71 |
| 94 | `AvatarRenderService.private bool HasStoredAppearance()` | storedappearanceがあるか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:58 |
| 100 | `AvatarRenderService.private void SyncOwnedEquipments(AvatarAppearanceData appearance)` | ownedequipmentsを同期する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:86 |
| 117 | `AvatarRenderService.private static void EnsurePartsManagerInitialized(PartsManager partsManager)` | パーツ管理initializedを確保する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:70, Assets/Scripts/Systems/GameData/AvatarRenderService.cs:82, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:210 |

### `Assets/Scripts/Systems/GameData/AvatarService.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 26 | `AvatarService.[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]         private static void Bootstrap()` | runtime初期化するon読み込むmethodを処理する | 必要 | [RuntimeInitializeOnLoadMethod] 属性で Unity/Editor から呼ばれる |
| 32 | `AvatarService.public static AvatarService EnsureInitialized()` | initializedを確保する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:278, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:290, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:291 |
| 44 | `AvatarService.public AvatarService(GameSaveService saveService, InventoryService inventoryService)` | アバターサービスを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:37 |
| 51 | `AvatarService.public AvatarAppearanceData GetCurrentAppearance()` | 現在値appearanceを取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:73, Assets/Scripts/Systems/GameData/AvatarRenderService.cs:80, Assets/Scripts/Systems/GameData/AvatarRenderService.cs:96 |
| 56 | `AvatarService.public void SetCurrentAppearance(AvatarAppearanceData appearance, bool markDirty = true)` | 現在値appearanceを設定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:85, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:189 |
| 74 | `AvatarService.public IReadOnlyList<AvatarPartStateData> GetAllPartStates()` | allpartstatesを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:295 |
| 82 | `AvatarService.public AvatarPartStateData GetOrCreatePartState(PartsType partType)` | or生成するpartstateを取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:122, Assets/Scripts/Systems/GameData/AvatarService.cs:135, Assets/Scripts/Systems/GameData/AvatarService.cs:146 |
| 100 | `AvatarService.public bool TryEquip(int equipmentId, bool isVisible = true)` | equipを試みる | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:109 |
| 112 | `AvatarService.public bool TryEquip(PartsType partType, int equipmentId, bool isVisible = true)` | equipを試みる | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:109 |
| 133 | `AvatarService.public void Unequip(PartsType partType)` | unequipを処理する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 144 | `AvatarService.public void SetVisibility(PartsType partType, bool isVisible)` | visibilityを設定する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 154 | `AvatarService.public bool IsEquipped(int equipmentId)` | equippedか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:26, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:276, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:299 |
| 159 | `AvatarService.public bool TryGetEquippedEquipmentId(PartsType partType, out int equipmentId)` | 取得するequipped装備idを試みる | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 166 | `AvatarService.public IReadOnlyList<EquipmentData> GetSelectableEquipments(PartsType partType, bool ownedOnly = true)` | selectableequipmentsを取得する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 171 | `AvatarService.public string GetColorHtml(ColorTargetType target)` | 色htmlを取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:180, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:82, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:155 |
| 177 | `AvatarService.public bool TryGetColor(ColorTargetType target, out Color color)` | 取得する色を試みる | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 183 | `AvatarService.public void SetColor(ColorTargetType target, string htmlColor)` | 色を設定する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:690, Assets/Scripts/Core/PartsManager.cs:720, Assets/Scripts/Core/PartsManager.cs:744 |
| 207 | `AvatarService.private void Initialize()` | 対象を初期化する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:102, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:154, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:243 |
| 216 | `AvatarService.private AvatarAppearanceData EnsureAppearance()` | appearanceを確保する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:53, Assets/Scripts/Systems/GameData/AvatarService.cs:76, Assets/Scripts/Systems/GameData/AvatarService.cs:84 |
| 225 | `AvatarService.private void EnsureDefaultColor(AvatarAppearanceData appearance, ColorTargetType target, bool markDirty)` | 既定色を確保する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:65, Assets/Scripts/Systems/GameData/AvatarService.cs:66, Assets/Scripts/Systems/GameData/AvatarService.cs:67 |
| 240 | `AvatarService.private static string NormalizeHtmlColor(string htmlColor)` | normalizehtml色を処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:185 |

### `Assets/Scripts/Systems/GameData/BattleProgressService.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 26 | `BattleProgressService.[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]         private static void Bootstrap()` | runtime初期化するon読み込むmethodを処理する | 必要 | [RuntimeInitializeOnLoadMethod] 属性で Unity/Editor から呼ばれる |
| 32 | `BattleProgressService.public static BattleProgressService EnsureInitialized()` | initializedを確保する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:278, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:290, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:291 |
| 43 | `BattleProgressService.public BattleProgressService(GameSaveService saveService, IReadOnlyList<BattleStageData> battleStageData)` | バトル進行サービスを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:37 |
| 50 | `BattleProgressService.public void RefreshDefinitions(IReadOnlyList<BattleStageData> battleStageData)` | definitionsを更新する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:47, Assets/Scripts/Systems/GameData/InventoryService.cs:50 |
| 57 | `BattleProgressService.public IReadOnlyList<StageProgressData> GetAllProgress()` | all進行を取得する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 65 | `BattleProgressService.public StageProgressData GetOrCreateStageProgress(int stageId)` | or生成するステージ進行を取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:84, Assets/Scripts/Systems/GameData/BattleProgressService.cs:89, Assets/Scripts/Systems/GameData/BattleProgressService.cs:94 |
| 82 | `BattleProgressService.public bool IsUnlocked(int stageId)` | unlockedか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:164, Assets/Scripts/Systems/GameData/BattleProgressService.cs:170, Assets/Scripts/Systems/GameData/BattleProgressService.cs:174 |
| 87 | `BattleProgressService.public bool IsCleared(int stageId)` | clearedか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:170 |
| 92 | `BattleProgressService.public void UnlockStage(int stageId)` | unlockステージを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1117 |
| 102 | `BattleProgressService.public void SetLastSelectedStage(int stageId)` | lastselectedステージを設定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:119 |
| 112 | `BattleProgressService.public void SetCurrentStage(int stageId)` | 現在値ステージを設定する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:383 |
| 123 | `BattleProgressService.public void RecordStageClear(int stageId, int score = 0, string rank =   , float clearTimeSeconds = 0f)` | recordステージclearを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1112 |
| 146 | `BattleProgressService.public IReadOnlyList<BattleStageData> GetOrderedStageDefinitions()` | orderedステージdefinitionsを取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:157, Assets/Scripts/Systems/GameData/BattleProgressService.cs:199 |
| 155 | `BattleProgressService.public int GetRecommendedStageId()` | recommendedステージidを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Main/HomeScreen.cs:62, Assets/Scripts/Systems/GameData/BattleProgressService.cs:191 |
| 184 | `BattleProgressService.public int GetCurrentOrRecommendedStageId()` | 進行状況から現在選択中のステージIDを取得 | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:361 |
| 194 | `BattleProgressService.public int GetNextStageId(int stageId)` | nextステージidを取得する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:1114, Assets/Scripts/Features/Main/HomeScreen.cs:47 |
| 214 | `BattleProgressService.private void Initialize()` | 対象を初期化する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:102, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:154, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:243 |
| 221 | `BattleProgressService.private BattleProgressData EnsureBattleProgress()` | バトル進行を確保する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:59, Assets/Scripts/Systems/GameData/BattleProgressService.cs:67, Assets/Scripts/Systems/GameData/BattleProgressService.cs:104 |
| 228 | `BattleProgressService.private void RebuildLookups()` | rebuildlookupsを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:21, Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:26, Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:39 |
| 243 | `BattleProgressService.private void EnsureInitialUnlockedStages()` | initialunlockedstagesを確保する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:218 |
| 274 | `BattleProgressService.private void PruneInvalidStageProgress()` | pruneinvalidステージ進行を処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:217 |
| 287 | `BattleProgressService.private static bool IsBetterRank(string candidateRank, string currentRank)` | betterrankか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:131 |
| 298 | `BattleProgressService.private static int GetRankScore(string rank)` | rankscoreを取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:295, Assets/Scripts/Systems/GameData/BattleProgressService.cs:295 |

### `Assets/Scripts/Systems/GameData/BattleSceneTransitionState.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 11 | `BattleSceneTransitionState.public static int ConsumeSelectedStageId()` | 値を取り出してリセットする（一度だけ使う用途） | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:357 |

### `Assets/Scripts/Systems/GameData/InventoryService.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 29 | `InventoryService.[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]         private static void Bootstrap()` | runtime初期化するon読み込むmethodを処理する | 必要 | [RuntimeInitializeOnLoadMethod] 属性で Unity/Editor から呼ばれる |
| 35 | `InventoryService.public static InventoryService EnsureInitialized()` | initializedを確保する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:278, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:290, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:291 |
| 46 | `InventoryService.public InventoryService(GameSaveService saveService, IReadOnlyList<EquipmentData> equipmentData)` | インベントリサービスを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/InventoryService.cs:40 |
| 53 | `InventoryService.public void RefreshDefinitions(IReadOnlyList<EquipmentData> equipmentData)` | definitionsを更新する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:47, Assets/Scripts/Systems/GameData/InventoryService.cs:50 |
| 60 | `InventoryService.public IReadOnlyList<InventoryEntryData> GetAllEntries()` | allentriesを取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/InventoryService.cs:69, Assets/Scripts/Systems/GameData/InventoryService.cs:85 |
| 67 | `InventoryService.public IReadOnlyList<EquipmentData> GetOwnedDefinitions()` | owneddefinitionsを取得する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 76 | `InventoryService.public IReadOnlyList<EquipmentData> GetDefinitionsByPartType(PartsType partType, bool ownedOnly = false)` | definitionsbyparttypeを取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:168, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:225 |
| 94 | `InventoryService.public bool TryGetDefinition(int equipmentId, out EquipmentData definition)` | 取得するdefinitionを試みる | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:303, Assets/Scripts/Systems/GameData/AvatarService.cs:103, Assets/Scripts/Systems/GameData/AvatarService.cs:115 |
| 99 | `InventoryService.internal bool TryGetByPartsIndex(PartsType partType, int partsIndex, out EquipmentData definition)` | 取得するbyパーツindexを試みる | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:30 |
| 104 | `InventoryService.public bool HasEquipment(int equipmentId)` | 装備があるか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:110, Assets/Scripts/Systems/GameData/AvatarService.cs:104, Assets/Scripts/Systems/GameData/AvatarService.cs:117 |
| 109 | `InventoryService.public bool TryGetEntry(int equipmentId, out InventoryEntryData entry)` | 取得するentryを試みる | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/InventoryService.cs:106, Assets/Scripts/Systems/GameData/InventoryService.cs:145, Assets/Scripts/Systems/GameData/InventoryService.cs:158 |
| 116 | `InventoryService.public InventoryEntryData GrantEquipment(int equipmentId, int quantity = 1, bool isUnlocked = true)` | grant装備を処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:112 |
| 143 | `InventoryService.public bool RemoveEquipment(int equipmentId, int quantity = 1)` | 装備を削除する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 156 | `InventoryService.public void MarkEquipped(int equipmentId)` | markequippedを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:128, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:196 |
| 165 | `InventoryService.private void Initialize()` | 対象を初期化する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:102, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:154, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:243 |
| 171 | `InventoryService.private InventoryData EnsureInventory()` | インベントリを確保する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/InventoryService.cs:62, Assets/Scripts/Systems/GameData/InventoryService.cs:111, Assets/Scripts/Systems/GameData/InventoryService.cs:121 |
| 178 | `InventoryService.private void RebuildLookups()` | rebuildlookupsを処理する | 必要 | コード参照あり: Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:21, Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:26, Assets/Scripts/Data/MasterData/BattleSkillMasterDatabase.cs:39 |
| 221 | `InventoryService.private void SyncDefaultOwnedEquipments()` | 既定ownedequipmentsを同期する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/InventoryService.cs:168 |

### `Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 14 | `PartsManagerAvatarAdapter.public static AvatarAppearanceData CaptureAppearance(PartsManager partsManager, InventoryService inventoryService)` | captureappearanceを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:84, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:53 |
| 58 | `PartsManagerAvatarAdapter.public static void ApplyAppearance(PartsManager partsManager, AvatarAppearanceData appearance, InventoryService inventoryService)` | appearanceを適用する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:71, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:211 |
| 88 | `PartsManagerAvatarAdapter.private static void ApplyPartState(PartsManager partsManager, AvatarPartStateData state, InventoryService inventoryService)` | partstateを適用する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:73 |
| 116 | `PartsManagerAvatarAdapter.private static bool TryResolvePartsIndex(             AvatarPartStateData state,             InventoryService inventoryService,             out int partsIndex)` | 解決するパーツindexを試みる | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:93 |
| 138 | `PartsManagerAvatarAdapter.private static bool IsDerivedPart(PartsType partType)` | derivedpartか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:22, Assets/Scripts/Systems/GameData/PartsManagerAvatarAdapter.cs:70 |

### `Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 25 | `AvatarAppearanceJsonStore.public static void Save(AppearanceData appearanceData)` | データをシリアライズ/デシリアライズする | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:87, Assets/Scripts/Systems/Save/GameSaveService.cs:94, Assets/Scripts/Systems/Save/GameSaveService.cs:103 |
| 47 | `AvatarAppearanceJsonStore.public static bool TryLoad(out AppearanceData appearanceData)` | 読み込むを試みる | 必要 | コード参照あり: Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:63 |
| 58 | `AvatarAppearanceJsonStore.public static bool TryApplyTo(PartsManager partsManager)` | 適用するtoを試みる | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 70 | `AvatarAppearanceJsonStore.private static bool TryReadCurrentSaveData(out SaveData saveData)` | データをシリアライズ/デシリアライズする | 必要 | コード参照あり: Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:51 |
| 92 | `AvatarAppearanceJsonStore.private static bool IsEmpty(AppearanceData appearanceData)` | emptyか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:55, Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:44 |
| 100 | `AvatarAppearanceJsonStore.private static AppearanceData CloneAppearanceData(AppearanceData appearanceData)` | cloneappearancedataを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:35, Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:54 |
| 113 | `AvatarAppearanceJsonStore.private static List<AppearanceData.PartsEntry> CloneParts(List<AppearanceData.PartsEntry> parts)` | cloneパーツを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:107 |
| 132 | `AvatarAppearanceJsonStore.private static List<AppearanceData.ColorEntry> CloneColors(List<AppearanceData.ColorEntry> colors)` | clonecolorsを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:108 |
| 151 | `AvatarAppearanceJsonStore.private static List<AppearanceData.VisibilityEntry> CloneVisibility(List<AppearanceData.VisibilityEntry> visibility)` | clonevisibilityを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:109 |

### `Assets/Scripts/Systems/Save/DefaultGameSaveFactory.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 8 | `DefaultGameSaveFactory.public UserData Create()` | 対象を生成する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveJsonSerializer.cs:39, Assets/Scripts/Systems/Save/GameSaveService.cs:72, Assets/GUIPack-Clean&Minimalist/Demo/Scripts/Popup.cs:56 |
| 34 | `DefaultGameSaveFactory.public string CreateTimestamp()` | timestampを生成する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/DefaultGameSaveFactory.cs:10, Assets/Scripts/Systems/Save/GameSaveService.cs:84, Assets/Scripts/Systems/Save/GameSaveService.cs:101 |

### `Assets/Scripts/Systems/Save/GameSaveJsonSerializer.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 26 | `GameSaveJsonSerializer.public static string Serialize(UserData userData)` | UserData → JSON文字列 | 必要 | コード参照あり: Assets/Scripts/Systems/Save/JsonGameSaveRepository.cs:47 |
| 34 | `GameSaveJsonSerializer.public static UserData Deserialize(string json)` | JSON文字列 → UserData | 必要 | コード参照あり: Assets/Scripts/Systems/Save/JsonGameSaveRepository.cs:33 |
| 51 | `GameSaveJsonSerializer.private static UserData ConvertLegacy(LegacyUserDataDocument legacy)` | 旧形式へ変換する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveJsonSerializer.cs:48 |
| 118 | `EquipmentIdJsonConverter.public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)` | JSONを書き込む | 必要 | override 実装でランタイム/Editor から呼ばれる |
| 123 | `EquipmentIdJsonConverter.public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)` | JSONを読み取る | 必要 | override 実装でランタイム/Editor から呼ばれる |
| 135 | `EquipmentIdJsonConverter.private static int NormalizeNumericId(int equipmentId)` | normalizenumericidを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveJsonSerializer.cs:127 |
| 140 | `EquipmentIdJsonConverter.private static int ParseEquipmentId(string text)` | parse装備idを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveJsonSerializer.cs:128 |
| 157 | `StageIdJsonConverter.public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)` | JSONを書き込む | 必要 | override 実装でランタイム/Editor から呼ばれる |
| 162 | `StageIdJsonConverter.public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)` | JSONを読み取る | 必要 | override 実装でランタイム/Editor から呼ばれる |
| 174 | `StageIdJsonConverter.private static int ParseStageId(string text)` | parseステージidを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveJsonSerializer.cs:167 |

### `Assets/Scripts/Systems/Save/GameSaveService.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 23 | `GameSaveService.[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]         private static void Bootstrap()` | runtime初期化するon読み込むmethodを処理する | 必要 | [RuntimeInitializeOnLoadMethod] 属性で Unity/Editor から呼ばれる |
| 29 | `GameSaveService.public static GameSaveService EnsureInitialized()` | initializedを確保する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:278, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:290, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:291 |
| 44 | `GameSaveService.public GameSaveService(             GameSession session,             IGameSaveRepository repository,             DefaultGameSaveFactory defaultFactory,             LegacyAvatarSaveMigration legacyMigration)` | game保存するサービスを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:34 |
| 56 | `GameSaveService.public void ReloadSession()` | reloadセッションを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:40 |
| 62 | `GameSaveService.public UserData LoadOrCreateDefault()` | or生成する既定を読み込む | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:58 |
| 92 | `GameSaveService.public void SaveSession()` | セッションを保存する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:89, Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:92, Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:72 |
| 98 | `GameSaveService.public void Save(UserData userData)` | 対象を保存する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:87, Assets/Scripts/Systems/Save/GameSaveService.cs:94, Assets/Scripts/Systems/Save/GameSaveService.cs:103 |
| 106 | `GameSaveService.public void MarkDirty()` | markdirtyを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:71, Assets/Scripts/Systems/GameData/AvatarService.cs:96, Assets/Scripts/Systems/GameData/AvatarService.cs:129 |
| 111 | `GameSaveService.private static void Normalize(UserData userData)` | normalizeを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:201, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:419, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:431 |

### `Assets/Scripts/Systems/Save/GameSession.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 28 | `GameSession.[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]         private static void Bootstrap()` | runtime初期化するon読み込むmethodを処理する | 必要 | [RuntimeInitializeOnLoadMethod] 属性で Unity/Editor から呼ばれる |
| 34 | `GameSession.private void Awake()` | 参照取得とイベント登録を行う | 必要 | 型が動的生成され、そのライフサイクルで呼ばれる: Assets/Scripts/Systems/Save/GameSession.cs:86, Assets/Scripts/Systems/Save/GameSession.cs:95 |
| 46 | `GameSession.public void Initialize(UserData userData)` | 対象を初期化する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:102, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:154, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:243 |
| 53 | `GameSession.public void ReplaceUserData(UserData userData, bool markDirty = false)` | replaceuserdataを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSession.cs:63 |
| 60 | `GameSession.[System.Obsolete(                              )]         public void ReplaceSaveData(UserData userData, bool markDirty = false)` | obsoleteを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:14, Assets/Scripts/Systems/Save/GameSession.cs:14 |
| 66 | `GameSession.public void MarkDirty()` | markdirtyを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:71, Assets/Scripts/Systems/GameData/AvatarService.cs:96, Assets/Scripts/Systems/GameData/AvatarService.cs:129 |
| 71 | `GameSession.public void ClearDirty()` | cleardirtyを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:95 |
| 76 | `GameSession.public void SetCurrentStageId(int stageId)` | 現在値ステージidを設定する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/BattleProgressService.cs:120 |
| 81 | `GameSession.private static GameSession EnsureInstance()` | instanceを確保する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSession.cs:26, Assets/Scripts/Systems/Save/GameSession.cs:31 |

### `Assets/Scripts/Systems/Save/JsonGameSaveRepository.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 20 | `JsonGameSaveRepository.public bool Exists()` | existsを処理する | 必要 | コード参照あり: Assets/Scripts/Editor/PartsManagerEditor.cs:165, Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:73, Assets/Scripts/Systems/Save/GameSaveService.cs:66 |
| 25 | `JsonGameSaveRepository.public UserData Load()` | 対象を読み込む | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:68, Assets/Scripts/Systems/Save/IGameSaveRepository.cs:11, Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark01.cs:46 |
| 42 | `JsonGameSaveRepository.public void Save(UserData userData)` | 対象を保存する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:87, Assets/Scripts/Systems/Save/GameSaveService.cs:94, Assets/Scripts/Systems/Save/GameSaveService.cs:103 |

### `Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 18 | `LegacyAvatarSaveMigration.public bool TryApply(UserData userData)` | 適用するを試みる | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:73 |
| 30 | `LegacyAvatarSaveMigration.private static bool TryLoadLegacyAppearance(out LegacyAvatarAppearanceData appearanceData)` | データをシリアライズ/デシリアライズする | 必要 | コード参照あり: Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:20 |
| 56 | `LegacyAvatarSaveMigration.private static AvatarAppearanceData ConvertAppearance(LegacyAvatarAppearanceData legacyAppearance)` | appearanceへ変換する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:24 |
| 112 | `LegacyAvatarSaveMigration.bool> BuildVisibilityMap(LegacyAvatarAppearanceData legacyAppearance)` | visibilitymapを構築する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:58 |
| 129 | `LegacyAvatarSaveMigration.private static void MergeEquippedItemsIntoInventory(InventoryData inventory, AvatarAppearanceData appearance)` | mergeequippeditemsintoインベントリを処理する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:26 |
| 155 | `LegacyAvatarSaveMigration.private static bool IsEmpty(LegacyAvatarAppearanceData appearanceData)` | emptyか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/AvatarAppearanceJsonStore.cs:55, Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:44 |
| 163 | `LegacyAvatarSaveMigration.private static bool IsDerivedPartType(PartsType partType)` | derivedparttypeか判定する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:66, Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:81, Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:120 |
| 168 | `LegacyAvatarSaveMigration.private static int BuildLegacyEquipmentId(PartsType partType, int index)` | 旧形式装備idを構築する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/LegacyAvatarSaveMigration.cs:73 |

### `Assets/Scripts/Systems/Save/Models/UserData.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 107 | `UserBattleProfileData.public static List<UserBattleSkillSlotData> CreateDefaultSkillSlots()` | 既定skillslotsを生成する | 必要 | コード参照あり: Assets/Scripts/Systems/Save/GameSaveService.cs:145, Assets/Scripts/Systems/Save/GameSaveService.cs:149, Assets/Scripts/Systems/Save/Models/UserData.cs:105 |

### `Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 13 | `AvatarDialogRequest.public AvatarDialogRequest(Player player)` | アバターダイアログrequestを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:66 |
| 23 | `AvatarDialogResult.private AvatarDialogResult(bool isSaved)` | アバターダイアログresultを処理する | 必要 | デリゲート/識別子として参照される: Assets/Scripts/Features/Home/HomeUIScript.cs:67, Assets/Scripts/Features/Home/HomeUIScript.cs:67, Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:21 |
| 56 | `AvatarDialog.public void Setup(AvatarDialogRequest request, DialogContext context)` | UIイベントの購読を設定する | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:111, Assets/Scripts/UI/Dialog/Base/DialogBase.cs:18 |
| 81 | `AvatarDialog.protected override void OnClickClose()` | クリック閉じる時の処理を行う | 必要 | override 実装でランタイム/Editor から呼ばれる / イベント購読で登録される: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:74, Assets/Scripts/UI/Dialog/Item/ItemDialog.cs:29 / コード参照あり: Assets/Scripts/UI/Dialog/Base/DialogBase.cs:68 |
| 88 | `AvatarDialog.private void OnClickSave()` | クリック保存する時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:75 / デリゲート/識別子として参照される: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:75 |
| 98 | `AvatarDialog.private void DespawnDialogPreview()` | despawnダイアログプレビューするを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:83, Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:93 |
| 108 | `AvatarDialog.private async UniTaskVoid RefreshSelectionFramesDeferred()` | selectionframesdeferredを更新する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:78 |

### `Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 33 | `AvatarEditorPresenter.public AvatarEditorPresenter(             AvatarService avatarService,             InventoryService inventoryService,             AvatarRenderService avatarRenderService,             AvatarAppearanceData initialAppearance)` | アバターeditorpresenterを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:56 |
| 46 | `AvatarEditorPresenter.public static AvatarEditorPresenter CreateDefault(PartsManager sourcePartsManager = null)` | 既定を生成する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:68 |
| 59 | `AvatarEditorPresenter.public void BindPreview(PartsManager previewPartsManager)` | プレビューするを結び付ける | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:69 |
| 65 | `AvatarEditorPresenter.public IReadOnlyList<AvatarPartSectionViewData> GetSections(bool includeLocked = false)` | sectionsを取得する | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 72 | `AvatarEditorPresenter.public AvatarPartSectionViewData GetSection(UICategory category, bool includeLocked = false)` | sectionを取得する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:68, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:382, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:166 |
| 101 | `AvatarEditorPresenter.public bool TrySelectEquipment(int equipmentId)` | select装備を試みる | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:188 |
| 131 | `AvatarEditorPresenter.public void ResetCategory(UICategory category)` | categoryをリセットする | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PanelPartsListControl.cs:227 |
| 141 | `AvatarEditorPresenter.public void SetColor(ColorTargetType target, Color color)` | 色を設定する | 必要 | コード参照あり: Assets/Scripts/Core/PartsManager.cs:690, Assets/Scripts/Core/PartsManager.cs:720, Assets/Scripts/Core/PartsManager.cs:744 |
| 152 | `AvatarEditorPresenter.public bool TryGetCurrentColor(ColorTargetType target, out Color color)` | 取得する現在値色を試みる | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:161 |
| 158 | `AvatarEditorPresenter.public IReadOnlyList<AvatarColorOptionViewData> GetColorOptions(ColorTargetType target, IReadOnlyList<Color> presetColors)` | 色optionsを取得する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/ColorPicker/ColorSelectScrollUIScript.cs:241 |
| 186 | `AvatarEditorPresenter.public void Commit()` | commitを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:90 |
| 205 | `AvatarEditorPresenter.public void ApplyDraftTo(PartsManager partsManager)` | drafttoを適用する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:271 |
| 214 | `AvatarEditorPresenter.private IReadOnlyList<AvatarPartOptionViewData> BuildOptions(             UICategory category,             IReadOnlyList<PartsType> partTypes,             bool includeLocked)` | optionsを構築する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:75 |
| 260 | `AvatarEditorPresenter.private string GetColorHtml(ColorTargetType target)` | 色htmlを取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:180, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:82, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:155 |
| 266 | `AvatarEditorPresenter.private void ApplyDraftToPreview()` | drafttoプレビューするを適用する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:62, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:127, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:138 |
| 274 | `AvatarEditorPresenter.private AvatarPartStateData GetPartState(PartsType partType)` | partstateを取得する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:224, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:281, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:374 |
| 279 | `AvatarEditorPresenter.private AvatarPartStateData GetOrCreatePartState(PartsType partType)` | or生成するpartstateを取得する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarService.cs:122, Assets/Scripts/Systems/GameData/AvatarService.cs:135, Assets/Scripts/Systems/GameData/AvatarService.cs:146 |
| 296 | `AvatarEditorPresenter.private AvatarColorData GetOrCreateColorState(ColorTargetType target)` | or生成する色stateを取得する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:144 |
| 312 | `AvatarEditorPresenter.private void ResetPart(PartsType partType)` | partをリセットする | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:118, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:135 |
| 319 | `AvatarEditorPresenter.private static AvatarAppearanceData CloneAppearance(AvatarAppearanceData source)` | cloneappearanceを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:42, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:188 |
| 347 | `AvatarEditorPresenter.private static void EnsureDefaultColors(AvatarAppearanceData appearance)` | 既定colorsを確保する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:43 |
| 358 | `AvatarEditorPresenter.private static void EnsureColor(AvatarAppearanceData appearance, ColorTargetType target)` | 色を確保する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:352, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:353, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:354 |
| 370 | `AvatarEditorPresenter.private PartsType? GetVisiblePartType(IEnumerable<PartsType> partTypes)` | 表示状態parttypeを取得する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:78 |
| 386 | `AvatarEditorPresenter.private static UICategory ToCategory(PartsType partType)` | tocategoryを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:111 |
| 397 | `AvatarEditorPresenter.private static bool TryGetColorTarget(UICategory category, out ColorTargetType target)` | 取得する色targetを試みる | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:81 |
| 413 | `AvatarEditorPresenter.private static bool Assign(ColorTargetType value, out ColorTargetType target)` | assignを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:406, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:407, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:408 |
| 419 | `AvatarEditorPresenter.private static void EnsurePartsManagerInitialized(PartsManager partsManager)` | パーツ管理initializedを確保する | 必要 | コード参照あり: Assets/Scripts/Systems/GameData/AvatarRenderService.cs:70, Assets/Scripts/Systems/GameData/AvatarRenderService.cs:82, Assets/Scripts/UI/Dialog/Avatar/AvatarEditorPresenter.cs:210 |

### `Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 25 | `PanelPartsControl.private void Awake()` | 参照取得とイベント登録を行う | 必要 | Unity ライフサイクルで実行される。スクリプト参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 34 | `PanelPartsControl.public void Init(PartsManager pm, PanelPartsListControl panelPartsList, AvatarEditorPresenter presenter = null)` | PartsManager と連動する PanelPartsList を使って、PanelParts を初期化する 子要素の PartsSlot をセットアップし | 必要 | コード参照あり: Assets/Scripts/Data/CharacterPrefabData.cs:30, Assets/Scripts/Editor/EquipmentMasterDatabaseBuilder.cs:35, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:440 |
| 47 | `PanelPartsControl.public static void SelectSlot(PartsSlot slot)` | selectslotを処理する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsListSlot.cs:64, Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:222 |
| 49 | `PanelPartsControl.public static void UnfocusSlot(PartsSlot slot)` | unfocusslotを処理する | 必要 | コード参照あり: Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/UI/PartsSlot.cs:227 |
| 51 | `PanelPartsControl.public void RefreshCurrentSlot()` | 現在値slotを更新する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:44 |
| 57 | `PanelPartsControl.public void RefreshSelectionFrame()` | selectionframeを更新する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:76, Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:77, Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:115 |
| 66 | `PanelPartsControl.public void SaveCurrentAppearance()` | 現在値appearanceを保存する | 保留 | スクリプト自体は asset 参照ありだがメソッド呼び出し根拠が弱い: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab, Assets/Layer Lab/2D Minimal-CharacterMaker/Scenes/DemoScene.unity |
| 75 | `PanelPartsControl.private void DoSelectSlot(PartsSlot slot)` | doselectslotを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:42, Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:47 |
| 85 | `PanelPartsControl.private void DoUnfocusSlot(PartsSlot slot)` | dounfocusslotを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:49 |
| 91 | `PanelPartsControl.private void RepositionSelectFrame(PartsSlot slot)` | repositionselectframeを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:63, Assets/Scripts/UI/Dialog/Avatar/PanelPartsControl.cs:80 |

### `Assets/Scripts/UI/Dialog/Avatar/PreviewHandle.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 11 | `PreviewHandle.internal PreviewHandle(string id, GameObject instance)` | ハンドルをプレビューする | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/PreviewStage.cs:56 |

### `Assets/Scripts/UI/Dialog/Avatar/PreviewPlacement.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `PreviewPlacement.public PreviewPlacement(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)` | 配置をプレビューする | 必要 | デリゲート/識別子として参照される: Assets/Scripts/UI/Dialog/Avatar/PreviewPlacement.cs:9, Assets/Scripts/UI/Dialog/Avatar/PreviewPlacement.cs:23, Assets/Scripts/UI/Dialog/Avatar/PreviewPlacement.cs:27 |
| 27 | `PreviewPlacement.public static PreviewPlacement At(Vector3 localPosition, float uniformScale = 1f)` | 位置と一様スケールだけ指定したい場合の簡易コンストラクタ。 | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |
| 31 | `PreviewPlacement.public static PreviewPlacement At(Vector3 localPosition, Vector3 localScale)` | 位置と3軸スケールを指定。 | 保留 | 公開メソッドだが静的参照を確認できず、外部接続の可能性が残る |

### `Assets/Scripts/UI/Dialog/Avatar/PreviewStage.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 33 | `PreviewStage.public PreviewHandle Spawn(GameObject prefab, PreviewPlacement? placement = null)` | 任意のプレハブを previewRoot 配下に生成する。 返却される PreviewHandle を保持すれば、後で Despawn で個別破棄できる。 | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:65, Assets/Scripts/UI/Dialog/Avatar/PreviewStage.cs:16, Assets/Scripts/UI/Dialog/Avatar/PreviewStage.cs:63 |
| 78 | `PreviewStage.public bool Despawn(PreviewHandle handle)` | 指定ハンドルに対応する生成物を破棄する。 | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:102, Assets/Scripts/UI/Dialog/Avatar/PreviewStage.cs:18 |
| 94 | `PreviewStage.public void ClearAll()` | このレンダラが管理している生成物を全て破棄する。 | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/PreviewStage.cs:18 |
| 107 | `PreviewStage.private void SetLayerRecursively(GameObject target, int layer)` | layerrecursivelyを設定する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/PreviewStage.cs:54, Assets/Scripts/UI/Dialog/Avatar/PreviewStage.cs:115 |
| 119 | `PreviewStage.private int ResolvePreviewLayer()` | プレビューするlayerを解決する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/PreviewStage.cs:54 |

### `Assets/Scripts/UI/Dialog/Base/DialogBackgroundManager.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 18 | `DialogBackgroundManager.public void Configure(Transform backgroundLayer)` | 対象を構成する | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:37, Assets/Scripts/Features/Home/HomeUIScript.cs:90 |
| 30 | `DialogBackgroundManager.private void CreateBackgroundIfNeeded()` | 背景ifneededを生成する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Base/DialogBackgroundManager.cs:60 |
| 58 | `DialogBackgroundManager.public void Push(IDialogBackgroundHandler dialog)` | pushを処理する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Base/DialogBase.cs:52 |
| 71 | `DialogBackgroundManager.public void Remove(IDialogBackgroundHandler dialog)` | 対象を削除する | 必要 | コード参照あり: Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:68, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:343, Assets/Scripts/Editor/StageBattlePatternBoardEditorWindow.cs:356 |
| 79 | `DialogBackgroundManager.private void RefreshBackgroundState()` | 背景stateを更新する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Base/DialogBackgroundManager.cs:68, Assets/Scripts/UI/Dialog/Base/DialogBackgroundManager.cs:76 |
| 91 | `DialogBackgroundManager.private void OnClickBackground()` | クリック背景時の処理を行う | 必要 | デリゲート/識別子として参照される: Assets/Scripts/UI/Dialog/Base/DialogBackgroundManager.cs:53 |
| 100 | `DialogBackgroundManager.private Transform ResolveBackgroundParent()` | 背景親を解決する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Base/DialogBackgroundManager.cs:35 |
| 105 | `DialogBackgroundManager.private static void StretchToFillParent(RectTransform rectTransform)` | to満たす親へ伸ばす | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:118, Assets/Scripts/Features/Home/HomeUIScript.cs:130, Assets/Scripts/Features/Home/HomeUIScript.cs:131 |

### `Assets/Scripts/UI/Dialog/Base/DialogBackgroundView.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 10 | `DialogBackgroundView.public void Initialize(System.Action onClick)` | 対象を初期化する | 必要 | コード参照あり: Assets/Scripts/Features/Battle/Demo/BattleScene.cs:102, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:154, Assets/Scripts/Features/Battle/Demo/BattleScene.cs:243 |
| 15 | `DialogBackgroundView.public void OnPointerClick(PointerEventData eventData)` | pointerクリック時の処理を行う | 必要 | 型が動的生成され、そのライフサイクルで呼ばれる: Assets/Scripts/UI/Dialog/Base/DialogBackgroundManager.cs:52 / UI イベントインターフェースから呼ばれる。参照: Assets/Scripts/UI/Dialog/Base/DialogBackgroundManager.cs:52 |

### `Assets/Scripts/UI/Dialog/Base/DialogBase.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 8 | `DialogContext.public DialogContext(PreviewStage avatarPreviewRenderer)` | ダイアログcontextを処理する | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:94 |
| 35 | `DialogBase.public void SetBackgroundManager(DialogBackgroundManager backgroundManager)` | 背景管理を設定する | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:110 |
| 40 | `DialogBase.public UniTask<TResult> OpenAsync()` | 対象を開く | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:113 |
| 59 | `DialogBase.protected virtual void OnClickClose()` | クリック閉じる時の処理を行う | 必要 | イベント購読で登録される: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:74, Assets/Scripts/UI/Dialog/Item/ItemDialog.cs:29 / コード参照あり: Assets/Scripts/UI/Dialog/Base/DialogBase.cs:68 |
| 64 | `DialogBase.public void OnBackgroundClickedFromManager()` | 背景clickedfrom管理時の処理を行う | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Base/DialogBackgroundManager.cs:97, Assets/Scripts/UI/Dialog/Base/IDialogBackgroundHandler.cs:10 |
| 72 | `DialogBase.protected void Close(TResult result)` | 対象を閉じる | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:85, Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:95, Assets/Scripts/UI/Dialog/Base/DialogBase.cs:61 |
| 92 | `DialogBase.private async UniTaskVoid RunPopupDestroyAsync(Animator animator)` | 不要オブジェクトを破棄する | 必要 | コード参照あり: Assets/Scripts/UI/Dialog/Base/DialogBase.cs:89 |

### `Assets/Scripts/UI/Dialog/Item/ItemDialog.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 11 | `ItemDialogResult.private ItemDialogResult(bool isConfirmed)` | アイテムダイアログresultを処理する | 必要 | デリゲート/識別子として参照される: Assets/Scripts/Features/Home/HomeUIScript.cs:78, Assets/Scripts/Features/Home/HomeUIScript.cs:78, Assets/Scripts/UI/Dialog/Item/ItemDialog.cs:9 |
| 27 | `ItemDialog.public void Setup(ItemDialogRequest request, DialogContext context)` | UIイベントの購読を設定する | 必要 | コード参照あり: Assets/Scripts/Features/Home/HomeUIScript.cs:111, Assets/Scripts/UI/Dialog/Base/DialogBase.cs:18 |
| 32 | `ItemDialog.protected override void OnClickClose()` | クリック閉じる時の処理を行う | 必要 | override 実装でランタイム/Editor から呼ばれる / イベント購読で登録される: Assets/Scripts/UI/Dialog/Avatar/AvatarDialog.cs:74, Assets/Scripts/UI/Dialog/Item/ItemDialog.cs:29 / コード参照あり: Assets/Scripts/UI/Dialog/Base/DialogBase.cs:68 |

## External Samples

外部アセット同梱のサンプル/デモスクリプトは、本体コードとの接続が見えない限り `不要候補` として整理しています。

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/CleanButton.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 26 | `CleanButton.protected override void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/OriginalPrefab/Button-Icon.prefab, Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab |
| 31 | `CleanButton.public override void OnPointerEnter(PointerEventData eventData)` | pointerenter時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/OriginalPrefab/Button-Icon.prefab, Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab |
| 38 | `CleanButton.public override void OnPointerExit(PointerEventData eventData)` | pointerexit時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/OriginalPrefab/Button-Icon.prefab, Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab |
| 45 | `CleanButton.public override void OnPointerDown(PointerEventData eventData)` | pointerdown時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/OriginalPrefab/Button-Icon.prefab, Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab |
| 51 | `CleanButton.public override void OnPointerUp(PointerEventData eventData)` | pointerup時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/OriginalPrefab/Button-Icon.prefab, Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-AvatarController.prefab |

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/Gradient.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 22 | `Gradient.public override void ModifyMesh(VertexHelper vh)` | modifymeshを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab, Assets/OriginalPrefab/Dialog-Item.prefab |
| 46 | `Matrix2x3.public Matrix2x3(float m00, float m01, float m02, float m10, float m11, float m12)` | matrix2x3を処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab, Assets/OriginalPrefab/Dialog-Item.prefab |
| 64 | `Gradient.private Matrix2x3 LocalPositionMatrix(Rect rect, Vector2 dir)` | localpositionmatrixを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab, Assets/OriginalPrefab/Dialog-Item.prefab |
| 82 | `Gradient.private Vector2 RotationDir(float angle)` | rotationdirを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/OriginalPrefab/Dialog-AvatarController 1.prefab, Assets/OriginalPrefab/Dialog-Item 1.prefab, Assets/OriginalPrefab/Dialog-Item.prefab |

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/Popup.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 21 | `Popup.public void Open()` | 対象を開く | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Chat.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Coins Shop.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Contact.prefab |
| 26 | `Popup.public void Close()` | 不要オブジェクトを破棄する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Chat.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Coins Shop.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Contact.prefab |
| 40 | `Popup.private IEnumerator RunPopupDestroy()` | 不要オブジェクトを破棄する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Chat.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Coins Shop.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Contact.prefab |
| 47 | `Popup.private void AddBackground()` | 背景を追加する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Chat.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Coins Shop.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Contact.prefab |
| 73 | `Popup.private void RemoveBackground()` | 背景を削除する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Chat.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Coins Shop.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Contact.prefab |

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/PopupOpener.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 17 | `PopupOpener.protected void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Life Shop.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Light/Light - Life Shop.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Scenes/Themes/Dark/Dark - Game.unity |
| 22 | `PopupOpener.public virtual void OpenPopup()` | オブジェクトを生成する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Life Shop.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Light/Light - Life Shop.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Scenes/Themes/Dark/Dark - Game.unity |

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/SceneTransition.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 17 | `SceneTransition.public void PerformTransition()` | performtransitionを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Prefabs/Navigation/Navigation.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Scenes/Demo/Demo.unity, Assets/GUIPack-Clean&Minimalist/Demo/Scenes/Themes/Dark/Dark - Game.unity |

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/SpriteSwapper.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 20 | `SpriteSwapper.public void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Video Player.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Light/Light - Video Player.prefab |
| 25 | `SpriteSwapper.public void SwapSprite()` | swapspriteを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Dark/Dark - Video Player.prefab, Assets/GUIPack-Clean&Minimalist/Demo/Resources/Popups/Light/Light - Video Player.prefab |

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/Tooltip.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 19 | `Tooltip.public virtual void OnPointerEnter(PointerEventData eventData)` | pointerenter時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Scenes/Demo/Demo.unity, Assets/GUIPack-Clean&Minimalist/Demo/Scenes/UI Elements/UI Elements.unity, Assets/GUIPack-Clean&Minimalist/Demo/Scenes/Themes/Dark/Dark - Game.unity |
| 32 | `Tooltip.public virtual void OnPointerExit(PointerEventData eventData)` | pointerexit時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/GUIPack-Clean&Minimalist/Demo/Scenes/Demo/Demo.unity, Assets/GUIPack-Clean&Minimalist/Demo/Scenes/UI Elements/UI Elements.unity, Assets/GUIPack-Clean&Minimalist/Demo/Scenes/Themes/Dark/Dark - Game.unity |

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/Transition.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 20 | `Transition.private void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 30 | `Transition.public static void LoadLevel(string level, float duration, Color color)` | levelを読み込む | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 39 | `Transition.private void StartFade(string level, float duration, Color fadeColor)` | startfadeを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 46 | `Transition.private IEnumerator RunFade(string level, float duration, Color fadeColor)` | 不要オブジェクトを破棄する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/URLOpener.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 16 | `URLOpener.public void OpenURL()` | urlを開く | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/Utils.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `Utils.public static IEnumerator FadeIn(CanvasGroup group, float alpha, float duration)` | fadeinを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 29 | `Utils.public static IEnumerator FadeOut(CanvasGroup group, float alpha, float duration)` | fadeoutを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark01.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 31 | `Benchmark01.IEnumerator Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark01_UGUI.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 36 | `Benchmark01_UGUI.IEnumerator Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark02.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 18 | `Benchmark02.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark03.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 19 | `Benchmark03.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 25 | `Benchmark03.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/Benchmark04.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 22 | `Benchmark04.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/CameraController.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 53 | `CameraController.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity |
| 69 | `CameraController.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity |
| 80 | `CameraController.void LateUpdate()` | 描画直前の更新を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity |
| 126 | `CameraController.void GetPlayerInput()` | プレイヤーinputを取得する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/ChatController.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 14 | `ChatController.void OnEnable()` | 有効化時の購読や初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/20 - Input Field with Scrollbar.unity |
| 19 | `ChatController.void OnDisable()` | 無効化時の購読解除や後始末を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/20 - Input Field with Scrollbar.unity |
| 25 | `ChatController.void AddToChatOutput(string newText)` | tochatoutputを追加する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/20 - Input Field with Scrollbar.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/DropdownSample.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `DropdownSample.public void OnButtonClick()` | ボタンクリック時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/26 - Dropdown Placeholder Example.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/EnvMapAnimator.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 13 | `EnvMapAnimator.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 21 | `EnvMapAnimator.IEnumerator Start ()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/ObjectSpin.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 26 | `ObjectSpin.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/28 - HDRP Shader Example.unity |
| 38 | `ObjectSpin.void Update()` | 毎フレームの更新を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/28 - HDRP Shader Example.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/ShaderPropAnimator.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 18 | `ShaderPropAnimator.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 27 | `ShaderPropAnimator.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 32 | `ShaderPropAnimator.IEnumerator AnimateProperties()` | animatepropertiesを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/SimpleScript.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 18 | `SimpleScript.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/21 - Script Example.unity |
| 51 | `SimpleScript.void Update()` | 毎フレームの更新を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/21 - Script Example.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/SkewTextExample.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 19 | `SkewTextExample.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 25 | `SkewTextExample.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 31 | `SkewTextExample.private AnimationCurve CopyAnimationCurve(AnimationCurve curve)` | copyアニメーションcurveを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 46 | `SkewTextExample.IEnumerator WarpText()` | Method to curve text along a Unity animation curve. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_DigitValidator.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `TMP_DigitValidator.public override char Validate(ref string text, ref int pos, char ch)` | validateを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_ExampleScript_01.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 25 | `TMP_ExampleScript_01.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/22 - Basic Scripting Example.unity |
| 54 | `TMP_ExampleScript_01.void Update()` | 毎フレームの更新を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/22 - Basic Scripting Example.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_FrameRateCounter.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 27 | `TMP_FrameRateCounter.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity |
| 64 | `TMP_FrameRateCounter.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity |
| 70 | `TMP_FrameRateCounter.void Update()` | 毎フレームの更新を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity |
| 104 | `TMP_FrameRateCounter.void Set_FrameCounter_Position(FpsCounterAnchorPositions anchor_position)` | framecounterpositionを設定する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example URP.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/24 - Surface Shader Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/Benchmark (Floating Text).unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_PhoneNumberValidator.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 15 | `TMP_PhoneNumberValidator.public override char Validate(ref string text, ref int pos, char ch)` | validateを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextEventCheck.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 13 | `TMP_TextEventCheck.void OnEnable()` | 有効化時の購読や初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 29 | `TMP_TextEventCheck.void OnDisable()` | 無効化時の購読解除や後始末を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 42 | `TMP_TextEventCheck.void OnCharacterSelection(char c, int index)` | characterselection時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 47 | `TMP_TextEventCheck.void OnSpriteSelection(char c, int index)` | spriteselection時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 52 | `TMP_TextEventCheck.void OnWordSelection(string word, int firstCharacterIndex, int length)` | wordselection時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 57 | `TMP_TextEventCheck.void OnLineSelection(string lineText, int firstCharacterIndex, int length)` | 線selection時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 62 | `TMP_TextEventCheck.void OnLinkSelection(string linkID, string linkText, int linkIndex)` | linkselection時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextEventHandler.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 99 | `TMP_TextEventHandler.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 123 | `TMP_TextEventHandler.void LateUpdate()` | 描画直前の更新を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 220 | `TMP_TextEventHandler.public void OnPointerEnter(PointerEventData eventData)` | pointerenter時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 226 | `TMP_TextEventHandler.public void OnPointerExit(PointerEventData eventData)` | pointerexit時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 232 | `TMP_TextEventHandler.private void SendOnCharacterSelection(char character, int characterIndex)` | sendoncharacterselectionを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 238 | `TMP_TextEventHandler.private void SendOnSpriteSelection(char character, int characterIndex)` | sendonspriteselectionを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 244 | `TMP_TextEventHandler.private void SendOnWordSelection(string word, int charIndex, int length)` | sendonwordselectionを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 250 | `TMP_TextEventHandler.private void SendOnLineSelection(string line, int charIndex, int length)` | sendon線selectionを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 256 | `TMP_TextEventHandler.private void SendOnLinkSelection(string linkID, string linkText, int linkIndex)` | sendonlinkselectionを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextInfoDebugTool.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 35 | `TMP_TextInfoDebugTool.void OnDrawGizmos()` | drawgizmos時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 100 | `TMP_TextInfoDebugTool.void DrawCharactersBounds()` | Method to draw a rectangle around each character. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 264 | `TMP_TextInfoDebugTool.void DrawWordBounds()` | Method to draw rectangles around each word of the text. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 368 | `TMP_TextInfoDebugTool.void DrawLinkBounds()` | Draw rectangle around each of the links contained in the text. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 472 | `TMP_TextInfoDebugTool.void DrawLineBounds()` | Draw Rectangles around each lines of the text. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 552 | `TMP_TextInfoDebugTool.void DrawBounds()` | Draw Rectangle around the bounds of the text object. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 564 | `TMP_TextInfoDebugTool.void DrawTextBounds()` | drawtextboundsを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 576 | `TMP_TextInfoDebugTool.void DrawRectangle(Vector3 BL, Vector3 TR, Color color)` | drawrectangleを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 586 | `TMP_TextInfoDebugTool.void DrawDottedRectangle(Vector3 bottomLeft, Vector3 topRight, Color color, float size = 5.0f)` | drawdottedrectangleを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 595 | `TMP_TextInfoDebugTool.void DrawSolidRectangle(Vector3 bottomLeft, Vector3 topRight, Color color, float size = 5.0f)` | drawsolidrectangleを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 602 | `TMP_TextInfoDebugTool.void DrawSquare(Vector3 position, float size, Color color)` | drawsquareを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 616 | `TMP_TextInfoDebugTool.void DrawCrosshair(Vector3 position, float size, Color color)` | drawcrosshairを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 626 | `TMP_TextInfoDebugTool.void DrawRectangle(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br, Color color)` | drawrectangleを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |
| 638 | `TMP_TextInfoDebugTool.void DrawDottedRectangle(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br, Color color)` | drawdottedrectangleを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/08 - Improved Text Alignment.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity, Assets/TextMesh Pro/Examples & Extras/Scenes/12a - Text Interactions.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextSelector_A.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 20 | `TMP_TextSelector_A.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 30 | `TMP_TextSelector_A.void LateUpdate()` | 描画直前の更新を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 143 | `TMP_TextSelector_A.public void OnPointerEnter(PointerEventData eventData)` | pointerenter時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 150 | `TMP_TextSelector_A.public void OnPointerExit(PointerEventData eventData)` | pointerexit時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_TextSelector_B.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 37 | `TMP_TextSelector_B.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |
| 58 | `TMP_TextSelector_B.void OnEnable()` | 有効化時の購読や初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |
| 64 | `TMP_TextSelector_B.void OnDisable()` | 無効化時の購読解除や後始末を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |
| 71 | `TMP_TextSelector_B.void ON_TEXT_CHANGED(Object obj)` | changedを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |
| 81 | `TMP_TextSelector_B.void LateUpdate()` | 描画直前の更新を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |
| 293 | `TMP_TextSelector_B.public void OnPointerEnter(PointerEventData eventData)` | pointerenter時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |
| 300 | `TMP_TextSelector_B.public void OnPointerExit(PointerEventData eventData)` | pointerexit時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |
| 307 | `TMP_TextSelector_B.public void OnPointerClick(PointerEventData eventData)` | オブジェクトを生成する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |
| 450 | `TMP_TextSelector_B.public void OnPointerUp(PointerEventData eventData)` | pointerup時の処理を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |
| 456 | `TMP_TextSelector_B.void RestoreCachedVertexAttributes(int index)` | restorecachedvertexattributesを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/12 - Link Example.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMP_UiFrameRateCounter.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 26 | `TMP_UiFrameRateCounter.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/18 - ScrollRect & Masking & Layout.unity |
| 52 | `TMP_UiFrameRateCounter.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/18 - ScrollRect & Masking & Layout.unity |
| 59 | `TMP_UiFrameRateCounter.void Update()` | 毎フレームの更新を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/18 - ScrollRect & Masking & Layout.unity |
| 90 | `TMP_UiFrameRateCounter.void Set_FrameCounter_Position(FpsCounterAnchorPositions anchor_position)` | framecounterpositionを設定する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/18 - ScrollRect & Masking & Layout.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TMPro_InstructionOverlay.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 24 | `TMPro_InstructionOverlay.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 56 | `TMPro_InstructionOverlay.void Set_FrameCounter_Position(FpsCounterAnchorPositions anchor_position)` | framecounterpositionを設定する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TeleType.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 22 | `TeleType.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/15 - Inline Graphics & Sprites.unity |
| 47 | `TeleType.IEnumerator Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/15 - Inline Graphics & Sprites.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TextConsoleSimulator.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 12 | `TextConsoleSimulator.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/17 - Old Computer Terminal.unity |
| 18 | `TextConsoleSimulator.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/17 - Old Computer Terminal.unity |
| 25 | `TextConsoleSimulator.void OnEnable()` | 有効化時の購読や初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/17 - Old Computer Terminal.unity |
| 31 | `TextConsoleSimulator.void OnDisable()` | 無効化時の購読解除や後始末を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/17 - Old Computer Terminal.unity |
| 38 | `TextConsoleSimulator.void ON_TEXT_CHANGED(Object obj)` | changedを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/17 - Old Computer Terminal.unity |
| 48 | `TextConsoleSimulator.IEnumerator RevealCharacters(TMP_Text textComponent)` | Method revealing the text one character at a time. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/17 - Old Computer Terminal.unity |
| 84 | `TextConsoleSimulator.IEnumerator RevealWords(TMP_Text textComponent)` | Method revealing the text one word at a time. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/17 - Old Computer Terminal.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TextMeshProFloatingText.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 37 | `TextMeshProFloatingText.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 49 | `TextMeshProFloatingText.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 113 | `TextMeshProFloatingText.public IEnumerator DisplayTextMeshProFloatingText()` | displaytextmeshprofloatingtextを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 169 | `TextMeshProFloatingText.public IEnumerator DisplayTextMeshFloatingText()` | displaytextmeshfloatingtextを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/TextMeshSpawner.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 18 | `TextMeshSpawner.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 23 | `TextMeshSpawner.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexColorCycler.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 13 | `VertexColorCycler.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity |
| 19 | `VertexColorCycler.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity |
| 29 | `VertexColorCycler.IEnumerator AnimateVertexColors()` | Method to animate vertex colors of a TMP Text object. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexJitter.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 28 | `VertexJitter.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity |
| 33 | `VertexJitter.void OnEnable()` | 有効化時の購読や初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity |
| 39 | `VertexJitter.void OnDisable()` | 無効化時の購読解除や後始末を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity |
| 45 | `VertexJitter.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity |
| 51 | `VertexJitter.void ON_TEXT_CHANGED(Object obj)` | changedを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity |
| 61 | `VertexJitter.IEnumerator AnimateVertexColors()` | Method to animate vertex colors of a TMP Text object. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/23 - Animating Vertex Attributes.unity |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexShakeA.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 20 | `VertexShakeA.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 25 | `VertexShakeA.void OnEnable()` | 有効化時の購読や初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 31 | `VertexShakeA.void OnDisable()` | 無効化時の購読解除や後始末を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 37 | `VertexShakeA.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 43 | `VertexShakeA.void ON_TEXT_CHANGED(Object obj)` | changedを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 53 | `VertexShakeA.IEnumerator AnimateVertexColors()` | Method to animate vertex colors of a TMP Text object. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexShakeB.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 19 | `VertexShakeB.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 24 | `VertexShakeB.void OnEnable()` | 有効化時の購読や初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 30 | `VertexShakeB.void OnDisable()` | 無効化時の購読解除や後始末を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 36 | `VertexShakeB.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 42 | `VertexShakeB.void ON_TEXT_CHANGED(Object obj)` | changedを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 52 | `VertexShakeB.IEnumerator AnimateVertexColors()` | Method to animate vertex colors of a TMP Text object. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/VertexZoom.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 20 | `VertexZoom.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 25 | `VertexZoom.void OnEnable()` | 有効化時の購読や初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 31 | `VertexZoom.void OnDisable()` | 無効化時の購読解除や後始末を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 38 | `VertexZoom.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 44 | `VertexZoom.void ON_TEXT_CHANGED(Object obj)` | changedを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |
| 54 | `VertexZoom.IEnumerator AnimateVertexColors()` | Method to animate vertex colors of a TMP Text object. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 |

### `Assets/TextMesh Pro/Examples & Extras/Scripts/WarpTextExample.cs`

| Line | Method | Overview | Status | Reason |
| ---: | --- | --- | --- | --- |
| 18 | `WarpTextExample.void Awake()` | 参照取得とイベント登録を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/25 - Sunny Days Example.unity |
| 24 | `WarpTextExample.void Start()` | 起動直後の初期化を行う | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/25 - Sunny Days Example.unity |
| 30 | `WarpTextExample.private AnimationCurve CopyAnimationCurve(AnimationCurve curve)` | copyアニメーションcurveを処理する | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/25 - Sunny Days Example.unity |
| 45 | `WarpTextExample.IEnumerator WarpText()` | Method to curve text along a Unity animation curve. | 不要候補 | 外部サンプル/デモ配下で、本体コードの運用対象から外せる候補 / サンプル asset 参照: Assets/TextMesh Pro/Examples & Extras/Scenes/25 - Sunny Days Example.unity |

## Scripts Without Methods

### Main / Integrated

- `Assets/Layer Lab/2D Minimal-CharacterMaker/Scripts/Core/PartsCategory.cs`
- `Assets/Scripts/Core/Const/UIColors.cs`
- `Assets/Scripts/Core/IsExternalInit.cs`
- `Assets/Scripts/Data/DTO/AppearanceData.cs`
- `Assets/Scripts/Data/DTO/BattleStageData.cs`
- `Assets/Scripts/Data/DTO/EquipmentData.cs`
- `Assets/Scripts/Data/DTO/StageBattleData.cs`
- `Assets/Scripts/Data/DTO/StageData.cs`
- `Assets/Scripts/Data/MasterData/MasterDataDefinitions.cs`
- `Assets/Scripts/Data/MasterData/StageBattleEnemyMasterData.cs`
- `Assets/Scripts/Data/MasterData/StageBattlePatternMasterData.cs`
- `Assets/Scripts/Data/MasterData/StageMasterData.cs`
- `Assets/Scripts/Features/Battle/Logic/BattlePathTraceResult.cs`
- `Assets/Scripts/Features/Battle/Runtime/BattleCellState.cs`
- `Assets/Scripts/Features/Main/Screen.cs`
- `Assets/Scripts/Systems/Save/IGameSaveRepository.cs`
- `Assets/Scripts/Systems/Save/Models/AvatarAppearanceData.cs`
- `Assets/Scripts/Systems/Save/Models/BattleProgressData.cs`
- `Assets/Scripts/Systems/Save/Models/InventoryData.cs`
- `Assets/Scripts/Systems/Save/SaveFileNames.cs`
- `Assets/Scripts/Systems/Scene/SceneNames.cs`
- `Assets/Scripts/UI/Dialog/Avatar/AvatarEditorViewData.cs`
- `Assets/Scripts/UI/Dialog/Base/IDialogBackgroundHandler.cs`
- `Assets/Scripts/UI/UIConstants.cs`

### External Samples

- `Assets/GUIPack-Clean&Minimalist/Demo/Scripts/CleanButtonConfig.cs`
