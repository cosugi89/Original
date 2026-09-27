using System;

public static class DebugStageSettings
{
    public static bool ShowDebugStages { get; private set; }

    public static event Action VisibilityChanged;
    public static event Action DungeonListRefreshRequested;

    public static void ToggleDebugStageVisibility()
    {
        SetDebugStageVisibility(!ShowDebugStages);
    }

    public static void SetDebugStageVisibility(bool isVisible)
    {
        if (ShowDebugStages == isVisible)
            return;

        ShowDebugStages = isVisible;
        VisibilityChanged?.Invoke();
    }

    public static void RequestDungeonListRefresh()
    {
        DungeonListRefreshRequested?.Invoke();
    }
}
