using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Screen fade coordinator. Exposes awaitable FadeIn/FadeOut so callers
/// (like SegmentManager during a heavy load) can sync cleanly.
///
/// Setup:
///   - Create a full-screen Canvas with a black Image covering it.
///   - Add a CanvasGroup to the Canvas and assign it here.
///   - Start alpha = 0 (transparent), blocksRaycasts = false.
///
/// Usage:
///   await transition.FadeOutAsync();
///   // ...do loading work...
///   await transition.FadeInAsync();
/// </summary>
public class TransitionController : MonoBehaviour
{
    [Tooltip("CanvasGroup on the fade overlay canvas. Alpha 0 = clear, 1 = opaque.")]
    public CanvasGroup fadeGroup;

    [Tooltip("Default duration (seconds) for fade in/out.")]
    public float defaultDuration = 0.5f;

    /// <summary>Fade to opaque (black). Blocks input while faded.</summary>
    public Task FadeOutAsync(float duration = -1f) => FadeAsync(1f, duration, blockRaycasts: true);

    /// <summary>Fade to clear. Re-enables input.</summary>
    public Task FadeInAsync(float duration = -1f) => FadeAsync(0f, duration, blockRaycasts: false);

    async Task FadeAsync(float target, float duration, bool blockRaycasts)
    {
        if (fadeGroup == null) return;

        if (duration < 0f) duration = defaultDuration;
        fadeGroup.blocksRaycasts = blockRaycasts || target > 0f;

        float start = fadeGroup.alpha;
        if (Mathf.Approximately(start, target) || duration <= 0f)
        {
            fadeGroup.alpha = target;
            fadeGroup.blocksRaycasts = blockRaycasts;
            return;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(start, target, t / duration);
            await Task.Yield();
        }
        fadeGroup.alpha = target;
        fadeGroup.blocksRaycasts = blockRaycasts;
    }
}
