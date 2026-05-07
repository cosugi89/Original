#if UNITY_EDITOR
using Assets.Scripts.Data.MasterData;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(EquipmentMasterDatabase))]
    public class EquipmentMasterDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (target is not EquipmentMasterDatabase database)
            {
                return;
            }

            var issues = database.GetValidationIssues();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            if (issues == null || issues.Count == 0)
            {
                EditorGUILayout.HelpBox("問題は見つかっていません。", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox($"{issues.Count} 件の問題があります。保存前に確認してください。", MessageType.Warning);

            for (var i = 0; i < issues.Count; i++)
            {
                var issue = issues[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(issue.Code, EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(issue.Message, EditorStyles.wordWrappedLabel);
                if (issue.Definition != null)
                {
                    EditorGUILayout.ObjectField("Asset", issue.Definition, typeof(EquipmentMasterData), false);
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
#endif
