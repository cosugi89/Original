# Home祭壇の独立パーツ

最終更新: 2026-10-12

## 素材と完成状態

- 石材37個: 土台の外周11個、中央床1個、階段2個、左右の石材・柱・台座・小石19個、柱頭と傾いたアーチ石4個。
- 祭壇の葉37枚: 左18枚、右19枚。根元と葉先を指定し、各葉の黒い輪郭を含めて独立した透明Spriteにする。
- 小道具2個: 書見台と戦果碑の本体と葉を分け、小道具本体の演出GameObject自身にButtonを持たせる。
- 小道具の葉16枚: 書見台7枚、戦果碑9枚。祭壇葉と合わせた独立葉の合計は53枚。

`stones.json` は元画像の継ぎ目と輪郭に沿う多角形、`leaves.json` は葉ごとの根元・葉先を持つ。矩形は透明余白を減らす保存領域としてだけ使う。元画像の可視画素を各パーツへ一度だけ割り当てる。石材の補描は後続パーツで覆われる元alpha 255領域に限定し、保持する石材・祭壇葉の合成は `parts.json` の `retainedRgbaDiff = 0` で確認する。Unity描画での輪郭・重なり・継ぎ目・隙間は別途Play確認する。

落下中に見える石材には、組み込み `image_gen` で補描した閉じた輪郭と面を使う。`restored-underlay.png` は葉の下の面の補完、`closed-stones-atlas.png` は最初の36石の独立した輪郭の補完。37個目の丸い小石は元絵で輪郭が閉じているため補描しない。生成指示は [generation-prompts.md](generation-prompts.md) に保存してある。

石材・葉・書見台・戦果碑は、演出に使ったGameObjectとSpriteを完成後も表示する。旧祭壇Image・旧小道具ButtonはSceneに作成時配置の参照として残し、通常MainScene起動時も非表示にする。実行時の `assembledSprite` と `ShowAssembledSprite()` は廃止し、完成時にSpriteやオブジェクトを交換しない。

`_Assembled.png` の生成素材は未参照の比較用として残す。演出と通常表示では使用しない。

小道具は常時「補描underlay → 根元pivotの独立葉 → 元の可視body（葉を除去）」の3層で描画する。本体2Imageは `HomeBuildPart.partImage` と `additionalImages` で同じalphaを使い、着地後も同じ層を維持する。木・石本体の輪郭を固定し、葉マスク・多角形所有領域・補描alpha・本体除外多角形で本体と葉を分類する。元可視画素は保持するが、元alpha 250～254の境界で補描が透けるため、元画像との生成時合成差は書見台で最大5/255、戦果碑で最大2/255。`props.json` に比較結果を保存する。Play比較で目立つ外見差は見られなかった。GPU描画の完全画素一致は保証しない。

演出用小道具自身のButtonへ `HomeScreen.settingsButton` / `achievementsButton` を配線し、既存のDialog callbackを使う。Buttonの `targetGraphic` は `Body_Visible`、Transitionは `None`。落下中からHUDフェード終了までは `interactable = false` とLoading Overlayで入力を遮断し、全構築完了後に有効にする。表示する物体とタップ対象を同じGameObjectにそろえる。

## 配置と動き

元画像は1150×1367、通常祭壇Spriteの領域は左上(42,36)・1086×1290。通常祭壇のPivotは(0.5,1)なので、画像上のパーツPivot(Px,Py)を参照ローカル座標 `(Px-585, 36-Py)` へ変換する。通常祭壇の回転・拡大率・色を引き継ぎ、画面サイズに追従する。

`HomeBuildRoot` はHome直下のfirst siblingへ置き、`PanelContent` CanvasGroup外で、Angel・STARTなどのHUDより下に描画する。HUDは `PanelContent`、`ResourceBar_Group`、`MainTabLayout` を同時にフェードする。

| 段階 | 個数 | 1個の時間 | 開始間隔 | 開始位置／Scale |
| --- | ---: | ---: | ---: | --- |
| 土台／階段 | 14 | 0.30秒 | 0.055秒 | 完成位置より上88、Scale 1 |
| 左右の石材 | 19 | 0.30秒 | 0.055秒 | 上112、Scale 1 |
| アーチ | 4 | 0.30秒 | 0.055秒 | 上140、Scale 1 |
| 祭壇の葉 | 37 | 0.22秒 | 0.02秒 | 移動なし、根元Pivotで0.94→1 |
| 書見台／戦果碑 | 2 | 0.34秒 | 0.10秒 | 上120、Scale 1 |
| 小道具の葉 | 16 | 0.22秒 | 0.02秒 | 本体2個着地後、移動なし、根元Pivotで0.94→1 |

