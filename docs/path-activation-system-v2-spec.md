# Path-Activation System v2 仕様書

- 最終更新: 2026-05-09
- 文書状態: 現行の正本
- 位置づけ: 本書は v1 を前提にしない。今後仕様が衝突した場合は、原則として本書を優先する。

## 1. 企画の中核コンセプト

Path-Activation System v2 は、敵の攻撃を見ながら 1 ターンに 1 本の行動パスを組み立て、通過順に行動を解決する思考型ターン制バトルである。

面白さの中心は次の 3 点にある。

- そのターンでどのルートを取るかという攻め方の選択
- Jump / Roll をどこで使い、何を避けて何を受けるかという危険処理の選択
- 無傷では届かない局面で、被弾込みでも Goal に到達するかを決める不利受容の選択

v2 では Skill を盤面ノードではなく盤外 UI の Skill パレットから事前選択する方式に改める。Skill は主役の入力ではなく、そのターンの Attack ノードの価値を変える準備要素として扱う。

## 2. スコープと前提条件

### 2.1 戦闘人数

- 戦闘は 1 対 1
- 初期実装では敵は 1 体のみ

### 2.2 進行形式

- ターン制
- プレイヤーは 1 ターンに 1 本だけパスを引く
- 敵は 1 ターンに基本 1 アクションのみ行う

### 2.3 盤面サイズ

- 盤面サイズは `StageBattlePatternMasterData.Board` で定義する
- 現行の既定値は 7 列 x 8 行
- 開始マスの既定値は中央列の最下段

### 2.4 レイヤー構成

- 上部レイヤー: プレイヤー、敵、ダメージ演出、勝敗演出
- 下部レイヤー: グリッド、ノード、危険、Start、Goal

## 3. 盤面要素

### 3.1 標準ノード種別

v2 標準盤面では次を扱う。

- Start
- Goal
- Attack
- Jump
- Roll
- Dance
- 危険
- 空白マス

### 3.2 Skill の扱い

- v2 では Skill は標準盤面ノードとして配置しない
- Skill は盤外 UI の Skill パレットから事前選択する
- 選択中 Skill は、そのターン中に通過した Attack ノードすべてを Skill 行動に変換する
- 特殊盤面として Skill ノードを持つ拡張は将来候補に残すが、標準仕様には含めない

### 3.3 マスの重なり

- 1 マスには 1 要素のみ配置可能
- ノードと危険は重複しない
- Goal の上に危険は置かない

### 3.4 空白マス

- 単なる通路として通過可能
- 固有効果は持たない
- Jump / Roll の保持中でも通過可能

## 4. 盤面配置方針

### 4.1 基本方針

- 当面は固定配置
- ランダム生成は将来拡張

### 4.2 Start / Goal

- Start は毎ターン固定位置
- Goal は敵やステージに応じて位置が変わる
- Goal は複数存在可能
- 初期実装では 1 個から 2 個程度を基準とする

### 4.3 Goal 配置思想

- Goal は危険の先に置かれることが多い
- ただし毎回回避前提にはしない
- 安全寄り Goal と危険越え前提の高価値 Goal の両方を許容する

### 4.4 既定盤面の目安

- Attack: 3 から 5
- Jump: 1 から 2
- Roll: 1 から 2
- Dance: 0 から 1
- Skill ノード: 0
- Goal: 1 から 2
- 危険: 5 から 9
- 空白は一定数残し、常に最大密度にはしない

### 4.5 回避ノード最低保証

- 回避なしでは Goal 到達不可の盤面では Jump または Roll を最低 1 つ配置する
- 回避なしでも Goal に届く盤面では、回避ノードなしを許容する

## 5. パス入力ルール

### 5.1 接続方向

- 4 方向隣接で接続可能

### 5.2 不正パス

以下は不正パスとする。

- 既に引いた線との交差
- 同じマスへの再訪
- Goal 到達後にさらに線を伸ばす行為

### 5.3 Goal の扱い

- Goal は終点専用マス
- パス途中の通過は不可
- Goal に触れた時点で確定可能状態に入る
- Goal 到達後に指が Goal から外れても確定可能状態は維持する
- 指を離した時点で確定する

### 5.4 Goal 未到達で指を離した場合

- 入力失敗
- パスは確定しない
- Start から引き直し可能
- 盤面は同ターン中変化しない
- 内部的には特殊敗北用の失敗累積対象になる

### 5.5 Skill 選択のタイミング

- Skill はパス開始前にのみ選択可能
- パス入力中は Skill を切り替えられない
- 同ターン中の引き直しでは選択済み Skill 状態を維持する

## 6. スタックと解決ルール

### 6.1 仮スタック

- パス入力中、通過したノードや危険は仮スタックとして扱う
- Goal 確定前は未確定

### 6.2 確定条件

- Goal に到達し、指を離した時点で仮スタックが確定する
- Goal に到達できなかった場合は不発

### 6.3 解決順

