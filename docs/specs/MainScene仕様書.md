# MainScene 仕様書

- 最終更新: 2026-10-13
- 文書状態: 運用中
- 位置づけ: `MainScene` の画面構成、タブ遷移、ホーム導線、Character / Progress / Missions / Armoryの方針を定義する。
- 関連文書: `docs/specs/Ability仕様書.md`, `docs/specs/装備仕様書.md`, `docs/specs/ユーザーデータ構造.md`, `docs/specs/戦闘システム仕様書.md`

## 1. 基本方針

`MainScene` は、戦闘以外の主要導線を集約するホーム系シーンである。起動時のデータ初期化は `TitleScene` が担当する。

- プレイヤーの現在状態を確認する。
- タブで主要機能へ移動する。
- ホームから戦闘へ遷移する。
- Character タブから装備変更、実戦用 Skill ロードアウト変更、見た目変更を行う。

戦闘中の一時状態やターン処理は `BattleScene` 側の責務とし、`MainScene` は永続データ、所持品、ロードアウト、見た目状態を編集する入口として扱う。
独立した `Assets/Scenes/Home.unity` は持たず、`Home` は `MainScene` 内のタブ画面として扱う。

TitleSceneから入場する時だけは、`MainScene` がHome表示物の初期状態を決め、`HomeEntranceDirector.StartIfRequested(...)` に入場演出の開始を依頼する。DirectorはHomeの基底背景を有効化してTitle背景の解除を通知した後、「土台／階段 → 左右の石材 → アーチ → 祭壇の葉 → 書見台／戦果碑 → 小道具の葉」の順で組み上げる。石材・小道具は上から落下しながらフェードインし、葉は根元付近のTransformを動かさず、小さなscale変化とalphaだけで表示する。Loading Overlayは構築完了まで維持する。AngelなどHome固有の表示物は `MainScene` が所有し、Directorから構築完了callbackを受けて表示する。

演出に使った石材・葉・書見台・戦果碑のGameObjectは完成後も表示を継続する。旧祭壇Imageと旧小道具Buttonは作成時の位置・サイズを参照するためSceneへ残し、通常のMainScene起動時も非表示にする。書見台・戦果碑のGameObject自身にButtonを持たせ、`HomeScreen` のSettings／Achievements参照をそのButtonへ接続する。構築中は操作を遮断し、HUDフェード完了後に有効にする。小道具の葉も葉単位のSpriteとし、本体の着地後に根元固定のalpha・小さなscale変化で生やす。

構築要求がない通常のMainScene表示、BattleSceneからの復帰、Tab切替では同じ個別SpriteのGameObjectを完成位置・完成alpha・完成scaleで即時表示する。Titleから渡す要求とLoading Overlayの引き継ぎは `HomeEntryRequest`、個々のパーツの完成位置・構築段階・移動・alpha表示は `HomeBuildPart` を使い、必須参照はInspectorで配線する。素材構成・時間・最終表示の検証方針は `docs/specs/TitleScene仕様書.md` を正とする。

Home構築パーツの `HomeBuildRoot` はHome直下のfirst siblingにあり、PanelContentの外でAngel・STARTより下に描画する。HUDは `PanelBottom` と、追加参照する `ResourceBar_Group`、`MainTabLayout` のCanvasGroupで同時に非表示・フェード・入力切替する。PreviewImageを含むPanelContentはalpha=1を維持してHUDから分離する。完成後も `HomeBuildRoot` の個別Spriteを表示する。Scene RootやPreviewImageの親は変更しない。

Homeの入場順序は「祭壇と小道具の構築 → Loading表示が完全に消える → Angel登場 → HomeReadyで開始できるチュートリアルをすべて再生 → HUD表示」とする。現在画面で開始できる後続案内も待ち、HUDフェード完了後に操作を有効化する。チュートリアルがない場合も通知の照合後そのままHUDを表示する。直接MainScene起動とBattleからの復帰では構築・Loadingを省き、Angelから先の順序を共通にする。詳細はチュートリアル仕様書を参照する。

## 2. 現行シーン構成

