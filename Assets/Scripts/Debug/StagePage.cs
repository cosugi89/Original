using System.Collections;
using UnityDebugSheet;

public sealed class StagePage : DefaultDebugPageBase
{
    protected override string Title { get; } = "Stage";

    public override IEnumerator Initialize()
    {
        AddPageLinkButton<StageVisibilityPage>("Visibility");
        AddPageLinkButton<StageProgressPage>("Progress");
        yield break;
    }
}
