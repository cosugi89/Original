using System;
using System.Collections.Generic;

namespace Assets.Scripts.Core.Appearance
{
    #region parts
    /// <summary>
    /// Assets parts �̕���
    /// </summary>
    public enum PartsType
    {
        Eye,
        Hair,
        Helmet,
        Beard,
        Chest,
        Sword,
        Axe,
        Bow,
        Shield,
        Wand,
        Staff,
        Spear,
        Blunt,
        Crossbow,
        SubItem,
        Arrow,
        HelmetHair,
        Skin
    }

    /// <summary>
    /// PartsType �̃J�e�S���[
    /// </summary>
    public enum UICategory
    {
        Hair,
        Eye,
        Beard,
        Skin,
        Helmet,
        Chest,
        HandRight,
        HandLeft,
    }

    public enum PartsExclusiveGroup
    {
        None,
        HandRight,
        HandLeft
    }

    /// <summary>
    /// �L�����N�^�[�J���[�̕ύX�Ώۃ^�C�v�i���A���A�ځA�q�Q�j
    /// </summary>
    public enum ColorTargetType
    {
        Skin,
        Hair,
        Eye,
        Beard
    }

    /// <summary>
    /// UICategory�ɑΉ�����PartsType�̃O���[�v��`����юQ�Ƌ@�\��񋟂���N���X
    /// </summary>
    public static class UICategoryConfig
    {
        /// <summary>
        /// �J�e�S���ƃp�[�c�̑Ή��\
        /// </summary>
        private static readonly Dictionary<UICategory, PartsType[]> SubTypes = new()
        {
            { UICategory.Hair, new[] { PartsType.Hair } },
            { UICategory.Eye, new[] { PartsType.Eye } },
            { UICategory.Beard, new[] { PartsType.Beard } },
            { UICategory.Skin, new[] { PartsType.Skin } },
            { UICategory.Helmet, new[] { PartsType.Helmet } },
            { UICategory.Chest, new[] { PartsType.Chest } },
            {
                UICategory.HandRight,
                new[]
                {
                    PartsType.Sword, PartsType.Axe, PartsType.Bow,
                    PartsType.Wand, PartsType.Staff, PartsType.Spear,
                    PartsType.Blunt, PartsType.Crossbow
                }
            },
            { UICategory.HandLeft, new[] { PartsType.Shield, PartsType.SubItem } },
        };

        /// <summary>
        /// �w�肵���J�e�S���Ɋ܂܂��PartsType�̔z���Ԃ�
        /// </summary>
        public static PartsType[] GetSubTypes(UICategory category) =>
            SubTypes.TryGetValue(category, out var types) ? types : Array.Empty<PartsType>();

        /// <summary>
        /// �w�肵���J�e�S����������PartsType�����J�e�S�����ǂ������肷��
        /// </summary>
        public static bool IsGroup(UICategory category) =>
            SubTypes.TryGetValue(category, out var types) && types.Length > 1;
    }
    #endregion parts

    #region animation
    public enum AnimationType
    {
        Idle = 0,
        Walk = 1,
        Attack = 2,
        Skill = 3,
        Run = 4,
        Roll = 5,
        Jump = 6,
        JumpAttack = 7,
        Dance = 8,
        Victory = 9,
        Defeat = 10,
        Stun = 11,
        Dead1 = 12,
        Dead2 = 13,
        Dead3 = 14,
        DoubleAttack = 15,
    }
    #endregion animation

    #region other
    /// <summary>
    /// �f���V�[���Ŏg�p����Q�[�����[�h�̒�`
    /// </summary>
    public enum GameMode
    {
        /// <summary>�L�����N�^�[�J�X�^�}�C�Y���[�h</summary>
        Home,
        /// <summary>�ړ�����Ȃǂ��s���̌����[�h</summary>
        Experience
    }

    /// <summary>
    /// �L�����N�^�[�A�Z�b�g�̌����ڃe�[�}�̎��
    /// </summary>
    public enum ThemeType
    {
        Fantasy,
        Military,
        SciFi
    }
    #endregion other
}