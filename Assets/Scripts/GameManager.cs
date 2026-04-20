using System;
using UnityEngine;

/// <summary>
/// Central coordinator for the train-ride experience.
/// Tracks the active segment, raises events when it changes, and serves
/// as a single point other systems (UI, SegmentManager, Audio, etc.) subscribe to.
///
/// Usage:
///   GameManager.Instance.OnSegmentChanged += (prev, next) => ...;
///   GameManager.Instance.AdvanceSegment();
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Tooltip("Segment definitions in playback order. Index 0 is the starting segment.")]
    public SegmentData[] segments;

    [Tooltip("Auto-start on scene load. Disable if an intro sequence should call StartExperience().")]
    public bool autoStart = true;

    public int CurrentIndex { get; private set; } = -1;
    public SegmentData CurrentSegment => IsValid(CurrentIndex) ? segments[CurrentIndex] : null;
    public SegmentData NextSegment => IsValid(CurrentIndex + 1) ? segments[CurrentIndex + 1] : null;
    public bool HasNext => IsValid(CurrentIndex + 1);

    /// <summary>Fired when the active segment changes. Args: (previousIndex, newIndex). Previous is -1 on first start.</summary>
    public event Action<int, int> OnSegmentChanged;
    public event Action OnExperienceStarted;
    public event Action OnExperienceCompleted;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (autoStart) StartExperience();
    }

    public void StartExperience()
    {
        if (segments == null || segments.Length == 0)
        {
            Debug.LogWarning("[GameManager] No segments configured.");
            return;
        }
        OnExperienceStarted?.Invoke();
        SetSegment(0);
    }

    /// <summary>Advance to the next segment. Called by SegmentTrigger when the train crosses a checkpoint.</summary>
    public void AdvanceSegment()
    {
        if (!HasNext)
        {
            OnExperienceCompleted?.Invoke();
            return;
        }
        SetSegment(CurrentIndex + 1);
    }

    public void SetSegment(int index)
    {
        if (!IsValid(index) || index == CurrentIndex) return;
        int prev = CurrentIndex;
        CurrentIndex = index;
        OnSegmentChanged?.Invoke(prev, CurrentIndex);
    }

    bool IsValid(int i) => segments != null && i >= 0 && i < segments.Length;
}
