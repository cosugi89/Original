# Path-Activation System v2 TODO

- 最終更新: 2026-04-30
- 参照元: `docs/path-activation-system-v2-spec.md`
- 方針: 仕様の理想と現在のコード差分を埋めるための実装 TODO を優先順で並べる

## 0. 進め方メモ

- `[x]` は現状 repo 上で概ね存在するもの
- `[ ]` は未着手または未接続
- 優先度は `P0 > P1 > P2 > P3`

## 1. P0: 正本化とデータ整合

- [x] ステージ ID を `int` と `string` の混在から 1 つに統一する
- [x] `BattleSceneTransitionState`、`StageData`、`BattleStageData`、`BattleProgressService` のキー設計を揃える
- [ ] `StageDatabase` / `StageMasterDatabase` / `StageMasterData` の命名を repo 全体で統一する
- [x] `MasterDataResourceLoader` が期待する `BattleStageMasterCatalog` 実アセットを復元するか、未使用なら読み込み経路を整理する
- [ ] 現在の `StageDatabase.asset` の内容を最新クラス構造と照合し、参照切れや古い型名を解消する

## 2. P1: バトルコアの本実装

- [x] ターン進行コントローラのデモ版を持つ
- [x] Attack、Jump、Roll、Dance、Goal、危険の最小解決ルールを持つ
- [x] Skill パレットのランタイム状態を持つ
- [x] `BattleNodeType`、`BattleEnemyActionType`、`BattleSkillSlotRuntime`、`BattleTurnResolutionReport` を本番名で追加し、`BattleDemo*` 名称依存を薄くする
- [x] `BattleBoardState` と `BattlePathDraft` を追加し、盤面状態と入力中状態を `BattleScene` から分離する
- [x] `BattlePathRuleEvaluator` と `BattlePathTracer` を追加し、8 方向、再訪、交差、Goal 後延長を pure C# で判定する
- [x] `BattleTurnResolver` を追加し、解決ロジックを `BattleScene` から切り出す
- [x] `BattleBoardController`、`BattleCellView`、`BattleTraceLineView`、`BattleTraceInputHandler`、`BattleHudController` を追加し、表示層を分離する
- [ ] `BattleDemoTurnScript` と `BattleDemoContentFactory` を回帰確認用フィクスチャへ限定し、本番ターン進行の依存先から外す
- [ ] プリセットターンスクリプト再生ではなく、実際の 5x5 盤面とノード配置を使う
- [x] ドラッグによるパス入力を実装する
- [x] 8 方向接続判定を実装する
- [x] 再訪、交差、Goal 後の延長を不正パスとして弾く
- [x] Goal 未到達で離した場合の引き直しフローを実装する
- [ ] 不正パス時の引き直しフローを実装する
- [x] 同ターン中の引き直しで盤面と Skill 選択が維持されるようにする

## 3. P1: 解決ルールの不足分

- [x] Skill 選択時に Attack を Skill に変換する
- [x] Skill 選択時は Jump Attack や Double Attack より Skill を優先する
- [ ] 実際に解決された Attack 回数に応じて `AttackChargeGain` を反映する
- [ ] Goal 到達後にのみ Skill 消費する現仕様を UI とログに明示する
- [ ] Goal に到達せず Attack も踏まなかった場合の Skill 不消費を仕様どおり担保する
- [ ] 危険の「攻撃グループ識別子」を実データとして導入する
- [ ] 複数危険マスが同一グループでも被弾 1 回だけになる処理をデータ主導にする
- [ ] 被弾後に後続スタック破棄と Goal 不発を明示的に演出へ接続する

## 4. P1: 敵行動と盤面生成

