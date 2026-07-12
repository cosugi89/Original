# プロジェクト全体 TODO

- 最終更新: 2026-07-12
- 主な参照元: `docs/specs/戦闘システム仕様書.md`, `docs/specs/戦闘画面仕様書.md`, `docs/specs/装備仕様書.md`, `docs/specs/スキル仕様書.md`, `docs/specs/タグ仕様書.md`
- 位置づけ: プロジェクト全体の実装 TODO。現時点では Path-Activation System v2 関連タスクが中心。
- 方針: 現行実装を正として、仕様との差分と未接続箇所を優先順で並べる
- 状態見直し: 2026-07-12 に属性採用方針を撤回し、タグ仕様書と属性撤去 TODO を追加

## 0. 進め方メモ

- `[x]` は現状 repo 上で概ね存在するもの
- `[ ]` は未着手、未接続、または仕様/UX が未確定のもの
- 優先度は `P0 > P1 > P2 > P3`
- ゲーム改善アイデアは `8. ゲーム改善アイデア TODO` に置き、採用方針が固まったら該当する実装 TODO や仕様書へ昇格する

## 1. P0: 基盤とデータ整合

- [x] ステージ ID を `int` と `string` の混在から数値 `StageId` に統一する
- [x] `BattleSceneTransitionState`、`StageData`、`StageMasterData`、`BattleProgressService` のキー設計を揃える
- [x] `StageDatabase` / `StageMasterDatabase` / `StageMasterData` の命名を repo 全体で統一する
- [x] `MasterDataResourceLoader` のステージ一覧読込を `StageMasterDatabase` ベースへ統一する
- [x] `StageMasterData`、`StageBattleEnemyMasterData`、`StageBattlePatternMasterData` の master 構造を追加する
- [x] `StageBattlePatternMasterData.Board` と `CellPlacements` を runtime DTO へ変換できるようにする
- [x] `StageBattlePatternBoardEditorWindow` で pattern 盤面を編集できるようにする

## 2. P1: バトル入力とターン解決

- [x] `BattleScene` のターン供給元を `StageData.Enemy.Patterns` へ差し替える
- [x] 敵 pattern 候補から 1 件をランダム選択し、直前 pattern の連続選択を避ける
- [x] `StageBattlePatternMasterData.Board` / `CellPlacements` から実盤面を生成する
- [x] `BattleBoardState` と `BattlePathDraft` を追加し、盤面状態と入力中状態を `BattleScene` から分離する
- [x] `BattlePathRuleEvaluator` と `BattlePathTracer` で 8 方向、再訪、交差、Goal 後延長を判定する
- [x] ドラッグによるパス入力を実装する
- [x] Goal 未到達で離した場合の引き直しフローを実装する
- [x] 同ターン中の引き直しで盤面と Skill 選択を維持する
- [x] `BattleTurnResolver` を追加し、Attack、Jump、Roll、Dance、Goal、危険の最小解決ルールを切り出す
- [x] Skill 選択時に Attack を Skill に変換する
- [x] Skill 選択時は Jump Attack や Double Attack より Skill を優先する
- [x] `SkillSlotRuntime` で Skill パレットの Ready、Charge、消費状態を扱う
- [x] `BattleDamageCalculator` と `BattlePlayerStats` で通常攻撃、Double Attack、Jump Attack、Skill Damage 効果を計算する
- [x] 旧属性相性の計算を `BattleTurnResolver` に接続する
- [x] 旧属性実装として、プレイヤー右手武器属性 / 胴装備補正と、敵右手武器属性 / 胴装備補正を戦闘コンテキストへ流す
- [x] 危険グループを runtime で上下左右連結から自動付与し、同一グループは被弾 1 回として扱う
- [x] 敵行動種別として `NormalAttack`、`Skill`、`Dance` を持つ
- [x] 敵 Dance 後の次ターン危険強化を、現行仕様どおり危険ダメージ 1.5 倍として扱う
- [ ] ドラッグ入力中はパス以外の操作を受け付けない入力ロックを実装する
- [ ] 不正入力時の UX を整理する。現状の「不正マスだけ拒否して継続」と、全リセット/失敗累積の扱いを仕様として確定する
- [ ] 実際に解決された Attack 回数に応じて `AttackChargeGain` を反映する
- [ ] Goal ノード解決時にのみ Skill 消費する現仕様を UI とログに明示する
- [ ] 被弾停止、プレイヤー敗北、敵撃破などで Goal 解決前にスタックが終了した場合の Skill 不消費をテストで担保する
- [ ] 被弾後に後続スタック破棄と Goal 不発を明示的に演出へ接続する

## 3. P1: UI と表示接続

