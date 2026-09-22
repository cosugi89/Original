# Unity / Trait Line トラブルシューティング Tips

- 最終更新: 2026-09-20
- 対象: Unity 6000.0 と Ability の Trait Tree 接続線

## 1. URP Global Settings の Missing Type 警告

### 症状

`UniversalRenderPipelineGlobalSettings` の import 時に、`LightmapSamplingSettings`、`URPReflectionProbeSettings`、`VrsRenderPipelineRuntimeResources` などの Missing Type 警告が出る。

### 原因

`Packages/manifest.json` / `packages-lock.json` の URP バージョンと、`Assets/UniversalRenderPipelineGlobalSettings.asset` が保存された時点の URP バージョンが一致していない。新しいURPで保存された Setting の型参照を、古いURPが復元できない状態である。

### 対応

1. `manifest.json`、`packages-lock.json`、Unity Editor のURPを同じ系統へそろえる。本プロジェクトの Unity 6000.0 では `com.unity.render-pipelines.universal` 17.0.4 を使用する。
2. Global Settings の YAML に残った、現行URPに存在しない型参照だけを除去する。Pipeline Asset、Graphics Settings、既存の有効な Setting は置き換えない。
3. `Assets/UniversalRenderPipelineGlobalSettings.asset` を再importし、Consoleに Missing Type 警告が出ないことを確認する。

URPのSetting警告はShaderのコンパイルエラーとは別物である。ただしシリアライズの不整合を残すとimportや設定変更の調査を難しくするため、先に解消する。

## 2. Trait Line の画像全体が着色される

### 症状

線だけをDisabled / Enabled色で描画したいのに、`Image` の矩形全体が灰色または金色で塗られる。

### 原因

`UI/Trait Line Reveal` は `Blend One OneMinusSrcAlpha` を使う。このブレンドはプリマルチプライドAlphaを前提にするが、修正前のFragment Shaderは透明ピクセルのRGBをAlphaで乗算せず返していた。そのため、Alphaが0のテクスチャ領域でもRGBが背景へ加算され、矩形全体が着色された。

### 対応

`Assets/Shaders/UI/TraitLineReveal.shader` のFragment Shaderで、return前に次を実行する。

```hlsl
color.rgb *= color.a;
return color;
```

これにより透明ピクセルのRGBが0になり、線テクスチャの不透明部分だけが描画される。Shaderを再importし、ConsoleにShader compile errorがないことを確認する。

### Transparent Cutoff の注意

`_TransparentCutoff` の `clip` は `UNITY_UI_ALPHACLIP` が有効なときだけ評価される。Material Inspector の `Use Alpha Clip` が無効なら、Cutoff値を変えても見た目は変化しない。

Cutoffは輪郭のしきい値調整用であり、透明RGBによる矩形着色の修正手段ではない。後者はプリマルチプライドAlphaで解消する。

## 3. Trait Line の参照配線を確認する

`PartsTraitLine` は実行時に各 `Image` 用のMaterial複製と色設定を作る。次のInspector参照が欠けると、対象のラインでは処理が走らない。

- `Center Line`
- `Left Line`
- `Right Line`

本プロジェクトでは、共通の `Parts-TraitLine.prefab` ではなく、`Parts-TraitTreeRow.prefab` 内のネスト済みインスタンスで参照をOverrideする構成である。

- `Parts-TraitLine-OneToOne`: `OneToOne/Center`
- `Parts-TraitLine-OneToTwo`: `OneToTwo/Left`、`OneToTwo/Right`
- `Parts-TraitLine-TwoToTwo`: `TwoToTwo/Left`、`TwoToTwo/Right`
- `Parts-TraitLine-TwoToOne`: `TwoToOne/Left`、`TwoToOne/Right`

表示不具合を調べる際はRootだけで判断せず、実際に描画している子 `Image` の `Material` が `Trait Line Reveal` か、該当するネストPrefabの参照が配線済みかを確認する。

## 4. 確認順

1. ConsoleでURP Missing TypeとShader compile errorを分けて確認する。
2. 実際に表示される子 `Image`、Sprite、Materialを確認する。
3. Materialの `Use Alpha Clip` と `Transparent Cutoff` を確認する。
4. `PartsTraitLine` の各参照が、利用するネストPrefabのインスタンスで配線されていることを確認する。
5. Shaderを再importし、Consoleに新しいError / Warningが出ないことを確認する。
