using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// ターンごとの行動ログ一覧を表示する。
    /// </summary>
    public sealed class BattleActionLogView : MonoBehaviour
    {
        [SerializeField] private GameObject rootObject;
        [SerializeField] private TMP_Text logText;

        public void ResetView()
        {
            if (rootObject != null)
            {
                rootObject.SetActive(false);
            }

            if (logText != null)
            {
                logText.text = string.Empty;
            }
        }

        public void SetEntries(IReadOnlyList<string> entries)
        {
            var hasEntries = entries != null && entries.Count > 0;
            if (rootObject != null)
            {
                rootObject.SetActive(hasEntries);
            }

            if (logText != null)
            {
                logText.text = hasEntries ? string.Join("\n", entries) : string.Empty;
            }
        }
    }
}
