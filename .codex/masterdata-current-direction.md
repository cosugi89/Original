# マスターデータ現行方針

このメモは、このワークスペースでの Codex の作業記憶です。
プロジェクト仕様ではなく、AI が実装判断をするときの補助資料として扱います。

最終更新: 2026-07-12

## 中心方針

- マスターデータの一覧 / 検索用 asset の命名は `Database` に統一する。
- マスターデータの識別子は `int` に統一する。
- 作成時の見通しが良い場合は、master asset 同士の参照を直接 `ScriptableObject` 参照として持たせる。
- `Database` asset は直接参照で表現できる作成時関係の正本ではなく、一覧や検索の補助として扱う。
- `BattleStage*` を長期的に別系列として残さず、その役割は `StageMaster*` へ吸収する。
- 検証はマスターデータ設計の主目的にしない。通常は作成時の軽さを優先する。
- `MasterDataResourceLoader` の代替処理や旧互換分岐は、残し続けるのではなく段階的に減らす。

## 命名方針

次のような名前を優先する。

- `StageMasterData`
- `StageMasterDatabase`
- `StageBattleEnemyMasterData`
- `StageBattleEnemyMasterDatabase`
- `StageBattlePatternMasterData`
- `StageBattlePatternMasterDatabase`
- `EquipmentMasterData`
- `EquipmentMasterDatabase`
- `BattleSkillMasterData`
- `BattleSkillOperationData`
- `BattleSkillMasterDatabase`
- `EffectAnimationMasterData`

同じ役割のものに `Catalog` と `Database` を混在させない。

## 識別子方針

- 新規のマスターデータ ID は `int` を使う。
- 既存の string ID は、維持する強い理由がない限り移行対象として考える。
- セーブ / 実行時データでも、マスターデータ参照は `int` ID を優先する。

## 参照方針

作成側の関係は、必要に応じて直接 `ScriptableObject` 参照で持ってよい。

- `Stage -> Enemy`
- `Enemy -> Pattern`
- `Equipment -> Skill`

スキル効果の採用区分や効果配列の詳細は `docs/specs/スキル仕様書.md` を正とする。

`Database` asset は次の用途で引き続き有用。

- グローバルな一覧
- ID 検索
- 選択 UI の候補元
- 移行や検証の補助

## ステージ方針

- `StageMasterData` を、長期的なステージマスターデータの唯一の正本にする。
- `BattleStageMasterData` / `BattleStageMasterCatalog` は望ましい最終形ではない。
- ステージ選択固有のデータが必要になった場合は、並行するステージマスター系列を維持するより `StageMasterData` の拡張を優先する。

## 検証方針

- 基本姿勢として、マスターデータに重い検証優先の構成は導入しない。
- 作成時の負担を低く保つ。
- 破損を直接防げる小さな局所チェックは許容するが、検証の仕組みが設計の中心にならないようにする。

## ローダー方針

`MasterDataResourceLoader` は移行期間中、一時的に代替処理 / 旧互換分岐を持ってよい。
長期的には次を削除する。

- 旧重複マスターパス
- asset 移行済みの古い互換分岐
- 本当の正本を見えにくくする不要な代替生成

広い互換中継役より、現在のマスターデータを素直に解決する単純なローダーを優先する。

## 今後のリファクタリング作業ルール

`Assets/Scripts/Data/MasterData` に触るときは、次の順番を優先する。

1. 並行命名や重複概念を取り除く。
2. ID 型を `int` に正規化する。
3. 直接参照による作成のほうが明確な箇所は維持する。
4. `Database` は共有検索 / 一覧レイヤーとしてだけ残す。
5. 現在の asset が移行済みなら、旧互換 / 代替経路を減らす。
