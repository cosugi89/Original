using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset dialogUxml;

    private Button startButton;
    private Button menuButton;

    private VisualElement modalLayer;

    private Button lastCloseButton;
    private Action lastCloseHandler;

    private void Start()
    {
        var document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogWarning("[TitleUI] UIDocument was not found.");
            return;
        }

        var root = document.rootVisualElement;
        modalLayer = root.Q<VisualElement>("modalLayer");

        startButton = root.Q<Button>("startButton");
        menuButton = root.Q<Button>("menuButton");

        if (startButton != null)
        {
            startButton.clicked += OnStartClicked;
        }

        if (menuButton != null)
        {
            menuButton.clicked += OnMenuClicked;
        }
    }

    private void OnStartClicked()
    {
        Debug.Log("[TitleUI] Start button clicked.");
    }

    private void OnMenuClicked()
    {
        Debug.Log("[TitleUI] Menu button clicked.");

        if (modalLayer == null || dialogUxml == null)
        {
            return;
        }

        if (modalLayer.childCount > 0)
        {
            return;
        }

        modalLayer.style.display = DisplayStyle.Flex;

        var dialog = dialogUxml.Instantiate();
        modalLayer.Add(dialog);

        var closeButton = dialog.Q<Button>("closeButton");
        if (closeButton == null)
        {
            return;
        }

        Action closeHandler = null;
        closeHandler = () =>
        {
            closeButton.clicked -= closeHandler;

            if (lastCloseButton == closeButton)
            {
                lastCloseButton = null;
                lastCloseHandler = null;
            }

            dialog.RemoveFromHierarchy();
            modalLayer.style.display = DisplayStyle.None;
        };

        lastCloseButton = closeButton;
        lastCloseHandler = closeHandler;
        closeButton.clicked += closeHandler;
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.clicked -= OnStartClicked;
        }

        if (menuButton != null)
        {
            menuButton.clicked -= OnMenuClicked;
        }

        if (lastCloseButton != null && lastCloseHandler != null)
        {
            lastCloseButton.clicked -= lastCloseHandler;
        }

        lastCloseButton = null;
        lastCloseHandler = null;

        if (modalLayer != null)
        {
            modalLayer.Clear();
            modalLayer.style.display = DisplayStyle.None;
        }
    }
}
