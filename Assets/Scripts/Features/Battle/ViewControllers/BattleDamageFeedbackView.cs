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

        private static void SetText(TMP_Text target, string value)
        {
            if (target != null)
            {
                target.text = value ?? string.Empty;
            }
        }
    }
}
