using Assets.Scripts.UI.Components;
using UnityEditor;
using UnityEditor.UI;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(GameButton), true)]
    [CanEditMultipleObjects]
    public sealed class GameButtonEditor : ButtonEditor
    {
        private SerializedProperty _frameType;
        private SerializedProperty _buttonType;
        private SerializedProperty _colorType;
        private SerializedProperty _sizeType;
        private SerializedProperty _contentType;
        private SerializedProperty _label;
        private SerializedProperty _title;
        private SerializedProperty _value;
        private SerializedProperty _iconSprite;
        private SerializedProperty _badgeVisible;
        private SerializedProperty _badgeLabel;
        private SerializedProperty _badgeSprite;
        private SerializedProperty _visualRoot;
        private SerializedProperty _backgroundImage;
        private SerializedProperty _lightImage;
        private SerializedProperty _highLightImage;
        private SerializedProperty _gradientImage;
        private SerializedProperty _shadowImage;
        private SerializedProperty _iconImage;
        private SerializedProperty _labelText;
        private SerializedProperty _titleText;
        private SerializedProperty _valueText;
        private SerializedProperty _selectedRoot;
        private SerializedProperty _disabledRoot;
        private SerializedProperty _loadingRoot;
        private SerializedProperty _badgeRoot;
        private SerializedProperty _badgeText;
        private SerializedProperty _badgeImage;
        private SerializedProperty _audioSource;
        private SerializedProperty _clickSoundType;
        private SerializedProperty _customClickSound;
        private SerializedProperty _disabledClickSoundType;
        private SerializedProperty _customDisabledClickSound;

        protected override void OnEnable()
        {
            base.OnEnable();

            _frameType = serializedObject.FindProperty("frameType");
            _buttonType = serializedObject.FindProperty("buttonType");
            _colorType = serializedObject.FindProperty("colorType");
            _sizeType = serializedObject.FindProperty("sizeType");
            _contentType = serializedObject.FindProperty("contentType");
            _label = serializedObject.FindProperty("label");
            _title = serializedObject.FindProperty("title");
            _value = serializedObject.FindProperty("value");
            _iconSprite = serializedObject.FindProperty("iconSprite");
            _badgeVisible = serializedObject.FindProperty("badgeVisible");
            _badgeLabel = serializedObject.FindProperty("badgeLabel");
            _badgeSprite = serializedObject.FindProperty("badgeSprite");
            _visualRoot = serializedObject.FindProperty("visualRoot");
            _backgroundImage = serializedObject.FindProperty("backgroundImage");
            _lightImage = serializedObject.FindProperty("lightImage");
            _highLightImage = serializedObject.FindProperty("highLightImage");
            _gradientImage = serializedObject.FindProperty("gradientImage");
            _shadowImage = serializedObject.FindProperty("shadowImage");
            _iconImage = serializedObject.FindProperty("iconImage");
            _labelText = serializedObject.FindProperty("labelText");
            _titleText = serializedObject.FindProperty("titleText");
            _valueText = serializedObject.FindProperty("valueText");
            _selectedRoot = serializedObject.FindProperty("selectedRoot");
            _disabledRoot = serializedObject.FindProperty("disabledRoot");
            _loadingRoot = serializedObject.FindProperty("loadingRoot");
            _badgeRoot = serializedObject.FindProperty("badgeRoot");
            _badgeText = serializedObject.FindProperty("badgeText");
            _badgeImage = serializedObject.FindProperty("badgeImage");
            _audioSource = serializedObject.FindProperty("audioSource");
            _clickSoundType = serializedObject.FindProperty("clickSoundType");
            _customClickSound = serializedObject.FindProperty("customClickSound");
            _disabledClickSoundType = serializedObject.FindProperty("disabledClickSoundType");
            _customDisabledClickSound = serializedObject.FindProperty("customDisabledClickSound");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Game Button", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_buttonType);
            var isNormal = _buttonType.hasMultipleDifferentValues ||
                _buttonType.enumValueIndex == (int)GameButton.ButtonType.Normal;
            if (isNormal)
            {
                EditorGUILayout.PropertyField(_frameType);
                EditorGUILayout.PropertyField(_colorType);
                EditorGUILayout.PropertyField(_sizeType);
                EditorGUILayout.PropertyField(_contentType);
                EditorGUILayout.PropertyField(_label);
                EditorGUILayout.PropertyField(_title);
                EditorGUILayout.PropertyField(_value);
                EditorGUILayout.PropertyField(_iconSprite);
                EditorGUILayout.PropertyField(_badgeVisible);
                if (_badgeVisible.boolValue)
                {
                    EditorGUILayout.PropertyField(_badgeLabel);
                }
                EditorGUILayout.PropertyField(_badgeSprite);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Wiring", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_visualRoot);
            EditorGUILayout.PropertyField(_backgroundImage);
            EditorGUILayout.PropertyField(_lightImage);
            EditorGUILayout.PropertyField(_highLightImage);
            EditorGUILayout.PropertyField(_gradientImage);
            EditorGUILayout.PropertyField(_shadowImage);
            EditorGUILayout.PropertyField(_iconImage);
            EditorGUILayout.PropertyField(_labelText);
            EditorGUILayout.PropertyField(_titleText);
            EditorGUILayout.PropertyField(_valueText);
            EditorGUILayout.PropertyField(_selectedRoot);
            EditorGUILayout.PropertyField(_disabledRoot);
            EditorGUILayout.PropertyField(_loadingRoot);
            EditorGUILayout.PropertyField(_badgeRoot);
            EditorGUILayout.PropertyField(_badgeText);
            EditorGUILayout.PropertyField(_badgeImage);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sound", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_audioSource);
            EditorGUILayout.PropertyField(_clickSoundType);
            if (IsCustomSound(_clickSoundType))
            {
                EditorGUILayout.PropertyField(_customClickSound);
            }

            EditorGUILayout.PropertyField(_disabledClickSoundType);
            if (IsCustomSound(_disabledClickSoundType))
            {
                EditorGUILayout.PropertyField(_customDisabledClickSound);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Apply Style"))
            {
                foreach (var targetObject in targets)
                {
                    var button = (GameButton)targetObject;
                    Undo.RecordObject(button, "Apply Game Button Style");
                    button.ApplyStyle();
                    EditorUtility.SetDirty(button);
                }
            }
        }

        private static bool IsCustomSound(SerializedProperty soundType)
        {
            return !soundType.hasMultipleDifferentValues &&
                soundType.enumValueIndex == (int)GameButton.SoundType.Custom;
        }
    }
}