現行 `MainScene.unity` は、メインパネル内に 5 つの画面を持つ。

| タブ index | 画面 | 役割 |
| --- | --- | --- |
| 0 | `Character` | 装備変更、Deck、Appearance、重要所持品の入口を持つタブ。 |
| 1 | `Progress` | `Traits`、ボス由来能力の `Insights`、Resurgence時だけの `Remnant` を扱う。詳細は `docs/specs/Ability仕様書.md` を正とする。 |
| 2 | `Home` | 初期表示。戦闘開始導線を持つ。 |
| 3 | `Missions` | `Angel`が提示する任務一覧。解放済みかつ未クリアのMain / Event Missionを動的表示し、クリア済みは一覧から外す。カード選択で詳細Dialogを開く。 |
| 4 | `Armory` | GoldでItemまたは装備を購入するカタログ。進行に応じて商品を増やし、解放済み商品はResurgence後も維持する。 |

初期表示は `Home` とする。

Missionカードを選択すると `Dialog-MissionDetail` を開く。DialogはMissionテーマ色、敵画像、初回クリア報酬を表示し、開始ボタンまたは `Enter` / テンキー `Enter` 押下でそのMissionを選択して `BattleScene` へ遷移する。開始操作は連続入力されても一度だけ受け付ける。Main／Event Missionとも入場鍵や鍵表示は持たない。Itemマスタの `IsHiddenUntilMissionClear` が有効な初回報酬は、未クリア時には未知アイコンで表示する。

`StageKind.Debug` のステージは通常は一覧に表示しない。DebugSheet の `Stage` ページは `Visibility` と `Progress` に分かれる。`Visibility` では Debug ステージの表示をトグルで切り替える。`Progress` では StageId を入力して Clear を押すと、同じ千番台かつ入力値以下の全ステージをクリア済みにする。表示または進捗を変更すると、開いている Missions 一覧も再生成される。Debug 表示フラグはセッション限定で保存しない。進捗の上書きは通常のセーブ対象であり、クリア回数などの記録値は変更しない。DebugSheet の `Equipment` ページでは全 `EquipmentData` を所持Switchとして表示し、個別に所持状態を切り替えられ、`Reset Owned Equipment` は現在セーブの全所持装備を削除する。`IsDefaultOwned` の装備は次回ゲームデータ初期化時に再付与される。

Build Settings には `Assets/Scenes/MainScene.unity` と `Assets/Scenes/BattleScene.unity` を登録する。
`Assets/Scenes/SampleScene.unity` は使用しない。

## 3. MainScene の責務

`MainScene` は、`GameInitializer` によるゲーム全体の初期化が完了した後、シーン開始時に以下を行う。

- `playerRoot` 配下に既存の `AppearanceController` があればそのプレビュー用に使い、無ければ `playerPrefab` から生成する。
- プレビューの `AppearanceController.Init()` を呼び、保存済みのアバター状態を反映する。
- シーン内に非表示状態で置かれている `GameObject/Character` を Character 用プレビューとして扱い、保存済みアバター状態の反映先にする。
- 表示用の `AngelHome` Prefab instance を `MainScene` の `angelHome` Inspector参照へ明示的に設定し、`Avatar Preview` layer で PreviewCamera に描画する。
- プレビューと Character 用プレビューが同じ Root を共有している場合は、Character 用プレビューを `playerPrefab` から別 Root に生成して表示切り替えの衝突を避ける。
- `HomeScreen.Init()` を呼び、戦闘開始ボタンを初期化する。
- `CharacterScreen.Init(...)` を呼び、Character タブの編集 UI を Character 用プレビューと Home 用プレビューへ接続する。

`MainScene` のタブ切り替え処理は、画面ごとのプレイヤー表示も切り替える。

- `Home` 選択時は `AngelHome` を表示し、Character プレビューを非表示にする。ただしTitleSceneからのHome入場演出中だけは、構築完了までAngelを非表示にする。
- `Character` 選択時は Character 用プレビューを表示し、`AngelHome` を非表示にする。
- その他タブでは、現時点では `AngelHome` と Character プレビューを非表示にする。

