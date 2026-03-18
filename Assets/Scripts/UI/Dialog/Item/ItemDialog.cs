using Assets.Scripts.UI.Dialog;
using LayerLab.ArtMakerUnity;
using UnityEngine;
using UnityEngine.UI;


public class ItemDialog : DialogBase<bool>
{
    [Header("UI")]
    [SerializeField] private Button closeButton;

    public override void Setup(Player player = null)
    {
        closeButton.onClick.AddListener(OnClickClose);
    }

    protected override void OnClickClose()
    {
        Close(false);
    }
}
