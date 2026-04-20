using TMPro;
using UnityEngine;

/// <summary>
/// Binds gameplay events to on-screen text. Purely a view layer —
/// holds no state, only reacts to events from GameManager / InteractionSystem / StationController.
///
/// Setup:
///   - Add a Canvas with TMP texts for segment name, description, hover hint, station hint.
///   - Drag them into the slots below.
///   - Drop references to GameManager, InteractionSystem, StationController (or leave null to skip wiring).
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Targets")]
    public TMP_Text segmentNameText;
    public TMP_Text segmentDescriptionText;
    public TMP_Text hoverHintText;
    public TMP_Text stationHintText;

    [Header("Sources")]
    public InteractionSystem interaction;
    public StationController station;

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSegmentChanged += HandleSegmentChanged;

        if (interaction != null)
            interaction.OnHover += HandleHover;

        if (station != null)
        {
            station.OnStationEntered += HandleStationEntered;
            station.OnStationExited += HandleStationExited;
        }

        SetText(hoverHintText, string.Empty);
        SetText(stationHintText, string.Empty);
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSegmentChanged -= HandleSegmentChanged;

        if (interaction != null)
            interaction.OnHover -= HandleHover;

        if (station != null)
        {
            station.OnStationEntered -= HandleStationEntered;
            station.OnStationExited -= HandleStationExited;
        }
    }

    void HandleSegmentChanged(int prev, int next)
    {
        var seg = GameManager.Instance.CurrentSegment;
        SetText(segmentNameText, seg != null ? seg.displayName : string.Empty);
        SetText(segmentDescriptionText, seg != null ? seg.description : string.Empty);
    }

    void HandleHover(IInteractable target)
    {
        SetText(hoverHintText, target != null ? target.Label : string.Empty);
    }

    void HandleStationEntered(SegmentData seg) => SetText(stationHintText, $"Arrived at {seg.displayName}");
    void HandleStationExited(SegmentData seg) => SetText(stationHintText, string.Empty);

    static void SetText(TMP_Text t, string value)
    {
        if (t != null) t.text = value;
    }
}
