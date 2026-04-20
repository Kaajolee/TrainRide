using System;
using UnityEngine;

/// <summary>
/// Stops the train at station-type segments and reports enter/exit events.
/// Listens to GameManager.OnSegmentChanged; pauses when the new segment is marked `isStation`.
///
/// Assign `train` to any MonoBehaviour that implements ITrainController.
/// Call `LeaveStation()` from a UI button or interaction hotspot to resume.
/// </summary>
public class StationController : MonoBehaviour
{
    [Tooltip("Drag the GameObject that carries your train-movement script (must implement ITrainController).")]
    public MonoBehaviour trainBehaviour;

    public event Action<SegmentData> OnStationEntered;
    public event Action<SegmentData> OnStationExited;

    private ITrainController train;
    private bool atStation;

    void Awake()
    {
        train = trainBehaviour as ITrainController;
        if (trainBehaviour != null && train == null)
            Debug.LogError($"[StationController] {trainBehaviour.name} does not implement ITrainController.");
    }

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

    void HandleSegmentChanged(int prevIndex, int newIndex)
    {
        var seg = GameManager.Instance.CurrentSegment;
        if (seg == null) return;

        if (seg.isStation && !atStation) EnterStation(seg);
        else if (!seg.isStation && atStation) ExitStation(seg);
    }

    void EnterStation(SegmentData seg)
    {
        atStation = true;
        train?.Pause();
        OnStationEntered?.Invoke(seg);
    }

    void ExitStation(SegmentData seg)
    {
        atStation = false;
        train?.Resume();
        OnStationExited?.Invoke(seg);
    }

    /// <summary>Resume the train manually (e.g. from a "Continue" button).</summary>
    public void LeaveStation()
    {
        if (!atStation) return;
        ExitStation(GameManager.Instance.CurrentSegment);
    }
}
