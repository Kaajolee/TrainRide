using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Streams ride segments in and out as the player progresses.
/// Keeps only the current and next segment in memory to minimize WebGL footprint.
///
/// Listens to GameManager.OnSegmentChanged — no other system needs to talk to it directly.
/// To swap to Addressables later, replace LoadAdditiveAsync / UnloadAsync.
/// </summary>
public class SegmentManager : MonoBehaviour
{
    [Tooltip("Optional: coordinates a fade during heavy loads. Leave null to skip.")]
    public TransitionController transition;

    [Tooltip("Fade out before loading / fade in after. Useful for large segments.")]
    public bool fadeDuringLoad = false;

    private readonly HashSet<string> loaded = new HashSet<string>();

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSegmentChanged += HandleSegmentChanged;
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSegmentChanged -= HandleSegmentChanged;
    }

    async void HandleSegmentChanged(int prevIndex, int newIndex)
    {
        await UpdateStreamingWindowAsync(prevIndex, newIndex);
    }

    /// <summary>
    /// Ensures (current, next) are loaded and everything else is unloaded.
    /// Safe to call repeatedly; scenes already loaded are skipped.
    /// </summary>
    async Task UpdateStreamingWindowAsync(int prevIndex, int newIndex)
    {
        if (fadeDuringLoad && transition != null) await transition.FadeOutAsync();

        var gm = GameManager.Instance;
        var current = gm.CurrentSegment;
        var next = gm.NextSegment;

        // Load current first so the player never falls through an unloaded world.
        if (current != null) await EnsureLoadedAsync(current.sceneName);
        if (next != null) await EnsureLoadedAsync(next.sceneName);

        // Unload anything outside the {current, next} window.
        var keep = new HashSet<string>();
        if (current != null) keep.Add(current.sceneName);
        if (next != null) keep.Add(next.sceneName);

        var toUnload = new List<string>();
        foreach (var s in loaded)
            if (!keep.Contains(s)) toUnload.Add(s);

        foreach (var s in toUnload) await UnloadAsync(s);

        if (fadeDuringLoad && transition != null) await transition.FadeInAsync();
    }

    async Task EnsureLoadedAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || loaded.Contains(sceneName)) return;

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null)
        {
            Debug.LogError($"[SegmentManager] Scene '{sceneName}' not in Build Settings.");
            return;
        }
        while (!op.isDone) await Task.Yield();
        loaded.Add(sceneName);
    }

    async Task UnloadAsync(string sceneName)
    {
        if (!loaded.Contains(sceneName)) return;
        var op = SceneManager.UnloadSceneAsync(sceneName);
        if (op == null) { loaded.Remove(sceneName); return; }
        while (!op.isDone) await Task.Yield();
        loaded.Remove(sceneName);

        // Release unused assets — valuable on WebGL where memory is tight.
        await Resources.UnloadUnusedAssets();
    }
}
