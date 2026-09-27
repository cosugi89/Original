# プロジェクト全体 TODO

- 最終更新: 2026-09-27
- 主な参照元: `docs/specs/MainScene仕様書.md`, `docs/specs/UI共通部品仕様書.md`, `docs/specs/戦闘システム仕様書.md`, `docs/specs/戦闘画面仕様書.md`, `docs/specs/装備仕様書.md`, `docs/specs/スキル仕様書.md`, `docs/specs/タグ仕様書.md`
- 位置づけ: プロジェクト全体の実装 TODO。現時点では Path-Activation System v2 と、下記「非戦闘側の着手順」に沿った Character / Progress / Missions / Armory改修タスクが中心。
- 方針: 現行実装を正として、仕様との差分と未接続箇所を優先順で並べる
- 状態見直し: 2026-07-12 に属性採用方針を撤回し、タグ仕様書と属性撤去 TODO を追加

## 0. 進め方メモ

- `[x]` は現状 repo 上で概ね存在するもの
- `[ ]` は未着手、未接続、または仕様/UX が未確定のもの
- 優先度は `P0 > P1 > P2 > P3`
- ゲーム改善アイデアは `9. ゲーム改善アイデア TODO` に置き、採用方針が固まったら該当する実装 TODO や仕様書へ昇格する

## 0.1 非戦闘側の着手順

この節は非戦闘側の未完了タスクについて、実装に着手する順番を示す優先度の正本とする。各機能の詳細タスクは後続節に置くが、優先度が衝突した場合はこの節を優先する。Event Mission用の消費キー撤去は完了済みであり、ここには含めない。

### 超高

- [ ] **最終ステータス共通算出**: Character表示とBattle開始時で同じ `HP / Attack / Defense` を導出する窓口を作る。基礎値は HP 450 / Attack 100 / Defense 0 とし、装備・Traits・Insightsの加算後、タグごとの一致装備加算分へ2 / 3 / 4部位一致時の25 / 50 / 75%ボーナスを加える。成立タグはすべて適用し、戦闘中の一時補正と現在HPは含めない。恒久Defenseは999へclampし、被ダメージを `元ダメージ × 333 / (333 + Defense)` へ軽減する。GuardGainは同一ターンだけDefenseを加算し、加算後のDefenseには上限を設けない。軽減後のHPダメージは端数を切り上げ、最低1とする。人間ボスにも同じ式を適用するが、ボス側は固有の基礎値だけから導出する。
- [ ] **Mission進行の基本ループ**: 前提Missionクリアによる解放、未クリアMissionだけの表示、カード選択からのBattle開始、クリア後の一覧除去、Point / Gold報酬受領を接続する。
- [ ] **ボス結果の保存基盤**: 人間ボスごとの通常撃破／救済、Insight解放、安全地帯数、Angel到達状態を保存する。Resurgence開始時にリセットする対象と、装備・Skill・Trait・Armory商品の維持対象を区別する。

### 高

- [ ] **Characterの完成**: 装備4枠、Deck選択、Appearance、最終ステータス表示を一画面として整え、装備変更後に表示値を即時更新する。
- [ ] **Deck上のタグ一致表示**: `DeckLoadoutDialog` に、現在選択中Deckで成立しているタグ、一致部位数、各タグが加えた `HP / Attack / Defense` ボーナスを表示する。Deck切替・装備変更のプレビュー中も表示を更新する。
- [ ] **装備候補のタグ一致プレビュー**: 装備選択Dialogで、未装備の候補を選んだ時は装備後に一致するタグを明滅表示する。現在装備中の候補の一致タグは点灯表示する。タグ未成立や対象外のタグは非強調とし、実際の装備切替前後で一貫して更新する。
- [ ] **Progressの再構成**: `Traits` と `Insights` を実装し、Resurgence時だけ `Remnant` タブを表示する。旧 `Artifact` / `Skill` タブは廃止する。
- [ ] **Armoryの商品設計と画面接続**: 商品マスターと購入・保存基盤は実装済み。商品一覧PrefabをMainSceneへ配線し、進行に応じた商品追加、Resurgence後も解放済み商品を維持する処理、救済に必要な装備を対象Missionより前に購入可能にする条件を実装する。
- [ ] **Mission結果の非戦闘演出**: 通常撃破／救済、報酬、Insight自動解放、新規バッジを戦闘終了後に通知し、次のMissionへ戻す導線を実装する。

