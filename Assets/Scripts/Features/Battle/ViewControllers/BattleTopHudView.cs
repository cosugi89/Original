using Assets.Scripts.Features.Battle.Runtime;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 戦闘全体に 1 つだけ存在する固定情報を表示する。
    /// </summary>
    public sealed class BattleTopHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text turnNumberText;
        [SerializeField] private TMP_Text waveNumberText;
        [SerializeField] private GameObject menuButtonObject;
        [SerializeField] private GameObject pauseButtonObject;

        public void ApplySession(BattleSessionState session)
        {
            if (session == null)
            {
                return;
            }

            SetText(turnNumberText, $"Turn {session.TurnNumber}");
            SetText(waveNumberText, $"Wave {session.WaveNumber}");
        }

        public void SetMenuVisible(bool visible)
        {
            if (menuButtonObject != null)
            {
                menuButtonObject.SetActive(visible);
            }
        }

        public void SetPauseVisible(bool visible)
        {
            if (pauseButtonObject != null)
            {
                pauseButtonObject.SetActive(visible);
            }
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
