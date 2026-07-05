using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// 1 ターンのパス解決結果。
    /// シーン側はこのレポートだけを見て HP 更新、演出、ログ表示を行う想定。
    /// </summary>
    public sealed class BattleTurnResolutionReport
    {
        private readonly List<BattleResolvedAction> _resolvedActions = new();
        private BattleResolvedActionSummary _cachedSummary;
        private bool _isSummaryDirty = true;

        public IReadOnlyList<BattleResolvedAction> ResolvedActions => _resolvedActions;

        public BattleResolvedActionSummary Summary
        {
            get
            {
                if (_isSummaryDirty || _cachedSummary == null)
                {
                    _cachedSummary = BattleResolvedActionSummary.Build(_resolvedActions);
                    _isSummaryDirty = false;
                }

                return _cachedSummary;
            }
        }

        public void ApplyResolvedAction(BattleResolvedAction action)
        {
            if (action == null)
            {
                return;
            }

            _resolvedActions.Add(action);
            _isSummaryDirty = true;
        }

        public void AddSystemLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            ApplyResolvedAction(new BattleResolvedAction
            {
                ActionType = BattleResolvedActionType.SystemLog,
                LogMessages = new[] { message },
            });
        }
    }
}
