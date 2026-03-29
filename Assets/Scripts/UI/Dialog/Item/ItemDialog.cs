using Assets.Scripts.UI.Dialog;
using UnityEngine;
using UnityEngine.UI;

public readonly struct ItemDialogRequest
{
}

public readonly struct ItemDialogResult
{
    private ItemDialogResult(bool isConfirmed)
    {
        IsConfirmed = isConfirmed;
    }

    public bool IsConfirmed { get; }

    public static ItemDialogResult Confirmed => new(true);
    public static ItemDialogResult Cancelled => new(false);
}

public class ItemDialog : DialogBase<ItemDialogResult>, IDialogRequestHandler<ItemDialogRequest>
{
    [Header("UI")]
    [SerializeField] private Button closeButton;

    public void Setup(ItemDialogRequest request, DialogContext context)
    {
        closeButton.onClick.AddListener(OnClickClose);
    }

    protected override void OnClickClose()
    {
        Close(ItemDialogResult.Cancelled);
    }
}
