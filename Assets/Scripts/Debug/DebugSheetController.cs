using UnityDebugSheet;
using UnityEngine;

public sealed class DebugSheetController : MonoBehaviour
{
    private void Start()
    {
        var rootPage = DebugSheet.Instance.GetOrCreateInitialPage();
        rootPage.AddPageLinkButton<ExampleDebugPage>("Ability");
        rootPage.AddPageLinkButton<PlayerDataDebugPage>("Player Data");
    }
}
