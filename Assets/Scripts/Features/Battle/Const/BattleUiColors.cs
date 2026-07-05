using UnityEngine;

namespace Assets.Scripts.Features.Battle.Const
{
    /// <summary>
    /// Battle UI の共通色をまとめる。
    /// </summary>
    public static class BattleUiColors
    {
        public static readonly Color NormalSkillButton = new(1f, 1f, 1f, 1f);
        public static readonly Color SelectedSkillButton = new(1f, 0.85f, 0.35f, 1f);
        public static readonly Color GuardGauge = new(0.36f, 0.82f, 1f, 1f);
        public static readonly Color AttackGauge = new(1f, 0.55f, 0.32f, 1f);
    }
}
