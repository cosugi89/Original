using System;
using UnityEngine;

namespace Assets.Scripts.Core.Appearance
{
    /// <summary>
    /// �L�����N�^�[�̃p�[�c�J�e�S�����`����
    /// �X�v���C�g�����_���[�A�T���l�C���A�\���ؑւ�J���[�ύX�̐ݒ���܂ށB
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
        /// �ŏ��̃����_���[�ɓo�^����Ă���X�v���C�g����Ԃ��B
        /// renderers��sprites��null�܂��͋�̏ꍇ��0��Ԃ��B
        /// </summary>
        public int SpriteCount => renderers != null && renderers.Length > 0 && renderers[0].Sprites != null
            ? renderers[0].Sprites.Length
            : 0;

        /// <summary>
        /// �T���l�C���X�v���C�g�̐���Ԃ��B
        /// </summary>
        public int ThumbnailCount => thumbnails != null ? thumbnails.Length : 0;
    }

    /// <summary>
    /// SpriteRenderer�ƁA���̃����_���[�Ɋ��蓖�ĉ\�ȃX�v���C�g�z���R�t����N���X�B
    /// �L�����N�^�[�p�[�c�̌����ڃo���G�[�V������\������B
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