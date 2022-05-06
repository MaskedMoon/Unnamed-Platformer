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
    private Vector2 _moveInput;

    [Header("Vertical Movement")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravity = 1;
    [SerializeField] private float _fallMultiplier = 5f;
    [SerializeField] private float _linerDragMultiplier = 0.3f; 
    private int _whichInteraction; //0 == Tap, 1 == Hold
    
    [Header("Collision Detection")]
    [SerializeField] private float _feetCheckRadius;
    private bool _isGrounded;

    [Header("Physics Variables")]
    [SerializeField] private float _linearDrag = 5f;

    [Header("Components")]
    [SerializeField] Transform _feetpos;
    [SerializeField] LayerMask _groundLayerMask;
    private Rigidbody2D _myRigidBody;

    private void Awake() 
    {
        _myRigidBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() 
    {
        Run(_moveInput.x);
        ModifyPhysics();
    }
    private void Update() 
    {
        _isGrounded = Physics2D.OverlapCircle(_feetpos.position,_feetCheckRadius,_groundLayerMask);
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
        if (_isGrounded && context.performed)
        {
            if(context.interaction is TapInteraction)
            {
                _myRigidBody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _whichInteraction = 0;
            }
            else if (context.interaction is HoldInteraction)
            {
                _myRigidBody.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
                _whichInteraction = 1;
            }
        }
    }

}
