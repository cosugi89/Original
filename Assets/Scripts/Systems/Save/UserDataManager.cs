using System.ComponentModel;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.Save
{
    /// <summary>
    /// シーンをまたいで共有する UserData の正本。
    /// GameInitializer が起動時にロードしたデータを保持する。
    /// </summary>
    public sealed class UserDataManager : MonoBehaviour
    {
        private static UserDataManager _instance;

        [Description("現在ゲーム全体で共有しているユーザーデータ本体。")]
        public UserData UserData { get; private set; } = new();

        public UserDataJsonStore UserDataStore { get; private set; }
        public InventoryManager Inventory { get; private set; }
        public AppearanceManager Appearance { get; private set; }
        public ProgressManager Progress { get; private set; }
        public DeckManager Decks { get; private set; }
        public AbilityProgressManager AbilityProgress { get; private set; }

        [Description("セッション初期化が完了しているかどうか。起動直後のガードに使う。")]
        public bool IsInitialized { get; private set; }

        [Description("保存対象データに未保存の変更があるかどうか。")]
        public bool IsDirty { get; private set; }

        [Description("現在実行中または遷移先として扱っているステージID。負数は未設定扱い。")]
        public int CurrentStageId { get; private set; } = -1;

        public static UserDataManager Instance => EnsureInstance();

        public static UserDataManager EnsureInitialized()
        {
            var manager = Instance;
            manager.InitializeGameData();
            return manager;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                SavePendingChanges();
        }

        private void OnApplicationQuit()
        {
            SavePendingChanges();
        }

        public void InitializeGameData()
        {
            if (IsInitialized)
                return;

            UserDataStore = new UserDataJsonStore(
                this,
                new UserDataJsonFile(),
                new DefaultUserDataFactory());
            UserDataStore.ReloadSession();

            Inventory = new InventoryManager(UserDataStore, MasterDataResourceLoader.LoadEquipmentData());
            Decks = new DeckManager(UserDataStore, Inventory);
            Appearance = new AppearanceManager(UserDataStore, Inventory, Decks);
            Progress = new ProgressManager(UserDataStore, MasterDataResourceLoader.LoadBattleStageData());

            if (IsDirty)
                UserDataStore.SaveSession();
        }

        internal void Initialize(UserData userData)
        {
            UserData = userData ?? new UserData();
            IsInitialized = true;
            IsDirty = false;
        }

        public void ReplaceUserData(UserData userData, bool markDirty = false)
        {
            UserData = userData ?? new UserData();
            IsInitialized = true;
            IsDirty = markDirty;
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        public void ClearDirty()
        {
            IsDirty = false;
        }

        public void SetCurrentStageId(int stageId)
        {
            CurrentStageId = stageId;
        }

        /// <summary>指定TreeのAbility進捗窓口を取得する。Battle/Status接続時も同じ導出結果を利用する。</summary>
        public AbilityProgressManager GetAbilityProgress(AbilityTreeMasterData tree)
        {
            if (AbilityProgress == null || AbilityProgress.Tree != tree)
            {
                AbilityProgress = new AbilityProgressManager(UserDataStore, tree);
            }

            return AbilityProgress;
        }

        private void SavePendingChanges()
        {
            if (!IsDirty || UserDataStore == null)
                return;

            UserDataStore.SaveSession();
        }

        private static UserDataManager EnsureInstance()
        {
            if (_instance != null)
                return _instance;

            var existing = FindFirstObjectByType<UserDataManager>();
            if (existing != null)
            {
                _instance = existing;
                DontDestroyOnLoad(existing.gameObject);
                return _instance;
            }

            var gameObject = new GameObject(nameof(UserDataManager));
            _instance = gameObject.AddComponent<UserDataManager>();
            DontDestroyOnLoad(gameObject);
            return _instance;
        }
    }
}
