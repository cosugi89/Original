#if UNITY_EDITOR
using Assets.Scripts.Data.MasterData;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(BattleDamageFormulaDefinition))]
    public sealed class BattleDamageFormulaDefinitionDrawer : PropertyDrawer
    {
        private const float VerticalSpacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null)
            {
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var currentY = position.y;
            var foldoutRect = new Rect(position.x, currentY, position.width, lineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            currentY += lineHeight + VerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                DrawPropertyLine(ref currentY, position, property, "isEnabled", "Use Custom Formula");
                var isEnabledProperty = property.FindPropertyRelative("isEnabled");
                if (isEnabledProperty != null && isEnabledProperty.boolValue)
                {
                    DrawPropertyLine(ref currentY, position, property, "parameterType", "Parameter");
                    DrawPropertyLine(ref currentY, position, property, "valueType", "Value Type");
                    DrawPropertyLine(ref currentY, position, property, "value", "Value");
                    DrawPropertyLine(ref currentY, position, property, "flatBonus", "Flat Bonus");
                    DrawPropertyLine(ref currentY, position, property, "finalMultiplier", "Final Multiplier");
                    DrawPropertyLine(ref currentY, position, property, "sourceScalarKey", "Source Scalar Key");
                    DrawPropertyLine(ref currentY, position, property, "sourceScalarFallback", "Source Scalar Fallback");
                    DrawPropertyLine(ref currentY, position, property, "minimumDamage", "Minimum Damage");

                    var helpRect = new Rect(position.x, currentY, position.width, lineHeight * 2f);
                    EditorGUI.HelpBox(
                        helpRect,
                        "未設定の項目は runtime 側で既定値へフォールバックします。Source Scalar Key を空にすると scalar 参照は省略されます。",
                        MessageType.None);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
            {
                return height;
            }

            height += VerticalSpacing + EditorGUIUtility.singleLineHeight;
            var isEnabledProperty = property.FindPropertyRelative("isEnabled");
            if (isEnabledProperty != null && isEnabledProperty.boolValue)
            {
                height += (EditorGUIUtility.singleLineHeight + VerticalSpacing) * 8f;
                height += (EditorGUIUtility.singleLineHeight * 2f) + VerticalSpacing;
            }

            return height;
        }

        private static void DrawPropertyLine(
            ref float currentY,
            Rect position,
            SerializedProperty rootProperty,
            string relativePropertyName,
            string label)
        {
            var property = rootProperty.FindPropertyRelative(relativePropertyName);
            if (property == null)
            {
                return;
            }

            var rect = new Rect(position.x, currentY, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, property, new GUIContent(label));
            currentY += EditorGUIUtility.singleLineHeight + VerticalSpacing;
        }
    }
}
#endif
