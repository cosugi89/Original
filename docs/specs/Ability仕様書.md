# Ability 仕様書

- 最終更新: 2026-09-22
- 文書状態: Trait Treeの進捗保存・購入・UI接続まで実装済み。Point報酬とBattle/Status反映は未実装。
- 位置づけ: MainScene の Ability タブにおける恒久成長Tree、Point、進捗保存の設計正本。

## 1. 目的とスコープ

Ability タブは、プレイヤーの恒久成長を管理する入口とする。初期実装ではサブタブを削除せず、`Trait` Tree だけを実装する。将来の `Artifact` などは、対応するゲーム機能が成立した時点で追加する。

`Trait` は装備・タグ・候補Skillによるビルドを置き換えず、基礎能力と行動傾向を補助する恒久成長として扱う。

## 2. Point

- 旧 `Gem` は `Point` として扱う。`Gold` は将来のShop用通貨として分離する。
- Point の初期所持数は 0。
- Point は主にステージ初回クリアと進行マイルストーンで付与する。周回報酬にはしない。
- ステージマスターはPoint報酬額を持ち、各ステージ進捗は報酬受領済み状態を保存する。
- Node購入ではPoint消費、NodeLevel更新、導出データ再計算、即時保存を一つの処理で行う。
- MainScene上部の `pointBalanceText` は `UserProfileData.Gold`、Abilityタブ内の `abilityPointText` は `AbilityProgressData.PointBalance` を表示する。GoldまたはPointを変更したControllerは `MainScene.RefreshPointTexts()` を呼び、表示を保存残高へ同期できる。UI参照が未設定でも、この更新処理は画面初期化を止めない。

## 3. Tree とNode

- 初期版は `Trait` Tree 1本だけを実装する。
- Treeは最終的に全取得不可とし、分岐は実際に排他となる。
- Nodeはそれぞれ独立した `AbilityNodeMasterData` Assetとする。Nodeは `UnlockOnly` と `Ranked` の二種類を持つ。ただしAssetの型を分けず、共通のNode定義へ `ProgressionType` とRank定義を持たせる。
- `UnlockOnly` はRankを1件、`Ranked` はRankを2件以上持つ。
- NodeのRankごとにPointコストと効果を定義する。
- `Prerequisite` 接続を一件も持たない開始Nodeは、初期解放の入口として最大Levelに達するまで常に購入可能とする。開始Nodeの購入コストは導出上 `0` とし、Pointを消費しない。
- 前提Nodeは必要Levelを指定できる。通常はLv.1で次Nodeを解放し、既存Nodeは最大Levelまで継続して強化できる。

## 4. 排他分岐

- 初期版のTreeにおける排他分岐は1回とする。ただしTree接続は複数の排他接続を持てるようにする。
- 分岐入口Node同士を `Exclusive` 接続で直接結ぶ。どちらかを最初にLv.1へ上げた時点で、接続先Nodeを購入不可にする。入口Nodeは `UnlockOnly` に限定しない。
- 入口以降のNodeは対応する入口Nodeを前提に持たせるため、反対Branchも連鎖的に購入不可となる。UIでは非表示にせずLocked状態と理由を表示する。
- 分岐後の共通Nodeは再合流できる。再合流Nodeの配置は複数の前提接続を持ち、`Any` 条件でいずれか一方を満たせばよい。
- リスペック・払い戻しは初期実装では提供しない。

## 5. 保存と導出

保存の正本はPoint残高とTree配置Nodeごとの現在Levelだけとする。

- `AbilityNodeMasterData.NodeId` はNodeの種類を示す不変文字列とする。例: `trait.vitality`。同じNode Masterを1本のTree内で複数回使ってよい。
- `AbilityTreeNodeEntry.TreeNodeId` はTree内の配置Nodeを示す一意かつ不変の文字列とする。例: `trait.vitality.2`。保存、進捗、効果、接続、排他の単位はこのIDであり、同じNode Masterを参照する別配置Nodeとは独立する。
- Level 0の配置Nodeは保存しない。
- `IsUnlocked`、選択済みBranch、能力値合計、解放済み機能フラグは保存しない。NodeLevelとTree定義から導出する。
- ロード時は、未知のTreeNodeIdを保持したまま無視する。Levelは0以上かつ参照するNode Masterの最大Level以下へclampする。重複TreeNodeIdは最大Levelを採用する。
- 排他グループに複数Branchの取得データがある不正状態では、保存順で最初の有効Nodeを正とし、他BranchのLevelを0へ戻して警告ログを出す。

Node効果は保存せず、起動時とNode購入時に `AbilityDerivedData` として再計算する。BattleとUIはこの実行時キャッシュを参照する。マスターの効果値を変更した場合、次回ロードまたは購入後の再計算から反映される。

## 6. 効果と能力値

初期のTrait効果は、`HP`、`Attack`、`Defense` の恒久加算を中心にする。乗算効果、Skill回転補助、機能解放はTree基盤と能力値算出ルールが安定してから追加する。

最終ステータスの合成順は未確定であり、StatusSceneのステータス算出仕様と合わせて決定する。

## 7. マスター管理

Tree構造は、Tree単位のマスターデータで接続として管理する。`AbilityNodeMasterData`、`AbilityTreeMasterData` とInspectorの検証表示は実装済みであり、Asset作成後にInspectorから問題を確認できる。

