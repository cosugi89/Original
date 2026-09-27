# MainScene 仕様書

- 最終更新: 2026-09-27
- 文書状態: 運用中
- 位置づけ: `MainScene` の画面構成、タブ遷移、ホーム導線、Character / Progress / Missions / Armoryの方針を定義する。
- 関連文書: `docs/specs/Ability仕様書.md`, `docs/specs/装備仕様書.md`, `docs/specs/ユーザーデータ構造.md`, `docs/specs/戦闘システム仕様書.md`

## 1. 基本方針

`MainScene` は、戦闘以外の主要導線を集約するホーム系シーンである。現時点では将来の `TitleScene` に置く起動処理も一時的に持つ。

- プレイヤーの現在状態を確認する。
- タブで主要機能へ移動する。
- ホームから戦闘へ遷移する。
- Status タブから装備変更、実戦用 Skill ロードアウト変更、見た目変更を行う。

戦闘中の一時状態やターン処理は `BattleScene` 側の責務とし、`MainScene` は永続データ、所持品、ロードアウト、見た目状態を編集する入口として扱う。
独立した `Assets/Scenes/Home.unity` は持たず、`Home` は `MainScene` 内のタブ画面として扱う。

## 2. 現行シーン構成

現行 `MainScene.unity` は、メインパネル内に 5 つの画面を持つ。

| タブ index | 画面 | 役割 |
| --- | --- | --- |
| 0 | `Character` | 装備変更、Deck、Appearance、重要所持品の入口を持つタブ。 |
| 1 | `Progress` | `Traits`、ボス由来能力の `Insights`、Resurgence時だけの `Remnant` を扱う。詳細は `docs/specs/Ability仕様書.md` を正とする。 |
| 2 | `Home` | 初期表示。戦闘開始導線を持つ。 |
| 3 | `Missions` | `Angel`が提示する任務一覧。解放済みかつ未クリアのMain / Event Missionを動的表示し、クリア済みは一覧から外す。カード選択による戦闘遷移は未接続。 |
| 4 | `Armory` | GoldでItemまたは装備を購入するカタログ。進行に応じて商品を増やし、解放済み商品はResurgence後も維持する。 |

初期表示は `Home` とする。

Missionカードを選択すると `Dialog-MissionDetail` を開く。DialogはMissionテーマ色、敵画像、初回クリア報酬を表示し、`Enter` 押下でそのMissionを選択して `BattleScene` へ遷移する。Main／Event Missionとも入場鍵や鍵表示は持たない。Itemマスタの `IsHiddenUntilMissionClear` が有効な初回報酬は、未クリア時には未知アイコンで表示する。

`StageKind.Debug` のステージは通常は一覧に表示しない。DebugSheet の `Stage` ページは `Visibility` と `Progress` に分かれる。`Visibility` では Debug ステージの表示をトグルで切り替える。`Progress` では StageId を入力して Clear を押すと、同じ千番台かつ入力値以下の全ステージをクリア済みにする。表示または進捗を変更すると、開いている Dungeon 一覧も再生成される。Debug 表示フラグはセッション限定で保存しない。進捗の上書きは通常のセーブ対象であり、クリア回数などの記録値は変更しない。DebugSheet の `Equipment` ページでは全 `EquipmentData` を所持Switchとして表示し、個別に所持状態を切り替えられる。`Reset Owned Equipment` は現在セーブの全所持装備を削除する。`IsDefaultOwned` の装備は次回ゲームデータ初期化時に再付与される。

Build Settings には `Assets/Scenes/MainScene.unity` と `Assets/Scenes/BattleScene.unity` を登録する。
`Assets/Scenes/SampleScene.unity` は使用しない。

## 3. MainScene の責務

`MainScene` は、`GameInitializer` によるゲーム全体の初期化が完了した後、シーン開始時に以下を行う。

