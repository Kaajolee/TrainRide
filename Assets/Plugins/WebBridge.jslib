// WebBridge.jslib
// Place this file in:  Assets/Plugins/WebGL/WebBridge.jslib
//
// These functions are injected into the Emscripten JS environment.
// Call them from C# with [DllImport("__Internal")].

mergeInto(LibraryManager.library, {

  // ── Unity → JS ──────────────────────────────────────────────────────────────
  // Fires window.dispatchUnityEvent(eventName, jsonPayload)
  // so the JS bridge can route it to subscribed handlers.

  DispatchToWeb: function (eventNamePtr, payloadPtr) {
    var eventName = UTF8ToString(eventNamePtr);
    var payload   = UTF8ToString(payloadPtr);

    if (typeof window.dispatchUnityEvent === 'function') {
      window.dispatchUnityEvent(eventName, payload);
    } else {
      console.warn('[Bridge] window.dispatchUnityEvent not found — is bridge.js loaded?');
    }
  },

  // ── Lifecycle signal ─────────────────────────────────────────────────────────
  // Call this once your scene/game is fully initialised.
  // The JS bridge will flush any queued SendMessage calls.

  SignalBridgeReady: function () {
    if (typeof window.__unityBridgeReady === 'function') {
      window.__unityBridgeReady();
    }
  },

  // ── Optional helpers ─────────────────────────────────────────────────────────

  // Read a value from JS-side state (returns pointer to UTF-8 string).
  // Usage in C#:  string val = GetWebValue("themeColor");
  GetWebValue: function (keyPtr) {
    var key    = UTF8ToString(keyPtr);
    var result = (window.__bridgeStore && window.__bridgeStore[key]) || '';
    var len    = lengthBytesUTF8(result) + 1;
    var buf    = _malloc(len);
    stringToUTF8(result, buf, len);
    return buf;
  },

  // Write a value to JS-side state so C# can push small data without a round-trip.
  SetWebValue: function (keyPtr, valuePtr) {
    var key   = UTF8ToString(keyPtr);
    var value = UTF8ToString(valuePtr);
    window.__bridgeStore = window.__bridgeStore || {};
    window.__bridgeStore[key] = value;
  },
});
