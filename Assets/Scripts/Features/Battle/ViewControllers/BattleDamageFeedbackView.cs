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
            rootObject.SetActive(false);
            enemyDamageText.text = string.Empty;
            enemyDamageAttributeText.text = string.Empty;
            playerDamageText.text = string.Empty;
            playerDamageAttributeText.text = string.Empty;
        }

        public void Apply(BattleResolvedActionSummary summary)
        {
            var hasEnemyDamage = summary.EnemyDamageTaken > 0 ||
                                 !string.IsNullOrWhiteSpace(summary.EnemyDamageAttributeSummary);
            var hasPlayerDamage = summary.PlayerDamageTaken > 0 ||
                                  !string.IsNullOrWhiteSpace(summary.PlayerDamageAttributeSummary);

            rootObject.SetActive(hasEnemyDamage || hasPlayerDamage);
            enemyDamageText.text = hasEnemyDamage ? $"-{summary.EnemyDamageTaken}" : string.Empty;
            enemyDamageAttributeText.text = summary.EnemyDamageAttributeSummary;
            playerDamageText.text = hasPlayerDamage ? $"-{summary.PlayerDamageTaken}" : string.Empty;
            playerDamageAttributeText.text = summary.PlayerDamageAttributeSummary;
        }
    }
}