### 3.1 起動時のデータ初期化

`GameInitializer` と `UserDataManager` は `TitleScene` 内の別々のRoot GameObjectに置く。`UserDataManager` だけを永続化し、`MainScene` は初期化済みの `UserDataManager` を利用する。

- `UserDataManager` は `UserData` の実行時の正本であり、`DontDestroyOnLoad` によってシーン遷移後も保持する。永続データへの外部からの入口は `UserDataManager.Instance` に統一する。
- `UserDataManager` は `Save`、`Inventory`、`Avatar`、`StageProgress`、`BattleSkillLoadout` を所有する。各領域のロジックは分けたまま、静的な個別サービスとしては公開しない。
- `GameInitializer` は `UserDataManager.InitializeGameData()` を呼ぶ。ここでローカル JSON をロードまたは新規作成してから、所持品、見た目、ステージ進捗、Skill ロードアウトの管理を順に準備する。
- 初期所持品、初期見た目、初期解放ステージなどの補完で未保存変更が生じた場合、`GameInitializer` は初期化完了時に保存する。
- 未保存変更は `UserDataManager` がアプリのバックグラウンド移行時または終了時に保存する。明示的な即時保存を行う既存の UI 導線は維持する。
- 現行Build Settingsの先頭シーンは `MainScene`。`TitleScene` を先頭へ登録する作業は未完了であり、EditorでのTitle起点Play確認は専用の `Title` Buttonから行う。初期化処理の内容は変更しない。

Home と Character の表示サイズ、位置、向きは、それぞれのシーン上の `Transform` で調整する。Dialog 内の表示サイズ、位置、向きは `AvatarDialog` の `previewLocalPosition` / `previewLocalScale` と `PreviewStage` 側で調整する。

## 4. MainScene の下部Tab仕様

下部の `Character`、`Progress`、`Home`、`Missions`、`Armory` Tabの切り替えは `MainScene` が担当する。`MainTabs` コンポーネントは使用しない。

- 各TabのNormal表示へ `PartsTabItem` を配置し、`MainScene` の `characterTab`、`progressTab`、`homeTab`、`missionsTab`、`armoryTab` にInspector参照で設定する。
- 対応画面も `MainScene` の各 `*TabScreen` にInspector参照で設定する。
- 起動時は `Home` を初期選択として扱う。
- タブ押下時は、選択画面だけを active にし、他の画面を inactive にする。
- 選択TabだけをFocused表示にし、ほかのTabはNormal表示にする。
- Home選択時は `AngelHome`、Character選択時は Character 用アバタープレビューだけを表示する。

タブの `Button.onClick` へ個別に永続イベントを設定するのではなく、`MainScene` が起動時に動的 listener を登録する。

### 4.1 ProgressScreen の仕様

`Progress` 画面には `ProgressScreen` を配置する。正式な内部構成は `Traits`、`Insights`、Resurgence時だけ表示する `Remnant` とする。

現行 `ProgressScreen` は移行前の `Artifact` / `Skill` / `Trait` Tabを保持し、`TraitTabScreen` を表示している。正式構成への再編は別タスクとする。

- 正式構成では `traitTab` / `insightsTab` / `remnantTab` と対応ScreenをInspectorで設定する。`remnantTab` はResurgence以外では非表示とする。
- Traits画面は `TraitTabScreen` を `traitTabScreen` として `ProgressScreen` から直接参照する。
- 初期選択は `Traits` とする。
- Progress内Tabのlistenerも `ProgressScreen` が起動時に登録し、Buttonの永続イベントは使わない。
- `ProgressScreen` の責務はTabと画面のactive切り替えだけとし、Tabの見た目、Tree表示、Point表示、購入処理は持たない。

## 5. Home 画面

`Home` は `HomeScreen` が担当する。

