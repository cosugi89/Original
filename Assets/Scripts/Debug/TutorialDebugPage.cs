using System.Collections;
using System.Linq;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Features.Tutorial;
using UnityDebugSheet;
using UnityEngine;

public sealed class TutorialDebugPage : DefaultDebugPageBase
{
    protected override string Title { get; } = "Tutorials";

    public override IEnumerator Initialize()
    {
        BuildItems();
        yield break;
    }

    private void BuildItems()
    {
        ClearItems();
        AddButton("Refresh", subText: "現在のセーブとSceneのチュートリアル状態を再取得します。", clicked: BuildItems);
        var manager = TutorialManager.Current;
        if (manager == null)
        {
            AddButton("Open Main or Battle", subText: "TutorialManagerがあるSceneでRefreshしてください。");
            return;
        }
        foreach (var master in manager.Tutorials.Where(item => item != null).OrderBy(item => item.TutorialId))
        {
            var tutorial = master;
            AddSwitch(manager.IsCompleted(tutorial.TutorialId), $"{tutorial.name} / Completed",
                $"ID: {tutorial.TutorialId} / Enabled: {tutorial.IsEnabled} / {tutorial.Repeat}。完了フラグはセーブへ即時保存します。",
                valueChanged: completed => TutorialManager.Current.SetCompletedForDebug(tutorial.TutorialId, completed));
            AddSwitch(TutorialManager.RepeatsForDebug(tutorial), $"{tutorial.name} / Repeat For Debug",
                "Editor / Development Buildで、完了・初回訪問済みでも契機ごとに再生。変更はセッション限定です。",
                valueChanged: repeat => TutorialManager.SetRepeatForDebug(tutorial.TutorialId, repeat));
            AddButton($"{tutorial.name} / Replay",
                subText: "現在画面で先頭から再生。発火条件・完了フラグ・Enabledを無視し、完了フラグは変更しません。対象UIのある画面で使ってください。",
                clicked: () => Replay(tutorial));
        }
    }

    private static void Replay(TutorialMasterData master)
    {
        var manager = TutorialManager.Current;
        if (manager == null || !manager.ReplayForDebug(master))
        {
            Debug.LogWarning("[DebugSheet] チュートリアルの対象画面が未表示、対象UIが未準備、または別の案内を実行中です。");
            return;
        }
        DebugSheet.Instance.Hide();
    }
}
