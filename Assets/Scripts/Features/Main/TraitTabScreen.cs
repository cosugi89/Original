using System.Collections.Generic;
using Assets.Scripts.Data.MasterData;
using Assets.Scripts.Systems.GameData;
using Assets.Scripts.Systems.Save;
using Assets.Scripts.UI.Components;
using UnityEngine;

namespace Assets.Scripts.Features.Main
{
    /// <summary>Abilityタブ内のTrait TreeをRow Prefabとして構築する画面。</summary>
    public sealed class TraitTabScreen : Screen
    {
        [SerializeField] private AbilityTreeMasterData traitTree;
        [SerializeField] private PartsTraitTreeRow rowPrefab;
        [SerializeField] private Transform content;

        private readonly List<PartsTraitTreeRow> _rows = new();
        private AbilityProgressManager _progress;

        private void OnEnable()
        {
            _progress ??= UserDataManager.EnsureInitialized().GetAbilityProgress(traitTree);
            BuildTree(_progress.Recalculate());
        }

        private void BuildTree(AbilityDerivedData derivedData)
        {
            ClearRows();
            var statesById = new Dictionary<string, AbilityTreeNodeState>();
            for (var i = 0; i < derivedData.NodeStates.Count; i++)
            {
                var state = derivedData.NodeStates[i];
                statesById.Add(state.Entry.TreeNodeId, state);
            }

            for (var rowIndex = traitTree.DisplayRows.Count - 1; rowIndex >= 0; rowIndex--)
            {
                var row = Instantiate(rowPrefab, content);
                var previousRow = rowIndex > 0 ? traitTree.DisplayRows[rowIndex - 1] : null;
                row.Setup(traitTree, traitTree.DisplayRows[rowIndex], previousRow, statesById);
                _rows.Add(row);
            }
        }

        private void ClearRows()
        {
            for (var i = 0; i < _rows.Count; i++)
            {
                Destroy(_rows[i].gameObject);
            }

            _rows.Clear();
        }
    }
}
