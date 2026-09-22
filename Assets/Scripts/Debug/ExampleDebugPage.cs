using System.Collections;
using Assets.Scripts.Systems.Save;
using UnityDebugSheet;
using UnityEngine;

public sealed class ExampleDebugPage : DefaultDebugPageBase
{
    private const string TraitTreeId = "trait";

    protected override string Title { get; } = "Ability";

    public override IEnumerator Initialize()
    {
        AddButton("Clear Trait Tree Save Data", clicked: ClearTraitTreeNodeLevels);

        yield break;
    }

    private static void ClearTraitTreeNodeLevels()
    {
        var removedCount = UserDataManager.EnsureInitialized().ClearAbilityTreeNodeLevels(TraitTreeId);
        Debug.Log($"[DebugSheet] Trait Treeの保存Levelをクリアしました。削除数: {removedCount}");
    }
}
