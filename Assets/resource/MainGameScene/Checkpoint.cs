using UnityEngine;

/// <summary>
/// Attach to a GameObject with a trigger Collider on each checkpoint around the track.
/// Set checkpointIndex in order: 0, 1, 2, ...
/// The LAST checkpoint (highest index) should also have isFinishLine = true.
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Tooltip("Order of this checkpoint (0, 1, 2, ...). Must match RaceManager.totalCheckpoints.")]
    public int checkpointIndex;

    [Tooltip("True only for the final checkpoint / finish line.")]
    public bool isFinishLine = false;

    void OnDrawGizmos()
    {
        // visualize checkpoints in Scene view
        Gizmos.color = isFinishLine ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f,
            isFinishLine ? $"FINISH ({checkpointIndex})" : $"CP {checkpointIndex}");
#endif
    }
}
