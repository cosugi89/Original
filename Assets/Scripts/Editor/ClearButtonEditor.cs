using Ricimi;
using UnityEditor;

[CustomEditor(typeof(CleanButton))]
public class ClearButtonEditor : UnityEditor.UI.ButtonEditor
{
    SerializedProperty fadeTime;
    SerializedProperty onHoverAlpha;
    SerializedProperty onClickAlpha;

    protected override void OnEnable()
    {
        base.OnEnable();
        fadeTime = serializedObject.FindProperty("fadeTime");
        onHoverAlpha = serializedObject.FindProperty("onHoverAlpha");
        onClickAlpha = serializedObject.FindProperty("onClickAlpha");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("CleanButton Config", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(fadeTime);
        EditorGUILayout.PropertyField(onHoverAlpha);
        EditorGUILayout.PropertyField(onClickAlpha);

        serializedObject.ApplyModifiedProperties();
    }
}