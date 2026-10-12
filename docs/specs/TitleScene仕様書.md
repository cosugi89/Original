# TitleScene 仕様書

- 最終更新: 2026-10-13
- 文書状態: 演出GameObjectの継続表示、小道具自身のButton、祭壇葉37枚・小道具葉16枚を実装。Unity EditorでTitle入場・完成表示・通常起動・タップ導線を確認済み。
- 位置づけ: 起動時の入口と、MainSceneのHome構築演出へつなぐシーン。

## 1. 画面構成

TitleScene は `The Red Fever` のタイトル、開始ボタン、PGSへの手動接続ボタン、バージョン表記、暗い廃墟と石畳の背景を持つ。Angel、祭壇、Homeの通貨表示、下部Tab、SettingsなどのHome固有UIは置かない。

- 初回および通常周回中は `Begin` Button を表示する。ローカルに進行中のセーブがある場合も、同じ `Begin` でその進行を再開する。
- `The Remnant` 獲得後は `Begin` Button を非表示にし、同じ配置の別Buttonである `Resurgence` Button を表示する。テキスト差し替えではなく、将来それぞれの見た目・演出を変えられる構成にする。
- PGS未認証時は、開始Buttonの下に `Connect Play Games` Button を表示する。押下時はPGSの手動認証を再試行し、成功時に非表示にする。ゲーム開始は常に可能とする。

画面上の概略は以下とする。

```text
             THE RED FEVER

               [ Begin ]
            または [ Resurgence ]

          [ Connect Play Games ]  ※PGS未認証時のみ

```

開始ボタンの後に、MainScene上で実際のHome祭壇が構築される。そのためTitleScene背景は「まだ拠点が組み上がっていない入口」として扱う。

## 2. 起動時のPGS認証

Google Play Games Services（PGS）は実績・ランキングなどのプラットフォーム機能に利用する。ゲームデータのGoogleアカウント連携、クラウドセーブ、クロスデバイス復元は別途検討し、この仕様には含めない。

1. Android実機では、アプリ起動時にPGS SDKを初期化し、Play Gamesのプラットフォーム認証をバックグラウンドで自動試行する。
2. Unity EditorではPGSの自動認証を試行しない。Editor上のTitleScene表示と`Begin`／`Resurgence`導線は、認証結果を待たずに確認できるようにする。
3. PGSプロフィールを持たないプレイヤーには、必要に応じてGoogle提供のプロフィール作成画面が表示される。
4. PGS認証の成功、失敗、キャンセルにかかわらず、TitleSceneを表示してゲーム開始を許可する。失敗を理由に`Begin`／`Resurgence`をDisabledにしない。
5. PGS認証済みの場合だけ、実績・ランキングを利用可能にする。以後の再試行はPGS側の仕組みに従う。

PGSプレイヤーIDは実績・ランキング用のIDとして扱い、現行のローカルセーブと結び付けない。

## 3. TitleからHomeへの遷移

