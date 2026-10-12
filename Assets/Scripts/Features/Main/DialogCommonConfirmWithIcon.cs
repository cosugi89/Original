using System;
using Assets.Scripts.Core.Const;
using Assets.Scripts.UI.Dialog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Features.Main
{
    public readonly struct RequestDialogCommonConfirmWithIcon
    {
        public RequestDialogCommonConfirmWithIcon(
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

    public readonly struct ResultDialogCommonConfirmWithIcon
    {
        private ResultDialogCommonConfirmWithIcon(bool isConfirmed) => IsConfirmed = isConfirmed;

        public bool IsConfirmed { get; }

        public static ResultDialogCommonConfirmWithIcon Confirmed => new(true);
        public static ResultDialogCommonConfirmWithIcon Cancelled => new(false);
    }

    [DialogAddress("Dialog/Common/ConfilmWithIcon")]
    public sealed class DialogCommonConfirmWithIcon :
        DialogBase<ResultDialogCommonConfirmWithIcon>,
        IDialogRequestHandler<RequestDialogCommonConfirmWithIcon>
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI numText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;

        private bool _canConfirm;

        public void Setup(RequestDialogCommonConfirmWithIcon request, DialogContext context)
        {
            EnsurePrefabReferences();
            _canConfirm = request.CanConfirm;

            titleText.SetText(TextLocalization.Get(request.Title));
            numText.gameObject.SetActive(request.Quantity >= 0);
            if (request.Quantity >= 0)
            {
                numText.SetText($"x{request.Quantity}");
            }
            descriptionText.SetText(TextLocalization.Get(request.Description));
            iconImage.sprite = request.Icon;
            iconImage.enabled = request.Icon != null;
            confirmButton.interactable = _canConfirm;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnClickConfirm);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClickClose);
        }

        protected override void OnClickClose() => Close(ResultDialogCommonConfirmWithIcon.Cancelled);

        private void OnClickConfirm()
        {
            if (_canConfirm)
            {
                Close(ResultDialogCommonConfirmWithIcon.Confirmed);
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
