using System.Collections;
using UnityDebugSheet;
using UnityEngine;

public sealed class StageVisibilityPage : DefaultDebugPageBase
{
    protected override string Title { get; } = "Visibility";

    public override IEnumerator Initialize()
    {
        AddSwitch(
            DebugStageSettings.ShowDebugStages,
            "Show Debug Stages",
            valueChanged: SetDebugStageVisibility);
        yield break;
    }

    private static void SetDebugStageVisibility(bool isVisible)
    {
        DebugStageSettings.SetDebugStageVisibility(isVisible);
        Debug.Log($"[DebugSheet] Debug Stage visibility: {DebugStageSettings.ShowDebugStages}");
    }
}
