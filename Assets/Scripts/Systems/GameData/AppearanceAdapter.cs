using System.ComponentModel;
using System.Linq;
using Assets.Scripts.Core;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    public class AppearanceAdapter
    {
        public static AppearanceAdapter Instance { get; private set; }

        [Description("現在の共有セッション。描画元となるアバター見た目の参照先。")]
        public UserDataManager Session { get; }

        [Description("共有アバターデータの参照と更新を担当するサービス。")]
        public AppearanceManager AppearanceManager { get; }

        [Description("所持装備の同期を担当するサービス。")]
        public InventoryManager InventoryManager { get; }

        [Description("dirty管理と永続化を担当する保存サービス。")]
        public UserDataJsonStore UserDataStore { get; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        public static AppearanceAdapter EnsureInitialized()
        {
            if (Instance != null)
                return Instance;

            var userDataManager = UserDataManager.EnsureInitialized();

            Instance = new AppearanceAdapter(
                userDataManager.UserDataStore,
                userDataManager.Appearance,
                userDataManager.Inventory);
            return Instance;
        }

        public AppearanceAdapter(
            UserDataJsonStore userDataStore,
            AppearanceManager appearanceManager,
            InventoryManager inventoryManager)
        {
            UserDataStore = userDataStore;
            Session = userDataStore.Session;
            AppearanceManager = appearanceManager;
            InventoryManager = inventoryManager;
        }

        public bool SyncSessionFromRendererIfNeeded(AppearanceController partsManager, bool saveAfterSync = false)
        {
            if (partsManager == null || HasStoredAppearance())
                return false;

            CaptureFrom(partsManager, saveAfterSync);
            return true;
        }

        public void ApplyTo(AppearanceController partsManager)
        {
            if (partsManager == null)
                return;

            EnsurePartsManagerInitialized(partsManager);
            PartsManagerAvatarAdapter.ApplyDeckAppearance(
                partsManager,
                AppearanceManager.GetCurrentAppearance(),
                InventoryManager);
        }

        public AvatarAppearanceData CaptureFrom(AppearanceController partsManager, bool saveAfterCapture = false)
        {
            if (partsManager == null)
                return AppearanceManager.GetCurrentAppearance();

            EnsurePartsManagerInitialized(partsManager);

            var appearance = PartsManagerAvatarAdapter.CaptureAppearance(partsManager, InventoryManager);
            AppearanceManager.SetAppearanceWithDeckEquipment(appearance);
            SyncOwnedEquipments(appearance);

            if (saveAfterCapture)
                UserDataStore.SaveSession();

            return appearance;
        }

        private bool HasStoredAppearance()
        {
            var appearance = AppearanceManager.GetCurrentAppearance();
            return appearance.Parts != null && appearance.Parts.Any(state => state != null);
        }

        private void SyncOwnedEquipments(AvatarAppearanceData appearance)
        {
            if (appearance?.Parts == null)
                return;

            foreach (var state in appearance.Parts)
            {
                if (state == null || state.EquipmentId <= 0)
                    continue;

                if (!InventoryManager.HasEquipment(state.EquipmentId))
                {
                    InventoryManager.GrantEquipment(state.EquipmentId, 1, true);
                }
            }
        }

        private static void EnsurePartsManagerInitialized(AppearanceController partsManager)
        {
            if (partsManager.ActiveIndices.Count == 0)
            {
                partsManager.Init();
            }
        }
    }
}
