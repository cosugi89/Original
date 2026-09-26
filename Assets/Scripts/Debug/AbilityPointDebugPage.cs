using System;
using System.Collections;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using UnityDebugSheet;
using UnityEngine;
using UnityEngine.UI;

public sealed class AbilityPointDebugPage : DefaultDebugPageBase
{
    private int _grantAmount = 1;

    protected override string Title { get; } = "Ability Point";

    public override IEnumerator Initialize()
    {
        AddInputField(
            "Grant Amount",
            value: _grantAmount.ToString(),
            placeholder: "1",
            contentType: InputField.ContentType.IntegerNumber,
            valueChanged: value => _grantAmount = int.TryParse(value, out var amount) ? amount : 0);
        AddButton("Grant Ability Point", clicked: GrantAbilityPoints);

        yield break;
    }

    private void GrantAbilityPoints()
    {
        var addedAmount = UserDataManager.EnsureInitialized().AddAbilityPoints(_grantAmount);
        if (addedAmount == 0)
        {
            Debug.LogWarning("[DebugSheet] 付与数には1以上を入力してください。");
            return;
        }

        var balance = UserDataManager.Instance.Inventory.GetItemCount(InventoryItemIds.AbilityPoint);
        Debug.Log($"[DebugSheet] Ability Pointを{addedAmount}付与しました。残高: {balance}");
    }
}
