using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float _acceleration = 1f;
    [SerializeField] private float _maxSpeed = 3f;
    [SerializeField] private float _startRunningSpeed = 0.3f;
    [SerializeField] private float _linearDrag = 5f;
    private Vector2 _moveInput;

    [Header("Vertical Movement")]
    [SerializeField] private int _extraJumpCount = 2;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravity = 1;
    [SerializeField] private float _fallMultiplier = 5f;
    [SerializeField] private float _linerDragMultiplier = 0.3f;
    [SerializeField] private float _coyoteTimerVal = 0.2f;

    private int _extraJumps;
    private float _coyoteTimer;
    private float? _jumpBuffer;
   
    private int _whichInteraction; //0 == Tap, 1 == Hold
    
    [Header("Collision Detection")]
    [SerializeField] private float _feetCheckRadius;
    private bool _isGrounded;

    [Header("Components")]
    [SerializeField] Transform _feetpos;
    [SerializeField] LayerMask _groundLayerMask;
    private Rigidbody2D _myRigidBody;

    private void Awake() 
    {
        _myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void Start() 
    {
        _extraJumps = _extraJumpCount;    
    }

    private void FixedUpdate() 
    {
        Run(_moveInput.x);
        ModifyPhysics();
    }
    private void Update() 
    {
        _isGrounded = Physics2D.OverlapCircle(_feetpos.position,_feetCheckRadius,_groundLayerMask);
//        Debug.Log("Grounded: " + _isGrounded + " | Extra Jumps: " + _extraJumps);
        if (_isGrounded)
        {
            //_extraJumps = _extraJumpCount;          
            _coyoteTimer = _coyoteTimerVal;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }

        
    }
 
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }
    private void Run(float horizontalMovement)
    {
        _myRigidBody.AddForce(Vector2.right * horizontalMovement * _acceleration, ForceMode2D.Force);
        FlipCharacter();

        if (Mathf.Abs(_myRigidBody.velocity.x) > _maxSpeed)
        {
            RunConstantlyAtMaxSpeed(_myRigidBody.velocity.x);
        }
    }
    private void RunConstantlyAtMaxSpeed(float rigidBodyVelocityX)
    {
         _myRigidBody.velocity = new Vector2 (_maxSpeed * Mathf.Sign(rigidBodyVelocityX),_myRigidBody.velocity.y);
    }
    private void FlipCharacter()
    {
        if (_moveInput.x != 0)
        {
            transform.localScale = new Vector2 (Mathf.Sign(_moveInput.x),1);
        }
    } 
    private void AddLinearDrag()
    {
        if (Mathf.Abs(_moveInput.x) < _startRunningSpeed || IsChangingDirections()) 
        {
            _myRigidBody.drag = _linearDrag;
        }
        else
        {
            _myRigidBody.drag = 0f;
        }
    }
    private bool IsChangingDirections()
    {
        return (_moveInput.x > 0 && _myRigidBody.velocity.x < 0) || (_moveInput.x < 0 && _myRigidBody.velocity.x > 0); 
    }
    private void FallWithIncreasedGravity()
    {
        _myRigidBody.gravityScale = _gravity * _fallMultiplier;
    }
    private void PerformShortJump()
    {
        _myRigidBody.gravityScale = _gravity * (_fallMultiplier/2);
    }
    private void ModifyPhysics()
    {
        if (_isGrounded)
        {
            AddLinearDrag();
            _myRigidBody.gravityScale = 0;
        }
        else
        {
            _myRigidBody.gravityScale = _gravity;
            _myRigidBody.drag = _linearDrag * _linerDragMultiplier;
            if (_myRigidBody.velocity.y < 0)
            {
                FallWithIncreasedGravity();
            }
            else if (_myRigidBody.velocity.y > 0 && _whichInteraction == 0)
            {
                PerformShortJump();
            }
        }

    }
    
    public void GetJumpInput(InputAction.CallbackContext context)
    {
        
        if (context.started)
        {
            _jumpBuffer = Time.time;
        }

        if (_coyoteTimer > 0f && (Time.time - _jumpBuffer >= 0) && context.performed && _extraJumps > 0)
        {
            Debug.Log("First Press: " + _extraJumps);
            _extraJumps--;
            if(context.interaction is TapInteraction)
            {
                //_myRigidBody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _myRigidBody.velocity = Vector2.up * _jumpForce;
                _whichInteraction = 0;
            }
            else if (context.interaction is HoldInteraction)
            {
                //.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _myRigidBody.velocity = Vector2.up * _jumpForce;
                _whichInteraction = 1;
            }
            _jumpBuffer = null;
        }
        else if (_extraJumps > 0 && context.performed)
        {
            Debug.Log("Second Press: " + _extraJumps);
            if(context.interaction is TapInteraction)
            {
                //_myRigidBody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _myRigidBody.velocity = Vector2.up * _jumpForce;
                _whichInteraction = 0;
            }
 
            _extraJumps--;
        }
        else
        {
            _coyoteTimer = 0f;
        }
    }

}
