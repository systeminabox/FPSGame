using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[DefaultExecutionOrder(-100)] // runs before gameplay scripts so they never read last frame's input
public class InputManager : Singleton<InputManager>
{
    public bool MenuOpenClosedInput { get; private set; }

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

    public bool JumpJustPressed { get; private set; }
    public bool JumpBeingHeld { get; private set; }
    public bool JumpReleased { get; private set; }

    public bool SprintInput { get; private set; }   // held
    public bool DashInput { get; private set; }     // pressed this frame
    public bool InteractInput { get; private set; }
    public bool PrimaryFireInput { get; private set; }        // held (full-auto)
    public bool PrimaryFireJustPressed { get; private set; }  // semi-auto
    public bool SecondaryFireInput { get; private set; }
    public bool DebugInput { get; private set; }

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    public PlayerInput PlayerInput => _playerInput;

    // Lets the controller decide whether to scale look by deltaTime
    public bool LookIsFromMouse => _playerInput.currentControlScheme == "KeyboardMouse";

    private PlayerInput _playerInput;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _dashAction;
    private InputAction _interactAction;
    private InputAction _primaryFireAction;
    private InputAction _secondaryFireAction;
    private InputAction _menuOpenCloseAction;
    private InputAction _debugAction;

    private InputSystemUIInputModule _uiModule;
    private bool _inputEnabled = true;
    private bool _allowPause = true;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        SetupInputActions();
    }

    private void SetupInputActions()
    {
        _moveAction = _playerInput.actions["Move"];
        _lookAction = _playerInput.actions["Look"];
        _jumpAction = _playerInput.actions["Jump"];
        _sprintAction = _playerInput.actions["Sprint"];
        _dashAction = _playerInput.actions["Roll"];
        _interactAction = _playerInput.actions["Interact"];
        _primaryFireAction = _playerInput.actions["Primary"];
        _secondaryFireAction = _playerInput.actions["Secondary"];
        _menuOpenCloseAction = _playerInput.actions["MenuOpenClose"];
        _debugAction = _playerInput.actions["ToggleConsole"];
    }

    public void SetInputEnabled(bool enabled, bool allowPause = true)
    {
        _inputEnabled = enabled;
        _allowPause = allowPause;

        if (_uiModule == null)
            _uiModule = FindFirstObjectByType<InputSystemUIInputModule>(FindObjectsInactive.Include);

        if (_uiModule != null)
            _uiModule.enabled = enabled;

        if (!enabled)
            ClearGameplayInputs();
    }

    private void Update()
    {
        UpdateInputs();
    }

    private void UpdateInputs()
    {
        MenuOpenClosedInput = _allowPause && _menuOpenCloseAction.WasPerformedThisFrame();

        if (!_inputEnabled)
        {
            ClearGameplayInputs();
            return;
        }

        MoveInput = _moveAction.ReadValue<Vector2>();
        LookInput = cursorInputForLook ? _lookAction.ReadValue<Vector2>() : Vector2.zero;
        
        Debug.Log($"Move: {MoveInput}, Look: {LookInput}");

        JumpJustPressed = _jumpAction.WasPressedThisFrame();
        JumpBeingHeld = _jumpAction.IsPressed();
        JumpReleased = _jumpAction.WasReleasedThisFrame();

        SprintInput = _sprintAction.IsPressed();
        DashInput = _dashAction.WasPressedThisFrame();
        InteractInput = _interactAction.WasPressedThisFrame();
        PrimaryFireInput = _primaryFireAction.IsPressed();
        PrimaryFireJustPressed = _primaryFireAction.WasPressedThisFrame();
        SecondaryFireInput = _secondaryFireAction.IsPressed();
        DebugInput = _debugAction.WasPerformedThisFrame();
    }

    private void ClearGameplayInputs()
    {
        MoveInput = Vector2.zero;
        LookInput = Vector2.zero;
        JumpJustPressed = false;
        JumpBeingHeld = false;
        JumpReleased = false;
        SprintInput = false;
        DashInput = false;
        InteractInput = false;
        PrimaryFireInput = false;
        PrimaryFireJustPressed = false;
        SecondaryFireInput = false;
        DebugInput = false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}