- `playerRoot` 配下に既存の `AppearanceController` があればそれを Home 用プレビューとして使い、無ければ `playerPrefab` から生成する。
- Home 用プレビューの `AppearanceController.Init()` を呼び、アバター表示の土台を作る。
- `AppearanceAdapter` で保存済みのアバター状態を Home 用プレビューへ反映する。
- シーン内に非表示状態で置かれている `GameObject/Character` を Status 用プレビューとして扱い、保存済みアバター状態の反映先にする。
- Home 用プレビューと Status 用プレビューが同じ Root を共有している場合は、Status 用プレビューを `playerPrefab` から別 Root に生成して表示切り替えの衝突を避ける。
- `HomeScreen.Init()` を呼び、戦闘開始ボタンを初期化する。
- `StatusScreen.Init(...)` を呼び、Status タブの編集 UI を Status 用プレビューと Home 用プレビューへ接続する。

`MainScene.ChangeScreen(...)` は、タブ切り替え後に画面ごとのプレイヤー表示を切り替える。

- `Home` 選択時は Home 用プレビューを表示し、Status 用プレビューを非表示にする。
- `Status` 選択時は Status 用プレビューを表示し、Home 用プレビューを非表示にする。
- その他タブでは、現時点では両プレビューを非表示にする。

### 3.1 起動時のデータ初期化

現行の `MainScene` には空の `GameInitializer` GameObject を置く。ここには `GameInitializer` と `UserDataManager` をアタッチする。

- `UserDataManager` は `UserData` の実行時の正本であり、`DontDestroyOnLoad` によってシーン遷移後も保持する。永続データへの外部からの入口は `UserDataManager.Instance` に統一する。
- `UserDataManager` は `Save`、`Inventory`、`Avatar`、`StageProgress`、`BattleSkillLoadout` を所有する。各領域のロジックは分けたまま、静的な個別サービスとしては公開しない。
- `GameInitializer` は `UserDataManager.InitializeGameData()` を呼ぶ。ここでローカル JSON をロードまたは新規作成してから、所持品、見た目、ステージ進捗、Skill ロードアウトの管理を順に準備する。
- 初期所持品、初期見た目、初期解放ステージなどの補完で未保存変更が生じた場合、`GameInitializer` は初期化完了時に保存する。
- 未保存変更は `UserDataManager` がアプリのバックグラウンド移行時または終了時に保存する。明示的な即時保存を行う既存の UI 導線は維持する。
- 将来 `TitleScene` を追加するときは、この `GameInitializer` GameObject を `TitleScene` へ移し、Build Settings の先頭シーンを `TitleScene` に切り替える。初期化処理の内容は移動しない。

Home と Status の表示サイズ、位置、向きは、それぞれのシーン上の `Transform` で調整する。Dialog 内の表示サイズ、位置、向きは `AvatarDialog` の `previewLocalPosition` / `previewLocalScale` と `PreviewStage` 側で調整する。

## 4. MainScene の下部Tab仕様

下部の `Character`、`Progress`、`Home`、`Missions`、`Armory` Tabの切り替えは `MainScene` が担当する。`MainTabs` コンポーネントは使用しない。

- 各TabのNormal表示へ `PartsTabItem` を配置し、`MainScene` の `statusTab`、`abilityTab`、`homeTab`、`dungeonTab`、`shopTab` にInspector参照で設定する。
- 対応画面も `MainScene` の各 `*TabScreen` にInspector参照で設定する。
- 起動時は `Home` を初期選択として扱う。
- タブ押下時は、選択画面だけを active にし、他の画面を inactive にする。
- 選択TabだけをFocused表示にし、ほかのTabはNormal表示にする。
- HomeとStatusの選択時は、対応するアバタープレビューだけを表示する。

タブの `Button.onClick` へ個別に永続イベントを設定するのではなく、`MainScene` が起動時に動的 listener を登録する。

### 4.1 ProgressScene の仕様

`Progress` 画面には `AbilityScene` を配置し、内部の `Traits`、`Insights`、Resurgence時だけ表示する `Remnant` を切り替える。

