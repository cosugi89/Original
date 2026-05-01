using System.ComponentModel;
using Assets.Scripts.Systems.Save.Models;
using UnityEngine;

namespace Assets.Scripts.Systems.Save
{
    public sealed class GameSession : MonoBehaviour
    {
        private static GameSession _instance;

        [Description("現在ゲーム全体で共有しているユーザーデータ本体。")]
        public UserData UserData { get; private set; } = new();

        [System.Obsolete("Use UserData instead.")]
        public UserData SaveData => UserData;

        [Description("セッション初期化が完了しているかどうか。起動直後のガードに使う。")]
        public bool IsInitialized { get; private set; }

        [Description("保存対象データに未保存の変更があるかどうか。")]
        public bool IsDirty { get; private set; }

        [Description("現在実行中または遷移先として扱っているステージID。負数は未設定扱い。")]
        public int CurrentStageId { get; private set; } = -1;

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

        public void Initialize(UserData userData)
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

        [System.Obsolete("Use ReplaceUserData instead.")]
        public void ReplaceSaveData(UserData userData, bool markDirty = false)
        {
            ReplaceUserData(userData, markDirty);
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
