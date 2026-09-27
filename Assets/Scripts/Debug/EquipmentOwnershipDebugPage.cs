using System.Collections;
using Assets.Scripts.Systems.Save;
using UnityDebugSheet;
using UnityEngine;

public sealed class EquipmentOwnershipDebugPage : DefaultDebugPageBase
{
    protected override string Title { get; } = "Equipment";

    public override IEnumerator Initialize()
    {
        BuildItems();
        yield break;
    }

    private void BuildItems()
    {
        ClearItems();
        var userDataManager = UserDataManager.EnsureInitialized();
        var inventory = userDataManager.Inventory;

        AddButton(
            "Reset Owned Equipment",
            subText: "現在セーブの所持装備をすべて削除します。初期所持装備は次回初期化時に再付与されます。",
            clicked: ResetEquipmentOwnership);

        foreach (var equipment in inventory.GetAllEquipmentDefinitions())
        {
            var equipmentId = equipment.EquipmentId;
            AddSwitch(
                inventory.HasEquipment(equipmentId),
                equipment.DisplayName,
                $"ID: {equipmentId} / {equipment.PartType} / Default: {equipment.IsDefaultOwned}",
                valueChanged: isOwned => SetEquipmentOwned(equipmentId, isOwned));
        }
    }

    private void ResetEquipmentOwnership()
    {
        var userDataManager = UserDataManager.EnsureInitialized();
        var removedCount = userDataManager.Inventory.ResetEquipmentOwnership();
        userDataManager.UserDataStore.SaveSession();
        BuildItems();
        Debug.Log($"[DebugSheet] Reset owned equipment. Removed count: {removedCount}");
    }

    private static void SetEquipmentOwned(int equipmentId, bool isOwned)
    {
        var userDataManager = UserDataManager.EnsureInitialized();
        if (!userDataManager.Inventory.SetEquipmentOwned(equipmentId, isOwned))
        {
            return;
        }

        userDataManager.UserDataStore.SaveSession();
        Debug.Log($"[DebugSheet] Equipment ownership changed: EquipmentId={equipmentId}, IsOwned={isOwned}");
    }
}