- `battleButton` 押下で `BattleScene` へ遷移する。
- `settingsButton` はHome左の演出用書見台GameObject自身のButtonへ設定し、既存の共通DialogでSettings入口を開く。現時点では設定項目の実装前の案内表示とし、実際の設定項目はこの入口へ追加する。
- `achievementsButton` はHome右の演出用戦果碑／奉納台GameObject自身のButtonへ設定し、既存の共通DialogでAchievements入口を開く。実績一覧の実装前は準備中の案内表示とする。
- 書見台・戦果碑のButtonは `targetGraphic = Body_Visible`、Transition `None` とする。構築中は `interactable = false` とLoading Overlayで入力を遮断し、Home HUDのフェード完了後に有効にする。小道具の葉は書見台7枚・戦果碑9枚の計16枚を、本体着地後に葉ごとに生やす。小道具本体の描画はunderlay・葉・元可視bodyの3層を常時維持し、本体2Imageは同じHomeBuildPartのalphaに連動する。
- 2026-10-12のEditor Play Mode確認では、Title入場・通常MainScene起動とも92個の個別GameObjectを完成後も維持し、旧祭壇Image・旧小道具Buttonの表示とSprite／GameObject交換は0だった。新Button2個によるSettings／Achievementsの開閉と、Progress Tab → Home Tabの画面Raycast往復を確認した。必須参照の欠落0件、Consoleエラー・警告0件。詳細な記録と完成画面比較は `docs/specs/TitleScene仕様書.md` を正とする。
- `useDebugBattleStageId` が true の場合は `debugBattleStageId` を使う。
- false の場合は `ProgressManager.GetRecommendedStageId()` から推奨ステージを取得する。
- 遷移前に `BattleSceneRequest.SelectedStageId` を設定する。
- Home 用の `Angel` 表示は `Assets/Resources/Prefabs/Angel/AngelHome.prefab` を使用する。Root の `Animator` には `Assets/Resources/Animations/Angel/AngelHome.controller` を明示的に設定し、既定状態 `Idle_Normal` は `Assets/Resources/Animations/Angel/Idle_Normal.anim` を3秒でループする。
- `AngelHome` は通常形 `Normal` と、将来の開示演出用に初期状態では非表示の `Revealed` を同じローカル位置・スケールで持つ。開示演出では両方を重ね、明度と alpha を逆方向に変化させる。Sprite の実行時探索は行わず、各 `SpriteRenderer` の Sprite は Prefab 上で明示的に設定する。
- `Angel-Front-Normal.png` と `Angel-Front-Revealed.png` の Sprite Pixels Per Unit はともに `1000` とする。`AngelHome` とその `Normal` / `Revealed` の Scale は `(1, 1, 1)` を保ち、表示サイズの補正を Transform の定数倍率へ逃がさない。`Idle_Normal` の上下移動量は `0.006` を基準とする。
- Angel の2画像は `Compression: None`、`Filter Mode: Bilinear`、mipmap 無効で取り込む。`PreviewCamera` の `RT_AvatarPreview` は `1024 × 1024` とし、Home での表示解像度を確保する。

Mission の初回クリア報酬は各 `StageMasterData.ClearRewards` で定義する。報酬は `Item`（`ItemStacks` へ数量を加算）または `Equipment`（所持装備として付与）とし、同じMissionで二重に付与しない。個別の報酬内容は各 Stage マスターで設定する。

Armory は `ArmoryProductDatabase` の商品一覧を正本とする。各 `ArmoryProductMasterData` は販売対象として `EquipmentMasterData` または `ItemMasterData` を直接参照し、数量、Gold価格、解放に必要なクリア済みMission、表示順、購入上限を持つ。Equipment商品に登録できるのは、Helmet、Chest、右手、左手の戦闘4部位だけとし、Hair／Eye／Beard／Skinなどの見た目専用パーツは登録不可とする。購入回数は `UserData.Armory` に保存し、Resurgence後も維持する。一覧は Item と Equipment で親・Prefab・View を分ける。Item は並び順を維持し、名称・付与数・必要に応じて残り購入回数を表示する。Equipment は購入可能品を先、売切・所持済み品を末尾に並べ、名称と短いタグ文を表示する。未解放品は非表示、売切品は Disabled とする。購入時は、Item は `Dialog-Common-ConfilmWithIcon`、Equipment は `Dialog-EquipmentDetail` を購入モードで開く。どちらもGold不足、売切、または既に所持している装備は確定操作をDisabledにする。確定時にGold消費、Item加算または装備付与、購入回数保存を同一処理で行う。