### 中

- [ ] **HomeのAngel**: HomeにAngelを常駐させ、タップで進行状況に応じた短い台詞を表示する。初回救済後は一時不在にし、次の通常撃破後に嘘の台詞とともに戻す。Resurgenceでは台詞差分・赤字演出を加える。
- [ ] **Event Mission**: 前提Missionクリアで解放される任意の一回限りMissionとして、報酬、世界の補足、追加Battleパターンを実装する。本編の真相、救済条件、安全地帯に必須の情報は置かない。

### 低

- [ ] **TitleSceneとThe Remnant**: 初回エンド後の`The Remnant`獲得演出、`Resurgence Unlocked`表示、TitleSceneからのResurgence開始導線を実装する。
- [ ] **Resurgence専用導線**: 同モードでのAngelチュートリアル台詞差分、Angel敗北後の同一結果での再挑戦、`Remnant`タブでの結果・既読台詞表示、通常撃破済みボスへの救済目的の再挑戦を実装する。
- [ ] **将来のChallengeと消費キー**: 恒常・周回型のChallengeを追加すると決めた段階でのみ、入場キーの種類、入手経路、消費タイミングを新規設計する。

## 0.2 全体横断の超高タスク

- [ ] **GuardGainの一時Defense化**: 現在のシールド残量を吸収する `GuardGain` を廃止し、効果成立からターン終了までDefenseを加算する一時補正へ置き換える。複数効果を加算し、Guard加算後のDefenseには上限を設けない。専用のGuard残量UIは作らない。
- [ ] **敵Guard状態**: 敵Skillによる一時Defense加算、最大HP割合から導出するGuard値、軽減前ダメージと実HPダメージの差分によるGuard値減少、Guard解除時のDefense加算除去を実装する。Guard値が0になるか、Skill設定の持続ターンを経過した時点で解除する。Guard中だけ敵UIにGuard値ゲージと残りターンを表示し、Defenseの正確な数値は出さない。解除後の再付与はSkill設定へ持たせる。
- [ ] **Danceの廃止**: `Dance` を戦闘仕様から廃止する。既存盤面assetのDanceマスを移行してから `BattleNodeType.Dance` を廃止し、ノード解決、アニメーション、通知、セル表示、Pattern Editor、救済条件候補、`DanceSkill` と装備詳細表示、舞踏タグ候補、関連仕様書を削除する。Unity enumの途中削除による既存assetの値ずれを避けるため、値の明示固定または廃止値保持を経由して移行する。敵Danceの次ターン危険1.5倍はDanceに依存しない `Frenzy` 等の敵行動へ置換するか、敵行動設計時に改めて採否を決める。

## 1. P0: 基盤とデータ整合

