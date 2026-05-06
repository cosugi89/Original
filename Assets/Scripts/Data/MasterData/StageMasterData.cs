using System;
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
        [Tooltip("旧構造の敵一覧。敵参照へ移行するまでの互換 fallback。")]
        private List<EnemyMasterData> enemies = new();

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

        /// <summary>旧構造の敵一覧。敵参照が空のときだけ fallback に使う。</summary>
        public List<EnemyMasterData> LegacyEnemies => enemies;

        /// <summary>このステージが参照する敵アセット一覧。</summary>
        public IReadOnlyList<StageBattleEnemyMasterData> EnemyRefs => enemyRefs ??= new List<StageBattleEnemyMasterData>();

        /// <summary>
        /// ステージ上に登場する敵1体ぶんのマスタデータ。
        /// </summary>
        [Serializable]
        public class EnemyMasterData
        {
            [SerializeField]
            [Tooltip("敵の表示名。")]
            private string enemyName = string.Empty;

            [SerializeField]
            [Tooltip("敵の最大HP。")]
            private int hp = 100;

            /// <summary>敵の表示名。</summary>
            public string Name => enemyName;

            /// <summary>敵の最大HP。</summary>
            public int Hp => hp;
        }
    }
}