- 通過順にそのまま順次解決する
- ダメージ、回避、被弾、補助効果は都度即時反映する

### 6.4 HP0 時の処理

- スタック解決中でもどちらかの HP が 0 になった時点で戦闘終了
- 後続スタックは解決しない

### 6.5 Skill 変換の反映タイミング

- そのターンに Skill を選択している場合、Attack ノード解決時に選択中 Skill へ変換する
- そのターン中に通過した Attack はすべて同じ Skill に変化する
- Attack を複数回通過した場合は同一 Skill が複数回解決される

## 7. 各要素の仕様

### 7.1 Attack

- 基本攻撃ノード
- Skill 未選択時は通常攻撃として解決する
- Attack を 2 回通過するとダブルアタック化する
- Jump 後の Attack はジャンプアタック化する
- Jump による通常危険回避を挟んでもジャンプアタックは成立する

設計上の基準値:

- 通常 Attack: 100
- ダブル Attack 追撃: 125
- Jump Attack: 150

Skill 選択中の扱い:

- Attack ノードは通常 Attack として解決しない
- 選択中 Skill の行動として解決する
- ダブルアタック化やジャンプアタック化より Skill 変換を優先する

### 7.2 Jump

- 通常危険を回避できる
- Attack をジャンプアタック化できる
- 攻め寄りの回避ノード

保持ルール:

- 空白マスのみを何マス挟んでも保持する
- 最初に接触した通常危険グループに対して回避成立
- 危険回避後でも Attack につながればジャンプアタック成立
- Attack、Dance、Roll、その他ノードを挟むと消失する
- Skill 選択中は Attack が Skill 化されるため、Jump の攻撃強化は発生しない

### 7.3 Roll

- 純粋な回避専用ノード
- 通常危険とスキル危険の両方に対応する
- 攻撃派生は持たない

保持ルール:

- 空白マスのみを何マス挟んでも保持する
- 最初に接触した対応危険グループに対して回避成立
- 別ノードを 1 つでも挟むと消失する

### 7.4 Dance

- 支援ノード
- 効果内容はアイテム装備に依存する

想定役割:

- 自己強化
- 回復
- 敵弱体
- その他支援効果

初期実装では盤面上の存在と将来拡張の窓口を先に定義し、詳細効果は後追いでもよい。

### 7.5 Skill

- Attack の上位互換ではなく別カテゴリの個別行動
- 内容や見た目は武器装備に依存する
- 初期は広範囲の危険を置く純粋攻撃寄りを想定する

Skill master が将来的に持つ代表項目:

- 属性
- 効果種別
- `EffectAnimationMasterData` 参照
- 説明文補助

補足:

- 対象指定は将来拡張とし、初期段階では単体前提でよい
- 演出は文字列キーではなく、`EffectAnimationMasterData` を direct 参照する形でもよい
- `EffectAnimationMasterData` 側は、少なくとも `EffectAnimationId`、表示名、説明、キャラ animation 名、VFX 用キー、SE 用キー、待機秒数を持つ

### 7.6 Skill パレット

- 各武器は最大 4 スロットの Skill パレットを持つ
- スロット状態は Ready、Charging、Locked、Empty を持つ
- 1 ターンに選択できる Skill は 1 種類のみ
- Skill はパス開始前に選択する
- Goal 未到達や不正パスで引き直す場合は選択済み状態を維持する
- Skill 選択状態でパスが確定した時点でその Skill は使用扱いになる
- Attack を 1 回も通らず不発でも、そのターンの選択は消費する

### 7.7 装備候補スキルと実戦ロードアウト

- `EquipmentMasterData` は、その装備に紐づく候補スキルを最大 3 件まで持てる
- 候補スキルは `BattleSkillMasterData` として個別 ScriptableObject で管理する
- 候補スキルは「その装備が提供できるスキル一覧」であり、そのまま戦闘で使う 4 枠とは別概念とする
- 実際にバトルで使用できる Skill パレット 4 枠は、現在装備中の各装備が持つ候補スキル群から別途選抜して構成する
- 例: 3 装備がそれぞれ 3 件の候補スキルを持つ場合、最大 9 件の候補群から 4 件を選ぶ
- この「候補スキル」と「実戦ロードアウト 4 枠」は保存上も分けて扱える設計にする
- 現時点の実装では、この選抜ロジックや保存経路はまだ未実装であり、将来実装項目とする

### 7.8 Goal

- パスの終点
- 仮スタック確定トリガー
- 被弾せず最後まで解決が到達した場合のみ Goal 効果が有効

設計方針:

- 初期実装では単一 Goal 効果でよい
- Goal 固有補正より、到達難度と到達直前の状態差で価値差を作る
- 将来は Jump Goal、Dance Goal などの拡張を許容する

### 7.9 危険

- 危険は盤面上の特殊マス
- パス上を通行可能
- 通過時に被弾または回避判定が発生する

危険種別:

- 通常危険
- スキル危険

攻撃グループ:

- 危険は内部的に攻撃グループ識別子を持つ
- 同一グループの危険を複数マス踏んでも被弾判定は 1 回だけ
- Jump / Roll が成立した場合はそのグループ全体を回避扱いにする

被弾時:

- 被弾した危険の解決までは成立
- その後の未解決スタックは破棄
- Goal 効果も不発
- 被弾前に成立済みの効果は残る

## 8. 回避と被弾の詳細

### 8.1 回避成立条件

- Jump は通常危険のみ対応
- Roll は通常危険とスキル危険に対応
- どちらも空白マスは保持可能
- 別ノードを挟むと消失

### 8.2 被弾前提盤面

- 通常戦でも被弾前提でしか Goal に届かない盤面を許容する
- 回避を使えば被害を軽減できる盤面はある程度の頻度で出る前提とする
- 回避ノードを使っても被弾回避不可の盤面は、主に強敵、ボス、大技ターンの例外局面で許容する

## 9. 敵仕様

### 9.1 基本方針

- 敵の行動カテゴリはプレイヤーと同系統の概念を使う
- ただし内部処理は敵専用の簡略ルールでよい
- 敵は 1 ターンに基本 1 アクション
- 敵はプレイヤーの Skill パレット UI を使わない

### 9.2 行動差別化

- 敵戦型
- 装備
- 行動傾向
- Skill 使用率
- Dance 使用率
- 危険配置パターン

### 9.3 敵戦型

- 各敵は 1 つの敵戦型を持つ
- 初期実装の敵戦型は `Guard型`、`Burst型`、`Rush型` の 3 種とする
- 敵戦型は、その敵がどう圧をかけ、どのような Break 成功条件や攻めどきを持つかを規定する
- 直接ダメージは、原則としてプレイヤーが危険地帯に乗ったタイミングでのみ発生する
- 敵戦型は主に、危険地帯の量、危険グループの分かれ方、Break の有無、Break 後の隙の質を変える
- 基本解は `Attack` ノード通過数だけで成立する形を優先し、特定 Skill 系統の装備を前提条件にしない

### 9.4 Guard型

- 敵ゲージは純粋な Guard 値として扱う
- `Attack` ノード通過やそれに準ずる攻撃行動で Guard を削る
- Guard が `0` になると `Break` とする
- Guard型の `Break` は、`Burst型` のような 1 ターン完全無防備ではなく、装甲が剥がれて `軽装` 状態へ移行する意味で使う
- `軽装` 中は被ダメージが増える
- その代わり敵は身軽になり、次ターン以降の危険地帯は増える
- 初期実装では `軽装` は複数ターン継続する前提とし、具体ターン数は別途調整する
- `軽装` 終了後は通常状態へ戻り、再び Guard を持つ

### 9.5 Burst型

- 敵ゲージは大技までの進行度を表す
- ゲージは基本的にターン経過で進む
- これとは別に内部的な Break 判定値を持ち、`Attack` ノード通過やそれに準ずる攻撃行動で蓄積する
- Break 判定値が必要量に達した時点で `Break` とする
- Burst型の `Break` 成功時は、次の 1 ターンだけ敵が完全無防備になり、そのターンの危険地帯は `0` になる
- `Break` に間に合わなかった場合は、次ターンに高威力の回避困難な危険地帯を置く
- 初期実装では、ひび演出のような別レイヤーは持たず、ゲージ色の変化で Break 成功に近い状態を示す

### 9.6 Rush型

- 敵ゲージはそのターン中の攻勢残量を表す
- Rush ゲージは毎ターン開始時に最大値から始まる
- `Attack` ノード通過やそれに準ずる攻撃行動で Rush ゲージを減らす
- ターン終了時の残りゲージ段階に応じて、次ターンの危険地帯量を決める
- 残量が多いほど次ターンの危険地帯は多く、残量が少ないほど次ターンの危険地帯は少ない
- Rush型には `Break` 報酬を持たせない
- 問いの中心は「今ターンどこまで攻勢を削って次ターンの盤面を軽くするか」とする

### 9.7 敵タイプ

- ボス: 手作業設計
- 通常敵: タイプ分けで管理

### 9.8 初期実装で最初に作る通常敵

- バランス型
- 現在の実装では「敵が持つ pattern 候補からランダム選択」が先行実装
- 行動重みや行動選択ルールは将来実装とする

### 9.9 敵 Dance

- 主目的は次ターン危険強化
- 初期実装では危険強化 1 パターンのみでよい

### 9.10 敵 Skill

- 初期は広範囲攻撃系
- 広い範囲に危険を置く純粋攻撃

## 10. 属性と装備

### 10.1 初期装備枠

- 武器 1 枠
- アイテム 1 枠
- 胴装備 1 枠
- 頭装備と左手装備は将来拡張

### 10.2 装備ごとの役割

- 武器: Attack、Skill、属性、見た目、Skill パレット
- アイテム: Dance
- 胴装備: 弱点と耐性

補足:

- 実データ上は `EquipmentMasterData` ごとに候補スキルを最大 3 件まで持てる
- ただし「どの装備枠が何系統の候補スキルを持ちやすいか」はゲームデザイン上の裁量として残す
- 実戦用の 4 スロットは装備が持つ候補スキル群から別途選ぶ
- 右手武器は、通常 Attack に適用される属性も持つ
- Skill の属性は武器そのものではなく `BattleSkillMasterData` 側で個別に持てる

### 10.3 Skill スロット解放

- 4 スロットは武器ごとに設定する
- すべてが初期解放とは限らない
- 解放状態は武器ごとに管理できる設計にする

補足:

- 上記の「4 スロット」は実戦用 Skill パレットを指す
- 装備 master 側の候補スキル件数とは別に扱う

### 10.4 属性

- 初期は 4 属性想定
- 主用途は弱点、耐性によるダメージ補正
- 状態異常は属性ではなく将来的に Skill 由来で扱う
- 通常 Attack の属性は装備中の右手武器から解決する
- Skill の属性は `BattleSkillMasterData` から解決する

### 10.5 属性相性

- プレイヤー武器と敵胴装備
- 敵武器とプレイヤー胴装備
- 補正値は defender 側の胴装備が持つ `Attribute + DamagePercent` の entry list で決める
- `100` を等倍、`100` 未満を耐性、`100` 超を弱点、`0` を無効とする
- 現時点では「属性同士の相関表」は持たず、胴装備側の補正値をそのまま使う
- 攻撃側に属性がない場合、または defender 側に対応属性の補正がない場合は等倍とする

### 10.6 表示方針

- 弱点、耐性は常時 UI 表示しない
- ダメージ表示の装飾差で相性を示す
- 現行の暫定実装では、まずプレビュー文言とログに `属性名/弱点 or 耐性 or 無効` を出す
- 本命のダメージ数字装飾差は別途 UI 実装とする

## 11. 持続効果

- 一時効果: そのターンのパス解決中だけ有効
- 持続効果: ターンをまたいで残る

初期実装では主にターン開始時とターン終了時だけ処理する。スタック途中で発動する持続効果は将来拡張とする。

## 12. 勝敗条件

### 12.1 基本勝敗

- 基本は HP0 による勝敗

### 12.2 特殊敗北

- Goal 到達失敗の累積による特殊敗北を持つ
- 意味づけはプレイヤーが隙を見せた状態

### 12.3 累積対象

- Goal まで届かず指を離した
- 不正パスで入力失敗した

以下は累積対象にしない。

- 被弾による Goal 効果不発
- HP ダメージによる通常敗北

### 12.4 表示方針

- 特殊敗北カウントは UI に出さない
- セリフや演出、説明文で匂わせる

## 13. UI 要件

### 13.1 必須表示情報

- 自分 HP
- 敵 HP
- 敵名
- ターン数
- 盤面要素一式
- Skill パレット
- 敵戦型ゲージ
- ダメージ量
- 属性相性表現
- 敵 Dance 後の次ターン危険強化表示
- 特殊敗北演出用の敵短文
- 行動ログ

### 13.2 Skill パレット表示

- 武器に紐づく 4 スロット
- 準備ゲージ表示
- Ready が一目で分かる
- 未取得と未設定が見分けやすい

### 13.3 敵戦型ゲージ表示

- 敵 HP 付近に、敵戦型に対応するゲージを 1 本だけ表示する
- 初期実装では `Guard`、`Burst`、`Rush` の戦型名または対応アイコンを併記してよい
- `Guard型` では残り Guard 値を表示する
- `Burst型` ではゲージ量を大技進行度とし、ゲージ色の変化で Break 成功に近い状態を示す
- `Rush型` ではそのターン中の Rush 残量を表示し、ターン終了時に次ターン危険地帯量へ反映する
- 初期実装では、複数バー表示やひびの追加オーバーレイは持たない

### 13.4 入力中可視化

- 仮スタック一覧は出さない
- 短い確定情報テキストで示す
- おすすめルートや推測は言わない

例:

- いける
- 危ない
- 跳ぶ!
- 続ける!
- 決める

## 14. 1 ターンの基本フロー

1. ターン開始時効果を処理
2. 敵行動に応じた盤面、危険、Goal を表示
3. 必要に応じて Skill を事前選択
4. プレイヤーがパス入力
5. Goal 到達後、指を離して確定
6. スタックを通過順に解決
7. ターン終了時効果を処理
8. 次ターンへ移行

## 15. データとシーン遷移

### 15.1 戦闘用ステージデータ

現行方針では、戦闘用 master data は次の `ScriptableObject` 群で構成する。

- `StageMasterData`
  - `StageId`
  - `StageName`
  - `Description`
  - `BackgroundImage`
  - `PreviewImage`
  - `SortOrder`
  - `IsInitiallyUnlocked`
  - `EnemyRefs`
