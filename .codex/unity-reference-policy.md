# Unity 参照方針

このメモは、この workspace で Codex が Unity 実装を行う際のコーディング規約です。
プロジェクト文書ではなく、AI 作業時の判断基準として扱ってください。

## 基本方針

- 必須の `[SerializeField]` 参照は、Scene / prefab / ScriptableObject の作成時設定で配線済みである前提にする。
- 必須の serialized field に対して、実行時 null ガード、遅延代替代入、警告だけ出して return する処理を追加しない。
- 本当に任意参照の serialized field が必要な場合は、フィールド名、Tooltip、周辺コードのいずれかで「任意である」ことを明示してから optional 扱いにする。

## 参照の取り方

- Unity 依存は、明示的な serialized reference を優先する。
  - Scene 上の object 参照
  - prefab 参照
  - ScriptableObject 参照
  - Editor tooling で生成・同期された Inspector 上のリスト参照
- 通常の runtime 依存では `Resources.Load<T>()` を避ける。
  - 負荷が大きい
  - 作成時の配線ミスを隠しやすい
  - asset のディレクトリ変更に弱い
- 既存の `Resources.Load<T>()` 代替処理に触る場合は、代替処理を維持するより serialized reference へ移行することを優先する。
- Editor 専用の生成ツールや移行ツールでは、そのツールの明示的な作成フローの一部として asset path を使ってよい。ただし実行時コードはその path に依存しない。

## 作成時の前提

- 必須 Inspector 参照の欠落は作成時エラーであり、実行時に回復する状態ではない。
- 代替 asset を静かに代入するより、セットアップ、Editor 生成、検証、Play Mode テストの段階で見える形で失敗させる。
- Scene の配線を将来の開発者やデザイナーが理解できるよう、参照は Inspector 上で見える状態を優先する。

## 現在の Battle UI 方針

- `BattleBoardController` は、盤面セル prefab と node visual database を Inspector 参照で受け取る。
- `BattleCellView` のアイコンと色は、割り当て済みの `BattleNodeVisualDatabase` から取得する。
- Battle 盤面表示に path-based loading を再導入しない。
