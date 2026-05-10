namespace Assets.Scripts.Features.Battle.Core
{
    /// <summary>
    /// Battle 機能全体で共通利用する enum / const 定義。
    /// 読み回りの負荷を増やしすぎないよう、共有値だけをここへ集約する。
    /// </summary>
    public static class BattleAnimationCatalog
    {
        public const string Idle = "Idle";
        public const string Attack = "Attack";
        public const string DoubleAttack = "Double Attack";
        public const string Jump = "Jump";
        public const string JumpAttack = "Jump Attack";
        public const string Roll = "Roll";
        public const string Dance = "Dance";
        public const string Skill = "Skill";
        public const string Stun = "Stun";
        public const string Victory = "Victory";
        public const string Defeat = "Defeat";

        public static string GetClipName(BattlePlayerAnimationCue cue)
        {
            return cue switch
            {
                BattlePlayerAnimationCue.Jump => Jump,
                BattlePlayerAnimationCue.Roll => Roll,
                BattlePlayerAnimationCue.Dance => Dance,
                BattlePlayerAnimationCue.Attack => Attack,
                BattlePlayerAnimationCue.DoubleAttack => DoubleAttack,
                BattlePlayerAnimationCue.JumpAttack => JumpAttack,
                BattlePlayerAnimationCue.Skill => Skill,
                BattlePlayerAnimationCue.Stun => Stun,
                BattlePlayerAnimationCue.Victory => Victory,
                BattlePlayerAnimationCue.Defeat => Defeat,
                _ => string.Empty,
            };
        }
    }

    /// <summary>
    /// 敵ターンの行動カテゴリ。
    /// デモ専用 enum から切り離し、盤面生成や敵 AI の共通キーとして使う。
    /// </summary>
    public enum BattleEnemyActionType
    {
        NormalAttack,
        Skill,
        Dance,
    }

    /// <summary>
    /// Path-Activation System v2 本番盤面で扱うノード種別。
    /// 現在のデモ enum から切り離し、将来の実グリッド実装で共通利用する。
    /// </summary>
    public enum BattleNodeType
    {
        Start,
        Empty,
        Jump,
        Roll,
        Dance,
        Attack,
        HazardNormal,
        HazardSkill,
        Goal,
    }

    /// <summary>
    /// パス入力中に発生しうる不正理由。
    /// </summary>
    public enum BattlePathValidationError
    {
        None,
        OutOfBounds,
        MissingCell,
        InvalidStart,
        NotAdjacent,
        Revisit,
        CrossedSegment,
        ExtendedAfterGoal,
        GoalNotReached,
    }

    /// <summary>
    /// 戦闘解決から表示層へ渡すプレイヤー行動アニメーションの手掛かり。
    /// BattleScene はこの列を順番に再生する。
    /// </summary>
    public enum BattlePlayerAnimationCue
    {
        Jump,
        Roll,
        Dance,
        Attack,
        DoubleAttack,
        JumpAttack,
        Skill,
        Stun,
        Victory,
        Defeat,
    }

    /// <summary>
    /// 属性相性の結果分類。
    /// ログや簡易UIで弱点/耐性を表現するための共通キーとして使う。
    /// </summary>
    public enum BattleAttributeEffectiveness
    {
        None,
        Neutral,
        Weak,
        Resist,
        Immune,
    }

    /// <summary>
    /// 1 回のトレース入力の状態。
    /// </summary>
    public enum BattlePathTraceStatus
    {
        None,
        Started,
        Appended,
        Backtracked,
        Ignored,
        Invalid,
        ReleasedWithoutGoal,
        Confirmed,
    }
}
