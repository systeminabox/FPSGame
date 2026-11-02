using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Door-specific layer on top of Interactable.
/// Put this on the door FRAME / ROOT, and put the Interactable on the MOVING door leaf (a child).
/// If both sit on the same moving object, the auto-open trigger volume will move with the door.
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Leave empty to auto-find an Interactable in this object's children.")]
    [SerializeField] private Interactable interactable;

    [Header("Auto Open / Close")]
    public bool autoOpen = false;
    public bool autoClose = false;
    [Tooltip("Width/depth of the proximity trigger. Only used when this script creates the trigger itself.")]
    public float autoOpenDistance = 4f;
    public float triggerHeight = 2.5f;

    [Header("Event Callbacks")]
    public UnityEvent<DoorController> onDoorOpenStarted = new UnityEvent<DoorController>();
    public UnityEvent<DoorController> onDoorOpenEnded = new UnityEvent<DoorController>();
    public UnityEvent<DoorController> onDoorCloseStarted = new UnityEvent<DoorController>();
    public UnityEvent<DoorController> onDoorCloseEnded = new UnityEvent<DoorController>();

    private PlayerDetector _playerDetector;

    // True from the moment the door starts opening until it has fully finished closing
    private bool _isOpen;

    // If a request arrives mid-animation, remember the latest one and apply it when the animation ends
    private bool _hasPendingRequest;
    private bool _pendingOpen;

    public bool IsOpen => _isOpen;
    public bool IsMoving => interactable != null && interactable.IsAnimating;


    private void Awake()
    {
        if (interactable == null) interactable = GetComponentInChildren<Interactable>();

        if (interactable == null)
        {
            Debug.LogError($"DoorController on '{name}' could not find an Interactable. Disabling.", this);
            enabled = false;
            return;
        }

        if (autoOpen || autoClose) SetupPlayerDetector();
    }

    private void Start()
    {
        // Read the starting state after every Awake has run (Interactable may apply its defaultState in Awake)
        if (interactable != null) _isOpen = interactable.IsActivated;
    }

    private void OnEnable()
    {
        if (interactable != null)
        {
            interactable.onAnimationStartEvent.AddListener(OnAnimationStart);
            interactable.onAnimationEndEvent.AddListener(OnAnimationEnd);
        }

        if (_playerDetector != null)
        {
            _playerDetector.OnPlayerEnter.AddListener(OnPlayerEnter);
            _playerDetector.OnPlayerExit.AddListener(OnPlayerExit);
        }
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.onAnimationStartEvent.RemoveListener(OnAnimationStart);
            interactable.onAnimationEndEvent.RemoveListener(OnAnimationEnd);
        }

        if (_playerDetector != null)
        {
            _playerDetector.OnPlayerEnter.RemoveListener(OnPlayerEnter);
            _playerDetector.OnPlayerExit.RemoveListener(OnPlayerExit);
        }
    }


    // ---------- Proximity detection ----------

    private void SetupPlayerDetector()
    {
        // Allow a designer-placed detector (custom shape, custom position) to take priority
        _playerDetector = GetComponentInChildren<PlayerDetector>();
        if (_playerDetector != null) return;

        GameObject triggerGO = new GameObject("DoorProximityTrigger");
        triggerGO.transform.SetParent(transform, false);
        triggerGO.layer = LayerMask.NameToLayer("Ignore Raycast"); // keep it out of interaction raycasts

        BoxCollider box = triggerGO.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(0f, triggerHeight * 0.5f, 0f);
        box.size = new Vector3(autoOpenDistance, triggerHeight, autoOpenDistance);

        _playerDetector = triggerGO.AddComponent<PlayerDetector>();
    }

    private void OnPlayerEnter() { if (autoOpen) RequestState(true); }
    private void OnPlayerExit() { if (autoClose) RequestState(false); }


    // ---------- Interactable callbacks ----------

    private void OnAnimationStart()
    {
        if (interactable.IsActivated)
        {
            _isOpen = true;
            onDoorOpenStarted.Invoke(this);
        }
        else
        {
            onDoorCloseStarted.Invoke(this);
        }
    }

    private void OnAnimationEnd()
    {
        if (interactable.IsActivated)
        {
            onDoorOpenEnded.Invoke(this);
        }
        else
        {
            _isOpen = false;
            onDoorCloseEnded.Invoke(this);
        }

        // Something asked for a different state while we were moving
        if (_hasPendingRequest)
        {
            bool open = _pendingOpen;
            _hasPendingRequest = false;
            RequestState(open);
        }
    }


    // ---------- State requests ----------

    // Ask for a state. Applies immediately if possible, otherwise queues the latest request.
    private void RequestState(bool open)
    {
        if (interactable == null) return;

        if (interactable.IsActivated == open)
        {
            _hasPendingRequest = false; // already heading to / at the requested state
            return;
        }

        if (interactable.CanInteract)
        {
            _hasPendingRequest = false;
            interactable.Interact();
        }
        else
        {
            _hasPendingRequest = true;
            _pendingOpen = open;
        }
    }

    // Public API. These are parameterless so they show up in UnityEvent dropdowns.
    public void OpenDoor() { RequestState(true); }
    public void CloseDoor() { RequestState(false); }

    // Interrupts any animation in progress and goes straight to the state
    public void OpenDoorForced() { if (interactable != null) interactable.InteractForceToState(true); }
    public void CloseDoorForced() { if (interactable != null) interactable.InteractForceToState(false); }

    // Single float parameter, so these also work from the inspector
    public void OpenDoorWithDelay(float delay) { StartCoroutine(DelayedRequest(true, delay)); }
    public void CloseDoorWithDelay(float delay) { StartCoroutine(DelayedRequest(false, delay)); }

    private IEnumerator DelayedRequest(bool open, float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, delay));
        RequestState(open);
    }
}