- [x] `BattleBoardController`、`BattleCellView`、`BattleTraceLineView`、`BattleTraceInputHandler`、`BattleHudController` を追加し、表示層を分離する
- [x] `BattleCellView` prefab 生成方式で盤面セルを描画できる
- [x] Skill ボタンの選択ハイライトがある
- [x] Skill ボタンにアイコン、チャージ fill、使用不可 overlay を反映する
- [x] `turnNumberText` と `waveNumberText` を毎ターン更新する
- [x] `background` と `previewImage` にステージ画像を反映する
- [x] `playerHPText` を `BattleHudController` 経由で更新する
- [x] 旧属性相性をプレビュー文言とログに暫定表示する
- [x] `docs/specs/戦闘画面仕様書.md` に縦長 9:16 前提、領域分割、戦闘イベント通知、常設ログ廃止方針を整理する
- [ ] 縦長 9:16 前提の `BattleScreenRoot` を作り、最上部 HUD / 上部バトル演出 / 情報・コマンド / 最下部盤面 / Overlay の領域を分ける
- [ ] `BattleBoardView` が盤面領域、セル生成、セル更新、パス線、入力フィードバックを所有する形へ整理する
- [ ] `EnemyUnitView` を作り、敵名、敵 HP、敵戦型ゲージを敵側表示へ寄せる
- [ ] `PlayerAreaView` を作り、プレイヤー HP とプレイヤー状態をプレイヤー側表示へ寄せる
- [ ] `BattleEventNoticeView` を実装し、技名、Goal、回避、被弾、危険強化、敵の体勢変化などを短命通知として表示する
- [ ] 常設の `confirmText` と行動ログ履歴を初期画面から外し、不正入力は盤面の赤表示、揺れ、トースト、バイブレーション候補へ分担する
- [ ] Skill パレットの Ready、Charging、Locked、Empty を見分けやすい本 UI として整える
- [ ] `waveNumberText` を実際の wave 概念に接続するか削除する
- [ ] 敵戦型ゲージ UI と `Guard / Burst / Rush` 表示を実装する
- [ ] タグ / セットボーナス表示 UI を実装する
- [ ] 勝利、敗北、再挑戦の本演出を実装する
- [ ] プレイヤー/敵のセリフ、フレーバー表示を実装する。行動履歴ログは初期画面には置かない

## 4. P1: ステージ遷移、進捗、セーブ

- [x] `HomeScreen` からバトルシーンへ遷移できる
- [x] `HomeScreen.GetNextStageId()` と debug stage 指定を持つ
- [x] 戦闘開始時に `BattleProgressService` の現在ステージ/最終選択ステージを更新する
- [x] 勝利時に `RecordStageClear()` を呼ぶ
- [x] 勝利時に次ステージを `UnlockStage()` する
- [x] `BattleProgressService` が初期解放ステージを進捗データへ反映する
- [x] バトル進捗保存用のモデルは存在する
- [x] `RecordStageClear()` は `BestScore`、`BestRank`、`BestClearTimeSeconds` を更新できる
- [ ] ステージ選択画面を追加し、`StageMasterDatabase` と進捗を接続する
- [ ] ステージ選択画面で未解放ステージの選択制御を実装する
- [ ] 戦闘失敗時の再挑戦導線を実装する
- [ ] スコア、ランク、クリアタイムの算出ルールを定義する
- [ ] 勝利時に実際の `score`、`rank`、`clearTimeSeconds` を `RecordStageClear()` へ渡す
- [ ] 戦闘終了時に `GameSaveService.SaveSession()` を呼ぶ導線を整える

## 5. P2: 敵行動、戦型、特殊敗北

- [ ] 敵の行動選択を pattern ランダムから AI / 重み / 難度ルールへ拡張するか決める
- [ ] 敵行動選択ルールを `StageBattleEnemyMasterData` などの実データへ追加する
- [ ] 敵 Skill による危険配置ルールを定義する
- [ ] 強敵、ボス、大技ターン向けの例外盤面ルールを定義する
- [ ] `StageBattleEnemyMasterData` に `EnemyKind` または敵戦型情報を追加する
- [ ] `Guard / Burst / Rush` の戦型ロジックと battle type settings を実装する
- [ ] Goal 到達失敗の累積カウントを内部状態として実装する
- [ ] 特殊敗北閾値を敵ごとに設定できるようにする
- [ ] 特殊敗北成立時の敗北演出を実装する
- [ ] 特殊敗北カウントは UI 非表示のまま、短文とフレーバーで圧を出す
- [ ] `StageBattleEnemyMasterData` にタイミング別フレーバーテキスト群を追加する
- [ ] フレーバーテキスト群は `Dictionary` ではなく `TriggerType + List<string>` の serializable entry list で持つ

## 6. P2: マスターデータ、ロードアウト、演出

