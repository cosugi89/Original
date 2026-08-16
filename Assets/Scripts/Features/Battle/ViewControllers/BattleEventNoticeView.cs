using System.Collections;
using Assets.Scripts.Features.Battle.Core;
using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 技名、Goal、回避、被弾、状態変化などの短命な戦闘通知を表示する。
    /// </summary>
    public sealed class BattleEventNoticeView : MonoBehaviour
    {
        [SerializeField] private GameObject rootObject;
        [SerializeField] private TMP_Text noticeText;
        [SerializeField, Min(0.1f)] private float visibleSeconds = 1.5f;

        private Coroutine _hideRoutine;

        public void Configure(GameObject root, TMP_Text text)
        {
            rootObject = root;
            noticeText = text;
        }

        public void ResetView()
        {
            StopHideRoutine();
            SetText(string.Empty);
            SetVisible(false);
        }

        public void Show(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                ResetView();
                return;
            }

            StopHideRoutine();
            SetText(message);
            SetVisible(true);
            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        public void ShowTurnResult(BattleResolvedActionSummary summary)
        {
            Show(BuildTurnResultNotice(summary));
        }

        public void ShowResolvedAction(BattleResolvedAction action)
        {
            var message = BuildResolvedActionNotice(action);
            if (!string.IsNullOrWhiteSpace(message))
            {
                Show(message);
            }
        }

        public void ShowInputRejected(string message)
        {
            Show(message);
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(visibleSeconds);
            _hideRoutine = null;
            SetVisible(false);
        }

        private void StopHideRoutine()
        {
            if (_hideRoutine == null)
            {
                return;
            }

            StopCoroutine(_hideRoutine);
            _hideRoutine = null;
        }

        private void SetVisible(bool visible)
        {
            if (rootObject != null)
            {
                rootObject.SetActive(visible);
            }

            if (noticeText != null)
            {
                noticeText.enabled = visible;
            }
        }

        private void SetText(string value)
        {
            if (noticeText != null)
            {
                noticeText.text = value ?? string.Empty;
            }
        }

        private static string BuildTurnResultNotice(BattleResolvedActionSummary summary)
        {
            if (summary == null)
            {
                return string.Empty;
            }

            if (summary.EnemyDefeated)
            {
                return "撃破";
            }

            if (summary.PlayerDefeated)
            {
                return "敗北";
            }

            if (summary.ResolvedSkillCount > 0 && !string.IsNullOrWhiteSpace(summary.SkillDisplayName))
            {
                return $"{summary.SkillDisplayName}!";
            }

            if (summary.NextTurnHazardBoostReserved)
            {
                return "危険が増した";
            }

            if (summary.StoppedByHazardHit || summary.TookHit || summary.PlayerDamageTaken > 0)
            {
                return "被弾";
            }

            if (summary.ResolvedHazardCount > 0)
            {
                return "回避";
            }

            if (summary.GoalReached)
            {
                return "Goal";
            }

            if (summary.ResolvedAttackCount > 0)
            {
                return "Attack";
            }

            return string.Empty;
        }

        private static string BuildResolvedActionNotice(BattleResolvedAction action)
        {
            if (action == null)
            {
                return string.Empty;
            }

            if (action.HasSemantic(BattleResolvedActionSemantics.EnemyDefeated))
            {
                return "撃破";
            }

            if (action.HasSemantic(BattleResolvedActionSemantics.PlayerDefeated))
            {
                return "敗北";
            }

            if (action.ActionType == BattleResolvedActionType.HazardHit ||
                action.HasSemantic(BattleResolvedActionSemantics.TookHit))
            {
                return "被弾";
            }

            if (action.ActionType == BattleResolvedActionType.HazardEvaded)
            {
                return "回避";
            }

            if (action.HasSemantic(BattleResolvedActionSemantics.ResolvedSkill) &&
                !string.IsNullOrWhiteSpace(action.DisplayName))
            {
                return $"{action.DisplayName}!";
            }

            if (action.ActionType == BattleResolvedActionType.HazardBoostReserved ||
                action.HasSemantic(BattleResolvedActionSemantics.NextTurnHazardBoostReserved))
            {
                return "危険が増した";
            }

            if (action.ActionType == BattleResolvedActionType.GoalReached)
            {
                return "Goal";
            }

            if (action.ActionType == BattleResolvedActionType.PlayerAttack)
            {
                return "Attack";
            }

            if (action.ActionType == BattleResolvedActionType.Dance)
            {
                return "Dance";
            }

            if (action.PlayerHealing > 0)
            {
                return "回復";
            }

            return string.Empty;
        }
    }
}
