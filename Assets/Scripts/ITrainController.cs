/// <summary>
/// Minimal contract for whatever drives the train forward.
/// Implement this on your existing train movement script so
/// StationController can pause/resume without tight coupling.
///
/// Example:
///   public class TrainMover : MonoBehaviour, ITrainController {
///       public bool IsMoving { get; private set; } = true;
///       public void Pause()  => IsMoving = false;
///       public void Resume() => IsMoving = true;
///   }
/// </summary>
public interface ITrainController
{
    bool IsMoving { get; }
    void Pause();
    void Resume();
}