- `traitTab` / `insightsTab` / `remnantTab` と対応ScreenをInspectorで設定する。`remnantTab` はResurgence以外では非表示とする。
- Traits画面は `TraitTabScreen` を `traitTabScreen` として `AbilityScene` から直接参照する。
- 初期選択は `Traits` とする。
- Ability内Tabのlistenerも `AbilityScene` が起動時に登録し、Buttonの永続イベントは使わない。
- `AbilityScene` の責務はTabと画面のactive切り替えだけとし、Tabの見た目、Tree表示、Point表示、購入処理は持たない。

## 5. Home 画面

`Home` は `HomeScreen` が担当する。

- `battleButton` 押下で `BattleScene` へ遷移する。
- `useDebugBattleStageId` が true の場合は `debugBattleStageId` を使う。
- false の場合は `ProgressManager.GetRecommendedStageId()` から推奨ステージを取得する。
- 遷移前に `BattleSceneRequest.SelectedStageId` を設定する。

Mission の初回クリア報酬は各 `StageMasterData.ClearRewards` で定義する。報酬は `Item`（`ItemStacks` へ数量を加算）または `Equipment`（所持装備として付与）とし、同じMissionで二重に付与しない。個別の報酬内容は各 Stage マスターで設定する。

Armory は `ArmoryProductDatabase` の商品一覧を正本とする。各 `ArmoryProductMasterData` は販売対象として `EquipmentMasterData` または `ItemMasterData` を直接参照し、数量、Gold価格、解放に必要なクリア済みMission、表示順、購入上限を持つ。Equipment商品に登録できるのは、Helmet、Chest、右手、左手の戦闘4部位だけとし、Hair／Eye／Beard／Skinなどの見た目専用パーツは登録不可とする。購入回数は `UserData.Armory` に保存し、Resurgence後も維持する。一覧は Item と Equipment で親・Prefab・View を分ける。Item は並び順を維持し、名称・付与数・必要に応じて残り購入回数を表示する。Equipment は購入可能品を先、売切・所持済み品を末尾に並べ、名称と短いタグ文を表示する。未解放品は非表示、売切品は Disabled とする。購入時は、Item は `Dialog-Common-ConfilmWithIcon`、Equipment は `Dialog-EquipmentDetail` を購入モードで開く。どちらもGold不足、売切、または既に所持している装備は確定操作をDisabledにする。確定時にGold消費、Item加算または装備付与、購入回数保存を同一処理で行う。

## 6. Character タブ

Character タブは、プレイヤーの装備変更、実戦用 Skill ロードアウト変更、見た目変更、重要所持品の確認を行うタブである。
装備変更はタブ内で即時選択し、髪、肌、目、ひげなどの見た目変更は `Button_Skin` から開くダイアログで行う。

### 6.1 UI 構成方針

