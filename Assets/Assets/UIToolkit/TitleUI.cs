using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset dialogUxml;

    private Button startButton;
    private Button menuButton;

    private VisualElement modalLayer;

    // keep reference to the last dialog close handler so we can unsubscribe if needed
    private Button lastCloseButton;
    private Action lastCloseHandler;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        modalLayer = root.Q<VisualElement>("modalLayer");

        startButton = root.Q<Button>("startButton");
        menuButton = root.Q<Button>("menuButton");

        startButton.clicked += OnStartClicked;
        menuButton.clicked += OnMenuClicked;
    }

    private void OnStartClicked()
    {
        Debug.Log("はじめるボタンが押された");
    }

    private void OnMenuClicked()
    {
        Debug.Log("メニューボタンが押された");

        // prevent opening multiple dialogs on top of each other
        if (modalLayer.childCount > 0)  
            return;

        modalLayer.style.display = DisplayStyle.Flex;

        var dialog = dialogUxml.Instantiate();      

        modalLayer.Add(dialog);

        var closeButton = dialog.Q<Button>("closeButton");

        Action closeHandler = null;
        closeHandler = () =>
        {
            // remove this handler
            closeButton.clicked -= closeHandler;

            // clear stored references
            if (lastCloseButton == closeButton)
            {
                lastCloseButton = null;
                lastCloseHandler = null;
            }

            dialog.RemoveFromHierarchy();

            modalLayer.style.display = DisplayStyle.None;
        };

        // store so we can unsubscribe in OnDestroy if needed
        lastCloseButton = closeButton;
        lastCloseHandler = closeHandler;

        closeButton.clicked += closeHandler;
    }

    void OnDestroy()
    {
        startButton.clicked -= OnStartClicked;
        menuButton.clicked -= OnMenuClicked;

        // unsubscribe any remaining close handler
        lastCloseButton.clicked -= lastCloseHandler; // rullになる
        lastCloseButton = null;
        lastCloseHandler = null;

        modalLayer.Clear();
        modalLayer.style.display = DisplayStyle.None;
    }
}