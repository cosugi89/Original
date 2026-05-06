using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;
using UnityEngine;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// ステージに紐づく本番用バトル定義。
    /// 固定盤面ベースで BattleDemoTurnScript を置き換えるためのマスターデータ。
    /// </summary>
    [Serializable]
    public class StageBattleMasterData
    {
        [SerializeField]
        private StageBattleBoardMasterData board = new();

        [SerializeField]
        private StageBattleEnemyMasterData enemy = new();

        [SerializeField]
        private List<StageTurnMasterData> turnDefinitions = new();

        public StageBattleBoardMasterData Board => board ??= new StageBattleBoardMasterData();

        public StageBattleEnemyMasterData Enemy => enemy ??= new StageBattleEnemyMasterData();

        public List<StageTurnMasterData> TurnDefinitions => turnDefinitions ??= new List<StageTurnMasterData>();
    }

    /// <summary>
    /// ステージ共通の盤面サイズと開始位置。
    /// </summary>
    [Serializable]
    public class StageBattleBoardMasterData
    {
        [SerializeField]
        private int width = 5;

        [SerializeField]
        private int height = 6;

        [SerializeField]
        private int startX = 2;

        [SerializeField]
        private int startY = 5;

        public int Width => width;

        public int Height => height;

        public int StartX => startX;

        public int StartY => startY;
    }

    /// <summary>
    /// ステージ戦闘の敵定義。
    /// </summary>
    [Serializable]
    public class StageBattleEnemyMasterData
    {
        [SerializeField]
        private string enemyName = string.Empty;

        [SerializeField]
        private int maxHp = 1;

        public string Name => enemyName;

        public int MaxHp => maxHp;
    }

    /// <summary>
    /// 1 ターンぶんの盤面と敵行動。
    /// </summary>
    [Serializable]
    public class StageTurnMasterData
    {
        [SerializeField]
        private string label = "Turn";

        [SerializeField]
        private BattleEnemyActionType enemyAction = BattleEnemyActionType.NormalAttack;

        [SerializeField]
        [TextArea]
        private string boardSummary = string.Empty;

        [SerializeField]
        private string confirmText = string.Empty;

        [SerializeField]
        [TextArea]
        private string notes = string.Empty;

        [SerializeField]
        private int enemyActionDamage;

        [SerializeField]
        private List<StageTurnHazardGroupMasterData> hazardGroups = new();

        [SerializeField]
        private List<StageTurnCellMasterData> cellPlacements = new();

        public string Label => label;

        public BattleEnemyActionType EnemyAction => enemyAction;

        public string BoardSummary => boardSummary;

        public string ConfirmText => confirmText;

        public string Notes => notes;

        public int EnemyActionDamage => enemyActionDamage;

        public List<StageTurnHazardGroupMasterData> HazardGroups => hazardGroups ??= new List<StageTurnHazardGroupMasterData>();

        public List<StageTurnCellMasterData> CellPlacements => cellPlacements ??= new List<StageTurnCellMasterData>();
    }

    /// <summary>
    /// 危険グループ単位のダメージ定義。
    /// </summary>
    [Serializable]
    public class StageTurnHazardGroupMasterData
    {
        [SerializeField]
        private int groupId = 1;

        [SerializeField]
        private int damage = 80;

        public int GroupId => groupId;

        public int Damage => damage;
    }

    /// <summary>
    /// 1 ターン盤面に置くセル定義。
    /// </summary>
    [Serializable]
    public class StageTurnCellMasterData
    {
        [SerializeField]
        private int x;

        [SerializeField]
        private int y;

        [SerializeField]
        private BattleNodeType nodeType = BattleNodeType.Empty;

        [SerializeField]
        private int hazardGroupId = -1;

        public int X => x;

        public int Y => y;

        public BattleNodeType NodeType => nodeType;

        public int HazardGroupId => hazardGroupId;
    }
}
