## UI Toolkit Memo 2026-03-10

- 使い方
- 基本
  - Scene 上の適当なオブジェクト `GameObject` に `UI Document` コンポーネントをアタッチ
  - `UI Document` の `Source Asset` に `uxml` をアタッチ
  - `UI Document` の `Panel Settings` に `PanelSettings` をアタッチ
  - `GameObject` に Script 例 `TitleUI` をアタッチ
  - Script 内で `UI Document` 例 `Title.uxml` の `rootVisualElement` を取得して、そこから必要な UI 要素を取得していく
- ダイアログ
  - ダイアログを表示する場合は、ダイアログ用の `UI Document` 例 `DialogView.uxml` を作成して、Script 内で必要なタイミングでインスタンス化して表示する
  - `DialogView` は `TitleUI` にアタッチ
  - 階層は、`Title` の `modalLayer` / `DialogView` の `dialog` のように各 `uxml` が親子になって紐づけられる

- 挫折
  - ドキュメントが不十分で、情報が散乱しているため、学習コストが高い
  - その影響もあってか、AI に質問しても、正確な回答が得られないことが多い
  - `uss` が思ったより CSS と異なり、スタイルの適用が難しい
  - Web と異なり開発画面でスタイルの確認ができないため、スタイルの調整が難しい
  - UI の調整は素早い画面のレスポンスありきだが、Unity ではコンパイルが挟まるため時間がかかる
  - デバッグが困難で、エラーが発生した場合の原因特定が難しい
  - 実務で活かす機会もないため、あえて使うメリットがない

- 結論
  - コードベースなため AI に丸投げできるだろうと甘く考えたが、余計に時間がかかる
  - 最新の UI システムという触れ込みに乗じたが、未整備な部分が多く、まだ時期じゃない
  - 素直に uGUI を使おう