- [ ] `TitleScene` を追加し、現行 `MainScene` の `GameInitializer` を移動して Build Settings の起点を切り替える。`The Remnant`獲得演出と `Resurgence` 解放・開始導線をここへ実装する。
- [x] ステージ ID を `int` と `string` の混在から数値 `StageId` に統一する
- [x] `BattleSceneRequest`、`StageData`、`StageMasterData`、`ProgressManager` のキー設計を揃える
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
- [x] `BattleTurnResolver` の処理入口を node type ごとの action class へ分け、行動単位で拡張しやすい形へ段階移行する
- [x] Skill 選択時に Attack を Skill に変換する
- [x] Skill 選択時は Jump Attack や Double Attack より Skill を優先する
- [x] `SkillSlotRuntime` で Skill パレットの Ready、Charge、消費状態を扱う
- [x] `BattleDamageCalculator` と `BattlePlayerStats` で通常攻撃、Double Attack、Jump Attack、Skill Damage 効果を計算する
- [x] 旧属性相性の計算を `BattleTurnResolver` に接続する
- [x] 旧属性実装として、プレイヤー右手武器属性 / 胴装備補正と、敵右手武器属性 / 胴装備補正を戦闘コンテキストへ流す
- [x] 危険グループを runtime で上下左右連結から自動付与し、同一グループは被弾 1 回として扱う
- [x] 敵行動種別として `NormalAttack`、`Skill`、`Dance` を持つ
- [x] 敵 Dance 後の次ターン危険強化を、現行仕様どおり危険ダメージ 1.5 倍として扱う
- [x] `ResolvedActions` から演出 beat と action 成立タイミングを作り、攻撃/被弾/Skill 効果を演出 event に合わせて逐次反映する
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
- [ ] `Parts-SkillItem` と `BattleSkillSlotView` を BattleScene の Skill パレット用Prefabへ配線する
- [x] `turnNumberText` と `waveNumberText` を毎ターン更新する
- [x] `background` と `previewImage` にステージ画像を反映する
- [x] `playerHPText` を `BattleHudController` 経由で更新する
- [x] 旧属性相性をプレビュー文言とログに暫定表示する
- [x] `docs/specs/戦闘画面仕様書.md` に縦長 9:16 前提、領域分割、戦闘イベント通知、常設ログ廃止方針を整理する
- [x] `BattleScreenRoot`、`BattleTopHudView`、`BattlePresentationView`、`EnemyAreaView` を追加し、戦闘画面の主要領域をコード上で分離する
- [x] `BattleBoardView` を追加し、盤面領域の所有者を `BattleBoardController` 互換で差し替え可能にする
- [x] `EnemyUnitView` を追加し、敵名、敵 HP、敵Skill由来のゲージ・状態を敵側 View へ寄せられるようにする
- [x] `PlayerAreaView` を追加し、プレイヤー HP とプレイヤー状態をプレイヤー側 View へ寄せられるようにする
- [x] `BattleEventNoticeView` を実装し、技名、Goal、回避、被弾、危険強化、敵の体勢変化などを短命通知として扱う
- [x] 常設の `confirmText` と行動ログ履歴を初期画面から外し、`confirmText` は必要に応じて短命通知のフォールバックとして使う
- [x] `BattleScene.unity` に `BattleScreenRoot` と現行 Scene で使う主要 View コンポーネントを割り当てる
- [x] `BattleScreenRoot` で縦長 9:16 の基本領域比率を適用し、`BattleBoardView` のセルサイズを盤面領域に合わせて自動調整する
- [x] Unity Play 実機相当の画面で `BattleScene` の余白、重なり、通知位置を確認して微調整する
- [ ] 不正入力は盤面の赤表示、揺れ、トースト、バイブレーション候補へ分担する
- [x] Skill パレットの Ready、Charging、Locked、Empty を見分けやすい本 UI として整える
- [ ] `waveNumberText` を実際の wave 概念に接続するか削除する
- [ ] 敵Skill由来のゲージ・状態UI、`Burst / Rush` 表示、敵Guard値ゲージ・残りターン表示を実装する
- [ ] タグ / セットボーナス表示 UI を実装する
- [ ] 勝利、敗北、再挑戦の本演出を実装する
- [ ] プレイヤー/敵のセリフ、フレーバー表示を実装する。行動履歴ログは初期画面には置かない

## 4. P1: ステージ遷移、進捗、セーブ

- [x] `HomeScreen` からバトルシーンへ遷移できる
- [x] `HomeScreen.GetNextStageId()` と debug stage 指定を持つ
- [x] 戦闘開始時に `ProgressManager` の現在ステージ/最終選択ステージを更新する
- [x] 勝利時に `RecordStageClear()` を呼ぶ
- [x] 解放条件を `StageMasterData.RequiredClearedStageIds` に持ち、クリア進捗から都度判定する
- [x] バトル進捗保存用のモデルは存在する
- [x] `RecordStageClear()` は `BestScore`、`BestRank`、`BestClearTimeSeconds` を更新できる
- [x] Mission一覧の基盤を `StageMasterDatabase` と進捗へ接続して動的生成する
- [ ] ステージ選択画面で未解放ステージの選択制御を実装する
- [ ] 戦闘失敗時の再挑戦導線を実装する
- [ ] スコア、ランク、クリアタイムの算出ルールを定義する
- [ ] 勝利時に実際の `score`、`rank`、`clearTimeSeconds` を `RecordStageClear()` へ渡す
- [ ] 戦闘終了時に `UserDataJsonStore.SaveSession()` を呼ぶ導線を整える

## 5. P1: MainScene / Status タブ

