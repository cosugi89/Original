using Assets.Scripts.UI.Components;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    public static class GameButtonMenuItems
    {
        private const string PrefabPath = "Assets/Resources/Prefabs/GameButton.prefab";
        private const string AnimatorControllerPath = "Assets/Resources/Animations/Button/Button.controller";
        private const string ButtonSpritePath = "Assets/Assets/Sprite_Common/Button/";
        private const string BadgeSpritePath = "Assets/Assets/Sprite_Dark/HUD/Alert_Dot_01_Red.png";

        [MenuItem("GameObject/UI/Game Button", false, 2031)]
        private static void CreateGameButton(MenuCommand menuCommand)
        {
            var parent = ResolveParent(menuCommand);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var buttonObject = prefab != null
                ? InstantiatePrefab(prefab, parent)
                : CreateFallbackGameButton(parent);

            EnsureEventSystem();
            Undo.RegisterCreatedObjectUndo(buttonObject, "Create Game Button");
            Selection.activeGameObject = buttonObject;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent)
        {
            var created = parent != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent)
                : (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (parent != null)
            {
                GameObjectUtility.SetParentAndAlign(created, parent.gameObject);
            }

            return created;
        }

        private static GameObject CreateFallbackGameButton(Transform parent)
        {
            var buttonObject = new GameObject(
                "GameButton",
                typeof(RectTransform),
                typeof(Image),
                typeof(CanvasGroup),
                typeof(Animator),
                typeof(AudioSource),
                typeof(GameButton));

            GameObjectUtility.SetParentAndAlign(buttonObject, parent != null ? parent.gameObject : null);
            var visualRoot = CreateRectTransform("VisualRoot", buttonObject.transform);
            var background = CreateImage("Bg", visualRoot);
            var light = CreateImage("Light", visualRoot);
            var highLight = CreateImage("HighLight", visualRoot);
            var gradient = CreateImage("Gradient", visualRoot);
            var shadow = CreateImage("Shadow", visualRoot);
            var icon = CreateImage("Icon", visualRoot);
            var label = CreateText("Label", visualRoot);
            var title = CreateText("TitleLabel", visualRoot);
            var value = CreateText("ValueLabel", visualRoot);
            var badge = CreateRectTransform("Badge", visualRoot);
            var badgeImage = CreateImage("Bg", badge);
            var badgeText = CreateText("Text", badge);

            ConfigureButton(
                buttonObject,
                visualRoot,
                background,
                light,
                highLight,
                gradient,
                shadow,
                icon,
                label,
                title,
                value,
                badge,
                badgeImage,
                badgeText);
            return buttonObject;
        }

        private static Transform ResolveParent(MenuCommand menuCommand)
        {
            var context = menuCommand.context as GameObject;
            if (context != null && context.GetComponentInParent<Canvas>(true) != null)
            {
                return context.transform;
            }

            var canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas != null)
            {
                return canvas.transform;
            }

            return CreateCanvas().transform;
        }

        private static GameObject CreateCanvas()
        {
            var canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");
            return canvasObject;
        }

        private static void ConfigureButton(
            GameObject buttonObject,
            RectTransform visualRoot,
            Image background,
            Image light,
            Image highLight,
            Image gradient,
            Image shadow,
            Image icon,
            TextMeshProUGUI label,
            TextMeshProUGUI title,
            TextMeshProUGUI value,
            RectTransform badge,
            Image badgeImage,
            TextMeshProUGUI badgeText)
        {
            var rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(322f, 130f);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;

            var animator = buttonObject.GetComponent<Animator>();
            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimatorControllerPath);

            var audioSource = buttonObject.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            var button = buttonObject.GetComponent<GameButton>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.Animation;
            button.animationTriggers = new AnimationTriggers
            {
                normalTrigger = "Normal",
                highlightedTrigger = "Highlighted",
                pressedTrigger = "Pressed",
                selectedTrigger = "Selected",
                disabledTrigger = "Disabled"
            };

            var serializedButton = new SerializedObject(button);
            serializedButton.FindProperty("frameType").enumValueIndex = (int)GameButton.FrameType.Button02;
            serializedButton.FindProperty("colorType").enumValueIndex = (int)GameButton.ColorType.Red;
            serializedButton.FindProperty("sizeType").enumValueIndex = (int)GameButton.SizeType.Medium;
            serializedButton.FindProperty("contentType").enumValueIndex = (int)GameButton.ContentType.Label;
            serializedButton.FindProperty("label").stringValue = "Button";
            serializedButton.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            serializedButton.FindProperty("backgroundImage").objectReferenceValue = background;
            serializedButton.FindProperty("lightImage").objectReferenceValue = light;
            serializedButton.FindProperty("highLightImage").objectReferenceValue = highLight;
            serializedButton.FindProperty("gradientImage").objectReferenceValue = gradient;
            serializedButton.FindProperty("shadowImage").objectReferenceValue = shadow;
            serializedButton.FindProperty("iconImage").objectReferenceValue = icon;
            serializedButton.FindProperty("labelText").objectReferenceValue = label;
            serializedButton.FindProperty("titleText").objectReferenceValue = title;
            serializedButton.FindProperty("valueText").objectReferenceValue = value;
            serializedButton.FindProperty("badgeRoot").objectReferenceValue = badge.gameObject;
            serializedButton.FindProperty("badgeText").objectReferenceValue = badgeText;
            serializedButton.FindProperty("badgeImage").objectReferenceValue = badgeImage;
            serializedButton.FindProperty("badgeSprite").objectReferenceValue = LoadBadgeSprite();
            serializedButton.FindProperty("button01BgSprite").objectReferenceValue = LoadSprite("Button_01_White_Bg.Png");
            serializedButton.FindProperty("button01InnerBorder1Sprite").objectReferenceValue = LoadSprite("Button_01_White_InnerBorder1.Png");
            serializedButton.FindProperty("button01InnerBorder2Sprite").objectReferenceValue = LoadSprite("Button_01_White_InnerBorder2.png");
            serializedButton.FindProperty("button02BgSprite").objectReferenceValue = LoadSprite("Button_02_White_Bg.png");
            serializedButton.FindProperty("button02LightSprite").objectReferenceValue = LoadSprite("Button_02_White_Light.png");
            serializedButton.FindProperty("button02HighLightSprite").objectReferenceValue = LoadSprite("Button_02_White_HighLight.png");
            serializedButton.FindProperty("button03BgSprite").objectReferenceValue = LoadSprite("Button_03_White_Bg.Png");
            serializedButton.FindProperty("button03LightSprite").objectReferenceValue = LoadSprite("Button_03_White_Light.Png");
            serializedButton.FindProperty("button03HighLightSprite").objectReferenceValue = LoadSprite("Button_03_White_HighLight.Png");
            serializedButton.FindProperty("button03GradientSprite").objectReferenceValue = LoadSprite("Button_03_White_Gradient.Png");
            serializedButton.FindProperty("button03ShadowSprite").objectReferenceValue = LoadSprite("Button_03_White_Shadow.Png");
            serializedButton.FindProperty("closeBackgroundSprite").objectReferenceValue =
                LoadSprite("Button_01_White_Bg.Png");
            serializedButton.FindProperty("closeLightSprite").objectReferenceValue =
                LoadSprite("Button_01_White_InnerBorder2.png");
            serializedButton.FindProperty("closeIconSprite").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Assets/Sprite_Common/~Demo/Demo_Icon/Icon_Close.png");
            serializedButton.FindProperty("audioSource").objectReferenceValue = audioSource;
            serializedButton.FindProperty("clickSoundType").enumValueIndex = (int)GameButton.SoundType.Button1;
            serializedButton.FindProperty("disabledClickSoundType").enumValueIndex = (int)GameButton.SoundType.Error;
            serializedButton.ApplyModifiedPropertiesWithoutUndo();

            button.ApplyVisual();
        }

        private static RectTransform CreateRectTransform(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return rectTransform;
        }

        private static Image CreateImage(string name, Transform parent)
        {
            var rectTransform = CreateRectTransform(name, parent);
            var image = rectTransform.gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent)
        {
            var rectTransform = CreateRectTransform(name, parent);
            var text = rectTransform.gameObject.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.88f, 0.88f, 0.88f, 1f);
            text.raycastTarget = false;
            return text;
        }

        private static Sprite LoadSprite(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath + fileName);
        }

        private static Sprite LoadBadgeSprite()
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(BadgeSpritePath);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            var eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
        }
    }
}
