using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Battle.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.Data.MasterData
{
    /// <summary>
    /// 敵行動にひもづく固定盤面パターン。
    /// board 情報は pattern と同時に編集できるよう、ここへ展開して持つ。
    /// </summary>
    [CreateAssetMenu(menuName = "Original/Master Data/Battle Pattern", fileName = "BattlePatternMasterData")]
    public class StageBattlePatternMasterData : ScriptableObject
    {
        [SerializeField]
        private int patternId;

        [SerializeField]
        private string label = "Turn";

        [SerializeField]
        private string description = string.Empty;

        [SerializeField]
        private BattleEnemyActionType enemyAction = BattleEnemyActionType.NormalAttack;

        [Header("行動情報")]
        [SerializeField]
        private string confirmText = string.Empty;

        [SerializeField]
        [Tooltip("%")]
        [FormerlySerializedAs("enemyActionDamageMultiplier")]
        private int damageMultiplier = 100;

        [Header("盤面情報")]
        [SerializeField]
        private StageBattleBoardMasterData board = new();

        [SerializeField]
        private List<StageTurnCellMasterData> cellPlacements = new();

        public int PatternId => patternId;

        public string Label => label;

        public string Description => description;

        public BattleEnemyActionType EnemyAction => enemyAction;

        public string ConfirmText => confirmText;

        public float DamageMultiplier => damageMultiplier / 100f;

        public StageBattleBoardMasterData Board => board ??= new StageBattleBoardMasterData();

        public IReadOnlyList<StageTurnCellMasterData> CellPlacements => cellPlacements ??= new List<StageTurnCellMasterData>();
    }

    /// <summary>
    /// Pattern ごとの盤面サイズと開始位置。
    /// 普段は触らない想定なので foldout でまとめて扱う。
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
    /// 1 ターン盤面に置くセル定義。
    /// 危険マスの連結は runtime で自動グループ化する。
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

        public int X => x;

        public int Y => y;

        public BattleNodeType NodeType => nodeType;
    }
}
