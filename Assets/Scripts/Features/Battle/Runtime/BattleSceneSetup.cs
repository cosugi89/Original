using System;
using System.Collections.Generic;
using Assets.Scripts.Data.DTO;

namespace Assets.Scripts.Features.Battle.Runtime
{
    /// <summary>
    /// BattleScene 初期化に必要なステージ・敵・プレイヤー情報の束。
    /// </summary>
    public sealed class BattleSceneSetup
    {
        public BattleSceneSetup(
            StageData stageData,
            BattlePlayerSnapshot playerSnapshot,
            StageBattleEnemyData enemyData,
            IReadOnlyList<StageBattlePatternData> patterns,
            string enemyName,
            int initialEnemyHp)
        {
            StageData = stageData;
            PlayerSnapshot = playerSnapshot;
            EnemyData = enemyData ?? new StageBattleEnemyData();
            Patterns = patterns ?? Array.Empty<StageBattlePatternData>();
            EnemyName = enemyName ?? string.Empty;
            InitialEnemyHp = initialEnemyHp > 0 ? initialEnemyHp : 1;
        }

        public StageData StageData { get; }

        public BattlePlayerSnapshot PlayerSnapshot { get; }

        public StageBattleEnemyData EnemyData { get; }

        public IReadOnlyList<StageBattlePatternData> Patterns { get; }

        public string EnemyName { get; }

        public int InitialEnemyHp { get; }

        public int InitialPlayerHp => PlayerSnapshot?.MaxHp ?? 1;
    }
}

