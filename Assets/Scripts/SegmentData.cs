using UnityEngine;

/// <summary>
/// Data describing a single ride segment. Create via:
///   Assets ▸ Create ▸ TrainRide ▸ Segment Data
///
/// Each segment corresponds to a scene that will be loaded additively.
/// `sceneName` must match a scene added to Build Settings.
/// </summary>
[CreateAssetMenu(menuName = "TrainRide/Segment Data", fileName = "SegmentData")]
public class SegmentData : ScriptableObject
{
    [Tooltip("Display name, shown in UI.")]
    public string displayName;

    [TextArea(2, 6)]
    public string description;

    [Tooltip("Scene name as registered in Build Settings. Loaded additively by SegmentManager.")]
    public string sceneName;

    [Tooltip("If true, the train pauses when this segment becomes active (station-type segment).")]
    public bool isStation;
}
