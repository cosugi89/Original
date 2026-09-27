using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    public enum DungeonColorTheme
    {
        Blue = 0,
        Pink = 1,
        Green = 2,
        Gray = 3
    }

    public enum StageKind
    {
        Main,
        Event,
        Debug
    }

    public enum StageUnlockRequirementMode
    {
        All,
        Any
    }

    public enum MissionRewardType
    {
        Item,
        Equipment
    }

    [System.Serializable]
    public sealed class MissionRewardEntry
    {
        [SerializeField, Tooltip("報酬の種類です。Item は ItemStacks、Equipment は所持装備へ追加します。")]
        private MissionRewardType rewardType;

        [SerializeField, Tooltip("Item 報酬の ItemId。Equipment の場合は 0 のままにします。")]
        private int itemId;

        [SerializeField, Tooltip("Equipment 報酬の EquipmentId。Item の場合は 0 のままにします。")]
        private int equipmentId;

        [SerializeField, Min(1), Tooltip("付与数です。")]
        private int quantity = 1;

        public MissionRewardType RewardType => rewardType;
        public int ItemId => itemId;
        public int EquipmentId => equipmentId;
        public int Quantity => quantity;
    }

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
        [Tooltip("ダンジョン一覧カードのラベルに表示する画像。")]
        private Sprite dungeonLabelImage;

        [SerializeField]
        [Tooltip("ダンジョン一覧カードに使う配色テーマ。")]
        private DungeonColorTheme dungeonColorTheme;

        [SerializeField]
        [Tooltip("ステージ一覧の並び順。小さいほど前に表示する。")]
        private int sortOrder = 0;

        [SerializeField]
        [Tooltip("ステージの種別。ID の千の位は見分け用の規約で、実際の扱いはこの値で決める。")]
        private StageKind stageKind;

        [SerializeField]
        [Tooltip("クリア条件の判定方法。")]
        private StageUnlockRequirementMode unlockRequirementMode;

        [SerializeField]
        [Tooltip("解放に必要なクリア済み StageId。空なら最初から解放される。")]
        private List<int> requiredClearedStageIds = new();

        [SerializeField, Tooltip("Mission 初回クリア時に付与する報酬です。")]
        private List<MissionRewardEntry> clearRewards = new();

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

        /// <summary>ダンジョン一覧カードのラベル画像。</summary>
        public Sprite DungeonLabelImage => dungeonLabelImage;

        /// <summary>ダンジョン一覧カードの配色テーマ。</summary>
        public DungeonColorTheme DungeonColorTheme => dungeonColorTheme;

        /// <summary>ステージ一覧の並び順。</summary>
        public int SortOrder => sortOrder;

        /// <summary>ステージ種別。</summary>
        public StageKind StageKind => stageKind;

        /// <summary>クリア条件の判定方法。</summary>
        public StageUnlockRequirementMode UnlockRequirementMode => unlockRequirementMode;

        /// <summary>解放に必要なクリア済み StageId。</summary>
        public IReadOnlyList<int> RequiredClearedStageIds => requiredClearedStageIds ??= new List<int>();

        /// <summary>Mission 初回クリア時に付与する報酬。</summary>
        public IReadOnlyList<MissionRewardEntry> ClearRewards => clearRewards ??= new List<MissionRewardEntry>();

        /// <summary>このステージが参照する敵アセット一覧。</summary>
        public IReadOnlyList<StageBattleEnemyMasterData> EnemyRefs => enemyRefs ??= new List<StageBattleEnemyMasterData>();
    }
}
