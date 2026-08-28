using System;
using Assets.Scripts.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillFrame : MonoBehaviour
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
        _presentationMode = SkillFramePresentationMode.Standard;
        SetState(SkillFrameState.Normal);
        SetIcon(_normalImage, icon);
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
        _presentationMode = SkillFramePresentationMode.Standard;
        SetState(SkillFrameState.Disabled);
        SetIcon(_disabledImage, icon);
    }

    public void ShowEmpty()
    {
        _presentationMode = SkillFramePresentationMode.Standard;
        SetState(SkillFrameState.Empty);
    }

    public void ShowLocked(string lockText)
    {
        _presentationMode = SkillFramePresentationMode.Standard;
        SetState(SkillFrameState.Locked);
        _lockText.text = lockText ?? string.Empty;
    }

    private void SetState(SkillFrameState state)
    {
        _normal.SetActive(state == SkillFrameState.Normal);
        _disabled.SetActive(state == SkillFrameState.Disabled);
        _empty.SetActive(state == SkillFrameState.Empty);
        _lock.SetActive(state == SkillFrameState.Locked);
        if (_presentationMode == SkillFramePresentationMode.Battle)
        {
            _normalFillImage.gameObject.SetActive(state == SkillFrameState.Normal);
        }
    }

    private static void SetIcon(Image image, Sprite icon)
    {
        image.sprite = icon;
        image.enabled = icon != null;
        image.preserveAspect = true;
    }
}