- [x] Status タブに現在の装備枠一覧を表示する
- [x] Ability タブ向けに `AbilityNodeMasterData` / `AbilityTreeMasterData`、Inspector検証、Tree Builder Windowを追加する。Node Masterは再利用可能とし、Tree配置ごとの `TreeNodeId`、Rank、Tree接続、排他、循環依存を検証できるようにする
- [ ] Progress内Tabを `Traits` / `Insights` / Resurgence限定の `Remnant` へ再構成する。旧 `Artifact` / `Skill` Tabを廃止し、`Remnant` の表示可否を周回状態へ接続する。
- [x] Ability タブのTrait Tree基盤を実装する。Point残高/NodeLevel保存、排他分岐、Tree UIの接続まで完了。
- [x] `PartsTraitTreeRow` / `PartsTraitLine` のPrefabを作成して`TraitTabScreen`のRow Prefab / Scroll Contentへ配線する。各Line Imageには `UI/Trait Line Reveal` Materialと曲線の進行度を持つReveal Mapを設定する。
- [x] Trait Itemクリックで詳細Dialogを開き、購入可能Nodeだけ購入ボタンを表示して、結果をTree表示へ即時反映する。
- [x] Trait Tree表示の責務を整理する。購入可否とは別にNode/Lineの表示状態を一箇所で導出し、排他経路のDisabled表示を末端まで一貫させる。
- [x] Trait Treeの更新を最適化する。購入時は全Rowを再生成せず、既存RowのItem/Line表示だけを更新できるようにする。
- [x] Gold / Ability PointのResource Bar更新をUI探索から分離する。通貨変更通知または明示的な表示更新窓口を定義し、購入・報酬・Debug付与の各経路を同期する。
- [x] Trait Tree Itemで未使用の `VisualState.Special`、alert/up表示フラグを削除する。
- [ ] Trait Tree購入時のLine Reveal Animationを接続する。`PartsTraitLine` の再生APIと購入前後の導出結果を比較して、新規に有効化された接続線だけを再生する。
- [ ] Ability進捗のセーブ互換性を強化する。重複 `TreeNodeId` の安全な無視、正規化結果の即時永続化、不正な空Rankの購入拒否、排他ルート下流を含む矛盾保存の補正方針を実装し、複合キー・Level 0除外・Any/All・排他・不正保存補正をEditModeテストで保証する。
- [ ] `DisplayRows` と `Links` の整合をMasterData検証で保証する。現行Row UIが表現できる隣接Row間の 1→1 / 1→2 / 2→2（左→左・右→右）/ 2→1 接続だけを許可し、飛び越し・交差・表現不能な前提接続はエラーにする。
- [ ] ステージ初回クリアのPoint報酬と受領済み状態を実装する
- [ ] ステータス算出仕様の確定後、Abilityの導出済み能力補正をBattleとStatus表示へ接続する
- [x] Status タブで所持済み / 解放済み装備から装備を選択できるようにする
- [x] 装備変更を `UserData.Profile.Avatar.Parts` と MainScene 上のプレイヤープレビューへ反映する
- [x] `Button_Skin` から髪、肌、目、ひげなどの見た目変更ダイアログを開く
- [x] 見た目変更ダイアログの保存を `UserData.Profile.Avatar` と MainScene 上の各プレイヤープレビューへ反映する
- [x] Status タブの変更保存タイミングを確定し、`UserDataJsonStore.SaveSession()` へ接続する
- [x] 装備変更により使用不可になった Skill ロードアウトを自動で外す
- [x] Status タブで実戦用 Skill ロードアウト4枠を表示し、Skill 設定ダイアログから変更できるようにする
- [x] 戦闘用4装備・Skill4枠を正本とするデッキ3件と `CurrentDeckId` を保存モデルへ実装し、旧 `Avatar.Parts` / 単一 `SkillLoadout` を廃止する
- [x] `DeckLoadoutUtility` を実装し、デッキ保存時・適用時に装備を正として無効Skillを空枠化する
- [x] StatusSceneのデッキ一覧ダイアログから、内容確認とデッキ切替を行えるようにする
- [ ] 敗戦後に `DeckLoadoutDialog` を開けるようにする。再挑戦までの導線と、デッキ切替後にどの時点で再挑戦へ戻るかは別途整理する
- [x] Dialog 基盤を `DialogService` / 専用 `DialogCanvas` / `DialogAddressAttribute` ベースへ移行し、呼び出し側の prefab 参照と parent 指定を外す
- [x] 汎用 `GameButton` を `Button` 継承 + 単体 Prefab ベースで追加し、見た目 enum / SE enum / Badge を切り替えられるようにする
- [ ] `Dialog-EquipmentDetail` 以外の Dialog と主要 UI の既存 `Button` を、触るタイミングで `GameButton` へ段階的に置き換える
- [ ] アスペクト比の変更でDialogとFooter以外のUIが崩れないよう、MainScene UI全体のレイアウトを対応する
- [ ] アバター表示周辺の余白に、装備変更時の戦闘ステータス、候補スキル、タグ一致ボーナスなど、今後必要な情報を表示できる領域を設計・実装する
- [ ] `BattleSkillMasterData` にスキルの解放状態を追加し、`Dialog-EquipmentDetail` の候補行を解放済みは `NormalState`、未解放は `LockState` で表示する。