旧形式で作成した `Assets/Resources/MasterData/Abilities/TraitTree.asset` は移行対象として残すが、新形式のNode参照を持たないため使用前に作り直す。動作確認には `Assets/Resources/MasterData/Abilities/TraitTreeSample.asset` と `Abilities/Nodes/` 配下の4 Nodeを使う。サンプルTreeは `Basic Training` の後に `Vitality` / `Strength` の排他分岐を置き、`Adapt` へ `Any` 前提で再合流した後、1→1の終端Nodeを置く。正式なゲームバランス用データではない。

- `AbilityNodeMasterData`: Node ID、表示情報、Icon、`ProgressionType`、Rank一覧と効果。Tree接続は持たない。
- `AbilityTreeMasterData`: Tree ID、表示情報、`AbilityTreeNodeEntry` の一覧、接続一覧。画面配置は持たない。
- `AbilityTreeNodeEntry`: Tree内での配置Nodeを表す。固有の `TreeNodeId` と参照先の `AbilityNodeMasterData`、前提判定を持つ。同じNode Masterへの複数配置を許可する。
- `AbilityTreeLinkDefinition`: `Prerequisite`（From TreeNodeId → To TreeNodeId、必要Level）または `Exclusive`（両配置Node間の無向排他）を定義する。
- `Any` / `All` はTree上のNode Entryが持つ前提判定であり、複数前提接続をどう満たすかだけを示す。管理用Groupは持たない。
- 次Node一覧は保存せず、Tree接続から導出する。UIの接続線も同じ接続を使う。
- Treeのゲーム上の接続は `Links` を正本とする。画面の縦Row配置は `AbilityTreeMasterData.DisplayRows` に保存し、並び順はTreeの始点から終点までとする。各RowはCenter 1件、またはLeft / Right 2件のいずれかで `TreeNodeId` を参照する。開始・終端・前後Node数・接続線パターンはDisplayRowsとLinksから導出し、重複保存しない。
- `TraitTabScreen` はScroll Contentに `PartsTraitTreeRow` PrefabをDisplayRowsごとに生成する。接続線は同じImageをDisabled色とEnabled色で表示し、解放時は曲線に沿った進行度を記録したReveal Mapを使って、Disabled色からEnabled色へ始点から終点まで切り替わる演出を行う。
- Tree表示では、自身がLv.1以上ならFocused表示とする。未取得の開始NodeはNormal、未取得の後続Itemは直前Rowの必要前提NodeがLv.1以上ならNormal、それ以外はLockedとする。Lockedは前提不足により将来解放され得る状態であり、鍵アイコンとLevel Textを表示しない。排他で選ばれなかったNode、および到達不能な前提を持つNodeはDisabledとし、鍵アイコンとLevel Textを表示しない。`TwoToTwo` では、対応する直前前提NodeがDisabledならその継続NodeもDisabledにする。複数前提は `Any` ではいずれか、`All` ではすべての直前接続元がLv.1以上であることを必要とする。通常の接続線は接続先NodeがLv.1以上のときEnabled色で表示する。`TwoToOne` の合流線は、合流先NodeがLv.1以上であり、かつ対応する接続元NodeがLv.1以上の側だけEnabled色で表示する。合流先が未取得なら左右ともDisabled色で表示する。これは購入可能状態ではなく、取得済みの経路を示す表示規則とする。
- Trait Itemは表示状態にかかわらず詳細Dialogを開く。`TraitItemDetailDialog` はNode状態とItemの `VisualStyle` を受け取る。最大Level、排他、到達不能な前提、不足している前提では購入ボタンを非表示にする。Point不足だけは購入ボタンを表示し、押下時は現時点では不足内容をConsoleへ出力して購入を行わない。購入可能な場合、ボタンは購入要求だけを返し、`TraitTabScreen` が `AbilityProgressManager.TryPurchase` を呼ぶ。購入後は導出結果からTreeを即時再構築する。
- MasterDataの編集には `Tools > Original > Ability Tree Graph` または `AbilityTreeMasterData` Inspectorの「Tree編集ウィンドウを開く」を使える。Window左側は進行開始点を下、後続Nodeを上に置く接続グラフであり、配置Node間の接続を線で表示する。カードは表示名、`TreeNodeId`、前提条件だけを表示し、カード選択で右側の編集対象を切り替える。右側では通常操作として開始Nodeまたは後続Nodeを追加し、分岐・合流、接続編集、Node定義編集、削除操作は折りたたみ項目に分ける。既存の配置Nodeへの接続は再合流に使い、新規追加とは区別する。再合流候補は配置Node単位で表示し、自己接続、重複、循環だけを除外して理由を表示する。排他経路から `All` 条件の配置Nodeへ合流する候補は接続可能とし、「いずれか」へ変更する必要があることを表示する。選択済み配置のNode Masterは接続を維持したまま差し替えられる。「接続を確認・編集」では接続単位で削除と必要Level変更を行い、配置Nodeを削除する場合は関連接続だけを削除して他の配置NodeとNode Master Assetを残す。Node Masterを編集すると、そのAssetを参照する全配置Nodeに反映される。Nodeの画面座標は保存せず、ゲームのUIレイアウトや実行時データには含めない。
- Editor検証では、TreeNodeId重複、Tree外の配置Node接続、空・重複・自己接続、到達不能な必要Level、循環依存、Rank数と `ProgressionType` の不整合を検出する。

## 8. 実装順

1. AbilityTreeMasterDataとEditor検証を作成する。完了。
2. Point、NodeLevelの保存モデル、導出と購入を追加する。完了。ステージ初回報酬受領状態は未実装。
3. Trait Treeの表示・購入・排他分岐を実装する。完了。
4. ステータス算出仕様の確定後、Trait効果をBattleとStatus表示へ接続する。未実装。