### 5.1 Achievements

Homeの戦果碑／奉納台はAchievements画面を開く。現行は実装前の案内Dialogを表示し、一覧画面は後続タスクで追加する。Achievementsは、進行、救済を含む選択、収集・成長、Resurgence／結末の到達を対象にするゲーム内実績である。スコア、ランク、クリアタイムは解除条件に使わない。

- 解除状態はローカルセーブを正本とし、通信なしでもゲーム内の一覧と解除通知を利用できる。
- 未解除の隠し実績は、名称と条件を伏せて表示できる。
- Google Play Games Servicesへの実績送信は、ゲーム内実績IDを起点とする任意のプラットフォーム連携として後から追加する。未認証、オフライン、または連携未対応でもゲーム内の解除状態は変えない。

### 5.2 Settings

Settings は `音量`、`表示`、`アカウント`、`クレジット` の一覧から詳細Dialogを開く構成とする。各項目の実装は段階的に追加する。

- Credits の表示内容は `CreditsPage` を正本とする。
- 初期登録するゲームエンジンクレジットは、`Unity`（Unity Technologies、https://unity.com/）とする。
- 初期登録するビジュアル素材クレジットは、`Character Assets / UI Dark Theme`（Layer Lab）とする。
- 初期登録する音楽クレジットは、`MUSEUM`（作曲：木原眼鏡 / STUDIO AGASHA、https://studio-agasha.booth.pm/items/1683919）とする。購入済みBGM素材を利用し、商品ページに記載された商用BGM利用条件に従う。著作権は作者に帰属する。

## 6. Character タブ

Character タブは、プレイヤーの装備変更、実戦用 Skill ロードアウト変更、見た目変更、重要所持品の確認を行うタブである。
装備変更はタブ内で即時選択し、髪、肌、目、ひげなどの見た目変更は `Button_Skin` から開くダイアログで行う。

### 6.1 UI 構成方針

- `CharacterScreen` は画面全体の状態遷移とデータ更新を担当し、タブ、装備一覧、装備枠、Skill 枠は各 View に分離する。
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
- Character 用アバタープレビューの表示、RenderTexture 接続、見た目反映、ダイアログ中の一時非表示は `StatusAvatarPreviewView` が担当する。`CharacterScreen` はこの View を通じてプレビューを操作する。

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

Character タブは以下のデータを参照、更新する。

- 所持装備: `UserData.Inventory.Equipments`
- 現在の見た目状態: `UserData.Profile.AvatarAppearance`
- 装備マスター: `EquipmentMasterData`
- 実行時装備データ: `EquipmentData`
- 実戦用の戦闘構成: `UserData.Profile.Decks` と `CurrentDeckId`
- 表示適用: `AppearanceController` と `AppearanceAdapter`

未装備は現行方針どおり `EquipmentId = 0` として扱う。

### 6.4 装備変更

現行実装では、以下の枠を Character タブで扱う。

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

Character タブの `SlotSkill_01` 〜 `SlotSkill_04` は `StatusSkillSlotView` が担当し、`DeckManager.EnsureUsableLoadout(...)` で得た現在デッキの実戦用 Skill を表示する。

各 `StatusSkillSlotView` は `PartsSkillItem` を Inspector 参照し、表示状態の直接切替や画像・テキストへの直接代入は行わない。