- [x] 敵行動種別として `NormalAttack`、`Skill`、`Dance` を持つ
- [ ] 敵の行動選択をターンスクリプト固定ではなく AI 重みで決める
- [ ] `Balance` 型の行動比率を実データ化する
- [ ] 敵 Skill による危険配置ルールを定義する
- [ ] 敵 Dance による次ターン危険強化を本来の仕様に合わせて「危険マス数 1.5 倍」へ整理する
- [ ] 現在の「危険ダメージ 1.5 倍」実装を残すか置き換えるかを決める
- [ ] 強敵、ボス、大技ターン向けの例外盤面ルールを定義する

## 5. P1: UI 接続

- [x] Skill ボタンの選択ハイライトがある
- [x] `turnNumberText` を毎ターン更新する
- [ ] `waveNumberText` を実際の wave 概念に接続するか削除する
- [ ] `background` にステージ背景を反映する
- [ ] 敵名、プレイヤー HP、敵 HP を HUD に反映する
- [ ] Skill パレットの Ready、Charging、Locked、Empty を UI に表示する
- [ ] Skill 準備ゲージを表示する
- [ ] 確定情報テキストを実 UI として表示する
- [ ] `次ターン危険強化` 表示を実装する
- [ ] 行動ログ UI を実装する
- [ ] 属性相性のダメージ装飾差を実装する

## 6. P1: ステージ遷移と進捗

- [x] `HomeScreen` からバトルシーンへ遷移できる
- [x] `GetNextStageId()` を実装する
- [x] 戦闘開始時に `BattleProgressService` の最終選択ステージを更新する
- [x] 勝利時に `RecordStageClear()` を呼ぶ
- [ ] 初期解放ステージと未解放ステージの選択制御を実装する
- [ ] 戦闘失敗時の再挑戦導線を実装する
- [ ] ステージ選択画面を追加し、`BattleStageMasterCatalog` と進捗を接続する

## 7. P2: マスターデータ拡張

- [ ] 敵を名前と HP だけでなく、行動傾向、危険配置傾向、特殊敗北閾値、演出テキストまで持てるようにする
- [ ] 武器データに Skill パレット構成を持たせる
- [ ] アイテムデータに Dance 効果を持たせる
- [ ] 胴装備データに属性耐性を持たせる
- [ ] 属性定義マスタを追加する
- [ ] 盤面テンプレートや危険配置テンプレートのデータ化を行う

## 8. P2: 特殊敗北と演出

- [ ] Goal 到達失敗の累積カウントを内部状態として実装する
- [ ] 特殊敗北閾値を敵ごとに設定できるようにする
- [ ] 特殊敗北成立時の敗北演出を実装する
- [ ] 特殊敗北カウントは UI 非表示のまま、短文とフレーバーで圧を出す
- [ ] 危険ターン演出を盤面変化と短いセリフに接続する

## 9. P2: 戦闘結果とセーブ

- [x] バトル進捗保存用のモデルは存在する
- [ ] スコア算出を定義する
- [ ] ランク算出を定義する
- [ ] クリアタイム計測を定義する
- [ ] 勝利時に `BestScore`、`BestRank`、`BestClearTimeSeconds` を更新する
- [ ] 戦闘終了時に `GameSaveService.SaveSession()` を呼ぶ導線を整える

## 10. P3: 将来拡張

- [ ] Goal 効果の固有化
- [ ] Dance Goal の持ち越し効果
- [ ] 頭装備
- [ ] 左手装備
- [ ] 状態異常 Skill
- [ ] スタック途中で発動する持続効果
- [ ] 盤面や Goal に干渉する Skill
- [ ] 特殊盤面としての Skill ノード
- [ ] 複数敵
- [ ] ランダム盤面生成

## 11. まず着手する順番

1. ステージ ID とマスターデータ命名の整合を取る
2. 実グリッドとパス入力を入れる
3. Goal 到達と引き直しの正式フローを入れる
4. Skill パレット UI とゲージをつなぐ
5. 敵行動から盤面生成する仕組みに置き換える
6. 勝敗結果を進捗とセーブへ接続する