## 6. P2: 敵行動、戦型、特殊敗北

- [ ] 敵Skillの使用条件を、ボスごとの行動順、HP段階、現在状態で定義できるようにする。プレイヤーと同じチャージ制は使わない。
- [ ] 敵 Skill による危険配置ルールを定義する
- [ ] 強敵、ボス、大技ターン向けの例外盤面ルールを定義する
- [ ] `StageBattleEnemyMasterData` に敵装備、敵Skill構成、Skillごとの設定を追加する
- [ ] `Guard / Burst / Rush` などの敵SkillロジックとSkill設定、敵Skillから付与できる状態を実装する。Burstは予告した次ターンに広範囲・高ダメージの危険ノードを置くが、予告UI、Breakの有無、具体数値は通常ボス盤面試作後に決める。
- [ ] Goal 到達失敗の累積カウントを内部状態として実装する
- [ ] 特殊敗北閾値を敵ごとに設定できるようにする
- [ ] 特殊敗北成立時の敗北演出を実装する
- [ ] 特殊敗北カウントは UI 非表示のまま、短文とフレーバーで圧を出す
- [ ] `StageBattleEnemyMasterData` にタイミング別フレーバーテキスト群を追加する
- [ ] フレーバーテキスト群は `Dictionary` ではなく `TriggerType + List<string>` の serializable entry list で持つ
- [ ] 人間ボス12体前後の通常撃破／救済結果を保存し、結末分岐へ接続する。通常撃破は死亡を直接語らず、倒れ方と以後の不在で示す。
- [ ] 救済の特殊勝利を実装する。Goal解決時に敵HPを残して勝利する型と、条件付きHP0を救済へ変換する型を持たせる。条件は装備、Skill、Attack / 回避ノードの累積などから構成し、UIで直接表示しない。
- [ ] `Insights`を実装する。人間ボスの通常撃破／救済のどちらでも同じ能力を自動解放し、Progressの `Insights` 一覧と新規バッジ、戦闘・Characterへの共通効果を接続する。HP / Attack / Defense加算に加え、Skill charge促進のような再利用可能な特殊ルールを扱えるようにする。救済時だけ安全地帯を1マス追加し、再救済で能力を重複付与しない。
- [ ] 上位存在戦の盤面を実装する。候補危険マスから救済数分をランダムに安全地帯へ変え、Start / Goal / 役割ノード以外を危険化する。候補配置、役割ノード数、乱数再現性は通常ボス盤面試作後に決める。
- [ ] 序盤の攻撃主体で読み取りやすい救済と、後半の非攻撃救済を含む人間ボス群を設計する。救済に必要な装備は挑戦時点でArmoryから購入可能にする。
- [ ] `The Red Fever`、`Angel`、最後の通常ボス（元観察対象）、バッドルート、上位存在戦のシナリオと演出を実装する。`Angel`の嘘 `I took care of the one you spared.` とスタッフロールの生存一枚絵を含める。
- [ ] `The Remnant`のタイトル獲得演出、`Resurgence`開始、引継ぎ・リセット処理を実装する。装備、Skill、Trait、Armory商品は維持し、Mission進捗、ボス結果、Insights、安全地帯はリセットする。Mission再クリアでPointとGoldを再取得可能にする。
- [ ] Resurgence用のチュートリアル台詞差分と赤字演出を実装する。上位存在戦敗北後は同周回の結果を維持して再挑戦可能にする。
- [ ] Resurgence時だけProgressに `Remnant` タブを表示する。ボス結果・一周目のボス台詞・救済達成状況を表示し、上位存在到達後は通常撃破済みボスへの救済目的の再挑戦導線を提供する。
- [ ] バッドエンド用に、終了時点の主人公の装備・Skill・Abilityを敵へ引き継ぎ、初期状態の主人公との敗北固定戦を実装する。通常のステージ敵マスターへ保存済みデッキを上書きしない。
- [ ] 人間ボスと赤い目の主人公用の赤目表示を、既存の通常 Eye 色変更に依存しない専用の Sprite / overlay / material で実装する。