- `StatusScreen` は画面全体の状態遷移とデータ更新を担当し、タブ、装備一覧、装備枠、Skill 枠は各 View に分離する。
- 各 View の必須 UI 参照とカテゴリ、スロット番号は Inspector で明示的に設定する。名前検索、子階層走査、実行時の既定 UI 生成による補完は行わない。
- 装備一覧はカテゴリごとのテンプレート行を元に、候補数に応じて行を生成する。ScrollRect の Content は全候補を収め、固定20件で表示を打ち切らない。`All` 以外のカテゴリでは先頭に `Empty` 状態の解除行を置き、押下するとそのカテゴリの装備を即時に外す。
- 装備一覧の `ListItem_Template` ルートは Raycast を受ける透明な `Image` を持つ。候補行のクリックはこのルートの `StatusEquipmentListItemView` へ届け、装備詳細ダイアログを開く。
- 装備カテゴリTabのNormal表示とFocus表示は同じカテゴリSpriteを使う。選択状態は枠や強調だけを変え、HeadgearとArmorなどのカテゴリSpriteを入れ替えない。
- `Parts-EquipmentItem` は、装備枠と一覧行で共有する見た目専用Prefabとする。Prefabに付く `PartsEquipmentItem` は画像、テキスト、状態、種別アイコン、枠色だけを管理する。`Empty` ではNormal用の画像、名称、SpecialBorder、種別アイコンと親の `TypeArea` を明示的に非表示にし、Empty状態の表示だけを残す。
- `PartsEquipmentItem`、`PartsSkillItem`、`SkillFrame` は `IItemStateView` を実装し、外部へは `ShowNormal` / `ShowDisabled` / `ShowEmpty` / `ShowLocked` の4状態だけを公開する。旧 `Add1` は `Empty` に統合し、`Add2` は状態APIとして扱わない。状態enumは各Viewのprivate実装とする。
- `StatusEquipmentSlotView` はMainScene上の各装備スロットに付く中間View、`StatusEquipmentListItemView` は各 ScrollRect の Content に置く非表示の `ListItem_Template` に付く中間Viewとする。実行時はこのテンプレートを複製して候補数分の一覧行を生成する。両Viewは装備データを `PartsEquipmentItem` の公開APIへ渡し、用途固有のクリック操作だけを扱う。
- `Parts-SkillItem` は Skill 枠の見た目専用Prefabとする。Prefabに付く `PartsSkillItem` は、Skillアイコン、名前、`Icon-Attack` / `Icon-Shield` / `Icon-Armor` / `Icon-HeadGear` の種別アイコンObject、Normal / Locked / Disabled / Empty の表示状態、Focusだけを管理する。種別表示はSpriteの差替えではなく、該当するPrefab内Objectの有効状態を切り替える。`Empty` と種別なしSkillでは種別アイコンの親 `TypeArea` ごと非表示にする。保存、候補選択、バトルの Runtime は扱わない。
- `StatusSkillSlotView` はMainScene上の各 Skill スロットに付く中間Viewとし、ロードアウトデータを `PartsSkillItem` の公開APIへ渡し、Skill設定ダイアログを開くクリックだけを扱う。
- 装備枠と Skill 枠の通常フレームは各スロットの Prefab/Scene構成として常時配置する。表示データの有無でフレーム自体を生成、複製しない。
- Status 用アバタープレビューの表示、RenderTexture 接続、見た目反映、ダイアログ中の一時非表示は `StatusAvatarPreviewView` が担当する。`StatusScreen` はこの View を通じてプレビューを操作する。

### 6.2 役割

- 現在のアバターを確認する。
- 所持装備から装備を変更する。
- 装備変更による見た目反映を即時確認する。
- 実戦用 Skill ロードアウト4枠の現在設定を確認し、設定ダイアログから変更する。
- `Button_Skin` から見た目変更ダイアログを開く。
- 装備変更により使えなくなった実戦用 Skill ロードアウトを自動で外す。
- 最終 `HP / Attack / Defense`、候補Skill、タグ一致ボーナスへの影響を確認する。
- `DeckLoadoutDialog` では、現在選択中Deckで成立したタグ、一致部位数、各タグが加えた `HP / Attack / Defense` を表示する。Deck切替と装備変更のプレビュー中にも即時更新する。
- 装備選択Dialogでは、現在装備中の候補で成立しているタグを点灯し、未装備候補では装備した場合に成立するタグを明滅して予告する。

装備データは、現行方針どおり戦闘用データと見た目用データを兼用する。詳細は `docs/specs/装備仕様書.md` を正とする。

### 6.3 扱うデータ

Status タブは以下のデータを参照、更新する。

- 所持装備: `UserData.Inventory.Equipments`
- 現在の見た目状態: `UserData.Profile.AvatarAppearance`
- 装備マスター: `EquipmentMasterData`
- 実行時装備データ: `EquipmentData`
- 実戦用の戦闘構成: `UserData.Profile.Decks` と `CurrentDeckId`
- 表示適用: `AppearanceController` と `AppearanceAdapter`

未装備は現行方針どおり `EquipmentId = 0` として扱う。

### 6.4 装備変更

現行実装では、以下の枠を Status タブで扱う。

- 頭装備
- 胴装備
- 右手装備
- 左手装備

`Items` は装備枠ではなく、装備できない重要所持品を表示する独立したDialogまたはページとして Character タブから開く。装備一覧カテゴリには含めない。

