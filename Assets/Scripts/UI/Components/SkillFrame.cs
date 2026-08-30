using System;
using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillFrame : MonoBehaviour, IItemStateView
{
    private enum SkillFrameState
    {
        Normal,
        Disabled,
        Empty,
        Locked
    }

    private enum SkillFramePresentationMode
    {
        Standard,
        Battle
    }

    [SerializeField] private GameObject _normal;
    [SerializeField] private GameObject _disabled;
    [SerializeField] private GameObject _empty;
    [SerializeField] private GameObject _lock;
    [SerializeField] private Image _normalImage;
    [SerializeField] private Image _normalFillImage;
    [SerializeField] private Image _disabledImage;
    [SerializeField] private TMP_Text _lockText;
    [SerializeField] private FeedbackButton _button;

    private SkillFramePresentationMode _presentationMode;

    public void ShowNormal(Sprite icon)
    {
        ShowNormal();
        SetIcon(_normalImage, icon);
    }

    public void ShowNormal()
    {
        _presentationMode = SkillFramePresentationMode.Standard;
        SetState(SkillFrameState.Normal);
    }

    public void ShowBattleNormal(Sprite icon)
    {
        _presentationMode = SkillFramePresentationMode.Battle;
        SetState(SkillFrameState.Normal);
        SetIcon(_normalImage, icon);
        SetBattleChargeProgress(0f);
    }

    public void SetBattleChargeProgress(float chargeProgress01)
    {
        _normalFillImage.fillAmount = 1f - Mathf.Clamp01(chargeProgress01);
    }

    public void SetFrameAction(Action action)
    {
        _button.SetAction(action);
    }

    public void ShowDisabled(Sprite icon)
    {
        ShowDisabled();
        SetIcon(_disabledImage, icon);
    }

    public void ShowDisabled()
    {
        _presentationMode = SkillFramePresentationMode.Standard;
        SetState(SkillFrameState.Disabled);
    }

    public void ShowEmpty()
    {
        _presentationMode = SkillFramePresentationMode.Standard;
        SetState(SkillFrameState.Empty);
    }

    public void ShowLocked(string lockText)
    {
        ShowLocked();
        _lockText.text = lockText ?? string.Empty;
    }

    public void ShowLocked()
    {
        _presentationMode = SkillFramePresentationMode.Standard;
        SetState(SkillFrameState.Locked);
    }

    private void SetState(SkillFrameState state)
    {
        _normal.SetActive(state == SkillFrameState.Normal);
        _disabled.SetActive(state == SkillFrameState.Disabled);
        _empty.SetActive(state == SkillFrameState.Empty);
        _lock.SetActive(state == SkillFrameState.Locked);
        if (_normalFillImage != null)
        {
            _normalFillImage.gameObject.SetActive(
                _presentationMode == SkillFramePresentationMode.Battle &&
                state == SkillFrameState.Normal);
        }
    }

    private static void SetIcon(Image image, Sprite icon)
    {
        image.sprite = icon;
        image.enabled = icon != null;
        image.preserveAspect = true;
    }
}
