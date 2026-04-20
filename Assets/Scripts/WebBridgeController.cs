// WebBridgeManager.cs
// Drop this onto a GameObject named exactly "GameManager" in your scene.
// (The name must match what the JS side uses in bridge.send('GameManager', ...))
//
// Requires Unity 2021+ with WebGL platform.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WebBridgeManager : MonoBehaviour
{
    // ── JSLib imports ────────────────────────────────────────────────────────
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void DispatchToWeb(string eventName, string payload);
    [DllImport("__Internal")] private static extern void SignalBridgeReady();
    [DllImport("__Internal")] private static extern string GetWebValue(string key);
    [DllImport("__Internal")] private static extern void SetWebValue(string key, string value);
#else
    // Editor stubs so the project compiles outside WebGL
    private static void DispatchToWeb(string e, string p) => Debug.Log($"[Bridge stub] → {e}: {p}");
    private static void SignalBridgeReady() => Debug.Log("[Bridge stub] Ready");
    private static string GetWebValue(string k) => "";
    private static void SetWebValue(string k, string v) => Debug.Log($"[Bridge stub] set {k}={v}");
#endif

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Start()
    {
        // Tell the JS bridge it can flush its send queue
        SignalBridgeReady();

        // Example: let the page know which scene/section is active at boot
        Emit("SceneReady", new { scene = gameObject.scene.name });
    }

    // ── Sending events to JS ─────────────────────────────────────────────────

    /// <summary>
    /// Dispatch a strongly-typed event to the JS bridge.
    /// </summary>
    public static void Emit(string eventName, object payload = null)
    {
        string json = payload != null ? JsonUtility.ToJson(payload) : "{}";
        DispatchToWeb(eventName, json);
    }

    // ─── Receiving messages from JS ──────────────────────────────────────────
    // These are the public methods JS calls via:
    //   bridge.send('GameManager', 'MethodName', JSON.stringify(payload))

    /// <summary>JS → Unity: navigate the player to a portfolio section.</summary>
    public void NavigateToSection(string jsonPayload)
    {
        var msg = JsonUtility.FromJson<SectionPayload>(jsonPayload);
        Debug.Log($"[Bridge] Navigate → {msg.section}");
        // TODO: trigger your scene/camera transition here
        Emit("NavigationStarted", new { section = msg.section });
    }

    /// <summary>JS → Unity: unlock an achievement or section.</summary>
    public void UnlockSection(string jsonPayload)
    {
        var msg = JsonUtility.FromJson<SectionPayload>(jsonPayload);
        Debug.Log($"[Bridge] Unlock → {msg.id}");
        // TODO: call your game-logic unlock system
        Emit("SectionUnlocked", new { id = msg.id, timestamp = Time.time });
    }

    /// <summary>JS → Unity: apply a theme/colour change from the page.</summary>
    public void SetTheme(string jsonPayload)
    {
        var msg = JsonUtility.FromJson<ThemePayload>(jsonPayload);
        Debug.Log($"[Bridge] Theme → {msg.theme}");
        Emit("ThemeChanged", new { theme = msg.theme });
    }

    /// <summary>JS → Unity: pause signal (e.g. tab hidden).</summary>
    public void OnWebPause(string _) => Time.timeScale = 0f;

    /// <summary>JS → Unity: resume signal.</summary>
    public void OnWebResume(string _) => Time.timeScale = 1f;

    // ── Shared state helpers ─────────────────────────────────────────────────

    public string ReadWebValue(string key) => GetWebValue(key);
    public void WriteWebValue(string key, string value) => SetWebValue(key, value);

    // ── Payload structs ──────────────────────────────────────────────────────

    [Serializable] private struct SectionPayload { public string section; public string id; }
    [Serializable] private struct ThemePayload { public string theme; }
}