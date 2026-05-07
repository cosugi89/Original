using System.ComponentModel;
using System.Linq;
using Assets.Scripts.Core;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.GameData
{
    public class AvatarRenderService
    {
        public static AvatarRenderService Instance { get; private set; }

        [Description("現在の共有セッション。描画元となるアバター見た目の参照先。")]
        public GameSession Session { get; }

        [Description("共有アバターデータの参照と更新を担当するサービス。")]
        public AvatarService AvatarService { get; }

        [Description("所持装備の同期を担当するサービス。")]
        public InventoryService InventoryService { get; }

        [Description("dirty管理と永続化を担当する保存サービス。")]
        public GameSaveService SaveService { get; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInitialized();
        }

        public static AvatarRenderService EnsureInitialized()
        {
            if (Instance != null)
                return Instance;

            var saveService = GameSaveService.EnsureInitialized();
            var inventoryService = InventoryService.EnsureInitialized();
            var avatarService = AvatarService.EnsureInitialized();

            Instance = new AvatarRenderService(saveService, avatarService, inventoryService);
            return Instance;
        }

        public AvatarRenderService(
            GameSaveService saveService,
            AvatarService avatarService,
            InventoryService inventoryService)
        {
            SaveService = saveService;
            Session = saveService.Session;
            AvatarService = avatarService;
            InventoryService = inventoryService;
        }

        public bool SyncSessionFromRendererIfNeeded(PartsManager partsManager, bool saveAfterSync = false)
        {
            if (partsManager == null || HasStoredAppearance())
                return false;

            CaptureFrom(partsManager, saveAfterSync);
            return true;
        }

        public void ApplyTo(PartsManager partsManager)
        {
            if (partsManager == null)
                return;

            EnsurePartsManagerInitialized(partsManager);
            PartsManagerAvatarAdapter.ApplyAppearance(
                partsManager,
                AvatarService.GetCurrentAppearance(),
                InventoryService);
        }

        public AvatarAppearanceData CaptureFrom(PartsManager partsManager, bool saveAfterCapture = false)
        {
            if (partsManager == null)
                return AvatarService.GetCurrentAppearance();

            EnsurePartsManagerInitialized(partsManager);

            var appearance = PartsManagerAvatarAdapter.CaptureAppearance(partsManager, InventoryService);
            AvatarService.SetCurrentAppearance(appearance);
            SyncOwnedEquipments(appearance);

            if (saveAfterCapture)
                SaveService.SaveSession();

            return appearance;
        }

        private bool HasStoredAppearance()
        {
            var appearance = AvatarService.GetCurrentAppearance();
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

                if (!InventoryService.HasEquipment(state.EquipmentId))
                {
                    InventoryService.GrantEquipment(state.EquipmentId, 1, true);
                }
            }
        }

        private static void EnsurePartsManagerInitialized(PartsManager partsManager)
        {
            if (partsManager.ActiveIndices.Count == 0)
            {
                partsManager.Init();
            }
        }
    }
}
