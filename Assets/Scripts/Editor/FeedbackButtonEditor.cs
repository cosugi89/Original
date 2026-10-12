using Assets.Scripts.UI.Components;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(FeedbackButton))]
    public sealed class FeedbackButtonEditor : ButtonEditor
    {
        private const string AnimatorControllerPath = "Assets/Resources/Animations/Button/Button.controller";
        private const string ClickSoundPath = "Assets/Resources/Sounds/Button-1.wav";

        private SerializedProperty _animator;
        private SerializedProperty _audioSource;
        private SerializedProperty _clickSound;
        private SerializedProperty _clickTrigger;

        protected override void OnEnable()
        {
            base.OnEnable();
            _animator = serializedObject.FindProperty("feedbackAnimator");
            _audioSource = serializedObject.FindProperty("audioSource");
            _clickSound = serializedObject.FindProperty("clickSound");
            _clickTrigger = serializedObject.FindProperty("clickTrigger");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Click Feedback", EditorStyles.boldLabel);

            if (GUILayout.Button("フィードバックをセットアップ", GUILayout.Height(24f)))
            {
                SetupFeedbackComponents();
            }

            EditorGUILayout.PropertyField(_animator);
            EditorGUILayout.PropertyField(_audioSource);
            EditorGUILayout.PropertyField(_clickSound);
            EditorGUILayout.PropertyField(_clickTrigger);
            serializedObject.ApplyModifiedProperties();
        }

        private void SetupFeedbackComponents()
        {
            foreach (var targetObject in targets)
            {
                var button = (FeedbackButton)targetObject;
                var targetGameObject = button.gameObject;
                Undo.RecordObject(button, "Setup Feedback Button");

                var animator = targetGameObject.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = Undo.AddComponent<Animator>(targetGameObject);
                }

                Undo.RecordObject(animator, "Setup Feedback Button");
                animator.runtimeAnimatorController =
                    AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorControllerPath);

                var audioSource = targetGameObject.GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = Undo.AddComponent<AudioSource>(targetGameObject);
                }

                Undo.RecordObject(audioSource, "Setup Feedback Button");
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;

                button.transition = Selectable.Transition.Animation;
                button.animationTriggers = new AnimationTriggers
                {
                    normalTrigger = "Normal",
                    highlightedTrigger = "Highlighted",
                    pressedTrigger = "Pressed",
                    selectedTrigger = "Selected",
                    disabledTrigger = "Disabled"
                };

                var buttonSerializedObject = new SerializedObject(button);
                buttonSerializedObject.FindProperty("feedbackAnimator").objectReferenceValue = animator;
                buttonSerializedObject.FindProperty("audioSource").objectReferenceValue = audioSource;
                buttonSerializedObject.FindProperty("clickSound").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<AudioClip>(ClickSoundPath);
                buttonSerializedObject.ApplyModifiedProperties();
                PrefabUtility.RecordPrefabInstancePropertyModifications(button);
                PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
                PrefabUtility.RecordPrefabInstancePropertyModifications(audioSource);
            }
        }
    }
}
