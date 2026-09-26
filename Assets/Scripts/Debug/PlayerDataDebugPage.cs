using System.Collections;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using UnityDebugSheet;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerDataDebugPage : DefaultDebugPageBase
{
    private int _gold;
    private int _abilityPoint;

    protected override string Title { get; } = "Player Data";

    public override IEnumerator Initialize()
    {
        var inventory = UserDataManager.EnsureInitialized().Inventory;
        _gold = inventory.GetItemCount(InventoryItemIds.Gold);
        _abilityPoint = inventory.GetItemCount(InventoryItemIds.AbilityPoint);

        AddInputField("Gold", value: _gold.ToString(), placeholder: "0",
            contentType: InputField.ContentType.IntegerNumber,
            valueChanged: value => _gold = ParseQuantity(value));
        AddInputField("Ability Point", value: _abilityPoint.ToString(), placeholder: "0",
            contentType: InputField.ContentType.IntegerNumber,
            valueChanged: value => _abilityPoint = ParseQuantity(value));
        AddButton("Apply Item Counts", clicked: ApplyItemCounts);

        yield break;
    }

    private void ApplyItemCounts()
    {
        var manager = UserDataManager.EnsureInitialized();
        var inventory = manager.Inventory;
        var changed = false;
        changed |= inventory.SetItemCount(InventoryItemIds.Gold, _gold);
        changed |= inventory.SetItemCount(InventoryItemIds.AbilityPoint, _abilityPoint);

        if (!changed)
        {
            Debug.Log("[DebugSheet] ItemStacksの所持数は変更されていません。");
            return;
        }

        manager.AbilityProgress?.Recalculate();
        manager.UserDataStore.SaveSession();
        Debug.Log("[DebugSheet] ItemStacksの所持数を更新しました。");
    }

    private static int ParseQuantity(string value)
    {
        return int.TryParse(value, out var quantity) ? Mathf.Max(0, quantity) : 0;
    }
}
