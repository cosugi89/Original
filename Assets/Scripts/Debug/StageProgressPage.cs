using System;
using System.Collections;
using Assets.Scripts.Systems.Save;
using UnityDebugSheet;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageProgressPage : DefaultDebugPageBase
{
    private int _clearThroughStageId = 1001;

    protected override string Title { get; } = "Progress";

    public override IEnumerator Initialize()
    {
        AddInputField(
            "Clear Through Stage ID",
            subText: "同じ千番台で指定 ID 以下のステージをクリア済みにします。",
            value: _clearThroughStageId.ToString(),
            placeholder: "1002",
            contentType: InputField.ContentType.IntegerNumber,
            valueChanged: value => _clearThroughStageId = int.TryParse(value, out var stageId) ? stageId : -1);
        AddButton("Clear", clicked: ClearStagesThrough);
        yield break;
    }

    private void ClearStagesThrough()
    {
        var updatedCount = UserDataManager.EnsureInitialized().Progress
            .ClearStagesThroughForDebug(_clearThroughStageId);
        if (updatedCount == 0)
        {
            Debug.LogWarning($"[DebugSheet] Stage ID {_clearThroughStageId} に一致する千番台のステージがありません。");
            return;
        }

        DebugStageSettings.RequestDungeonListRefresh();
        Debug.Log($"[DebugSheet] Stage ID {_clearThroughStageId} 以下をクリア済みにしました。対象数: {updatedCount}");
    }
}
