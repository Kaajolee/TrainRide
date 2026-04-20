/// <summary>
/// Anything the player can click on via the InteractionSystem.
/// Attach a component implementing this to a GameObject with a Collider.
///
/// Example:
///   public class InfoHotspot : MonoBehaviour, IInteractable {
///       public string Label => "Project details";
///       public void Interact() => Debug.Log("Show project details UI");
///   }
/// </summary>
public interface IInteractable
{
    /// <summary>Short label shown in the UI when the hotspot is hovered/targeted.</summary>
    string Label { get; }

    /// <summary>Invoked on click.</summary>
    void Interact();
}