1. `Begin` または `Resurgence` の入力を一度だけ受け付け、`Title UI Canvas` を非表示にする。`Title Background Canvas` は不透明のまま維持し、MainSceneの `HomeBackground` が有効化された通知を受けてから解除することで、シーン切替中に背景の空白や背後の描画を見せない。
2. `Loading Canvas` を表示したまま `MainScene` を非同期ロードする。Loading Canvasは背景を見失わない濃度（黒画像 alpha `0.55`）で画面全体を暗くする黒画像と、`Assets/Resources/Sprite/HUD/Loading_01_1.png` を表示する画像で構成する。ロード中はLoading画像を72度ずつZ軸回転させ、Overlayはロードの完了で即消さない。
3. MainSceneはHomeの基底背景を表示し、配置参照として残す旧祭壇Image・旧小道具Button、Angel、Home HUDを非表示にする。石材・小道具の独立Spriteは完成位置より上、alpha 0から開始する。葉は根元付近を基準とする完成位置を保ち、alpha 0から開始する。小道具の演出GameObjectにあるButtonは構築中に操作できない状態にする。
4. 「土台／階段 → 左右の石材 → アーチ → 祭壇の葉 → 書見台／戦果碑 → 小道具の葉」の順で組み上げる。石材は各段階内の開始を少しずつずらし、前段階の着地が完了してから次段階を開始する。石材と小道具は上からの落下＋フェードイン、葉は移動なしのフェードインと小さなscale変化で表示する。小道具の葉は本体の着地後に生やす。物理演算は使わない。
5. 構築中はLoading Overlayとその入力遮断を維持する。小道具の葉まで全表示を終えたら完成表示を短く保持し、Loading Overlayだけをフェードアウトする。フェード完了と破棄を待ってからAngelを表示する。HUDは非表示のまま。演出に使った石材・葉・書見台・戦果碑は完成画面へ残す。
6. Angelが描画できるフレームを挟み、TutorialManagerの表示待機を解除してHomeReadyを通知する。通常周回の未完了時とデバッグ繰り返し有効時はAngelの導入会話を開始する。PreviewImageのアルファに沿って暗幕を抜き、透明部分の背景は暗くする。メッセージは下側に表示し、文字送り中の全面タップで全文表示、次のタップで台詞を送る。HUDはまだ非表示・操作不可。
7. 現在のHomeで開始できるすべての案内と、完了保存で発火した後続案内が終わってからHUDをフェードインする。案内がない場合はHomeReadyの照合後そのまま進む。HUDフェード完了時に小道具自身のButtonとHome HUDの入力を有効にする。旧祭壇Image・旧小道具Buttonは表示しない。別画面・非表示HUDの操作を要求する案内は表示後の判定へ渡し、入場待ちの停止原因にしない。

通常のMainScene起動、BattleSceneからの復帰、Tab切替では、同じ個別SpriteのGameObjectを完成位置・完成alpha・完成scaleで即時表示し、構築演出を再生しない。

## 4. 現在の実装基盤

