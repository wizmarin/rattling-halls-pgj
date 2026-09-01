using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    // Components variables
    PlayerInput _playerInput;
    CharacterController _characterController;
    Animator _animator;

    // Hash variables
    int _isWalkingHash;
    int _isRunningHash;

    // Movement variables
    private Vector2 _inputDirection;
    private Vector3 _characterVector;
    private Vector3 _worldVector;
    
    // State variables
    private bool _isMovementPressed;
    private bool _isRunToggled;
    
    // Speed variables
    private readonly float _walkMultiplier = 3.0f;
    private readonly float _runMultiplier = 6.0f;
    
    // Rotation variables
    private readonly float _smoothTime = 0.4f;
    private float _currentVelocity;
    
    private void Awake()
    {
        // Initialize components
        _playerInput = new PlayerInput();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        
        // Initialize hash variables
        _isWalkingHash = Animator.StringToHash("IsWalking");
        _isRunningHash = Animator.StringToHash("IsRunning");
        
        // Register Move callbacks
        _playerInput.Player.Move.started += OnMovementInput;
        _playerInput.Player.Move.performed += OnMovementInput;
        _playerInput.Player.Move.canceled += OnMovementInput;

        // Register Run callbacks
        _playerInput.Player.RunToggle.started += OnRunInput;
        _playerInput.Player.RunToggle.performed += OnRunInput;
        _playerInput.Player.RunToggle.canceled += OnRunInput;
    }
    
    void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleRotation();
        HandleAnimation();
    }

    void OnEnable()
    {
        _playerInput.Player.Enable();
    }
    
    void OnDisable()
    {
        _playerInput.Player.Disable();
    }
    
    void OnMovementInput(InputAction.CallbackContext context)
    {
        _inputDirection = (context.ReadValue<Vector2>());
        _characterVector = Vector3.right * _inputDirection.x + Vector3.forward * _inputDirection.y;
        _worldVector = transform.TransformDirection(_characterVector);
        _isMovementPressed = _inputDirection.x != 0 || _inputDirection.y != 0;
    }

    void OnRunInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Later add code that will show the UI element with the progress bar
            Debug.Log("Run toggle started");
        }
        else if (context.performed)
        {
            _isRunToggled = !_isRunToggled;
            Debug.Log("Run toggle performed: " + _isRunToggled);
        }
        else if (context.canceled)
        {
            // Later add code that will hide the UI element with the progress bar
            Debug.Log("Run toggle canceled");
        }
    }

    void HandleMovement()
    {
        if (_isRunToggled)
        {
            _characterController.Move(_worldVector * (_runMultiplier * Time.deltaTime));
        }
        else
        {
            _characterController.Move(_worldVector * (_walkMultiplier * Time.deltaTime));
        }
    }

    void HandleGravity()
    {
        if (_characterController.isGrounded)
        {
            float gravity = -.05f;
            _characterVector.y += gravity;
        }
        else
        {
            float gravity = -9.81f;
            _characterVector.y += gravity;
        }
    }

    void HandleRotation()
    {
        // Prevent the character from rotating back to the world forward if there is no input.
        if (_inputDirection.sqrMagnitude == 0) return;
        
        // Converts the world-space movement direction into a target yaw angle (degrees, around Y).
        float targetAngle = Mathf.Atan2(_worldVector.x, _worldVector.z) * Mathf.Rad2Deg;
        
        // Smoothly rotates towards the target angle.
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentVelocity, _smoothTime);
        
        // Sets the character's rotation to the target angle.'
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }


    void HandleAnimation()
    {
        bool isWalking = _animator.GetBool(_isWalkingHash);
        bool isRunning = _animator.GetBool(_isRunningHash);

        if (_isMovementPressed && !_isRunToggled && !isWalking)
        {
            _animator.SetBool(_isWalkingHash, true);
        }
        else if (!(_isMovementPressed && !_isRunToggled) && isWalking)
        {
            _animator.SetBool(_isWalkingHash, false);
        }

        if (_isMovementPressed && _isRunToggled && !isRunning)
        {
            _animator.SetBool(_isRunningHash, true);
        }
        else if (!(_isMovementPressed && _isRunToggled) && isRunning)
        {
            _animator.SetBool(_isRunningHash, false);
        }
    }
}