- `StageBattleEnemyMasterData`
  - `EnemyId`
  - `Name`
  - `MaxHp`
  - `Damage`
  - `BattleType`
  - `BattleTypeSettings`
  - `AppearanceSlots`
  - `AppearanceColors`
  - `Patterns`
- `StageBattlePatternMasterData`
  - `PatternId`
  - `Label`
  - `Description`
  - `EnemyAction`
  - `ConfirmText`
  - `DamageMultiplier`
  - `Board`
  - `CellPlacements`
- `EquipmentMasterData`
  - `EquipmentId`
  - `DisplayName`
  - `PartType`
  - `PartsIndex`
  - `Icon`
  - `SortOrder`
  - `IsDefaultOwned`
  - `AssignableSkills`
  - `NormalAttackAttribute`
  - `AttributeModifiers`
  - `CategoryTags`
- `BattleSkillMasterData`
  - `SkillId`
  - `DisplayName`
  - `Description`
  - `Icon`
  - `RequiredCharge`
  - `StartingCharge`
  - `TurnChargeGain`
  - `AttackChargeGain`
  - `Damage`
  - `Attribute`
  - `EffectType`
  - `EffectAnimation`
  - `DescriptionSupplement`
- `EffectAnimationMasterData`
  - `EffectAnimationId`
  - `DisplayName`
  - `Description`
  - `CharacterAnimationName`
  - `VisualEffectKey`
  - `SoundEffectKey`
  - `WaitSeconds`
- `AttributeMasterData`
  - `AttributeId`
  - `DisplayName`
  - `Description`
  - `AccentColor`
  - `Icon`
  - `SortOrder`

補足:

- authoring 側の参照は `Stage -> Enemy -> Pattern`、`Equipment -> Skill` の direct `ScriptableObject` 参照で持つ
- `Database` asset は一覧と ID 逆引きの補助レイヤーとして扱う
- runtime DTO は ID リストではなく、解決済みの enemy / pattern / skill / attribute を受け取る
- 危険グループは編集データに持たず、runtime で上下左右連結から自動グループ化する
- `BattleTypeSettings` は敵戦型ごとの設定値をまとめる serializable data とし、初期実装では別 `ScriptableObject` に分割しなくてよい

### 15.2 ステージ進捗データ

進捗側は以下を保持できる設計とする。

- StageId
- IsUnlocked
- IsCleared
- ClearCount
- BestScore
- BestRank
- BestClearTimeSeconds
- LastClearedAtUtc

### 15.3 遷移状態

- バトル開始時に選択ステージ ID を引き継ぐ
- 戦闘シーンはその ID を一度だけ消費して初期化に使う

### 15.4 現状の ID 整合性方針

- 論理上は 1 つのステージに 1 つの一意キーを持つ
- 現行コードでは `BattleSceneTransitionState`、`StageData`、`StageMasterData`、`BattleProgressService` のキーを数値 StageId に揃えた
- 実行時の「未選択」状態は `-1` を未設定値として扱う

## 16. 現状実装スナップショット

この節は 2026-05-09 時点の repo 実装を示す。ここは設計理想ではなく現物基準とする。

### 16.1 戦闘入口

- `HomeScreen` が `BattleSceneTransitionState.SelectedStageId` に数値 StageId を入れて `BattleScene` へ遷移する
- デバッグ時は Inspector の `debugBattleStageId` を使い、未指定時は `BattleProgressService.GetRecommendedStageId()` で補完する

### 16.2 ステージ読込

- `MasterDataResourceLoader.TryLoadStageData(int stageId, out StageData)` で `Resources/MasterData/StageDatabase` から読む
- `StageMasterDatabase` から `StageData` を生成する
- `StageMasterData.EnemyRefs` の先頭 1 体だけを現在の戦闘用敵として採用する
- enemy は `AppearanceSlots / AppearanceColors` と `Patterns` を持ち、loader が解決済み DTO に落とす
- `BattleScene` は `StageData.Enemy.Patterns` を現在ターンの供給源として消費する

### 16.3 現在のデモ戦闘パラメータ

- 初期プレイヤー HP: 450
- 通常 Attack: 100
- ダブル Attack 追撃: 125
- Jump Attack: 150

### 16.4 現在のデモ Skill スロット

- Slot 1: `Wide Blast`, Ready 閾値 3, 初期 3, 毎ターン +1, Damage 140
- Slot 2: `Pierce Volley`, Ready 閾値 5, 初期 2, 毎ターン +1, Damage 220
- Slot 3: Locked
- Slot 4: Empty

補足:

- `UserBattleSkillSlotData.SkillId` が設定されている場合は、`BattleSkillMasterData` を引いて属性や効果種別も runtime へ流す
- `SkillId` が未設定の旧 save は、暫定的にデモ名から sample skill へ補完する
- 装備ごとの候補スキル最大 3 件と、そこから選ぶ実戦用 4 枠の正式な保存/選抜導線はまだ未接続である

### 16.5 現在のデモ進行

