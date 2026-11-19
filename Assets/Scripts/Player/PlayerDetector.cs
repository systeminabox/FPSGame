using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sits on a trigger collider. Fires OnPlayerEnter when the first player enters
/// and OnPlayerExit when the last player leaves.
/// Requires a Rigidbody on the player (a CharacterController counts) for trigger events to fire.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerDetector : MonoBehaviour
{
    public string playerTag = "Player";

    public UnityEvent OnPlayerEnter = new UnityEvent();
    public UnityEvent OnPlayerExit = new UnityEvent();

    // Counts colliders inside, so a player with several colliders (or several players) doesn't double-fire
    private int _insideCount;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        _insideCount++;
        if (_insideCount == 1) OnPlayerEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        _insideCount = Mathf.Max(0, _insideCount - 1);
        if (_insideCount == 0) OnPlayerExit.Invoke();
    }
}