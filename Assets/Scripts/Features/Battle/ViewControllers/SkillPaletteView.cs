using System;
using Assets.Scripts.Features.Battle.Runtime;
using UnityEngine;

namespace Assets.Scripts.Features.Battle.ViewControllers
{
    /// <summary>
    /// 戦闘中の Skill パレット全体と選択状態を管理する。
    /// </summary>
    public sealed class SkillPaletteView : MonoBehaviour
    {
        [SerializeField] private BattleSkillSlot[] skillSlots = Array.Empty<BattleSkillSlot>();

        private Action<int> _slotClicked;

        public int SlotCount => skillSlots != null ? skillSlots.Length : 0;

        public void BindSlotClickActions(Action<int> onSlotClicked)
        {
            UnbindSlotClickActions();
            _slotClicked = onSlotClicked;

            if (skillSlots == null)
            {
                return;
            }

            for (var i = 0; i < skillSlots.Length; i++)
            {
                var slot = skillSlots[i];
                var slotIndex = i;
                slot?.BindClickAction(() => _slotClicked?.Invoke(slotIndex));
            }
        }

        public void UnbindSlotClickActions()
        {
            if (skillSlots != null)
            {
                for (var i = 0; i < skillSlots.Length; i++)
                {
                    skillSlots[i]?.UnbindClickAction();
                }
            }

            _slotClicked = null;
        }

        public void ApplyPlayerSnapshot(BattlePlayerSnapshot playerSnapshot)
        {
            var runtimes = playerSnapshot?.SkillSlots ?? Array.Empty<SkillSlotRuntime>();
            if (skillSlots == null)
            {
                return;
            }

            for (var i = 0; i < skillSlots.Length; i++)
            {
                var slotView = skillSlots[i];
                if (slotView == null)
                {
                    continue;
                }

                slotView.SetRuntime(i < runtimes.Count ? runtimes[i] : null);
            }
        }

        public void RefreshSlots(int selectedSlotIndex, Color normalColor, Color selectedColor)
        {
            if (skillSlots == null)
            {
                return;
            }

            for (var i = 0; i < skillSlots.Length; i++)
            {
                var slotView = skillSlots[i];
                if (slotView == null)
                {
                    continue;
                }

                slotView.RefreshRuntime();
                slotView.SetSelectedVisual(
                    i == selectedSlotIndex,
                    normalColor,
                    selectedColor);
            }
        }
    }
}
