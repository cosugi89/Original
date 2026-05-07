using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// 1ステージぶんのマスタデータ。
    /// StageMasterDatabase から ID 参照される ScriptableObject として扱う。
    /// </summary>
    [CreateAssetMenu(menuName = "Original/Master Data/Stage", fileName = "StageMasterData")]
    public class StageMasterData : ScriptableObject
    {
        [SerializeField]
        [Tooltip("ステージを一意に識別する数値ID。保存・遷移・読込のキーに使う。")]
        private int stageId;

        [SerializeField]
        [Tooltip("画面表示用のステージ名。")]
        private string stageName = string.Empty;

        [SerializeField]
        [TextArea]
        [Tooltip("ステージ選択画面などで補足表示に使う説明文。")]
        private string description = string.Empty;

        [SerializeField]
        [Tooltip("戦闘背景として使用するスプライト。")]
        private Sprite backgroundImage;

        [SerializeField]
        [Tooltip("ステージ選択画面で表示するプレビュー画像。")]
        private Sprite previewImage;

        [SerializeField]
        [Tooltip("ステージ一覧の並び順。小さいほど前に表示する。")]
        private int sortOrder = 0;

        [SerializeField]
        [Tooltip("新規セーブ時に最初から解放されているステージかどうか。")]
        private bool isInitiallyUnlocked = true;

        [SerializeField]
        [Tooltip("このステージが参照する敵アセット一覧。先頭が現在の主対象。")]
        private List<StageBattleEnemyMasterData> enemyRefs = new();

        /// <summary>保存・遷移で使うステージID。</summary>
        public int StageId => stageId;

        /// <summary>ステージ名。</summary>
        public string StageName => stageName;

        /// <summary>ステージ説明文。</summary>
        public string Description => description;

        /// <summary>戦闘背景スプライト。</summary>
        public Sprite BackgroundImage => backgroundImage;

        /// <summary>プレビュー画像スプライト。</summary>
        public Sprite PreviewImage => previewImage;

        /// <summary>ステージ一覧の並び順。</summary>
        public int SortOrder => sortOrder;

        /// <summary>初期解放ステージかどうか。</summary>
        public bool IsInitiallyUnlocked => isInitiallyUnlocked;

        /// <summary>このステージが参照する敵アセット一覧。</summary>
        public IReadOnlyList<StageBattleEnemyMasterData> EnemyRefs => enemyRefs ??= new List<StageBattleEnemyMasterData>();
    }
}