## 7. P2: マスターデータ、ロードアウト、演出

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
- [x] `EquipmentMasterData.AssignableSkills` の上限を最大 2 件へ変更し、validation / tooltip / docs / DTO 変換を揃える
- [x] 旧属性実装を runtime DTO / 装備派生データ / 戦闘コンテキスト / ダメージ計算から無効化し、戦闘仕様から属性相性を外す
- [x] `BattleSkillOperationType` と周辺データを、`Damage` / `ChargeGain` / `Heal` / `GuardGain` などのスキル仕様書の効果区分に合わせて再整理する
- [x] `BattleSkillParameterType.Speed` を active parameter から外し、互換用の `LegacySpeed = 4` として残す
- [x] スキル効果を配列順に複数解決し、途中効果が失敗しても後続効果を継続する処理を実装する
- [x] `SuccessRate` を戦闘解決へ接続し、タグ、状態、盤面結果、敵行動などから補正できる拡張点を用意する
- [x] `ChargeGain` / `Heal` / `GuardGain` の初期スキル効果を実装する
- [x] `Buff` / `Debuff` を Parameter 対象の一時補正として扱うランタイムを実装する
- [x] `EquipmentMasterData.CategoryTags` をタグ一致ボーナス判定へ接続する
- [x] `EffectAnimationData.CharacterAnimationName` / `WaitSeconds` を Skill 効果ごとの Timeline へ接続する
- [ ] 実際に使用する Effect / VFX / SE 資産を作成する
- [ ] `EffectAnimationData.VisualEffectKey` / `SoundEffectKey` の VFX / SE 実再生処理を `BattleScene` / runtime に接続する
- [ ] 盤面テンプレートや危険配置テンプレートを、単発 pattern 以外で再利用できるデータとして整理する
- [ ] 旧属性 field / asset の完全削除可否と既存 serialized data の移行方針を決める

## 8. P3: 将来拡張

- [ ] Goal 効果の固有化
- [ ] 頭装備
- [ ] 左手装備
- [ ] `Status` 系スキル効果
- [ ] スタック途中で発動する持続効果
- [ ] `BoardManipulation` / `HazardControl` 系スキル効果
- [ ] `ExtraAction` 系スキル効果
- [ ] 特殊盤面としての Skill ノード
- [ ] 複数敵
- [ ] ランダム盤面生成

## 9. ゲーム改善アイデア TODO

このセクションは、仕様化前の「ゲームをもっと良くする案」を置く場所。
実装する方針が固まったものは、P0〜P3 の該当セクション、仕様書、または個別設計へ移す。
採用しない案、重複した案、古くなった案は残し続けずに削除する。

### 9.1 体験・手触り

### 9.2 戦闘の深み

### 9.3 成長・収集

### 9.4 演出・世界観

- [ ] `docs/specs/世界観仕様書.md` に基づき、冒頭チュートリアルの天使による導入、パス操作の説明、初期会話を実装する。
- [ ] 赤い目、敵の危険、敵 HP / 敵Skill由来の状態、被弾、敵スキルを赤系で統一し、プレイヤー側・確定済みパスとの配色ルールを UI / VFX へ反映する。
- [ ] 人間ボスのステージ前後フレーバーと、赤い目から回復する勝利演出を設計・実装する。
- [ ] ラスボス用の天使形態と赤・黒を基調にした本来の姿、最終ステージ、クリア演出を制作・実装する。
- [ ] クリア済み Main Stage を通常一覧から外す場合の、再挑戦・記録閲覧・報酬確認の導線を設計する。

## 10. まず着手する順番

1. ドラッグ入力中の操作ロックを実装する
2. Skill 消費条件の UI 明示とテストを追加する
3. `AttackChargeGain` を解決済み Attack 数に応じて反映する
4. 勝敗演出、再挑戦導線、戦闘終了時 save を接続する
5. ステージ選択画面と未解放ステージ制御を実装する