現在は実グリッド入力ベースで、ターン内容は「敵が持つ `Patterns` から 1 件をランダム選択する」段階である。候補が複数ある場合は、直前と同一 pattern の連続選択を避ける。sample では以下の 6 pattern を持つ。

- Opening Attack
- Jump Evade
- Skill Showcase
- Enemy Dance Turn
- Roll Against Skill Hazard
- Hit Stops Remaining Stack

`BattleScene` の現行 scene 上では、`Panels` 配下の既存 UI ノード群を盤面セルとして再利用している。

- 盤面見た目は 7 列 x 8 行
- `Panels` の GridLayoutGroup は縦優先で子要素を並べる
- 開始マスは中央列の最下段に固定する
- fallback path は pattern の配置セルをもとに runtime で構築する

### 16.6 現在の危険強化

- 設計書では危険マス数 1.5 倍を想定している
- 現在のデモコードでは危険ダメージを 1.5 倍している

### 16.7 現在の危険グループ

- 危険グループは master data に持たない
- `StageBattleRuntimeAdapter` が盤面生成後に、上下左右連結した危険マスへ同じ `HazardGroupId` を自動付与する
- 同一グループは被弾 1 回だけ、Jump / Roll による回避もグループ単位で処理する

### 16.8 現在の属性接続

- プレイヤー通常攻撃属性は、現在装備中の右手武器 `EquipmentMasterData.NormalAttackAttribute` から解決する
- プレイヤー被ダメ補正は、現在装備中の胴装備 `EquipmentMasterData.AttributeModifiers` から解決する
- 敵通常攻撃属性は、敵 `AppearanceSlots` に設定した右手武器から解決する
- 敵被ダメ補正は、敵 `AppearanceSlots` に設定した胴装備から解決する
- Skill 属性は `BattleSkillMasterData.Attribute` から解決する
- 現在の表示は、プレビュー文言とログに `属性名/弱点 or 耐性 or 無効` を出す段階である

### 16.9 現在の UI 接続状態

- `previewImage` は反映される
- `background` はステージ背景として反映される
- `turnNumberText` と `waveNumberText` は更新される
- `playerHPText` は `現在HP/最大HP` で更新される
- Skill ボタンの選択色更新は実装済み
- `Panels` が見つかる場合は `BattleBoardController` と `BattleTraceInputHandler` を runtime で自動接続する
- 属性相性の本命 UI は未実装だが、プレビュー文言には暫定表示される

### 16.10 現在未接続のロジック

- 特殊敗北カウント
- 不正パス失敗と特殊敗北カウントの接続
- 勝利、敗北の本演出
- バトル結果の最終 save 接続
- `AttackChargeGain` の実消費
- 装備候補スキルから実戦用 4 枠を選ぶ保存/画面/接続
- 敵戦型 `Guard / Burst / Rush` の本実装
- 敵戦型ゲージ UI と `Burst` の色変化表示
- 属性相性の本命ダメージ装飾 UI
- `EffectAnimationMasterData` を使った実エフェクト再生

## 17. 初期実装向け簡略化ルール

- 敵は 1 体のみ
- 盤面は固定またはスクリプト再生
- 敵戦型は `Guard型`、`Burst型`、`Rush型` の 3 種を優先する
- Skill は盤外パレット選択式
- Skill は純粋攻撃寄り中心
- 防具は胴装備のみ
- 頭装備、左手装備は考慮しない
- 状態異常は主役にしない
- 盤面属性ギミックなし
- 危険ターン演出は同系統で統一
- 持続効果は主にターン開始と終了時のみ処理

## 18. 将来拡張

- Goal 効果の本格拡張
- Dance Goal の持ち越し効果
- 頭装備
- 左手装備
- 状態異常系 Skill
- スタック途中で発動する持続効果
- 盤面や Goal へ干渉する Skill
- Skill ノードを使う特殊盤面
- ランダム生成盤面
- 複数敵

## 19. 具体値が未確定の項目

以下は設計意図はあるが、まだ最終値が固定されていない。

- プレイヤー Skill の正式性能
- 敵 Skill の正式性能
- Skill 準備値の加算量
- Skill Ready に必要な閾値
- Skill 使用後のゲージ初期値
- `Guard型` の Guard 最大値と `軽装` 継続ターン数
- `Burst型` の大技進行速度と Break 必要量
- `Rush型` の残量段階ごとの危険地帯量
- Skill スロット解放条件
- 特殊敗北演出の具体内容
- 属性ごとの正式補正値
- 敵 AI の重みテーブル詳細

## 20. リファクタ後の実装構成案

### 20.1 基本方針

- `BattleScene` はシーン司令塔に縮退し、盤面状態、入力判定、解決ロジック、HUD 更新を直接抱え込まない
- pure C# の戦闘コアと `MonoBehaviour` の表示層を分離する
- 旧デモ実装には依存せず、`StageData.Enemy.Patterns` を本番盤面の正本とする
- `グリッドパズルゲーム.txt` の責務分離思想は採用するが、`match-3` 前提の概念名は持ち込まない

