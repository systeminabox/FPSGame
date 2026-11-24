using Unity.Cinemachine;
using UnityEngine;

public class ShipController : VehicleController
{
    
    
    private float _currentThrottle = 0.0f;
    [SerializeField] private float maxThrottle = 1.0f;
    [SerializeField] private float throttleIncrement = 0.1f;
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float turnSpeed = 10.0f;
    [SerializeField] private float LookRotationSpeed = 10.0f;
    
    
    private const float _threshold = 0.01f;
    private float _cinemachineTargetPitch;
    private float _speed;
    private float _rotationVelocity;
    
    public void TakeControl()
    {
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _currentThrottle = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        Look(InputManager.Instance.ShipLookInput);
        HandleTurn(InputManager.Instance.ShipMoveInput);
        if(InputManager.ShipThrottleUpInput)HandleThrottleUp();
        if(InputManager.ShipThrottleDownInput)HandleThrottleDown();
        if(InputManager.ShipCutThrottleInput)CutThrottle();
        if(InputManager.ShipMaxThrottleInput)MaxThrottle();
        Thrust();
    }

    private void Thrust()
    {
        _speed = moveSpeed * Time.deltaTime * _currentThrottle;
        CharacterController.Move(_speed * transform.forward);
    }

    protected override void Look(Vector2 lookInput)
    {
        if (InputManager.Instance.ShipLookInput.sqrMagnitude >= _threshold)
        {
            // Mouse delta is already per-frame; stick input needs deltaTime
            float deltaTimeMultiplier = InputManager.Instance.LookIsFromMouse ? 1.0f : Time.deltaTime;

            //_cinemachineTargetPitch += InputManager.Instance.ShipLookInput.x * LookRotationSpeed * deltaTimeMultiplier;
            _rotationVelocity = InputManager.Instance.ShipLookInput.x * LookRotationSpeed * deltaTimeMultiplier * LookSensitivityInv;

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            //CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            CinemachineCameraTarget.transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void HandleTurn(Vector2 moveInput)
    {
        transform.Rotate(Vector3.up, moveInput.x * turnSpeed * Time.deltaTime);
    }

    private void HandleThrottleUp()
    {
        if (_currentThrottle < maxThrottle)
        {
            _currentThrottle += throttleIncrement;
        }
        else
        {
            _currentThrottle = maxThrottle;
        }
        Debug.Log(_currentThrottle);
    }
    
    private void HandleThrottleDown()
    {
        if (_currentThrottle > 0.0f)
        {
            _currentThrottle -= throttleIncrement;
        }
        else
        {
            _currentThrottle = 0.0f;
        }
        Debug.Log(_currentThrottle);
    }
    
    private void CutThrottle()
    {
        _currentThrottle = 0.0f;
        Debug.Log("Cutting throttle");
    }

    private void MaxThrottle()
    {
        _currentThrottle = maxThrottle;  
        Debug.Log("Maxing throttle");
    }
}