Character タブ内の装備カテゴリは以下のとおりである。

Tabの表示順は `All`、`Weapon`、`Shield`、`Headgear`、`Armor` とする。

| カテゴリ | 対応する `UICategory` | 備考 |
| --- | --- | --- |
| `All` | `HandRight`, `HandLeft`, `Chest`, `Helmet` | 装備変更対象をまとめて表示する。 |
| `Weapon` | `HandRight` | 右手装備を表示する。 |
| `Shield` | `HandLeft` | 左手装備を表示する。 |
| `Armor` | `Chest` | 胴装備を表示する。 |
| `Headgear` | `Helmet` | 頭装備を表示する。 |

装備一覧の項目、または装備済みのSlotEquipmentをクリックすると装備詳細ダイアログを開く。SlotEquipment起点では `ButtonArea` ごと装備操作を非表示にして詳細表示だけを行う。装備一覧起点では `Button_Equip` を表示し、選択可能かつ未装備の項目を装備できる。
装備一覧での選択と解除は、所持済みかつ解放済みの装備だけを対象にする。
カテゴリ別一覧の解除行は確認ダイアログを開かず、対象カテゴリを未装備にして保存する。装備解除で使用不能になったSkillは、装備変更時と同様に実戦用ロードアウトから外す。

`Items` は装備一覧とは別の専用一覧であり、解除行、`Equip` ボタン、装備スロットは持たない。`InventoryData.ItemStacks` のうち `ItemMasterData.IsImportantItem` が有効な Item を、表示名・説明・アイコンを持つマスターから解決して表示する。各ItemMasterDataは、所持数が0でも一覧に表示するかを `IsAlwaysVisibleInInventory`、所持数をUIに表示するかを `IsQuantityVisibleInInventory` で定義する。Gold と Ability Point は両方を有効にし、所持数0でも `0` を表示する。`The Remnant` は所持数が1以上のときだけ表示し、所持数は表示しない。`Dialog-ItemInventory` は `ItemMasterDatabase` をPrefab参照で受け取り、所持数が1以上の表示対象Itemと `IsAlwaysVisibleInInventory` が有効なItemを `Parts-InventoryItem` として先頭から並べる。表示枠は常に最大15枠とし、残りはアイコン・数量を持たず操作できない Disabled 状態で表示する。Content の `ScrollRect` は縦スクロールを無効化する。表示はダイアログを開いた時点の所持状態を使い、開いている間の所持数変更をリアルタイム更新しない。選択したItemの名前と説明を表示する。
装備スロットと装備一覧の各行は、背景フレーム、アイコン領域、名称を常に表示し、装備データやプール再利用状態によって表示有無が揺れないようにする。
装備アイコンが未設定の場合はスロットまたは一覧テンプレートの代替アイコン、表示名が空の場合は `EquipmentId` 由来の表示名にフォールバックする。
戦闘装備は `CurrentDeckId` の `DeckData` に反映し、髪・目・ひげ・色などの見た目専用状態は `UserData.Profile.AvatarAppearance` に保存する。変更後は `MainScene` 上のプレイヤープレビューと、非表示の `GameObject/Character` にも即時反映する。
同じ `UICategory` 内で複数の `PartsType` を持つ装備枠では、選択した装備以外の同カテゴリ内パーツを自動で未装備にする。
例として、右手装備で `Sword` を選択した場合、`Axe`、`Bow`、`Wand`、`Staff`、`Spear`、`Blunt`、`Crossbow` など同じ右手カテゴリのパーツは外れる。

### 6.5 Skill ロードアウト

Status タブの `SlotSkill_01` 〜 `SlotSkill_04` は `StatusSkillSlotView` が担当し、`DeckManager.EnsureUsableLoadout(...)` で得た現在デッキの実戦用 Skill を表示する。

各 `StatusSkillSlotView` は `PartsSkillItem` を Inspector 参照し、表示状態の直接切替や画像・テキストへの直接代入は行わない。