- `HomeEntryRequest` はTitleから一度だけ消費されるHome構築要求、Title背景の解除callback、Loading Overlayの更新callbackと終了処理を返すFunc<IEnumerator>を渡す。Directorが終了処理をyieldし、Loadingフェード完了を実際に待ってから次の表示へ進む。
- `TitleSceneController` は開始入力、3つの遷移Canvas、`MainScene` 非同期ロードを担う。`beginButton` と `resurgenceButton` を別々のInspector参照として持ち、`The Remnant`（ItemId `3`）の所持状態に応じてどちらか一方だけを表示する。`connectPlayGamesButton` はPGS未認証時だけ表示し、`connectPlayGamesRequested` を通じてPGS手動認証を要求する。`loadingImageTransform` はロード中に72度ずつZ軸回転する。
- 現行の `TitleScene.unity` には描画順を固定した `Title Background Canvas`（順序0）、`Title Canvas`（Title UI、順序1）、`Loading Canvas`（順序2）がある。通常時は3つとも `Main Camera` を参照する `Screen Space - Camera` とし、Title UIを通常どおり描画する。開始入力後に持ち越す `Title Background Canvas` と `Loading Canvas` だけを `Screen Space - Overlay` へ切り替え、シーン切替後のCameraから独立させる。`TitleSceneController` の開始Button、Title背景Canvas、Loading Canvas、Loading画像を含む必須参照はInspectorで配線済みである。`TitleScene_demo.unity` は試作シーンであり、実装対象にはしない。
- `Title Canvas` 直下の `Image_Title`、`SocialSignButton_Group`、`Text_Ver` は `localPosition.z = 0` とする。Camera背後へUIが配置される不具合を防ぎ、開始Buttonを表示・Raycastできるようにする。X/Y座標・scale・既存スタイルは保持する。
- `GameInitializer`、`UserDataManager`、`TitleSceneController` はTitleSceneの別々のRootに配置する。`UserDataManager` と `TitleSceneController` は自身を一度だけ永続化し、`GameInitializer.Awake()` がローカルセーブを初期化した後、Inspector参照した `TitleSceneController.Initialize(userDataManager)` を呼ぶ。`TitleSceneController` 自身は `Awake()` を持たず、この順序でのみ初期化する。シーン遷移時は `User Data Manager` Root、`Title Scene Controller` Root、`Title Background Canvas` Root、`Loading Canvas` Rootを保持する。`Title Canvas` はStart入力時に非表示となり、Sceneのアンロードとともに破棄する。Home基底背景の表示後にTitle背景を、Home構築完了後にLoading CanvasとTitleSceneControllerを破棄する。
- Unity Editorでは、Play Buttonの横にある `Title` Buttonから、一度だけTitleScene起点でPlay Modeを開始できる。停止後は、既存のPlay Mode開始Scene設定へ戻す。
- `HomeEntranceDirector` はMainSceneに置き、構築要求がある時だけ `HomeBuildPart` を開始位置へ準備して再生する。通常表示時には同じパーツを即時完成状態へ設定する。Homeの基底背景、構築パーツ、小道具Button、Home HUDの表示・入力タイミングを担当し、Angelを含むHome固有の表示物は `MainScene` が所有する。構築完了時にcallbackでHome固有表示物の表示を通知する。
- `HomeBuildPart` は `targetTransform`、`referenceTransform`、`partImage`、`additionalImages`、完成位置 `finalOffset`、構築段階 `buildStage`、開始位置 `startOffsetY` をInspectorで受け取る。参照RectTransformの座標系を使って完成位置とscaleをそろえ、Image自身のcolor alphaを変化させる。小道具の本体2Imageは同じHomeBuildPartの `partImage` と `additionalImages` に配線し、同じalphaで落下・フェードする。各ImageにCanvasGroupは使わない。葉のpivotとTransformは根元または自然な接地点付近に置く。
- MainSceneには `HomeEntranceDirector` を配置し、`buildParts` に石材37個、`foliageParts` に祭壇の葉37枚、`propParts` に書見台・戦果碑／奉納台2個、`propFoliageParts` に小道具の葉16枚、`propButtons` に小道具自身のButtonを配線する。小道具の葉は書見台7枚・戦果碑9枚。`homeStaticAltar` と `legacyPropObjects` は作成時の位置・サイズ参照に使う旧オブジェクトであり、通常起動時も非表示にする。必須参照はInspectorから渡し、実行時の `Resources.Load` や名前による探索を追加しない。
- 演出用の書見台・戦果碑GameObject自身にButtonを持たせ、`HomeScreen.settingsButton` と `HomeScreen.achievementsButton` をそのButtonへ配線する。既存のSettings／Achievements callbackを使う。小道具Buttonの `targetGraphic` は `Body_Visible`、Transitionは `None`。石材・葉のImageは常に `raycastTarget = false`。小道具Buttonは落下中からHUDフェード終了まで `interactable = false` とし、Loading Overlayで画面入力を遮断し、完了後に有効にする。
- Home HUDは `PanelBottom` のCanvasGroupと、`additionalHudCanvasGroups` の `ResourceBar_Group`、`MainTabLayout` を同時に非表示・フェード・入力切替する。PreviewImageを含む `PanelContent` のCanvasGroupはalpha=1に維持し、HUDと分離する。通貨・下部Tab・STARTはチュートリアルの終了後に表示する。`HomeBuildRoot` はHome直下のfirst siblingに置き、PanelContent外で構築パーツを描画する。Scene Rootの再編やPreviewImageの親変更は行わない。
- 完成表示に使う個別Spriteは `HomeBuildRoot` に残す。旧祭壇Image・旧小道具Buttonへの表示切り替えを行わないため、完成後の物体とタップ対象が同じGameObjectにそろう。

### 4.1 祭壇素材と完成表示の一致

