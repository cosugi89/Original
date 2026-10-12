using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// MainSceneなどを編集中でも、Toolbarから一度だけTitleScene起点でPlay Modeを開始する。
    /// </summary>
    [InitializeOnLoad]
    public static class TitleScenePlayModeToolbar
    {
        private const string TitleScenePath = "Assets/Scenes/TitleScene.unity";
        private const string ButtonName = "OriginalTitleScenePlayButton";
        private const string IsTitleStartPendingKey = "Original.TitleScenePlayMode.Pending";
        private const string PreviousStartScenePathKey = "Original.TitleScenePlayMode.PreviousStartScenePath";

        private static readonly Type ToolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static readonly FieldInfo ToolbarRootField = ToolbarType?.GetField("m_Root", BindingFlags.Instance | BindingFlags.NonPublic);

        private static Button _titleButton;

        static TitleScenePlayModeToolbar()
        {
            EditorApplication.update += EnsureButton;
            EditorApplication.playModeStateChanged += RestorePlayModeStartScene;
        }

        private static void EnsureButton()
        {
            if (_titleButton != null && _titleButton.panel != null)
            {
                _titleButton.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
                return;
            }

            if (ToolbarType == null || ToolbarRootField == null)
            {
                return;
            }

            var toolbar = Resources.FindObjectsOfTypeAll(ToolbarType);
            if (toolbar.Length == 0)
            {
                return;
            }

            var root = ToolbarRootField.GetValue(toolbar[0]) as VisualElement;
            var playModeZone = root?.Q<VisualElement>("ToolbarZonePlayMode");
            if (playModeZone == null)
            {
                return;
            }

            _titleButton = playModeZone.Q<Button>(ButtonName);
            if (_titleButton != null)
            {
                return;
            }

            _titleButton = new Button(StartTitleScene)
            {
                name = ButtonName,
                text = "Title",
                tooltip = "Play from TitleScene"
            };
            _titleButton.style.marginLeft = 4f;
            _titleButton.style.marginRight = 4f;
            _titleButton.style.height = 20f;
            _titleButton.style.minWidth = 38f;
            _titleButton.style.paddingLeft = 6f;
            _titleButton.style.paddingRight = 6f;
            _titleButton.style.backgroundColor = new Color(0.31f, 0.31f, 0.31f, 1f);
            _titleButton.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            _titleButton.style.borderTopColor = new Color(0.39f, 0.39f, 0.39f, 1f);
            _titleButton.style.borderBottomColor = new Color(0.16f, 0.16f, 0.16f, 1f);
            _titleButton.style.borderLeftColor = new Color(0.39f, 0.39f, 0.39f, 1f);
            _titleButton.style.borderRightColor = new Color(0.16f, 0.16f, 0.16f, 1f);
            _titleButton.style.borderTopLeftRadius = 2f;
            _titleButton.style.borderTopRightRadius = 2f;
            _titleButton.style.borderBottomLeftRadius = 2f;
            _titleButton.style.borderBottomRightRadius = 2f;
            _titleButton.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
            playModeZone.Add(_titleButton);
        }

        private static void StartTitleScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var titleScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(TitleScenePath);
            if (titleScene == null)
            {
                return;
            }

            SessionState.SetString(
                PreviousStartScenePathKey,
                EditorSceneManager.playModeStartScene != null ? AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene) : string.Empty);
            SessionState.SetBool(IsTitleStartPendingKey, true);
            EditorSceneManager.playModeStartScene = titleScene;
            EditorApplication.isPlaying = true;
        }

        private static void RestorePlayModeStartScene(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(IsTitleStartPendingKey, false))
            {
                return;
            }

            var previousStartScenePath = SessionState.GetString(PreviousStartScenePathKey, string.Empty);
            EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previousStartScenePath)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(previousStartScenePath);
            SessionState.EraseBool(IsTitleStartPendingKey);
            SessionState.EraseString(PreviousStartScenePathKey);
        }
    }
}