- スロット数は `UserProfileData.BattleSkillLoadoutSlotCount` の 4 枠を正とする。
- スロットに Skill が設定されている場合は、候補 Skill の `DisplayName` と `Icon` を表示する。
- Skill の `Icon` は背景つきの正方画像でもスロット背景フレームを隠さないよう、枠内側に余白を取って表示する。
- Skill枠の種別アイコンは、右手、左手、胴、兜の4種を用いる。各Skillは、そのSkillを提供している現在装備中の枠に対応する種別アイコンを表示する。
- 初回表示ではCanvasレイアウト確定後にアイコンサイズを計算する。保存や再選択などの後続操作を行わなくても、すべてのSlotSkillでIconを表示する。
- 未設定または現在の装備から使用できない Skill は `Empty` として表示する。
- スロットを押下すると `SkillLoadoutDialog` を開き、押下したスロットを選択状態にする。
- ダイアログ内では、`SelectedSkill` の4個の `PartsSkillItem` に現在のドラフト上のSkillロードアウトをアイコンのみで表示する。編集対象スロットはダイアログを開いたStatus側のSkill枠で決まり、`SelectedSkill` は操作を持たない。`PartsSkillItem` 内部の名前テキストと種別アイコンは常に非表示にする。
- 候補 Skill は、現在デッキの `EquipmentData.AssignableSkills` から `DeckManager.GetCandidateSkillsFromEquippedEquipment()` で取得する。対象は右手、左手、胴、兜の4装備枠に限定し、Hair、Eye、Beard、Skin などの見た目専用パーツは候補元に含めない。
- 候補 Skill は `SkillInfoArea` のスクロール領域に、`Dialog-SkillLoadout.prefab` の `Content` に置いた非表示の `SkillLoadoutCandidateItemView` テンプレートを複製して表示する。外部の候補行Prefabは参照しない。候補数を8件に固定せず、装備枠や候補数の拡張に追従できるようにする。
- 候補行のアイコン枠は `PartsSkillItem` を使う。候補行では `ShowSkillIconOnly(icon)` を使い、`PartsSkillItem` 内部のSkill名テキストと種別アイコンは常に非表示にする。名前と説明は行側のテキストで表示し、現在選択中スロットへ割り当てられている候補だけ選択Frameを表示する。
- 候補 Skill の選択によるロードアウト変更時に、ダイアログ内のドラフトと `SelectedSkill` 表示を即時に切り替え、同時に `DeckManager.SaveLoadout(...)` で現在デッキへ保存する。SaveボタンとClearボタンは使わない。
- Skill ロードアウトは現在デッキの `SkillIds` に反映し、他のユーザーデータと同じメインセーブJSONへ保存する。
- `Close` または背景クリックで閉じた場合も、変更済みの Skill は戻さない。未変更で閉じた場合はStatus表示の再描画を行わない。

### 6.6 見た目変更ダイアログ

見た目変更は戦闘デッキと独立して扱い、髪・目・ひげ・肌の見た目パーツと色だけを変更する。
Status タブ上の `Button_Skin` を押下すると `AvatarDialog` を開き、Dialog 内の `PreviewStage` に一時生成した `Character` で編集中の見た目を確認する。

- パーツ変更: `Hair`、`Eye`、`Beard`、`Skin` のみを `AvatarPartStateData` に `PartType`、`EquipmentId`、`Visible` として保存する。
- 色変更: `AvatarColorData` に `ColorTargetType` と色を保存する。

色変更対象は現行の見た目仕様に合わせ、少なくとも以下を候補にする。

- `Skin`
- `Hair`
- `Eye`
- `Beard`

`Helmet`、`Chest`、`HandRight`、`HandLeft` は現在デッキの戦闘装備であり、このダイアログからは表示・変更・保存しない。`Arrow` と `HelmetHair` は派生パーツとして扱い、Status タブから直接保存対象にしない。表示状態は `AppearanceController` の同期処理に任せる。

