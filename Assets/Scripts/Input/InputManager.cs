using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Project-agnostic input hub built on PlayerInput.
///
/// - One always-on "global" action map (menu, console, ...).
/// - Any number of "control modes" (Player, Ship, Vehicle, ...), each bound to one action map.
///   Only the current mode's map is enabled, so the other modes read as zero/false automatically.
/// - Everything project-specific (map names, modes, start mode) is serialized.
/// - Consumers either call the generic readers (IsPressed, ReadVector2, ...) with an action path
///   such as "Player/Move", or use a per-project partial class (see InputManager.Crawler.cs)
///   that exposes friendly properties on top of these readers.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public partial class InputManager : Singleton<InputManager>
{
    [Serializable]
    public class ControlMode
    {
        [Tooltip("Name used with SetMode().")]
        public string id = "Player";

        [Tooltip("Action map that is enabled while this mode is active.")]
        public string actionMap = "Player";

        [Tooltip("Lock and hide the cursor while this mode is active.")]
        public bool lockCursor = true;
    }

    [Header("Action Maps")]
    [Tooltip("Map that stays enabled in every mode (pause, console, ...). Leave empty if unused.")]
    [SerializeField] private string globalActionMap = "Global";

    [Header("Control Modes")]
    [SerializeField] private List<ControlMode> modes = new List<ControlMode>
    {
        new ControlMode { id = "Player", actionMap = "Player", lockCursor = true },
        new ControlMode { id = "Ship",   actionMap = "Ship",   lockCursor = true },
    };

    [Tooltip("Mode entered on Start. Leave empty to start with no mode active.")]
    [SerializeField] private string startingMode = "Player";

    [Tooltip("Frames the new mode's map stays disabled after a switch. Stops the button that caused " +
             "the switch from also being read by the new mode in the same press. 0 = no lockout.")]
    [SerializeField, Min(0)] private int switchLockoutFrames = 1;

    [Header("Control Scheme")]
    [Tooltip("Name of the PlayerInput control scheme that uses the mouse.")]
    [SerializeField] private string mouseSchemeName = "KeyboardMouse";

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    public PlayerInput PlayerInput => _playerInput;

    // Lets the controller decide whether to scale look by deltaTime
    public bool LookIsFromMouse => _playerInput.currentControlScheme == mouseSchemeName;

    /// <summary>Id of the active mode, or an empty string when no mode is active.</summary>
    public string CurrentModeId => _currentMode != null ? _currentMode.id : string.Empty;

    /// <summary>Raised on every mode change as (previousId, newId).</summary>
    public event Action<string, string> ModeChanged;

    private PlayerInput _playerInput;
    private InputSystemUIInputModule _uiModule;

    private ControlMode _currentMode;
    private bool _inputEnabled = true;
    private bool _allowGlobal = true;
    private int _lockoutUntilFrame = -1; // -1 = no lockout active

    private readonly Dictionary<string, InputAction> _actionCache = new Dictionary<string, InputAction>();
    private readonly HashSet<string> _warned = new HashSet<string>();

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
    }

    // Start (not Awake) so PlayerInput has already run its own OnEnable and enabled its default map;
    // we then take over and enable exactly what we want.
    private void Start()
    {
        _currentMode = FindMode(startingMode);
        if (!string.IsNullOrEmpty(startingMode) && _currentMode == null)
            Debug.LogError($"InputManager: starting mode '{startingMode}' is not in the modes list.", this);

        if (_currentMode != null)
        {
            cursorLocked = _currentMode.lockCursor;
            SetCursorState(cursorLocked);
        }

        ApplyActionMaps();
    }

    private void Update()
    {
        if (_lockoutUntilFrame >= 0 && Time.frameCount >= _lockoutUntilFrame)
        {
            _lockoutUntilFrame = -1;
            ApplyActionMaps();
        }
    }

    // ------------------------------------------------------------------
    // Modes
    // ------------------------------------------------------------------

    public bool IsMode(string id) => CurrentModeId == id;

    /// <summary>
    /// Switch control mode. Pass an empty string for "no mode" (only the global map stays live).
    /// immediate skips the switch lockout.
    /// </summary>
    public bool SetMode(string id, bool immediate = false)
    {
        ControlMode next = null;
        if (!string.IsNullOrEmpty(id))
        {
            next = FindMode(id);
            if (next == null)
            {
                Debug.LogError($"InputManager: unknown control mode '{id}'.", this);
                return false;
            }
        }

        if (next == _currentMode)
            return true;

        string previous = CurrentModeId;
        _currentMode = next;

        _lockoutUntilFrame = (immediate || switchLockoutFrames <= 0)
            ? -1
            : Time.frameCount + switchLockoutFrames;

        ApplyActionMaps();

        if (next != null)
        {
            cursorLocked = next.lockCursor;
            SetCursorState(cursorLocked);
        }

        ModeChanged?.Invoke(previous, CurrentModeId);
        return true;
    }

    private ControlMode FindMode(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        foreach (var mode in modes)
            if (mode.id == id) return mode;
        return null;
    }

    // ------------------------------------------------------------------
    // Enable / disable
    // ------------------------------------------------------------------

    /// <summary>
    /// Turn gameplay input on or off (menus, dialogue, cutscenes, camera blends).
    /// While disabled, the current mode's map is off. The global map stays on if allowGlobal is true
    /// so the player can still close a menu.
    /// </summary>
    public void SetInputEnabled(bool enabled, bool allowGlobal = true)
    {
        _inputEnabled = enabled;
        _allowGlobal = allowGlobal;

        if (_uiModule == null)
            _uiModule = FindFirstObjectByType<InputSystemUIInputModule>(FindObjectsInactive.Include);

        if (_uiModule != null)
            _uiModule.enabled = enabled;

        ApplyActionMaps();
    }

    private void ApplyActionMaps()
    {
        if (_playerInput == null || _playerInput.actions == null)
            return;

        bool globalOn = _inputEnabled || _allowGlobal;
        bool modeOn = _inputEnabled && _lockoutUntilFrame < 0;

        var wanted = new HashSet<string>();
        if (globalOn && !string.IsNullOrEmpty(globalActionMap))
            wanted.Add(globalActionMap);
        if (modeOn && _currentMode != null)
            wanted.Add(_currentMode.actionMap);

        if (!string.IsNullOrEmpty(globalActionMap))
            SetMapEnabled(globalActionMap, wanted.Contains(globalActionMap));

        foreach (var mode in modes)
            SetMapEnabled(mode.actionMap, wanted.Contains(mode.actionMap));
    }

    private void SetMapEnabled(string mapName, bool enable)
    {
        if (string.IsNullOrEmpty(mapName))
            return;

        var map = _playerInput.actions.FindActionMap(mapName, false);
        if (map == null)
        {
            WarnOnce($"map:{mapName}", $"InputManager: action map '{mapName}' not found in the Input Actions asset.");
            return;
        }

        // Only touch maps whose state actually changes, so redundant calls never cancel held inputs.
        if (map.enabled == enable)
            return;

        if (enable) map.Enable();
        else map.Disable();
    }

    // ------------------------------------------------------------------
    // Generic readers. Use "Map/Action" paths ("Player/Move") to avoid name clashes between maps.
    // Actions in disabled maps return false / zero.
    // ------------------------------------------------------------------

    public InputAction GetAction(string actionPath)
    {
        if (string.IsNullOrEmpty(actionPath))
            return null;

        if (_actionCache.TryGetValue(actionPath, out var cached))
            return cached;

        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();

        var action = _playerInput.actions.FindAction(actionPath, false);
        if (action == null)
        {
            WarnOnce($"action:{actionPath}", $"InputManager: action '{actionPath}' not found in the Input Actions asset.");
            return null;
        }

        _actionCache[actionPath] = action;
        return action;
    }

    public bool IsPressed(string actionPath) => GetAction(actionPath)?.IsPressed() ?? false;
    public bool WasPressedThisFrame(string actionPath) => GetAction(actionPath)?.WasPressedThisFrame() ?? false;
    public bool WasReleasedThisFrame(string actionPath) => GetAction(actionPath)?.WasReleasedThisFrame() ?? false;
    public bool WasPerformedThisFrame(string actionPath) => GetAction(actionPath)?.WasPerformedThisFrame() ?? false;

    public Vector2 ReadVector2(string actionPath) => GetAction(actionPath)?.ReadValue<Vector2>() ?? Vector2.zero;
    public float ReadFloat(string actionPath) => GetAction(actionPath)?.ReadValue<float>() ?? 0f;

    private void WarnOnce(string key, string message)
    {
        if (_warned.Add(key))
            Debug.LogWarning(message, this);
    }

    // ------------------------------------------------------------------
    // Cursor
    // ------------------------------------------------------------------

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    public static void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}