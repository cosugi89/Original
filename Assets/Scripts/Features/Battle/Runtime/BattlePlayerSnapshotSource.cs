using Assets.Scripts.Data.DTO;
using Assets.Scripts.Systems.Save.Models;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// スナップショット生成元となるプロフィール・装備・スキル情報を束ねる。
    /// </summary>
    public sealed class BattlePlayerSnapshotSource
    {
        public BattlePlayerSnapshotSource(
            UserProfileData profile,
            BattleEquipmentDerivedData equipmentDerivedData)
        {
            Profile = profile ?? new UserProfileDemoData();
            EquipmentDerivedData = equipmentDerivedData ?? new BattleEquipmentDerivedData();
        }

        public UserProfileData Profile { get; }

        public BattleEquipmentDerivedData EquipmentDerivedData { get; }
    }
}

