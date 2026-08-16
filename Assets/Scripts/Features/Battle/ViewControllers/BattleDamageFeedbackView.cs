using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// プレイヤーと敵に発生したダメージ値と属性補足を表示する。
    /// </summary>
    public sealed class BattleDamageFeedbackView : MonoBehaviour
    {
        [SerializeField] private GameObject rootObject;
        [SerializeField] private TMP_Text enemyDamageText;
        [SerializeField] private TMP_Text enemyDamageAttributeText;
        [SerializeField] private TMP_Text playerDamageText;
        [SerializeField] private TMP_Text playerDamageAttributeText;

        public void ResetView()
        {
            if (rootObject != null)
            {
                rootObject.SetActive(false);
            }

            SetText(enemyDamageText, string.Empty);
            SetText(enemyDamageAttributeText, string.Empty);
            SetText(playerDamageText, string.Empty);
            SetText(playerDamageAttributeText, string.Empty);
        }

        public void Apply(BattleResolvedActionSummary summary)
        {
            if (summary == null)
            {
                ResetView();
                return;
            }

            var hasEnemyDamage = summary.EnemyDamageTaken > 0 ||
                                 !string.IsNullOrWhiteSpace(summary.EnemyDamageAttributeSummary);
            var hasPlayerDamage = summary.PlayerDamageTaken > 0 ||
                                  !string.IsNullOrWhiteSpace(summary.PlayerDamageAttributeSummary);

            if (rootObject != null)
            {
                rootObject.SetActive(hasEnemyDamage || hasPlayerDamage);
            }

            SetText(enemyDamageText, hasEnemyDamage ? $"-{summary.EnemyDamageTaken}" : string.Empty);
            SetText(enemyDamageAttributeText, summary.EnemyDamageAttributeSummary);
            SetText(playerDamageText, hasPlayerDamage ? $"-{summary.PlayerDamageTaken}" : string.Empty);
            SetText(playerDamageAttributeText, summary.PlayerDamageAttributeSummary);
        }

        public void Apply(BattleResolvedAction action)
        {
            if (action == null)
            {
                return;
            }

            var enemyDamageValue = string.Empty;
            var enemyAttribute = string.Empty;
            var playerDamageValue = string.Empty;
            var playerAttribute = string.Empty;

            var resolution = action.DamageResolution;
            if (resolution != null && resolution.Damage > 0)
            {
                if (action.TargetsEnemy)
                {
                    enemyDamageValue = $"-{resolution.Damage}";
                    enemyAttribute = resolution.AttributeSummary;
                }
                else
                {
                    playerDamageValue = $"-{resolution.Damage}";
                    playerAttribute = resolution.AttributeSummary;
                }
            }

            if (action.PlayerHealing > 0)
            {
                playerDamageValue = $"+{action.PlayerHealing}";
                playerAttribute = "回復";
            }

            var hasEnemyFeedback = !string.IsNullOrWhiteSpace(enemyDamageValue) ||
                                   !string.IsNullOrWhiteSpace(enemyAttribute);
            var hasPlayerFeedback = !string.IsNullOrWhiteSpace(playerDamageValue) ||
                                    !string.IsNullOrWhiteSpace(playerAttribute);
            if (!hasEnemyFeedback && !hasPlayerFeedback)
            {
                return;
            }

            if (rootObject != null)
            {
                rootObject.SetActive(true);
            }

            SetText(enemyDamageText, enemyDamageValue);
            SetText(enemyDamageAttributeText, enemyAttribute);
            SetText(playerDamageText, playerDamageValue);
            SetText(playerDamageAttributeText, playerAttribute);
        }

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }
    }
}