- スロット数は `UserProfileData.BattleSkillLoadoutSlotCount` の 4 枠を正とする。
- スロットに Skill が設定されている場合は、候補 Skill の `DisplayName` と `Icon` を表示する。
- Skill の `Icon` は背景つきの正方画像でもスロット背景フレームを隠さないよう、枠内側に余白を取って表示する。
- Skill枠の種別アイコンは、右手、左手、胴、兜の4種を用いる。各Skillは、そのSkillを提供している現在装備中の枠に対応する種別アイコンを表示する。
- 初回表示ではCanvasレイアウト確定後にアイコンサイズを計算する。保存や再選択などの後続操作を行わなくても、すべてのSlotSkillでIconを表示する。
- 未設定または現在の装備から使用できない Skill は `Empty` として表示する。
- スロットを押下すると `SkillLoadoutDialog` を開き、押下したスロットを選択状態にする。
- ダイアログ内では、`SelectedSkill` の4個の `PartsSkillItem` に現在のドラフト上のSkillロードアウトをアイコンのみで表示する。編集対象スロットはダイアログを開いたCharacter側のSkill枠で決まり、`SelectedSkill` は操作を持たない。`PartsSkillItem` 内部の名前テキストと種別アイコンは常に非表示にする。
- 候補 Skill は、現在デッキの `EquipmentData.AssignableSkills` から `DeckManager.GetCandidateSkillsFromEquippedEquipment()` で取得する。対象は右手、左手、胴、兜の4装備枠に限定し、Hair、Eye、Beard、Skin などの見た目専用パーツは候補元に含めない。
- 候補 Skill は `SkillInfoArea` のスクロール領域に、`Dialog-SkillLoadout.prefab` の `Content` に置いた非表示の `SkillLoadoutCandidateItemView` テンプレートを複製して表示する。外部の候補行Prefabは参照しない。候補数を8件に固定せず、装備枠や候補数の拡張に追従できるようにする。
- 候補行のアイコン枠は `PartsSkillItem` を使う。候補行では `ShowSkillIconOnly(icon)` を使い、`PartsSkillItem` 内部のSkill名テキストと種別アイコンは常に非表示にする。名前と説明は行側のテキストで表示し、現在選択中スロットへ割り当てられている候補だけ選択Frameを表示する。
- 候補 Skill の選択によるロードアウト変更時に、ダイアログ内のドラフトと `SelectedSkill` 表示を即時に切り替え、同時に `DeckManager.SaveLoadout(...)` で現在デッキへ保存する。SaveボタンとClearボタンは使わない。
- Skill ロードアウトは現在デッキの `SkillIds` に反映し、他のユーザーデータと同じメインセーブJSONへ保存する。
- `Close` または背景クリックで閉じた場合も、変更済みの Skill は戻さない。未変更で閉じた場合はCharacter表示の再描画を行わない。

### 6.6 見た目変更ダイアログ

見た目変更は戦闘デッキと独立して扱い、髪・目・ひげ・肌の見た目パーツと色だけを変更する。
Character タブ上の `Button_Skin` を押下すると `AvatarDialog` を開き、Dialog 内の `PreviewStage` に一時生成した `Character` で編集中の見た目を確認する。

- パーツ変更: `Hair`、`Eye`、`Beard`、`Skin` のみを `AvatarPartStateData` に `PartType`、`EquipmentId`、`Visible` として保存する。
- 色変更: `AvatarColorData` に `ColorTargetType` と色を保存する。

色変更対象は現行の見た目仕様に合わせ、少なくとも以下を候補にする。

- `Skin`
- `Hair`
- `Eye`
- `Beard`

`Helmet`、`Chest`、`HandRight`、`HandLeft` は現在デッキの戦闘装備であり、このダイアログからは表示・変更・保存しない。`Arrow` と `HelmetHair` は派生パーツとして扱い、Character タブから直接保存対象にしない。表示状態は `AppearanceController` の同期処理に任せる。

`Assets/Resources/Prefabs/Dialog-AvatarController 1.prefab` は、カテゴリ枠と候補枠を各1件の非表示テンプレートとして持ち、外見カテゴリ・所持候補の件数ぶん動的に生成する。`ColorPicker`、Save、CloseはPrefab内の既存要素を使う。Dialog 内での変更は編集中プレビューへ反映し、保存ボタン押下時に `AvatarEditorPresenter.Commit()` で `AppearanceManager` へ反映する。外見専用モードでは `InventoryManager.MarkEquipped(...)`、`DeckManager`、戦闘装備には触れない。

