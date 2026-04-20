using UnityEngine;

/// <summary>
/// Placed along the track. When the train (or a tagged child) passes through,
/// tells the GameManager to advance to the next segment.
///
/// Setup:
///   - Add to a GameObject with a Collider set as "Is Trigger".
///   - Place it at the transition point between two segments.
///   - Ensure the train has the tag configured in `trainTag` (default: "Player").
/// </summary>
[RequireComponent(typeof(Collider))]
public class SegmentTrigger : MonoBehaviour
{
    [Tooltip("Tag used to identify the train/player. The trigger ignores everything else.")]
    public string trainTag = "Player";

    [Tooltip("Fires only once — prevents re-triggering if the train backs up.")]
    public bool oneShot = true;

    [Tooltip("Optional: advance to a specific segment index. -1 = advance by one.")]
    public int targetSegmentIndex = -1;

    private bool fired;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (fired && oneShot) return;
        if (!other.CompareTag(trainTag)) return;

        var gm = GameManager.Instance;
        if (gm == null) return;

        if (targetSegmentIndex >= 0) gm.SetSegment(targetSegmentIndex);
        else gm.AdvanceSegment();

        fired = true;
    }
}
