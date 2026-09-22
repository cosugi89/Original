using System.Collections;
using UnityDebugSheet;

public sealed class PlayerDataDebugPage : DefaultDebugPageBase
{
    protected override string Title { get; } = "Player Data";

    public override IEnumerator Initialize()
    {
        AddPageLinkButton<AbilityPointDebugPage>("Ability Point");

        yield break;
    }
}
