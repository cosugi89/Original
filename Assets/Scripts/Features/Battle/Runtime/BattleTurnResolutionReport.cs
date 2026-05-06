using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// 1 ターンのパス解決結果。
    /// シーン側はこのレポートだけを見て HP 更新、演出、ログ表示を行う想定。
    /// </summary>
    public class BattleTurnResolutionReport
    {
        private readonly List<string> _logEntries = new();
        private readonly List<BattlePlayerAnimationCue> _playerAnimationCues = new();

        public bool PathConfirmed { get; set; }

        public bool GoalReached { get; set; }

        public bool GoalEffectTriggered { get; set; }

        public bool TookHit { get; set; }

        public bool StoppedByHazardHit { get; set; }

        public bool EnemyDefeated { get; set; }

        public bool PlayerDefeated { get; set; }

        public bool SelectedSkillConsumed { get; set; }

        public int EnemyDamageTaken { get; set; }

        public int PlayerDamageTaken { get; set; }

        public int ResolvedAttackCount { get; set; }

        public int ResolvedSkillCount { get; set; }

        public int ResolvedHazardCount { get; set; }

        public int ResolvedDanceCount { get; set; }

        public int ResolvedEnemyActionCount { get; set; }

        public int ResolvedNodeCount { get; set; }

        public IReadOnlyList<string> LogEntries => _logEntries;

        public IReadOnlyList<BattlePlayerAnimationCue> PlayerAnimationCues => _playerAnimationCues;

        public void AddLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _logEntries.Add(message);
        }

        public void AddPlayerAnimationCue(BattlePlayerAnimationCue cue)
        {
            _playerAnimationCues.Add(cue);
        }
    }
}