### 20.2 クラス一覧と配置パス

初回リファクタで追加または改名対象とするクラスは以下とする。

| レイヤー | クラス名 | 配置パス | 役割 |
| --- | --- | --- | --- |
| Scene | `BattleScene` | `Assets/Scripts/Features/Battle/Demo/BattleScene.cs` | 当面は既存ファイルを維持し、初期化、ターン開始、結果反映、進捗保存、各コントローラ仲介のみを担当する |
| Core | `BattleGridPosition` | `Assets/Scripts/Features/Battle/Core/BattleGridPosition.cs` | 盤面座標。`x`,`y` と隣接判定、等価比較を持つ値オブジェクト |
| Core | `BattleNodeType` | `Assets/Scripts/Features/Battle/Core/BattleNodeType.cs` | `Start`,`Empty`,`Jump`,`Roll`,`Dance`,`Attack`,`HazardNormal`,`HazardSkill`,`Goal` を持つ本番 enum |
| Core | `BattleEnemyActionType` | `Assets/Scripts/Features/Battle/Core/BattleEnemyActionType.cs` | 敵のターン行動種別。`NormalAttack`,`Skill`,`Dance` を持つ本番 enum |
| Core | `BattlePathValidationError` | `Assets/Scripts/Features/Battle/Core/BattlePathValidationError.cs` | `NotAdjacent`,`Revisit`,`CrossedSegment`,`ExtendedAfterGoal`,`OutOfBounds` などの不正理由を表す |
| Runtime | `BattleCellState` | `Assets/Scripts/Features/Battle/Runtime/BattleCellState.cs` | 1 マスぶんの状態。座標、`BattleNodeType`、危険グループ ID、表示用補助フラグを持つ |
| Runtime | `BattleBoardState` | `Assets/Scripts/Features/Battle/Runtime/BattleBoardState.cs` | 可変サイズ盤面全体。サイズ、セル配列、`Start`、`Goal` 一覧、検索 API を持つ |
| Runtime | `BattlePathDraft` | `Assets/Scripts/Features/Battle/Runtime/BattlePathDraft.cs` | 入力中の仮パス。通過座標列、到達済み `Goal`、確定可能状態、最後の線分集合を持つ |
| Runtime | `BattleSkillSlotRuntime` | `Assets/Scripts/Features/Battle/Runtime/BattleSkillSlotRuntime.cs` | Ready、Charge、消費、表示ラベルを持つ戦闘スキル枠ランタイム |
| Runtime | `BattleSessionState` | `Assets/Scripts/Features/Battle/Runtime/BattleSessionState.cs` | プレイヤー HP、敵 HP、ターン数、Wave、選択 Skill、次ターン危険強化、特殊敗北カウントなどの戦闘進行状態 |
| Runtime | `BattleTurnContext` | `Assets/Scripts/Features/Battle/Runtime/BattleTurnContext.cs` | 1 ターン解決に必要な固定値。攻撃力、敵行動種別、危険強化状態、選択 Skill を束ねる |
| Runtime | `BattleTurnResolutionReport` | `Assets/Scripts/Features/Battle/Runtime/BattleTurnResolutionReport.cs` | 解決結果。確定可否、与ダメ、被ダメ、Goal 成否、Skill 消費、行動ログ、終了フラグを返す |
| Logic | `BattlePathRuleEvaluator` | `Assets/Scripts/Features/Battle/Logic/BattlePathRuleEvaluator.cs` | 4 方向接続、再訪禁止、線分交差禁止、Goal 後延長禁止を判定する pure C# ルール評価器 |
| Logic | `BattlePathTracer` | `Assets/Scripts/Features/Battle/Logic/BattlePathTracer.cs` | ドラッグ中の入力を `BattlePathDraft` に反映する。前のマスへの戻り Undo と `Goal` 到達状態維持を担当する |
| Logic | `BattleTurnResolver` | `Assets/Scripts/Features/Battle/Logic/BattleTurnResolver.cs` | 確定パスを通過順に解決し、攻撃、Skill 変換、回避、被弾、Goal 不発を `BattleTurnResolutionReport` にまとめる |
| Logic | `BattleBoardGenerator` | `Assets/Scripts/Features/Battle/Logic/BattleBoardGenerator.cs` | ステージ定義と敵行動からターン盤面を生成する。固定テンプレート利用もここに寄せる |
| Logic | `BattleEnemyIntentPlanner` | `Assets/Scripts/Features/Battle/Logic/BattleEnemyIntentPlanner.cs` | 敵 AI または暫定重みから、そのターンの `NormalAttack` / `Skill` / `Dance` を決める |
| Presentation | `BattleBoardController` | `Assets/Scripts/Features/Battle/Presentation/BattleBoardController.cs` | `BattleBoardState` をグリッド表示へ反映し、セル View の生成と更新を担当する `MonoBehaviour` |
| Presentation | `BattleCellView` | `Assets/Scripts/Features/Battle/Presentation/BattleCellView.cs` | 単一セルの見た目とヒット判定を担当する `MonoBehaviour` |
| Presentation | `BattleTraceLineView` | `Assets/Scripts/Features/Battle/Presentation/BattleTraceLineView.cs` | ドラッグ中の線、確定可能状態、交差不可フィードバックを描画する `MonoBehaviour` |
| Presentation | `BattleTraceInputHandler` | `Assets/Scripts/Features/Battle/Presentation/BattleTraceInputHandler.cs` | Pointer 入力を `BattlePathTracer` に流し、確定イベントを `BattleScene` へ通知する `MonoBehaviour` |
| Presentation | `BattleHudController` | `Assets/Scripts/Features/Battle/Presentation/BattleHudController.cs` | HP、ターン、Skill パレット、確定テキスト、危険強化予告を更新する `MonoBehaviour` |

