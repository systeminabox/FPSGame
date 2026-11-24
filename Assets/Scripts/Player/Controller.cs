using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Controller : MonoBehaviour
{
    
    

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("The Cinemachine Virtual Camera that follows this controller")]
    public CinemachineCamera CinemachineCamera;
    
    
    
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -90.0f;
    [Tooltip("How sensitive the camera is to mouse input. Internally inverted 1/value")]
    public float LookSensitivity = 1.0f;
    protected float LookSensitivityInv => 1.0f / LookSensitivity;
    
    protected bool InputEnabled = true;
    protected CharacterController CharacterController;
    
    protected InputManager InputManager;
    
    protected static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    
    protected virtual void Awake()
    {
        CharacterController = GetComponent<CharacterController>();
        InputManager = InputManager.Instance;
    }
    
    public void SetCanMove(bool canMove)
    {
        InputEnabled = canMove;
    }
    
    protected virtual void Update()
    {
        Move(InputManager.MoveInput);
        Look(InputManager.LookInput);
    }
    
    protected virtual void Move(Vector2 moveInput)
    {
        Debug.LogError("Move() called with no override", gameObject);
    }
    
    protected virtual void Look(Vector2 lookInput)
    {
        Debug.LogError("Look() called with no override", gameObject);
    }
    
    
}
