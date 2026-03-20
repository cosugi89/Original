using System;
using UnityEngine;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// キャラクターのパーツカテゴリを定義するクラス。
    /// スプライトレンダラー、サムネイル、表示切替やカラー変更の設定を含む。
    /// </summary>
    [Serializable]
    public class PartsCategory
    {
        /// <summary>このカテゴリが表すパーツタイプ</summary>
        public PartsType type;

        /// <summary>UIに表示される人間向けの表示名</summary>
        public string displayName;

        /// <summary>SpriteRendererと対応するスプライト配列を持つパートレンダラーの配列</summary>
        public PartRenderer[] renderers;

        /// <summary>このカテゴリが表示/非表示（装備/解除）の切り替え可能かどうか</summary>
        public bool canToggle = true;

        /// <summary>このカテゴリがカラー変更に対応しているかどうか</summary>
        public bool canChangeColor;

        /// <summary>カラー変更時に使用する対象タイプ</summary>
        public ColorTargetType colorTarget;

        /// <summary>このカテゴリがすべてのテーマで共通かどうか</summary>
        public bool isCommon;

        /// <summary>パーツ一覧UIで使用する専用サムネイルスプライト（任意）</summary>
        public Sprite[] thumbnails;

        /// <summary>
        /// 最初のレンダラーに登録されているスプライト数を返す。
        /// renderersやspritesがnullまたは空の場合は0を返す。
        /// </summary>
        public int SpriteCount => renderers != null && renderers.Length > 0 && renderers[0].sprites != null
            ? renderers[0].sprites.Length
            : 0;

        /// <summary>
        /// サムネイルスプライトの数を返す。
        /// </summary>
        public int ThumbnailCount => thumbnails != null ? thumbnails.Length : 0;
    }

    /// <summary>
    /// SpriteRendererと、そのレンダラーに割り当て可能なスプライト配列を紐付けるクラス。
    /// キャラクターパーツの見た目バリエーションを表現する。
    /// </summary>
    [Serializable]
    public class PartRenderer
    {
        /// <summary>キャラクターに設定される対象のSpriteRenderer</summary>
        public SpriteRenderer renderer;

        /// <summary>このレンダラーに設定可能なスプライトの配列</summary>
        public Sprite[] sprites;
    }
}