候補がないカテゴリでは、Prefab参照の `emptyText` に `No options available` を表示する。外見専用モードの保存は `AppearanceManager.SetCosmeticAppearance(...)` を使い、現在デッキの装備・Skill・選択中デッキを変更しない。
保存後は `AppearanceAdapter.ApplyTo(...)` で Home 用プレビューと Character 用プレビューへ反映し、`UserDataJsonStore.SaveSession()` で保存する。
閉じるボタンまたは背景クリックで閉じた場合は保存しない。
Dialog 表示中は、Character タブ本体側のプレビューを一時的に非表示にし、Dialog 内プレビューだけを表示する。

Dialog 表示は `DialogService` / `DialogHost` / `DialogBase` を使う。`DialogHost` は必要になった時点で最前面用の `DialogCanvas` を動的生成し、その中に `Backdrop` と `DialogParent` を置く。`CharacterScreen` などの呼び出し側は prefab 参照や parent 指定を持たず、`DialogService.OpenAsync<...>()` でダイアログを開く。
黒背景は `DialogHost` が管理し、開いているダイアログがひとつ以上背景表示を要求している場合に 1 枚だけ表示する。背景クリック可否や背景クリックで閉じるかどうかは `DialogBase.SetBackdrop(...)`、`SetBackdropVisible(...)`、`SetBackdropInteractable(...)`、`SetBackdropCloseEnabled(...)` でダイアログ単位に指定する。
見た目は `Assets/Resources/Prefabs_Sample/Demo/Scenes/Settings.prefab` の中央 Popup と暗い背景を参考にしつつ、実体 Prefab / Script はサンプル資産を直接使わず、プロジェクト側へ作成する。
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
- `AppearanceAdapter.ApplyTo(...)` で Home 用プレビューと Character 用プレビューを更新する。
- `DeckManager.EnsureUsableLoadout(persistImmediately: true)` で、装備変更後に使えなくなった Skill を現在デッキから自動で外す。
- `UserDataJsonStore.SaveSession()` で即時保存する。

Skill ロードアウトは、Skill 設定ダイアログ内で候補 Skill を選択した時点で自動保存する。

- `SlotSkill_01` 〜 `SlotSkill_04` のいずれかを押し、Skill 設定ダイアログを開く。
- ダイアログ内で対象スロットと候補 Skill を選ぶ。
- 候補 Skill 選択または Clear 実行時に `DeckManager.SaveLoadout(...)` を呼び、現在デッキの `SkillIds` を更新する。
- 保存後は Character タブの Skill スロット表示を更新する。
- Unity Editor の Domain Reload 無効環境でも再生開始時に保存済み JSON を読み直せるよう、保存データに依存する static singleton は `RuntimeInitializeLoadType.SubsystemRegistration` でリセットする。

## 7. 未確定事項

- Missionsカードの選択時の未解放制御と戦闘遷移。
- Character タブの最終 UI レイアウトと4つ目の入口の要否。
- 見た目変更ダイアログの最終レイアウトと、`Settings.prefab` を基準にした細部調整。
- 装備変更による戦闘ステータス、候補スキル、タグ一致ボーナスのプレビュー表示方針。

## 8. 次期実装: 戦闘デッキ

戦闘用の右手・左手・兜・胴と、戦闘Skill4枠はデッキ3件として保存・切替する。見た目用の髪・目・ひげ・色は `AvatarAppearance` に分離し、デッキには含めない。Characterのデッキ一覧ダイアログと、敗戦後の再挑戦導線からデッキを切り替えられるようにする。詳細は `docs/specs/デッキ仕様書.md` を正とする。

## チュートリアルのデバッグ

DebugSheetのTutorialsページで、現在のDatabaseに登録されたチュートリアルごとにCompleted（セーブへ即時反映）、Repeat For Debug（Editor／Development Buildのセッション限定）、Replay（フラグを変えず現在画面で再生）を操作できる。HomeのAngel導入はデバッグ時の繰り返しを初期設定とする。通常ビルドはOncePerSaveで再表示しない。詳細はチュートリアル仕様書を参照する。