補足:

- 初回リファクタでは scene や prefab の GUID 変更を避けるため、`BattleScene` 自体は現パスに残してよい

### 20.3 依存ルール

- `Presentation` は `Logic` と `Runtime` を参照してよい
- `Logic` は `Runtime` と `Core` のみを参照し、`UnityEngine` に依存しない
- `Runtime` は `Core` のみを参照し、`MonoBehaviour` を持たない
- `Data` 層の `StageData` と `MasterDataResourceLoader` は `BattleScene` または `BattleBoardGenerator` から読み出し、セル View からは直接参照しない
- 旧デモ専用データは持たず、本番フローは `StageData.Enemy.Patterns` を直接使う

### 20.4 既存 `BattleScene` からの責務移管

現在の `BattleScene` の責務は、以下のように分割する。

| 現在 `BattleScene` にある責務 | 移管先 |
| --- | --- |
| ステージ ID 解決、進捗反映、シーン初期化 | `BattleScene` のまま維持 |
| ステージ見た目反映 | `BattleHudController` と `BattleBoardController` |
| Skill ボタン選択と見た目更新 | `BattleHudController` と `BattleSkillSlotRuntime` |
| Pattern 候補の選択と盤面供給 | `StageData.Enemy.Patterns` を読み、将来的には `BattleEnemyIntentPlanner` と `BattleBoardGenerator` へ整理する |
| `BattleDemoPathResolver.Resolve(...)` の呼び出し | `BattleTurnResolver.Resolve(...)` |
| Goal 到達判定と `PathConfirmed` 分岐 | `BattlePathTracer` と `BattleTurnResolver` |
| 危険強化フラグ管理 | `BattleSessionState` |
| HP 更新、勝敗更新、次ステージ解放 | `BattleScene` が `BattleTurnResolutionReport` を受けて反映する |

### 20.5 依存関係図

```mermaid
graph TD
    Loader["MasterDataResourceLoader"]
    StageDTO["StageData / StageMasterData"]
    Progress["BattleProgressService"]
    Scene["BattleScene"]
    Planner["BattleEnemyIntentPlanner"]
    Generator["BattleBoardGenerator"]
    Session["BattleSessionState"]
    Board["BattleBoardState"]
    Skill["BattleSkillSlotRuntime"]
    Input["BattleTraceInputHandler"]
    Tracer["BattlePathTracer"]
    Rules["BattlePathRuleEvaluator"]
    Draft["BattlePathDraft"]
    Resolver["BattleTurnResolver"]
    Report["BattleTurnResolutionReport"]
    BoardCtrl["BattleBoardController"]
    Hud["BattleHudController"]
    Cell["BattleCellView"]
    Line["BattleTraceLineView"]

    Loader --> StageDTO
    StageDTO --> Scene
    Progress --> Scene
    Scene --> Planner
    Scene --> Generator
    Scene --> Session
    Scene --> Skill
    Planner --> Generator
    Generator --> Board
    Scene --> BoardCtrl
    Scene --> Hud
    BoardCtrl --> Board
    BoardCtrl --> Cell
    Input --> Tracer
    Tracer --> Rules
    Tracer --> Draft
    Rules --> Board
    Scene --> Input
    Scene --> Resolver
    Resolver --> Board
    Resolver --> Draft
    Resolver --> Session
    Resolver --> Skill
    Resolver --> Report
    Report --> Scene
    Draft --> Line
    Session --> Hud
    Skill --> Hud
```

### 20.6 導入順

- 第 1 段階: `BattleNodeType`、`BattleSkillSlotRuntime`、`BattleTurnResolver` を追加し、旧デモ名称から切り離す
- 第 2 段階: `BattleBoardState`、`BattlePathDraft`、`BattlePathRuleEvaluator`、`BattlePathTracer` を追加して実ドラッグ入力へ置き換える
- 第 3 段階: `BattleBoardController`、`BattleCellView`、`BattleTraceLineView`、`BattleHudController` を追加して `BattleScene` の UI 責務を外へ出す
- 第 4 段階: `BattleEnemyIntentPlanner` と `BattleBoardGenerator` を追加し、pattern 選択と盤面生成を整理する