開始位置はHomeBuildRootのUIローカル単位。各石材段階は最後の石の着地を待って次へ進み、葉の前に0.06秒待つ。石材・小道具は加速して落下し、終盤だけ着地を収める。葉は位置固定でalphaとscaleのみ変える。物理演算は使わない。

小道具の葉16枚がすべて表示されるまで待ち、全構築・完成保持・HUDフェードの設定時間は約5.03秒。実時間には読み込み、フレーム境界の待機、記録処理の時間が加わる。

小道具の葉は本体着地後に葉単位で生やす。Loading Overlayは全葉を含む構築と0.10秒の完成保持まで維持する。AngelとHome HUDの表示開始に合わせてLoadingを0.20秒でフェードアウトする。石材・葉のRaycastは無効、小道具自身のButtonはHUDフェード終了後に有効にする。通常MainScene起動、Battle復帰、Tab復帰では同じ個別Spriteを完成位置・完成alpha・完成scaleで即時表示する。

## 再生成と作成時配線

1. `sharp` が使えるNode.jsで `node Tools/HomeAltar/build-parts.mjs` と `node Tools/HomeAltar/build-props.mjs` を順に実行する。標準の依存解決で見つからない場合は、ライブラリのディレクトリを `CODEX_NODE_MODULES` へ指定する。
2. UnityのImport・Compile完了を待ち、MainSceneを開く。
3. `Tools/The Red Fever/Rebuild Home Altar Assembly` を実行する。

出力は `Assets/Resources/Sprite/Home/AltarBuild/Individual/`、配置情報は同フォルダの `parts.json` と `props.json`。Builderが2つのmanifestを結合し、Inspectorの必須参照と演出用小道具のButtonを配線する。ランタイムはパス検索や `Resources.Load` を使わない。旧祭壇Image・旧小道具Buttonは非表示の配置参照としてMainSceneに残す。旧Granularと旧大葉Spriteは新演出から参照しない。

## Play Modeの確認

Title起点のPlay Modeで `Tools/The Red Fever/Capture Home Entrance Play Mode` を実行すると、開始Buttonへの画面Raycastを検査してクリックし、各描画フレームと表示状態を `Temp/HomeEntrance/RetainedPlay/` へ記録する。外部ツールへのフォーカス移動でも、この確認中だけ描画を継続する。

`Tools/The Red Fever/Compare Retained Home Altar` はEditor QA専用。小道具を非表示にして祭壇を比較した後、比較中だけAngelを非表示・旧小道具を一時表示して小道具単独を比較し、元の表示状態へ復帰する。比較用の一時表示と、通常動作時の継続表示を区別する。

2026-10-12のTitle起点Editor確認は181描画フレーム・約9.169秒を記録した。最初のMainSceneは約1.363秒、小道具葉の初表示は約7.783秒、全葉完了は約8.625秒、Home操作可能は約8.961秒。読み込み・frame境界・記録処理を含む実時間であり、設定時間5.03秒とは区別する。

- 完成パーツ92個、新Button2個有効。旧祭壇・旧Buttonの表示、Sprite交換、GameObject交換はいずれも0。
- 背景欠落、早期Loading解除、構築中のButton早期有効化は各0frame。小道具葉の根元最大誤差は約 `2.3841858e-7` world単位で、位置計算の誤差範囲。
- 通常MainScene起動でも同じ構成を即時表示。Settings／AchievementsのDialog開閉とProgress → HomeのTab往復を画面Raycast・pointer clickで確認した。START Buttonも操作可能で画面Raycastがhandlerへ到達したが、Battle遷移の押下は行っていない。
- 必須参照欠落0件、Consoleエラー0件・警告0件。Play停止後も0件。
- Unity画面比較では祭壇の平均channel差約0.0250037/255、差3超は409,920pixel中1,824pixel。小道具の平均channel差約0.00118316/255、差3超は81pixel。輪郭samplingの差を含むが、目視で目立つ外見差は見られなかった。

`node Tools/HomeAltar/report-play.mjs` は同じ `sharp` 依存を使い、記録フレームから確認用GIF・コンタクトシート・集計JSONを作る。永続記録は `docs/captures/HomeEntrance/home-entrance-retained-2026-10-12.gif`、同名 `.png`、`home-completed-retained-2026-10-12.png`、`qa-summary-retained-2026-10-12.json`、`trace-retained-2026-10-12.json`。旧構成の記録も保持する。Android実機・異なる解像度・BattleScene復帰の確認は今回のEditor QAに含まず、全体TODOのTitleScene残作業で追跡する。
