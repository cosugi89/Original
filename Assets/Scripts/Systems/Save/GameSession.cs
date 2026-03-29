using System.ComponentModel;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.Save
{
    public sealed class GameSession : MonoBehaviour
    {
        private static GameSession _instance;

        [Description("現在ゲーム全体で共有している保存データ本体。")]
        public GameSaveData SaveData { get; private set; } = new();

        [Description("セッション初期化が完了しているかどうか。起動直後のガードに使う。")]
        public bool IsInitialized { get; private set; }

        [Description("保存対象データに未保存の変更があるかどうか。")]
        public bool IsDirty { get; private set; }

        [Description("現在実行中または遷移先として扱っているステージID。実行時専用で保存しなくてもよい。")]
        public string CurrentStageId { get; private set; } = "";

        public static GameSession Instance => EnsureInstance();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInstance();
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

        public void Initialize(GameSaveData saveData)
        {
            SaveData = saveData ?? new GameSaveData();
            IsInitialized = true;
            IsDirty = false;
        }

        public void ReplaceSaveData(GameSaveData saveData, bool markDirty = false)
        {
            SaveData = saveData ?? new GameSaveData();
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

        public void SetCurrentStageId(string stageId)
        {
            CurrentStageId = stageId ?? string.Empty;
        }

        private static GameSession EnsureInstance()
        {
            if (_instance != null)
                return _instance;

            var existing = FindFirstObjectByType<GameSession>();
            if (existing != null)
            {
                _instance = existing;
                DontDestroyOnLoad(existing.gameObject);
                return _instance;
            }

            var gameObject = new GameObject(nameof(GameSession));
            _instance = gameObject.AddComponent<GameSession>();
            DontDestroyOnLoad(gameObject);
            return _instance;
        }
    }
}