`Assets/Resources/Prefabs/Dialog-AvatarController 1.prefab` は、カテゴリ枠と候補枠を各1件の非表示テンプレートとして持ち、外見カテゴリ・所持候補の件数ぶん動的に生成する。`ColorPicker`、Save、CloseはPrefab内の既存要素を使う。Dialog 内での変更は編集中プレビューへ反映し、保存ボタン押下時に `AvatarEditorPresenter.Commit()` で `AppearanceManager` へ反映する。外見専用モードでは `InventoryManager.MarkEquipped(...)`、`DeckManager`、戦闘装備には触れない。

候補がないカテゴリでは、Prefab参照の `emptyText` に `No options available` を表示する。外見専用モードの保存は `AppearanceManager.SetCosmeticAppearance(...)` を使い、現在デッキの装備・Skill・選択中デッキを変更しない。
保存後は `AppearanceAdapter.ApplyTo(...)` で Home 用プレビューと Status 用プレビューへ反映し、`UserDataJsonStore.SaveSession()` で保存する。
閉じるボタンまたは背景クリックで閉じた場合は保存しない。
Dialog 表示中は、Status タブ本体側のプレビューを一時的に非表示にし、Dialog 内プレビューだけを表示する。

Dialog 表示は `DialogService` / `DialogHost` / `DialogBase` を使う。`DialogHost` は必要になった時点で最前面用の `DialogCanvas` を動的生成し、その中に `Backdrop` と `DialogParent` を置く。`StatusScreen` などの呼び出し側は prefab 参照や parent 指定を持たず、`DialogService.OpenAsync<...>()` でダイアログを開く。
黒背景は `DialogHost` が管理し、開いているダイアログがひとつ以上背景表示を要求している場合に 1 枚だけ表示する。背景クリック可否や背景クリックで閉じるかどうかは `DialogBase.SetBackdrop(...)`、`SetBackdropVisible(...)`、`SetBackdropInteractable(...)`、`SetBackdropCloseEnabled(...)` でダイアログ単位に指定する。
見た目は `Assets/Layer Lab/GUI Pro-MinimalGame/Theme_Dark/Prefabs/Prefabs~DemoScenes/Settings.prefab` の中央 Popup と暗い背景を参考にしつつ、実体 Prefab / Script は `Layer Lab` 配下には作らない。
ダイアログ prefab は Addressables で解決する。装備詳細ダイアログは `Assets/Resources/Prefabs/Dialog/Dialog-EquipmentDetail.prefab` を正本 Prefab とし、Addressables address は `Dialog/EquipmentDetail`、`EquipmentDetailDialog` には `[DialogAddress("Dialog/EquipmentDetail")]` を付与する。Item購入確認は `Assets/Resources/Prefabs/Dialog/Dialog-Common-ConfilmWithIcon.prefab` を正本 Prefab とし、Addressables address は `Dialog/Common/ConfilmWithIcon`、`DialogCommonConfirmWithIcon` には同じ `[DialogAddress]` を付与する。Skill 設定ダイアログは `Assets/Resources/Prefabs/Dialog-SkillLoadout.prefab` を正本 Prefab とし、Addressables address は `Dialog/SkillLoadout`、`SkillLoadoutDialog` には `[DialogAddress("Dialog/SkillLoadout")]` を付与する。見た目変更ダイアログは `Assets/Resources/Prefabs/Dialog-AvatarController 1.prefab` を正本 Prefab とし、Addressables address は `Dialog/AvatarController`、`AvatarDialog` には `[DialogAddress("Dialog/AvatarController")]` を付与する。Prefab 内の必須 UI 参照が未設定の場合はセットアップ不備として使用箇所で失敗させ、個別の null ガードによる回避、ダイアログ本体 UI の実行時動的生成、`Resources.Load<T>()` による代替探索は行わない。

