using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Features.Main;
using Assets.Scripts.Features.Title;
using Assets.Scripts.Systems.Save;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>Title起点のPlay Modeを、各描画frameと表示状態の記録で確認するEditor専用ツール。</summary>
public static class HomeEntrancePlayCapture
{
    [Serializable]
    private class Frame
    {
        public int index, stoneCount, leafCount, propCount, propLeafCount, completedPropLeaves;
        public int totalParts, visibleLegacyProps, interactableProps, spriteChangedCount, partObjectChangedCount;
        public float time, loadingAlpha, hudAlpha, meanBrightness, blackFraction, whiteFraction, yellowFraction, propLeafRootError;
        public string scene;
        public bool loadingExists, staticAltar, homeBackground;
    }
    [Serializable]
    private class Trace { public List<Frame> frames = new List<Frame>(); }

    [MenuItem("Tools/The Red Fever/Capture Home Entrance Play Mode")]
    public static void Begin()
    {
        if (!Application.isPlaying || SceneManager.GetActiveScene().name != "TitleScene")
            throw new InvalidOperationException("TitleScene起点のPlay Modeを開始してから記録してください。");
        var gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow.GetWindow(gameViewType).Focus();
        // 外部ツールの画面へフォーカスが移っても、確認用Play Modeだけは描画を継続する。
        Application.runInBackground = true;
        UnityEngine.Object.FindFirstObjectByType<UserDataManager>().StartCoroutine(Capture());
    }

