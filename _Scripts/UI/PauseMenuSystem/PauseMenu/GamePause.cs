using UnityEngine;

public static class GamePause
{
    public static bool IsPaused { get; private set; }

    public static event System.Action<bool> OnPauseChanged;

    public static void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;

        IsPaused = paused;
        OnPauseChanged?.Invoke(paused);
    }
}

