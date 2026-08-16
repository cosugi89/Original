using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using MainScreen = Assets.Scripts.Features.Main.Screen;

public class MainTabs : MonoBehaviour
{
    [SerializeField] private MainScene mainScene;
    [SerializeField] private Button[] tabButtons = Array.Empty<Button>();
    [SerializeField] private GameObject[] screens = Array.Empty<GameObject>();
    [SerializeField] private GameObject[] normalStates = Array.Empty<GameObject>();
    [SerializeField] private GameObject[] focusStates = Array.Empty<GameObject>();
    [SerializeField, Min(0)] private int defaultIndex = 2;

    private UnityAction[] _handlers = Array.Empty<UnityAction>();
    private bool _initialized;

    private void Awake()
    {
        Init();
    }

    private void OnDestroy()
    {
        for (var i = 0; i < tabButtons.Length && i < _handlers.Length; i++)
        {
            if (tabButtons[i] != null && _handlers[i] != null)
            {
                tabButtons[i].onClick.RemoveListener(_handlers[i]);
            }
        }
    }

    public void Init()
    {
        if (_initialized)
            return;

        _handlers = new UnityAction[tabButtons.Length];
        for (var i = 0; i < tabButtons.Length; i++)
        {
            var index = i;
            _handlers[i] = () => Select(index);

            if (tabButtons[i] != null)
            {
                tabButtons[i].onClick.AddListener(_handlers[i]);
            }
        }

        Select(ResolveInitialIndex(), notifyMainScene: false);
        _initialized = true;
    }

    public void Select(int index)
    {
        Select(index, notifyMainScene: true);
    }

    private void Select(int index, bool notifyMainScene)
    {
        if (index < 0 || index >= screens.Length)
            return;

        for (var i = 0; i < screens.Length; i++)
        {
            if (screens[i] != null)
            {
                screens[i].SetActive(i == index);
            }

            if (i < normalStates.Length && normalStates[i] != null)
            {
                normalStates[i].SetActive(true);
            }

            if (i < focusStates.Length && focusStates[i] != null)
            {
                focusStates[i].SetActive(i == index);
            }
        }

        if (!notifyMainScene || mainScene == null || screens[index] == null)
            return;

        var screen = screens[index].GetComponent<MainScreen>();
        if (screen != null)
        {
            mainScene.ChangeScreen(screen);
        }
    }

    private int ResolveInitialIndex()
    {
        return Mathf.Clamp(defaultIndex, 0, Math.Max(0, screens.Length - 1));
    }
}