`Dialog-EquipmentDetail` は `Parts-EquipmentItem` を装備表示に使用し、装備種別と `CategoryTags` をタグとして複数表示する。説明文は `EquipmentData.Description` を本文へ表示する。レアリティは `EquipmentData.Rarity` をラベルへ反映し、`Label_Tapered_01_Plum` の `Bg` / `InnerBorder` / `Deco` には対応する `UIColors.IColorSet` の `Background` / `InnerColor1` / `Glow` を適用する。スキル、戦闘補正、タグはそれぞれPrefab内の単一テンプレートを複製して表示し、`Scroll View/Viewport/Content` の `VerticalLayoutGroup` と `ContentSizeFitter` で表示件数に応じた高さにする。`Button_Close` は `GameButton` としてダイアログをキャンセルして閉じる。`Button_Equip` を含む `ButtonArea` はSlotEquipment起点だけ非表示にし、装備一覧起点では表示する。Armoryから開いた購入モードでは同じ `ButtonArea` を表示し、`Button_Equip` のラベルをGold価格へ切り替えて購入確定に使う。

装備詳細のスキル行は `PartsSkillItem` をアイコン枠として使う。通常は `NormalState` のみを表示し、`TypeArea`、Prefab内ラベル、Focusは常に隠す。スキルの解放状態を導入した後は、未解放行だけ `LockState` にする。

表示文言は英語のキー文字列を正本とし、表示直前に `TextLocalization.Get(key)` で解決する。現段階ではキーをそのまま返すが、日本語対応時は同APIの内部で選択言語の文字列へ変換する。Inspector固定文言のためのTMP継承は行わず、動的表示箇所からこの解決APIを呼ぶ。
言語別の翻訳値は `LocalizationMasterData` で保持し、`Assets/Resources/MasterData/Localization/` 配下に言語ごとのassetを置く。現段階では `Japanese.asset` に試験用の1件だけを登録し、`TextLocalization` からの読み込みは次段階で接続する。

### 6.7 保存タイミングと Skill ロードアウト

現行実装では、装備一覧の項目を押して詳細ダイアログを開き、`Equip` ボタンで装備変更を確定する。SlotEquipmentから開いた詳細ダイアログは確認専用であり、装備操作を表示しない。

- 装備リストの項目を選択、または解除行を押す。
- 詳細ダイアログで `Equip` ボタンを押す。
- `AvatarEditorPresenter.TrySelectEquipment(...)` で選択状態を更新する。
- `AvatarEditorPresenter.Commit()` で `AppearanceManager` へ反映する。
- `AppearanceAdapter.ApplyTo(...)` で Home 用プレビューと Status 用プレビューを更新する。
- `DeckManager.EnsureUsableLoadout(persistImmediately: true)` で、装備変更後に使えなくなった Skill を現在デッキから自動で外す。
- `UserDataJsonStore.SaveSession()` で即時保存する。

Skill ロードアウトは、Skill 設定ダイアログ内で候補 Skill を選択した時点で自動保存する。

- `SlotSkill_01` 〜 `SlotSkill_04` のいずれかを押し、Skill 設定ダイアログを開く。
- ダイアログ内で対象スロットと候補 Skill を選ぶ。
- 候補 Skill 選択または Clear 実行時に `DeckManager.SaveLoadout(...)` を呼び、現在デッキの `SkillIds` を更新する。
- 保存後は Status タブの Skill スロット表示を更新する。
- Unity Editor の Domain Reload 無効環境でも再生開始時に保存済み JSON を読み直せるよう、保存データに依存する static singleton は `RuntimeInitializeLoadType.SubsystemRegistration` でリセットする。

## 7. 未確定事項

- Missionsカードの選択時の未解放制御と戦闘遷移。
- Character タブの最終 UI レイアウトと4つ目の入口の要否。
- 見た目変更ダイアログの最終レイアウトと、`Settings.prefab` を基準にした細部調整。
- 装備変更による戦闘ステータス、候補スキル、タグ一致ボーナスのプレビュー表示方針。

## 8. 次期実装: 戦闘デッキ

戦闘用の右手・左手・兜・胴と、戦闘Skill4枠はデッキ3件として保存・切替する。見た目用の髪・目・ひげ・色は `AvatarAppearance` に分離し、デッキには含めない。StatusSceneのデッキ一覧ダイアログと、敗戦後の再挑戦導線からデッキを切り替えられるようにする。詳細は `docs/specs/デッキ仕様書.md` を正とする。
