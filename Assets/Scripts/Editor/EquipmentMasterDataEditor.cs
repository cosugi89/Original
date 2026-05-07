#if UNITY_EDITOR
using Assets.Scripts.Data.MasterData;
using UnityEditor;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(EquipmentMasterData))]
    public class EquipmentMasterDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (target is not EquipmentMasterData definition)
            {
                return;
            }

            var issues = definition.GetValidationIssues();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            if (issues == null || issues.Count == 0)
            {
                EditorGUILayout.HelpBox("この asset 単体では問題は見つかっていません。", MessageType.Info);
                return;
            }

            for (var i = 0; i < issues.Count; i++)
            {
                var issue = issues[i];
                EditorGUILayout.HelpBox(issue.Message, ToMessageType(issue.Severity));
                EditorGUILayout.LabelField(issue.Code, EditorStyles.miniLabel);
            }
        }

        private static MessageType ToMessageType(EquipmentMasterData.ValidationSeverity severity)
        {
            return severity switch
            {
                EquipmentMasterData.ValidationSeverity.Error => MessageType.Error,
                EquipmentMasterData.ValidationSeverity.Warning => MessageType.Warning,
                _ => MessageType.Info,
            };
        }
    }
}
#endif
