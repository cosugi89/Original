using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// バトルデモで使用する1ステージぶんのマスタデータ。
    /// StageMasterDatabase に並べて保持する想定で、ScriptableObject ではなく Serializable クラスとして扱う。
    /// </summary>
    [Serializable]
    public class StageMasterData
    {
        [SerializeField]
        [Tooltip("ステージを一意に識別する数値ID。遷移状態などからのキーに使用する。")]
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
        [Tooltip("このステージに登場する敵一覧。")]
        private List<EnemyMasterData> enemies = new();

        /// <summary>ステージID。</summary>
        public int StageId => stageId;

        /// <summary>ステージ名。</summary>
        public string StageName => stageName;

        /// <summary>ステージ説明文。</summary>
        public string Description => description;

        /// <summary>戦闘背景スプライト。</summary>
        public Sprite BackgroundImage => backgroundImage;

        /// <summary>プレビュー画像スプライト。</summary>
        public Sprite PreviewImage => previewImage;

        /// <summary>敵一覧。</summary>
        public List<EnemyMasterData> Enemies => enemies;

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
