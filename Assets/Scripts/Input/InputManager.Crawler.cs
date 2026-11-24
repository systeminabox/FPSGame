using UnityEngine;

/// <summary>
/// Crawler-specific layer on top of the reusable InputManager core.
/// Keeps the old property names so existing scripts keep compiling.
/// For a new project, copy InputManager.cs and write a new version of this file (or skip it
/// and call the generic readers directly).
///
/// Expected Input Actions layout:
///   Global : MenuOpenClose, ToggleConsole
///   Player : Move, Look, Jump, Sprint, Roll, Interact, Primary, Secondary, Inventory
///   Ship   : (add its actions here, then expose them below in the same pattern)
/// </summary>
public partial class InputManager
{
    // Mode ids (must match the ids in the Control Modes list in the inspector)
    public const string ModePlayer = "Player";
    public const string ModeShip = "Ship";

    // Global map
    public bool MenuOpenClosedInput => WasPerformedThisFrame("Global/MenuOpenClose");
    public bool DebugInput => WasPerformedThisFrame("Global/ToggleConsole");

    // Player map
    public Vector2 MoveInput => ReadVector2("Player/Move");
    public Vector2 LookInput => cursorInputForLook ? ReadVector2("Player/Look") : Vector2.zero;

    public bool JumpJustPressed => WasPressedThisFrame("Player/Jump");
    public bool JumpBeingHeld => IsPressed("Player/Jump");
    public bool JumpReleased => WasReleasedThisFrame("Player/Jump");

    public bool SprintInput => IsPressed("Player/Sprint");              // held
    public bool DashInput => WasPressedThisFrame("Player/Roll");        // pressed this frame
    public bool InteractInput => WasPressedThisFrame("Player/Interact");

    public bool PrimaryFireInput => IsPressed("Player/Primary");                    // held (full-auto)
    public bool PrimaryFireJustPressed => WasPressedThisFrame("Player/Primary");    // semi-auto
    public bool PrimaryFireReleased => WasReleasedThisFrame("Player/Primary");

    public bool SecondaryFireInput => IsPressed("Player/Secondary");
    public bool InventoryInput => WasPerformedThisFrame("Player/Inventory");
    
    //Ship map
    
    public Vector2 ShipMoveInput => ReadVector2("Ship/Move");
    public Vector2 ShipLookInput => cursorInputForLook ? ReadVector2("Ship/Look") : Vector2.zero;
    public bool ShipThrottleUpInput => WasPressedThisFrame("Ship/ThrottleUp");
    public bool ShipThrottleDownInput => WasPressedThisFrame("Ship/ThrottleDown");
    public bool ShipCutThrottleInput => WasPressedThisFrame("Ship/CutThrottle");
    public bool ShipMaxThrottleInput => WasPressedThisFrame("Ship/MaxThrottle");
    
    
    public void SetPlayerCanMove(bool canMove)
    {
        GameManager.Instance.SetPlayerCanMove(canMove);
    }
}