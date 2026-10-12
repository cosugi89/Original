# TitleScene 仕様書

- 最終更新: 2026-10-12
- 文書状態: 導線と実装基盤を決定。祭壇・葉・Home小道具の構築用Sprite分割とMainSceneへの仮配線は完了。演出の最終調整は未完了。
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
3. MainSceneはHomeの実パーツを完成位置より少し上、alpha 0の状態で開始する。
4. 石材パーツは少しずつ開始をずらし、下へ移動しながらフェードインする。物理演算は使わない。
5. 構築中もLoading Overlayを継続表示する。Angelは必要な状態でのみ構築後に表示する。
6. 初回導入が必要ならAngel表示後、Home HUD表示前にチュートリアルを開始できる。通常時はHome HUDをフェードインする。

通常のBattleSceneからMainSceneへの復帰やTab切替では、この構築演出を再生せず即時表示する。

## 4. 現在の実装基盤

- `HomeEntryRequest` はTitleから一度だけ消費されるHome構築要求、Title背景の解除callback、Loading Overlayの更新・完了callbackを渡す。
- `TitleSceneController` は開始入力、3つの遷移Canvas、`MainScene` 非同期ロードを担う。`beginButton` と `resurgenceButton` を別々のInspector参照として持ち、`The Remnant`（ItemId `3`）の所持状態に応じてどちらか一方だけを表示する。`connectPlayGamesButton` はPGS未認証時だけ表示し、`connectPlayGamesRequested` を通じてPGS手動認証を要求する。`loadingImageTransform` はロード中に72度ずつZ軸回転する。
- 現行の `TitleScene.unity` には描画順を固定した `Title Background Canvas`（順序0）、`Title Canvas`（Title UI、順序1）、`Loading Canvas`（順序2）がある。通常時は3つとも `Main Camera` を参照する `Screen Space - Camera` とし、Title UIを通常どおり描画する。開始入力後に持ち越す `Title Background Canvas` と `Loading Canvas` だけを `Screen Space - Overlay` へ切り替え、シーン切替後のCameraから独立させる。`TitleSceneController` の開始Button、Title背景Canvas、Loading Canvas、Loading画像を含む必須参照はInspectorで配線済みである。`TitleScene_demo.unity` は試作シーンであり、実装対象にはしない。
- `GameInitializer`、`UserDataManager`、`TitleSceneController` はTitleSceneの別々のRootに配置する。`UserDataManager` と `TitleSceneController` は自身を一度だけ永続化し、`GameInitializer.Awake()` がローカルセーブを初期化した後、Inspector参照した `TitleSceneController.Initialize(userDataManager)` を呼ぶ。`TitleSceneController` 自身は `Awake()` を持たず、この順序でのみ初期化する。シーン遷移時は `User Data Manager` Root、`Title Scene Controller` Root、`Title Background Canvas` Root、`Loading Canvas` Rootを保持する。`Title Canvas` はStart入力時に非表示となり、Sceneのアンロードとともに破棄する。Home基底背景の表示後にTitle背景を、Home構築完了後にLoading CanvasとTitleSceneControllerを破棄する。
- Unity Editorでは、Play Buttonの横にある `Title` Buttonから、一度だけTitleScene起点でPlay Modeを開始できる。停止後は、既存のPlay Mode開始Scene設定へ戻す。
- `HomeEntranceDirector` はMainSceneに置き、構築要求がある時だけ `HomeBuildPart` を開始位置へ準備して再生する。Homeの基底背景、構築パーツ、Home HUDの表示タイミングだけを担当し、Angelを含むHome固有の表示物は `MainScene` が所有する。`StartIfRequested(Action)` が要求の消費、準備、再生開始を一つの入口として行い、構築完了時にcallbackでHome固有表示物の表示を通知する。
- `HomeBuildPart` は1つのUIパーツImageの最終RectTransform位置を記録し、Image自身のcolor alphaを上方からの移動とともに変化させる。石材・小道具は `Image + HomeBuildPart` の単位とし、CanvasGroupは使わない。葉は移動せず、alphaと小さなscale変化だけで表示する。
- MainSceneには `HomeEntranceDirector` を配置し、`homeStaticAltar` に通常表示用の元祭壇Image、`buildParts` に石材15枚、`foliageParts` に葉2枚、`propParts` に書見台・戦果碑／奉納台の表示用複製2枚を配線している。`HomeBuildRoot` はHome直下に置き、構築演出中だけ有効化する。通常のMainScene起動では元祭壇Imageと本来の小道具Buttonを表示し、Titleからの入場時だけ元祭壇ImageとHome HUDを隠して構築パーツを使う。小道具の複製は演出完了後に隠し、HUDと一緒に元のButtonを再表示するため、既存のタップ処理は本来のButtonだけが担当する。Home HUD用の `CanvasGroup` とHome基底背景も配線済みである。Titleからの入場時は、基底背景を有効化してからTitle背景の解除を要求する。
- 祭壇の構築素材は、元画像 `Assets/Resources/Sprite/ChatGPT 画像 2026年10月4日 12_40_51.png` を同一キャンバス・同一原点の透明Spriteへ分割した `Assets/Resources/Sprite/Home/AltarBuild/` 配下の素材を使う。石材は `Granular/` 配下の15枚、葉は `Altar_Foliage_Left.png` と `Altar_Foliage_Right.png` である。石材と小道具は上方からの落下＋フェードイン、葉は石材の後に移動なしで表示する。

## 5. 未完了のScene作業

- TitleSceneの背景、`The Red Fever` のタイトルロゴ、別オブジェクトの`Begin` / `Resurgence` 開始Button、`Connect Play Games` Button、バージョン表記、Loading Canvasを配置し、`TitleSceneController` の必須Inspector参照を配線する。Loading Canvasには黒背景画像と`Loading_01_1.png`を配置し、画像を72度ずつZ軸回転させる。
- Build Settingsの起点をTitleSceneへ切り替える。
- Android実機でだけ起動時のPGS自動認証を接続する。PGS失敗時にも開始を止めず、Unity Editorでは認証をスキップする。PGSプラグインは未導入のため、認証処理そのものは未実装である。
- 仮配線済みのHome構築演出をPlay Modeで確認し、石材・小道具の開始位置・表示時間を最終調整する。
- Home HUDを構築パーツから分離したRootへまとめ、`HomeEntranceDirector` の `homeHudCanvasGroup` に設定する。
- 初回チュートリアルの開始判定と、演出完了後にHUDを出す接続を追加する。
