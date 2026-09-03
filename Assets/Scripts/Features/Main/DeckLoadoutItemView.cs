using System;
using Assets.Scripts.Systems.Save.Models;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    /// <summary>
    /// デッキ一覧内の1行を表示し、選択入力だけをDialogへ返すView。
    /// </summary>
    public sealed class DeckLoadoutItemView : MonoBehaviour
    {
        [SerializeField] private FeedbackButton itembutton; // デッキの選択ボタン
        [SerializeField] private GameObject selectedFrame; // 選択中のデッキに表示する枠
        [SerializeField] private RawImage avatarPreviewImage; // rawimageでデッキごとに装備しているアバターを表示させたい
        [SerializeField] private TextMeshProUGUI attackValuetext; // 装備を着たときの攻撃力の合計を表示する、変数名は適当なものに変更
        [SerializeField] private TextMeshProUGUI defenseValuetext; // 装備を着たときの防御力の合計を表示する、変数名は適当なものに変更
        [SerializeField] private PartsSkillItem skill1;
        [SerializeField] private PartsSkillItem skill2;
        [SerializeField] private PartsSkillItem skill3;
        [SerializeField] private PartsSkillItem skill4;

        public void Setup(DeckData deck, bool isSelected, Action<int> onClick)
        {
        }
    }
}