- [x] `EquipmentMasterData.AssignableSkills` で装備ごとの候補スキルを持てる
- [x] `EquipmentMasterData.AssignableSkills` は DTO 経由で runtime へ渡せる
- [x] 旧属性実装として、右手武器データに通常攻撃用の属性を持たせる
- [x] 旧属性実装として、胴装備データに属性補正を持たせる
- [x] 旧属性実装として、属性補正は `Dictionary` ではなく `Attribute + DamagePercent` の serializable entry list で持つ
- [x] 旧属性実装として、弱点、耐性、無効は同一の属性補正テーブルに統合する
- [x] `EquipmentMasterData` に装備カテゴリ補助タグを複数持てるようにする
- [x] 旧属性実装として、属性定義マスタを追加する
- [x] 旧属性実装として、`BattleSkillMasterData` に複数属性参照を追加する
- [x] `BattleSkillOperationData` に効果種別、対象、対象パラメータ、値種別、値、継続ターン、成功率を持たせる
- [x] `BattleSkillMasterData` に `EffectAnimationMasterData` 参照を追加する
- [x] `EffectAnimationMasterData` に表示名、説明、キャラ animation 名、VFX キー、SE キー、待機秒数を持たせる
- [x] Damage 効果の `Multiplier` / `Percentage` / `FixedValue` を `BattleDamageCalculator` で解決する
- [x] `docs/specs/スキル仕様書.md` にスキル効果一覧、採用区分、対象、値種別、参照パラメータ、成功率方針を整理する
- [ ] `EquipmentMasterData.AssignableSkills` の上限を最大 2 件へ変更し、validation / tooltip / docs を揃える
- [ ] 旧属性実装を撤去または無効化し、戦闘仕様から属性相性を外す
- [ ] `BattleSkillOperationType` と周辺データを、`Damage` / `ChargeGain` / `Heal` / `GuardGain` などのスキル仕様書の効果区分に合わせて再整理する
- [ ] `BattleSkillParameterType.Speed` を削除するか、互換用に残すか決めて実装へ反映する
- [ ] スキル効果を配列順に複数解決し、途中効果が失敗しても後続効果を継続する処理を実装する
- [ ] `SuccessRate` を戦闘解決へ接続し、タグ、状態、盤面結果、敵行動などから補正できる拡張点を用意する
- [ ] `ChargeGain` / `Heal` / `GuardGain` の初期スキル効果を実装する
- [ ] `Buff` / `Debuff` を `BattleStatModifiers` 相当の一時補正として扱うランタイムを実装する
- [ ] `EquipmentMasterData.CategoryTags` をタグ一致ボーナス判定へ接続する
- [ ] タグ一致ボーナスの成立条件を決める。初期は全一致または一定数一致のどちらかに絞る
- [ ] タグ一致ボーナスを `BattleStatModifiers` 相当の追加補正として適用する
- [ ] 装備詳細 UI にタグとタグ一致ボーナスを表示する
- [ ] 装備候補スキル群から実戦用 4 枠を選ぶロードアウトモデルを追加する
- [ ] 候補スキル群から実戦用 4 枠を選ぶ画面を追加し、選択結果を `UserData` に保存する
- [ ] `BattleScene` の固定 Skill スロット初期化を、保存済みロードアウトから構築する形へ差し替える
- [ ] 武器ごとの Skill パレット構成をマスターデータとして持つか、保存済みロードアウトだけで扱うか決める
- [ ] アイテムデータに Dance 効果を持たせる
- [ ] Skill 説明文補助の扱いを決める。現行 runtime の `DescriptionSupplement` は空文字固定
- [ ] 実際に使用する Effect / VFX / SE 資産を作成する
- [ ] `EffectAnimationData` の実再生処理を `BattleScene` / runtime に接続する
- [ ] 盤面テンプレートや危険配置テンプレートを、単発 pattern 以外で再利用できるデータとして整理する

## 7. P3: 将来拡張

- [ ] Goal 効果の固有化
- [ ] Dance Goal の持ち越し効果
- [ ] 頭装備
- [ ] 左手装備
- [ ] `Status` 系スキル効果
- [ ] スタック途中で発動する持続効果
- [ ] `BoardManipulation` / `HazardControl` 系スキル効果
- [ ] `ExtraAction` 系スキル効果
- [ ] 特殊盤面としての Skill ノード
- [ ] 複数敵
- [ ] ランダム盤面生成

## 8. ゲーム改善アイデア TODO

このセクションは、仕様化前の「ゲームをもっと良くする案」を置く場所。
実装する方針が固まったものは、P0〜P3 の該当セクション、仕様書、または個別設計へ移す。
採用しない案、重複した案、古くなった案は残し続けずに削除する。

### 8.1 体験・手触り

### 8.2 戦闘の深み

### 8.3 成長・収集

### 8.4 演出・世界観

## 9. まず着手する順番

1. 縦長 9:16 前提の `BattleScreenRoot` と主要領域を作る
2. `BattleBoardView` が盤面領域、セル生成、セル更新、パス線、入力フィードバックを所有する形へ整理する
3. `EnemyUnitView`、`PlayerAreaView`、`SkillPaletteView` を作り、情報を持ち主の近くへ寄せる
4. `BattleEventNoticeView` を実装し、常設 `confirmText` と行動ログ履歴を初期画面から外す
5. ドラッグ入力中の操作ロックを実装する
6. Skill 消費条件の UI 明示とテストを追加する
7. `AttackChargeGain` を解決済み Attack 数に応じて反映する
8. 勝敗演出、再挑戦導線、戦闘終了時 save を接続する
9. ステージ選択画面と未解放ステージ制御を実装する
10. ロードアウト保存と Skill スロット初期化を正式接続する
