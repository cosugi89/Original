using System;
using System.Collections.Generic;

namespace LayerLab.ArtMakerUnity
{
    #region parts
    /// <summary>
    /// Assets parts の分類
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
    /// PartsType のカテゴリー
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

    /// <summary>
    /// キャラクターカラーの変更対象タイプ（肌、髪、目、ヒゲ）
    /// </summary>
    public enum ColorTargetType
    {
        Skin,
        Hair,
        Eye,
        Beard
    }

    /// <summary>
    /// UICategoryに対応するPartsTypeのグループ定義および参照機能を提供するクラス
    /// </summary>
    public static class UICategoryConfig
    {
        /// <summary>
        /// カテゴリとパーツの対応表
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
        /// 指定したカテゴリに含まれるPartsTypeの配列を返す
        /// </summary>
        public static PartsType[] GetSubTypes(UICategory category) =>
            SubTypes.TryGetValue(category, out var types) ? types : Array.Empty<PartsType>();

        /// <summary>
        /// 指定したカテゴリが複数のPartsTypeを持つカテゴリかどうか判定する
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
    /// デモシーンで使用するゲームモードの定義
    /// </summary>
    public enum GameMode
    {
        /// <summary>キャラクターカスタマイズモード</summary>
        Home,
        /// <summary>移動操作などを行う体験モード</summary>
        Experience
    }

    /// <summary>
    /// キャラクターアセットの見た目テーマの種類
    /// </summary>
    public enum ThemeType
    {
        Fantasy,
        Military,
        SciFi
    }
    #endregion other
}