- 元画像は `Assets/Resources/Sprite/ChatGPT 画像 2026年10月4日 12_40_51.png`。石材はレンガ、階段、柱、アーチ石、台座の輪郭と継ぎ目に沿った独立Spriteとして扱い、完成画像を直線的な矩形マスクへ区切る方式は採用しない。
- 使用する素材は `Assets/Resources/Sprite/Home/AltarBuild/Individual/` 配下。石材37個の内訳は土台／階段14個（外周11個、中央台座1個、階段2個）、左右の石材19個、アーチ4個。祭壇の葉は左側18枚・右側19枚の計37枚で、各1枚を使う。小道具本体と独立葉の素材も同じ表示方針にそろえる。旧 `Granular/` の15枚や左右2塊の葉SpriteはHome入場演出へ配線しない。
- `Tools/HomeAltar/stones.json` は石材の輪郭・構築段階・順序、`Tools/HomeAltar/leaves.json` は葉の根元・先端位置を持つ素材生成用データである。Sprite自体は物体周囲の透明余白を除いて保存し、個別の完成位置・重なり順を持たせる。
- 元PNGの `1150 × 1367` に対し、通常表示用Spriteの可視キャンバスは左42・上36を基準とする `1086 × 1290`。構築用の座標はこの可視キャンバスへ合わせ、透明余白による位置ずれを防ぐ。
- 落下時に必要な、元絵では隠れていた石材の面は元絵と同じ正面視点・色・線で補完する。`Tools/HomeAltar/restored-underlay.png` と `closed-stones-atlas.png` の補描を物体の実寸へ登録し、閉じた輪郭を持つ石材として落下させる。元絵の見える面の内部をなじませ、他の物体の切断線を演出用輪郭へ持ち込まない。
- 石材内部の `assembledSprite` と `ShowAssembledSprite()` は実行時から廃止し、落下時と完成後に同じSpriteを保持する。補完面は、後続パーツで覆われる元画像のalpha 255領域に限定する。元の可視面をそのまま残し、保持する石材・祭壇葉の完成合成は `parts.json` の `retainedRgbaDiff = 0` で確認する。Unity上の重なり・継ぎ目・隙間はPlay Modeでも確認する。
- 葉は祭壇・小道具とも葉ごとの独立Spriteとし、根元基準のpivotを使う。落下させず、alphaとscale 0.94から1への変化だけで表示する。小道具本体は着地後に葉を生やせるよう、葉を含む元絵から本体と葉を分ける。完成座標は配置参照として残す旧小道具Buttonを参照する。
- 小道具の描画順は常時「補描underlay → 根元pivotの独立葉 → 元の可視body（葉を除去）」の3層とし、着地・葉の表示・完成後で層やSpriteを交換しない。木・石本体の輪郭を固定し、葉マスク・多角形の所有領域・補描alpha・本体除外多角形を組み合わせて分類する。元の可視画素は保持する。元alpha 250～254の境界に補描が透けるため、生成画像の合成差は書見台で最大5/255、戦果碑で最大2/255。Play Modeの比較では目立つ外見差は見られなかった。GPU描画の完全画素一致は保証しない。
- `_Assembled.png` の生成素材は未参照の比較用として残す。通常表示と演出では使用しない。
- 素材生成は `Tools/HomeAltar/build-parts.mjs` と `build-props.mjs`、Sceneの生成・Inspector配線は `Assets/Scripts/Editor/HomeAltarAssemblyBuilder.cs` が担当する。MainSceneを開いて `Tools/The Red Fever/Rebuild Home Altar Assembly` を実行すると、`parts.json` と `props.json` を結合して個別ImageとHomeBuildPartを作成し、構築順・pivot・完成座標・Sprite・HUD・小道具Button参照を保存する。探索や素材読込はこのEditor作成時に限り、実行時にはInspector参照を使う。
- 再生成・作成時配線・比較captureの手順は `Tools/HomeAltar/README.md`、補描時の生成指示は `Tools/HomeAltar/generation-prompts.md` を参照する。

### 4.2 演出時間と開始位置

`HomeEntranceDirector` の設定値をInspectorで配線し、`Time.unscaledDeltaTime` と `WaitForSecondsRealtime` で進める。石材は前半で加速し、最後の短い区間で着地を収める。横移動や回転、物理演算は加えない。