    private static IEnumerator Capture()
    {
        const string folder = "Temp/HomeEntrance/RetainedPlay";
        Directory.CreateDirectory(folder);
        var trace = new Trace();
        var start = Time.realtimeSinceStartup;
        var title = UnityEngine.Object.FindFirstObjectByType<TitleSceneController>();
        var readyTime = -1f;
        var inputSent = false;
        var originalSprites = new Dictionary<string, int>();
        var originalObjects = new Dictionary<string, int>();
        while (Time.realtimeSinceStartup - start < 12f)
        {
            yield return new WaitForEndOfFrame();
            var frame = new Frame { index = trace.frames.Count, time = Time.realtimeSinceStartup - start, scene = SceneManager.GetActiveScene().name };
            var loading = UnityEngine.Object.FindObjectsByType<CanvasGroup>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(g => g.name == "Loading Canvas");
            frame.loadingExists = loading != null;
            frame.loadingAlpha = loading == null ? 0 : loading.alpha;
            var director = UnityEngine.Object.FindFirstObjectByType<HomeEntranceDirector>();
            if (director != null)
            {
                var serialized = new SerializedObject(director);
                frame.hudAlpha = ((CanvasGroup)serialized.FindProperty("homeHudCanvasGroup").objectReferenceValue).alpha;
                frame.staticAltar = ((GameObject)serialized.FindProperty("homeStaticAltar").objectReferenceValue).activeInHierarchy;
                frame.homeBackground = ((GameObject)serialized.FindProperty("homeBackground").objectReferenceValue).activeInHierarchy;
                var legacyProps = serialized.FindProperty("legacyPropObjects");
                for (var i = 0; i < legacyProps.arraySize; i++)
                    if (((GameObject)legacyProps.GetArrayElementAtIndex(i).objectReferenceValue).activeInHierarchy) frame.visibleLegacyProps++;
                var buttons = serialized.FindProperty("propButtons");
                for (var i = 0; i < buttons.arraySize; i++)
                    if (((Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue).IsInteractable()) frame.interactableProps++;
                foreach (var part in UnityEngine.Object.FindObjectsByType<HomeBuildPart>(FindObjectsSortMode.None))
                {
                    frame.totalParts++;
                    var partImage = part.GetComponent<Image>();
                    if (!originalSprites.ContainsKey(part.name))
                    {
                        originalSprites.Add(part.name, partImage.sprite.GetInstanceID());
                        originalObjects.Add(part.name, part.GetInstanceID());
                    }
                    if (originalSprites[part.name] != partImage.sprite.GetInstanceID()) frame.spriteChangedCount++;
                    if (originalObjects[part.name] != part.GetInstanceID()) frame.partObjectChangedCount++;
                    if (partImage.color.a <= 0) continue;
                    if (part.name.StartsWith("Build_Leaf")) frame.leafCount++;
                    else if (part.name.StartsWith("Build_Stone")) frame.stoneCount++;
                    else if (part.name.StartsWith("Build_PropLeaf"))
                    {
                        frame.propLeafCount++;
                        if (partImage.color.a >= .999f) frame.completedPropLeaves++;
                        var leaf = new SerializedObject(part);
                        var reference = (RectTransform)leaf.FindProperty("referenceTransform").objectReferenceValue;
                        var target = (RectTransform)leaf.FindProperty("targetTransform").objectReferenceValue;
                        frame.propLeafRootError = Mathf.Max(frame.propLeafRootError,
                            Vector3.Distance(target.position, reference.TransformPoint(leaf.FindProperty("finalOffset").vector2Value)));
                    }
                    else frame.propCount++;
                }
                if (frame.hudAlpha == 1 && !frame.loadingExists)
                {
                    if (readyTime < 0) readyTime = frame.time;
                    if (frame.time - readyTime > .25f) break;
                }
            }
            var texture = ScreenCapture.CaptureScreenshotAsTexture();
            if (texture != null)
            {
                var pixels = texture.GetPixels32();
                var sum = 0f; var black = 0; var white = 0; var yellow = 0;
                foreach (var p in pixels)
                {
                    sum += (p.r + p.g + p.b) / 765f;
                    if (p.r < 8 && p.g < 8 && p.b < 8) black++;
                    if (p.r > 245 && p.g > 245 && p.b > 245) white++;
                    if (p.r > 220 && p.g > 180 && p.b < 60) yellow++;
                }
                frame.meanBrightness = sum / pixels.Length;
                frame.blackFraction = (float)black / pixels.Length;
                frame.whiteFraction = (float)white / pixels.Length;
                frame.yellowFraction = (float)yellow / pixels.Length;
                File.WriteAllBytes($"{folder}/frame-{frame.index:0000}.png", texture.EncodeToPNG());
                UnityEngine.Object.Destroy(texture);
            }
            trace.frames.Add(frame);
            if (!inputSent && frame.time >= .2f)
            {
                // 画面上のRaycastを通して、ユーザーがタップするButtonから通常の入口へ入る。
                var serializedTitle = new SerializedObject(title);
                var begin = (Button)serializedTitle.FindProperty("beginButton").objectReferenceValue;
                var resurgence = (Button)serializedTitle.FindProperty("resurgenceButton").objectReferenceValue;
                var button = begin.gameObject.activeInHierarchy ? begin : resurgence;
                var rect = (RectTransform)button.transform;
                var canvas = button.GetComponentInParent<Canvas>();
                var pointer = new PointerEventData(EventSystem.current)
                {
                    position = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rect.TransformPoint(rect.rect.center)),
                    button = PointerEventData.InputButton.Left
                };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                if (hits.Count == 0 || ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject) != button.gameObject)
                    throw new InvalidOperationException("Title開始Buttonに画面上のタップが届きません。");
                ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                inputSent = true;
            }
            File.WriteAllText(folder + "/trace.json", JsonUtility.ToJson(trace, true));
        }
        File.WriteAllText(folder + "/trace.json", JsonUtility.ToJson(trace, true));
        Debug.Log($"Home入場記録完了: {trace.frames.Count} frames / {Time.realtimeSinceStartup - start:F2} 秒 / {folder}");
    }

    [MenuItem("Tools/The Red Fever/Compare Retained Home Altar")]
    public static void CompareRetained()
    {
        if (!Application.isPlaying || SceneManager.GetActiveScene().name != "MainScene")
            throw new InvalidOperationException("Home入場完了後のPlay Modeで比較してください。");
        UnityEngine.Object.FindFirstObjectByType<UserDataManager>().StartCoroutine(Compare());
    }

    private static IEnumerator Compare()
    {
        var director = UnityEngine.Object.FindFirstObjectByType<HomeEntranceDirector>();
        var serialized = new SerializedObject(director);
        var hud = (CanvasGroup)serialized.FindProperty("homeHudCanvasGroup").objectReferenceValue;
        var originalAlpha = hud.alpha;
        hud.alpha = 0;
        var altar = (GameObject)serialized.FindProperty("homeStaticAltar").objectReferenceValue;
        // 旧祭壇はHUD配下、新小道具はHomeBuildRoot配下にあり、描画順が異なる。
        // 比較する祭壇以外を両方の画像から外し、小道具の被覆差を画素差に混ぜない。
        var propArray = serialized.FindProperty("propParts");
        var props = new List<GameObject>();
        for (var i = 0; i < propArray.arraySize; i++)
        {
            var prop = ((HomeBuildPart)propArray.GetArrayElementAtIndex(i).objectReferenceValue).gameObject;
            prop.SetActive(false);
            props.Add(prop);
        }
        var parts = new List<HomeBuildPart>();
        foreach (var property in new[] { "buildParts", "foliageParts" })
        {
            var array = serialized.FindProperty(property);
            for (var i = 0; i < array.arraySize; i++)
            {
                var part = (HomeBuildPart)array.GetArrayElementAtIndex(i).objectReferenceValue;
                part.ShowCompleted(false);
                parts.Add(part);
            }
        }
        altar.SetActive(false);
        yield return new WaitForEndOfFrame();
        SaveFrame("Temp/HomeEntrance/assembled-render.png");
        foreach (var part in parts) part.Hide();
        altar.SetActive(true);
        yield return new WaitForEndOfFrame();
        SaveFrame("Temp/HomeEntrance/static-render.png");
        // 通常画面も構築パーツを保持するので、比較後は同じパーツ表示へ戻す。
        altar.SetActive(false);
        foreach (var part in parts) part.ShowCompleted(false);
        foreach (var prop in props) prop.SetActive(true);
        hud.alpha = originalAlpha;
        // 同じ完成祭壇を背景に、小道具だけを旧Spriteと比較する。動くAngelは比較中だけ隠す。
        var main = UnityEngine.Object.FindFirstObjectByType<MainScene>();
        var angel = (GameObject)new SerializedObject(main).FindProperty("angelHome").objectReferenceValue;
        var angelWasActive = angel.activeSelf;
        angel.SetActive(false);
        yield return new WaitForEndOfFrame();
        SaveFrame("Temp/HomeEntrance/retained-props-render.png");
        var legacyArray = serialized.FindProperty("legacyPropObjects");
        var legacyProps = new List<GameObject>();
        for (var i = 0; i < legacyArray.arraySize; i++)
        {
            var prop = (GameObject)legacyArray.GetArrayElementAtIndex(i).objectReferenceValue;
            legacyProps.Add(prop);
            prop.SetActive(true);
        }
        foreach (var prop in props) prop.SetActive(false);
        yield return new WaitForEndOfFrame();
        SaveFrame("Temp/HomeEntrance/original-props-render.png");
        foreach (var prop in legacyProps) prop.SetActive(false);
        foreach (var prop in props) prop.SetActive(true);
        angel.SetActive(angelWasActive);
        Debug.Log("祭壇比較画像を保存しました: Temp/HomeEntrance/assembled-render.png / static-render.png");
    }

    private static void SaveFrame(string path)
    {
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.Destroy(texture);
    }

    [MenuItem("Tools/The Red Fever/Capture Current Home Frame")]
    public static void CaptureCurrent()
    {
        UnityEngine.Object.FindFirstObjectByType<UserDataManager>().StartCoroutine(CaptureOne());
    }

    private static IEnumerator CaptureOne()
    {
        yield return new WaitForEndOfFrame();
        SaveFrame("Temp/HomeEntrance/current-home.png");
    }
}
