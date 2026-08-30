using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.Const;
using Assets.Scripts.Data.DTO;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.UI.Components;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public readonly struct DialogEquipmentDetailRequest
    {
        public DialogEquipmentDetailRequest(AvatarPartOptionViewData option)
        {
            Option = option;
        }

        public AvatarPartOptionViewData Option { get; }
    }

    public readonly struct DialogEquipmentDetailResult
    {
        private DialogEquipmentDetailResult(bool isEquipRequested)
        {
            IsEquipRequested = isEquipRequested;
        }

        public bool IsEquipRequested { get; }

        public static DialogEquipmentDetailResult EquipRequested => new(true);
        public static DialogEquipmentDetailResult Cancelled => new(false);
    }

    [DialogAddress("Dialog/EquipmentDetail")]
    public sealed class DialogEquipmentDetail :
        DialogBase<DialogEquipmentDetailResult>,
        IDialogRequestHandler<DialogEquipmentDetailRequest>
    {
        [SerializeField] private PartsEquipmentItem equipmentItem;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI detailText;
        [SerializeField] private Transform tagsContent;
        [SerializeField] private StatusEquipmentDetailTagView tagTemplate;
        [SerializeField] private Transform skillsContent;
        [SerializeField] private StatusEquipmentDetailSkillView skillTemplate;
        [SerializeField] private Transform buffsContent;
        [SerializeField] private TextMeshProUGUI buffTemplate;
        [SerializeField] private RectTransform scrollContent;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private Image rarityBackgroundImage;
        [SerializeField] private Image rarityInnerColorImage;
        [SerializeField] private Image rarityDecoImage;
        [SerializeField] private GameButton equipButton;
        [SerializeField] private GameButton closeButton;

        private AvatarPartOptionViewData _option;
        private readonly List<StatusEquipmentDetailTagView> _tagViews = new();
        private readonly List<StatusEquipmentDetailSkillView> _skillViews = new();
        private readonly List<TextMeshProUGUI> _buffViews = new();
        private bool _templatesInitialized;

        public void Setup(DialogEquipmentDetailRequest request, DialogContext context)
        {
            _option = request.Option;
            Refresh();

            closeButton.onClick.RemoveListener(OnClickClose);
            closeButton.onClick.AddListener(OnClickClose);

            equipButton.onClick.RemoveListener(OnClickEquip);
            equipButton.onClick.AddListener(OnClickEquip);
        }

        protected override void OnClickClose()
        {
            Close(DialogEquipmentDetailResult.Cancelled);
        }

        private void OnClickEquip()
        {
            if (_option == null || !_option.CanSelect || _option.IsSelected)
            {
                return;
            }

            Close(DialogEquipmentDetailResult.EquipRequested);
        }

        private void Refresh()
        {
            if (_option == null)
            {
                return;
            }

            titleText.text = BuildDisplayName(_option);
            detailText.text = BuildDetailText(_option);
            ShowRarity(_option.Rarity);
            RefreshTags();
            RefreshSkills();
            RefreshBuffs();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollContent);

            equipmentItem.ShowNormal();
            equipmentItem.Setup(
                _option.Category,
                _option.Icon,
                BuildDisplayName(_option),
                specialBorderVisible: false,
                new UIColors.PlumColorSet());
            equipmentItem.SetChecked(_option.IsSelected);

            var canEquip = _option.CanSelect && !_option.IsSelected;
            equipButton.SetInteractable(canEquip);
            equipButton.SetLabel(!_option.CanSelect ? "Locked" : (_option.IsSelected ? "Equipped" : "Equip"));
        }

        private static string BuildDisplayName(AvatarPartOptionViewData option)
        {
            if (!string.IsNullOrWhiteSpace(option.DisplayName))
            {
                return option.DisplayName;
            }

            return option.EquipmentId > 0 ? $"Equipment {option.EquipmentId}" : option.PartType.ToString();
        }

        private static string BuildDetailText(AvatarPartOptionViewData option)
        {
            return option.Description ?? string.Empty;
        }

        private void ShowRarity(EquipmentRarity rarity)
        {
            rarityText.text = rarity.ToString();
            var colorSet = GetRarityColorSet(rarity);
            rarityText.color = colorSet.InnerColor1;
            rarityBackgroundImage.color = colorSet.Background;
            rarityInnerColorImage.color = colorSet.InnerColor1;
            rarityDecoImage.color = colorSet.Glow;
        }

        private void RefreshTags()
        {
            InitializeTemplates();
            var tags = new List<string> { _option.PartType.ToString() };
            if (_option.CategoryTags != null)
            {
                tags.AddRange(_option.CategoryTags.Where(tag => !string.IsNullOrWhiteSpace(tag)));
            }

            EnsureViewCount(_tagViews, tags.Count, () => Instantiate(tagTemplate, tagsContent));
            for (var i = 0; i < _tagViews.Count; i++)
            {
                var active = i < tags.Count;
                _tagViews[i].gameObject.SetActive(active);
                if (active)
                {
                    _tagViews[i].Setup(tags[i]);
                }
            }
        }

        private void RefreshSkills()
        {
            InitializeTemplates();
            var skills = _option.AssignableSkills ?? Array.Empty<BattleSkillData>();
            EnsureViewCount(_skillViews, skills.Count, () => Instantiate(skillTemplate, skillsContent));
            for (var i = 0; i < _skillViews.Count; i++)
            {
                var active = i < skills.Count;
                _skillViews[i].gameObject.SetActive(active);
                if (active)
                {
                    _skillViews[i].Setup(skills[i]);
                }
            }
        }

        private void RefreshBuffs()
        {
            InitializeTemplates();
            var modifiers = _option.BattleStatModifiers ?? Array.Empty<BattleStatModifierData>();
            EnsureViewCount(_buffViews, modifiers.Count, () => Instantiate(buffTemplate, buffsContent));
            for (var i = 0; i < _buffViews.Count; i++)
            {
                var active = i < modifiers.Count;
                _buffViews[i].gameObject.SetActive(active);
                if (active)
                {
                    _buffViews[i].text = BuildBuffText(modifiers[i]);
                }
            }
        }

        private void InitializeTemplates()
        {
            if (_templatesInitialized)
            {
                return;
            }

            tagTemplate.gameObject.SetActive(false);
            skillTemplate.gameObject.SetActive(false);
            buffTemplate.gameObject.SetActive(false);
            _templatesInitialized = true;
        }

        private static void EnsureViewCount<T>(List<T> views, int requiredCount, Func<T> createView)
            where T : Component
        {
            while (views.Count < requiredCount)
            {
                views.Add(createView());
            }
        }

        private static string BuildBuffText(BattleStatModifierData modifier)
        {
            var target = modifier.TargetType == BattleStatModifierTargetType.NamedValue
                ? modifier.NamedValueKey
                : modifier.ParameterType.ToString();
            var value = modifier.OperationType switch
            {
                BattleStatModifierOperationType.Add => $"{modifier.Value:+0.##;-0.##;0}",
                BattleStatModifierOperationType.Multiply => $"x{modifier.Value:0.##}",
                _ => $"{modifier.Value:0.##}"
            };
            return $"{target} {value}";
        }

        private static UIColors.IColorSet GetRarityColorSet(EquipmentRarity rarity)
        {
            return rarity switch
            {
                EquipmentRarity.Uncommon => new UIColors.GreenColorSet(),
                EquipmentRarity.Rare => new UIColors.BlueColorSet(),
                EquipmentRarity.Epic => new UIColors.PlumColorSet(),
                EquipmentRarity.Legendary => new UIColors.YellowColorSet(),
                _ => new UIColors.GrayColorSet()
            };
        }
    }

}
