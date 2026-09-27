using System;
using Assets.Scripts.Core.Const;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public readonly struct CommonConfirmWithIconDialogRequest
    {
        public CommonConfirmWithIconDialogRequest(
            string title,
            int quantity,
            string description,
            Sprite icon,
            bool canConfirm)
        {
            Title = title ?? string.Empty;
            Quantity = quantity;
            Description = description ?? string.Empty;
            Icon = icon;
            CanConfirm = canConfirm;
        }

        public string Title { get; }
        public int Quantity { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public bool CanConfirm { get; }
    }

    public readonly struct CommonConfirmWithIconDialogResult
    {
        private CommonConfirmWithIconDialogResult(bool isConfirmed) => IsConfirmed = isConfirmed;

        public bool IsConfirmed { get; }

        public static CommonConfirmWithIconDialogResult Confirmed => new(true);
        public static CommonConfirmWithIconDialogResult Cancelled => new(false);
    }

    [DialogAddress("Dialog/Common/ConfilmWithIcon")]
    public sealed class DialogCommonConfirmWithIcon :
        DialogBase<CommonConfirmWithIconDialogResult>,
        IDialogRequestHandler<CommonConfirmWithIconDialogRequest>
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI numText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;

        private bool _canConfirm;

        public void Setup(CommonConfirmWithIconDialogRequest request, DialogContext context)
        {
            EnsurePrefabReferences();
            _canConfirm = request.CanConfirm;

            titleText.SetText(TextLocalization.Get(request.Title));
            numText.SetText($"x{request.Quantity}");
            descriptionText.SetText(TextLocalization.Get(request.Description));
            iconImage.sprite = request.Icon;
            iconImage.enabled = request.Icon != null;
            confirmButton.interactable = _canConfirm;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnClickConfirm);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClickClose);
        }

        protected override void OnClickClose() => Close(CommonConfirmWithIconDialogResult.Cancelled);

        private void OnClickConfirm()
        {
            if (_canConfirm)
            {
                Close(CommonConfirmWithIconDialogResult.Confirmed);
            }
        }

        private void EnsurePrefabReferences()
        {
            if (titleText == null || numText == null || descriptionText == null || iconImage == null ||
                confirmButton == null || closeButton == null)
            {
                throw new InvalidOperationException($"{nameof(DialogCommonConfirmWithIcon)} has missing serialized prefab references.");
            }
        }
    }
}