| 対象 | 表示時間 | 開始間隔・待機 | 動き |
| --- | --- | --- | --- |
| 石材 | 1個につき0.30秒 | 同じ段階内で0.055秒ずつ開始をずらす。前段階の着地完了を待つ。 | 完成位置より上から落下し、前半でalphaを上げる。開始offsetは土台／階段+88、左右+112、アーチ+140（演出RootのUIローカル座標）。 |
| 葉 | 1枚につき0.22秒 | 全石材の着地後0.06秒待ち、0.02秒ずつ開始をずらす。 | 完成位置を保ち、根元を基準にscale 0.94から1へ拡大しながらフェードイン。 |
| 書見台・戦果碑 | 1個につき0.34秒 | 全葉の表示完了後に開始し、2個目を0.10秒遅らせる。 | 元Buttonの完成位置より+120から落下＋フェードイン。 |
| 小道具の葉 | 1枚につき0.22秒 | 小道具本体2個の着地後、16枚を0.02秒ずつ開始をずらす。全葉の完了を待つ。 | 根元位置を保ち、alphaとscale 0.94から1への変化で生やす。 |
| 完成保持 | 0.10秒 | 小道具の葉まで全表示を終えてから待つ。 | 演出GameObjectの完成位置を維持。 |
| Loading Overlay | 0.20秒 | 完成保持後に解除し、完了までAngel・HUDを待機する。 | フェードアウト後に破棄。 |
| Home HUD | 0.20秒 | Angel登場とチュートリアル全終了の後に開始する。 | フェードイン完了時に小道具Buttonを含めて入力を有効化する。 |

従来の構築・完成保持・HUDフェード設定は約5.03秒。現在はLoading解除とHUD表示を順番に実行するため、さらにLoadingフェードとframe境界の待機が加わる。チュートリアルの所要時間はプレイヤーの台詞送りによって変わる。

### 4.3 変更前のPlay Mode記録

2026-10-12にUnity Editorで、Title → Loading → 祭壇・小道具の構築 → 小道具の葉 → Angel／Home HUDを181描画フレーム、約9.169秒の記録で確認した。記録には読み込み・frame境界・capture処理の時間を含む。開始からMainScene初表示は約1.363秒、小道具の葉の初表示は約7.783秒、全葉完了は約8.625秒、Home操作可能は約8.961秒だった。

- 完成GameObjectは92個（石材37・祭壇葉37・小道具2・小道具葉16）、有効な小道具Buttonは2個。旧祭壇Image・旧小道具Buttonが表示されたframe、Spriteが交換された回数、GameObjectが交換された回数はいずれも0。
- 背景欠落、構築完了前のLoading解除、構築中の小道具Button早期有効化はいずれも0frame。小道具の葉の根元位置の最大誤差は約 `2.3841858e-7` world単位で、位置計算の誤差範囲だった。
- 通常MainScene起動でも同じ92個を即時完成表示し、旧配置参照は非表示、新Button2個は有効。書見台／戦果碑からSettings／AchievementsのDialogを画面Raycast・pointer clickで開閉できた。Progress Tab → Home Tabも画面Raycastで往復し、Homeの個別パーツ表示を維持した。START Buttonは操作可能で、画面Raycastから `Button_03_Red` のhandlerへ到達したが、Battle遷移の押下は行っていない。
- 必須参照の欠落0件、Consoleエラー0件・警告0件。Play停止後もエラー・警告0件だった。
- Unity描画の元画像との比較は、祭壇の平均channel差が約0.0250037/255、差3超のpixelが409,920pixel中1,824pixel。小道具の平均channel差が約0.00118316/255、差3超が81pixel。主に輪郭のsampling差であり、目視で目立つ外見差は見られなかった。素材RGBA検証とGPU描画の比較を区別する。

確認記録は `docs/captures/HomeEntrance/home-entrance-retained-2026-10-12.gif`、同名の `.png`、`home-completed-retained-2026-10-12.png`、`qa-summary-retained-2026-10-12.json`、`trace-retained-2026-10-12.json` に保存する。Android実機・異なる解像度での確認は今回のEditor確認に含めない。

## 5. 未完了のScene作業

- Build Settingsの起点をTitleSceneへ切り替える。
- Home入場演出のテンポ、輪郭、葉の位置、Button入力をAndroid実機と異なる解像度で確認し、BattleSceneからMainSceneへの復帰でも個別パーツを即時完成表示することを確認する。
- Android実機でだけ起動時のPGS自動認証を接続する。PGS失敗時にも開始を止めず、Unity Editorでは認証をスキップする。PGSプラグインは未導入のため、認証処理そのものは未実装である。
- HomeReady接続済みのAngel導入会話について、Android実機と異なる解像度で輪郭の暗幕透過・メッセージ・全面タップと操作復帰を確認する。詳細は `docs/specs/チュートリアル仕様書.md` を参照する。
