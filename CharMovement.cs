using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharMovement : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    private Transform _render;
    private PlayerVfxManager _vfx;
    private Rigidbody2D _rigidbody;
    private DrippyInput _input;
    private float _moveVector;
    private bool _isSprinting;
    private float _startTime;
    [HideInInspector]
    public bool canMove = true;
    [HideInInspector]
    public bool isGrounded = false;
    public float walkSpeed = 1.5f;
    public float runSpeed = 4.5f;
    public float speedUpTime = 0.5f;
    private bool justJumped = false;

    [Header("Jump Variables")]
    public float jumpForce = 3;
    public float jumpControlModifier = 0.5f;
    public float doubleJumpControlModifier = 0.5f;
    public float walkHorizontalMultiplyer = 1;
    public float runHorizontalMultiplyer = 1;
    public float airMoveMaxSpeed = 5f;
    public float landingPause = 0.2f;
    private bool canDoubleJump = false;
    private float airControlModifier;

    [Header("Ground Cast Variables")]
    public Vector2 groundBoxSize;
    public float groundCastDistance;
    public LayerMask groundLayer;

    [Header("Side Cast Variables")]
    public Vector2 sideBoxSize;
    public float sideCastDistance;


    void Awake()
    {
        _rigidbody = this.GetComponent<Rigidbody2D>();
        _render = transform.GetChild(0);
        _vfx = GetComponentInChildren<PlayerVfxManager>();
        _input = new DrippyInput();
        _input.gameplay.Jump.performed += OnJump;
    }

    void Update()
    {
        //ground check
        IsGrounded();

        _isSprinting = _input.gameplay.Sprint.IsPressed();

        //x-axis input
        if(_input.gameplay.Move.IsInProgress() && canMove)
        {
            _moveVector = _input.gameplay.Move.ReadValue<float>();

            if(isGrounded) OnMove(_moveVector);
            else OnAirMove(_moveVector);
        }
        else //redundancy to set to idle
        {
            _moveVector = 0;
            _animator.SetBool("isSprinting", false);
            _animator.SetFloat("moveVector", 0);
            _startTime = Time.time;
        }
    }

    private void OnMove(float _v)
    {
        //flip sprite
        _render.localScale = _v < 0 ? new Vector3(-1, 1, 1) : Vector3.one;

        _animator.SetBool("isSprinting", _isSprinting);

        float _speed = _isSprinting ? runSpeed : walkSpeed;
        float t = (Time.time - _startTime) / speedUpTime;
        float _rampUp = Mathf.SmoothStep(0, _speed, t);

        _rigidbody.velocity = new Vector2(_v * _rampUp, _rigidbody.velocity.y);

        _animator.SetFloat("moveVector", Mathf.Abs(_v));
        _animator.SetFloat("walkSpeed", Mathf.Abs(_v));
    }

    private void OnAirMove(float _v)
    {
        _rigidbody.velocity = new Vector2(Mathf.Clamp(_rigidbody.velocity.x + (_v * airControlModifier/10), -airMoveMaxSpeed, airMoveMaxSpeed), _rigidbody.velocity.y);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if(isGrounded)
        {
            _animator.Play("jump");

            float _multi = _isSprinting ? runHorizontalMultiplyer : walkHorizontalMultiplyer;
            _rigidbody.AddForce(new Vector2(_rigidbody.velocity.x * _multi, jumpForce), ForceMode2D.Impulse);
            
            justJumped = true;
        }
        else
        {
            if(canDoubleJump)
            {
                _animator.Play("doubleJump");
                _vfx.SpawnEffect("doubleJump", true);

                canDoubleJump = false;
                airControlModifier = doubleJumpControlModifier;
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0); //stop the fall

                float _multi = _isSprinting ? runHorizontalMultiplyer : walkHorizontalMultiplyer;
                _rigidbody.AddForce(new Vector2(_rigidbody.velocity.x * _multi, jumpForce), ForceMode2D.Impulse);
            }
        }
    }

    private void IsGrounded()
    {
        if(IsFalling() && Physics2D.BoxCast(transform.position, groundBoxSize, 0, -transform.up, groundCastDistance, groundLayer))
        {
            _animator.SetBool("isGrounded", true);
            isGrounded = true;

            airControlModifier = jumpControlModifier;
            canDoubleJump = true;

            if(justJumped)
            {
                canMove = false;
                justJumped = false;
                StartCoroutine(WaitForLandingPause());
            }        
        }
        else
        {
            if(Physics2D.BoxCast(transform.position, sideBoxSize, 0, transform.right, sideCastDistance, groundLayer) || Physics2D.BoxCast(transform.position, sideBoxSize, 0, -transform.right, sideCastDistance, groundLayer))
            {
                canMove = false;
            }
            else
            {
                canMove = true;
                _animator.SetBool("isGrounded", false);
                isGrounded = false;
            }
        }
    }

    private bool IsFalling()
    {
        return _rigidbody.velocity.y <= 0;
    }

    private IEnumerator WaitForLandingPause()
    {
        yield return new WaitForSeconds(landingPause);
        canMove = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * groundCastDistance, groundBoxSize);
        Gizmos.DrawWireCube(transform.position - transform.right * sideCastDistance, sideBoxSize);
        Gizmos.DrawWireCube(transform.position + transform.right * sideCastDistance, sideBoxSize);
    }
    
    void OnEnable()
    {
        _input.gameplay.Enable();
    }
    void OnDisable()
    {
        _input.gameplay.Disable();
    }
}
