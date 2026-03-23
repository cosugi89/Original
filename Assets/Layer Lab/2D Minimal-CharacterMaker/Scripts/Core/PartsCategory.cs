using System;
using UnityEngine;

namespace LayerLab.ArtMakerUnity
{
    /// <summary>
    /// キャラクターのパーツカテゴリを定義する
    /// スプライトレンダラー、サムネイル、表示切替やカラー変更の設定を含む。
    /// </summary>
    [Serializable]
    public class PartsCategory
    {
        [SerializeField] private PartsType type;
        [SerializeField] private string displayName;
        [SerializeField] private UICategory uiCategory;
        [SerializeField] private PartsExclusiveGroup exclusiveGroup;
        [SerializeField] private bool defaultVisible = true;
        [SerializeField] private PartRenderer[] renderers;
        [SerializeField] private bool canChangeColor;
        [SerializeField] private ColorTargetType colorTarget;
        [SerializeField] private bool isCommon;
        [SerializeField] private Sprite[] thumbnails;
        
        public PartsType Type 
        { 
            get => type; 
            set => type = value;
        }

        public string DisplayName 
        {
            get => displayName; 
            set => displayName = value;
        }

        public UICategory UICategory
        {
            get => uiCategory;
            set => uiCategory = value;
        }

        public PartsExclusiveGroup ExclusiveGroup
        {
            get => exclusiveGroup;
            set => exclusiveGroup = value;
        }

        public bool DefaultVisible
        {
            get => defaultVisible;
            set => defaultVisible = value;
        }

        public PartRenderer[] Renderers 
        {
            get => renderers;
            set => renderers = value; 
        }

        public bool CanChangeColor 
        { 
            get => canChangeColor;
            set => canChangeColor = value;
        }

        public ColorTargetType ColorTarget 
        { 
            get => colorTarget;
            set => colorTarget = value;
        }

        public bool IsCommon 
        { 
            get => isCommon; 
            set => isCommon = value; 
        }

        public Sprite[] Thumbnails
        { 
            get => thumbnails; 
            set => thumbnails = value; 
        }

        /// <summary>
        /// 最初のレンダラーに登録されているスプライト数を返す。
        /// renderersやspritesがnullまたは空の場合は0を返す。
        /// </summary>
        public int SpriteCount => renderers != null && renderers.Length > 0 && renderers[0].Sprites != null
            ? renderers[0].Sprites.Length
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
        [SerializeField] private SpriteRenderer renderer;
        [SerializeField] private Sprite[] sprites;

        public SpriteRenderer Renderer
        {
            get => renderer;
            set => renderer = value; 
        }

        public Sprite[] Sprites
        {
            get => sprites;
            set => sprites = value; 
        }
    }
}