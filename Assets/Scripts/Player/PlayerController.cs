using Unity.Cinemachine;
using UnityEngine;


    
    public class PlayerController : Controller
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the character can jump")]
        public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;
        [Tooltip("How long a jump press is remembered before landing")]
        public float JumpBufferTime = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;


        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = -53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _jumpBufferDelta;


        protected override void Awake()
        {
            base.Awake();
            Debug.Log(GameManager.Instance);
            GameManager.Instance.RegisterPlayer(gameObject);
        }

        private const float _threshold = 0.01f;

        private void Start()
        {
            
            

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            if (!InputEnabled) return;
            Move(InputManager.MoveInput);
        }

        private void LateUpdate()
        {
            if (!InputEnabled) return;
            Look(InputManager.LookInput);
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        }

        protected override void Look(Vector2 lookInput)
        {
         
            if (lookInput.sqrMagnitude >= _threshold)
            {
                // Mouse delta is already per-frame; stick input needs deltaTime
                float deltaTimeMultiplier = InputManager.LookIsFromMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += lookInput.y * RotationSpeed * deltaTimeMultiplier * LookSensitivityInv;
                _rotationVelocity = lookInput.x * RotationSpeed * deltaTimeMultiplier * LookSensitivityInv;

                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        

        

        protected override void Move(Vector2 moveInput)
        {
            float targetSpeed = InputManager.SprintInput ? SprintSpeed : MoveSpeed;

            if (moveInput == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(CharacterController.velocity.x, 0.0f, CharacterController.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = InputManager.analogMovement ? moveInput.magnitude : 1f;

            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            Vector3 inputDirection = Vector3.zero;

            if (moveInput != Vector2.zero)
            {
                inputDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            }

            CharacterController.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (InputManager.JumpBeingHeld && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
            }

// apply gravity over time if above terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity > _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            Gizmos.color = Grounded ? transparentGreen : transparentRed;

            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
        }
        
        //in here only for Crawler
        #if UNITY_EDITOR   
        public void WarpToPosition(Vector3 position, Vector3 CameraRotation)
        {
            transform.position = position;
            transform.rotation = Quaternion.Euler(CameraRotation);
        }
        #endif
    }
