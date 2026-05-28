using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Game.PlayerHandling
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent (typeof(BoxCollider2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Ground Check Settings:")]
        [SerializeField] private Transform[] _groundChecks;
        [Min(0.01f)]
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundCheckLayerMask;

        [Header("Movement Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _normalSpeed = 2.0f;
        [Min(0.1f)]
        [SerializeField] private float _sprintSpeed = 10.0f;
        [Min(0.01f)]
        [SerializeField] private float _smoothingTime = 0.2f;

        [Header("Jump Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _jumpHeight = 2.0f;
        [Min(0.1f)]
        [SerializeField] private float _jumpHoldForce = 15.0f;
        [Min(0.1f)]
        [SerializeField] private float _jumpHoldDuration = 1.0f;
        [Min(0.1f)]
        [SerializeField] private float _jumpTime = 0.5f;
        [Min(1.0f)]
        [SerializeField] private float _fallMultiplier = 2.0f;
        [SerializeField] private int _maxJumps = 3;
        [Min(0.05f)]
        [SerializeField] private float _jumpBufferTime = 0.2f;
        
        private Rigidbody2D _playerRB;
        private BoxCollider2D _playerCollider;
        
        private float _initialJumpVelocity = 0.0f;
        private float _gravity = 0.0f;
        private float _velocityY = 0.0f;
        
        private int _currentJumpCount = 0;
        private bool _jumpPressedThisFrame = false;
        
        private Vector2 _currentPosition;
        
        private bool _isGrounded = false;
        private float _jumpBufferCounter = 0.0f;
        
        private float _inputX = 0.0f;
        private bool _sprintPressed = false;
        private float _currentInputX = 0.0f;
        private float _currentInputXRef = 0.0f;

        private bool _jumpHeld = false;
        private float _jumpHoldCounter = 0.0f;
        private void Awake()
        {
            _playerRB = GetComponent<Rigidbody2D>();
            _playerCollider = GetComponent<BoxCollider2D>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _playerRB.bodyType = RigidbodyType2D.Kinematic;
            _playerCollider.isTrigger = true;
            _currentPosition = transform.position;
            SetupParameters();
        }

        private void OnValidate()
        {
            SetupParameters();
        }


        private void FixedUpdate()
        {
            if(_jumpBufferCounter > 0.0f)
            {
                _jumpBufferCounter -= Time.fixedDeltaTime;
            }
            _jumpPressedThisFrame = false;
            _isGrounded = IsGrounded();
            HandleJump();
            HandleGravity();
            HandleMovement();
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 extraDetectedMovement = (Mathf.Max(-_velocityY, 0.0f) * Time.fixedDeltaTime + 0.05f) * -Vector2.up;
            foreach (Transform groundCheck in _groundChecks)
            {
                if(groundCheck == null)
                {
                    continue;
                }

                #if UNITY_EDITOR
                
                Vector2 groundCheckPos = new Vector2(groundCheck.position.x, groundCheck.position.y);
                
                Handles.color = Color.red;
                Handles.DrawWireDisc(groundCheck.position, Vector3.forward, _groundCheckRadius * 2.0f);
                Handles.DrawLine(groundCheckPos, groundCheckPos + extraDetectedMovement);
                #endif
            }
        }

        public void HandleInputX(float inputX, bool sprintPressed)
        {
            _inputX = inputX;
            _sprintPressed = sprintPressed;
        }

        public void SetJumpHeld(bool jumpHeld)
        {
            _jumpHeld = jumpHeld;
        }

        public void Jump()
        {
            _jumpBufferCounter = _jumpBufferTime;
        }

        private void SetupParameters()
        {
            _initialJumpVelocity = (2.0f * _jumpHeight) / _jumpTime;
            _gravity = (-2.0f * _jumpHeight) / (_jumpTime * _jumpTime);
        }

        private void HandleMovement()
        {
            _currentPosition.y += _velocityY * Time.fixedDeltaTime;
            _currentInputX = Mathf.SmoothDamp(_currentInputX, _inputX, ref _currentInputXRef, _smoothingTime);
            _currentPosition.x += _currentInputX * Time.fixedDeltaTime * (_sprintPressed ? _sprintSpeed : _normalSpeed);
            _playerRB.MovePosition(_currentPosition);
        }

        private void HandleGravity()
        {
            if (_isGrounded && !_jumpPressedThisFrame)
            {
                _velocityY = 0.0f;
                _currentJumpCount = 0;
            }
            else
            {
                bool isFalling = _velocityY < 0.0f;
                float fallMultiplier = isFalling ? _fallMultiplier : 1.0f;

                if(_jumpHeld && _velocityY > 0.0f && _jumpHoldCounter > 0.0f)
                {
                    _velocityY += _jumpHoldForce * Time.fixedDeltaTime;
                    _jumpHoldCounter -= Time.fixedDeltaTime;
                }

                float oldVelocityY = _velocityY;
                float nextVelocityY = _velocityY + _gravity * fallMultiplier * Time.fixedDeltaTime;
                float newVelocityY = (oldVelocityY + nextVelocityY) / 2.0f;
                _velocityY = newVelocityY;
            }
        }

        private void HandleJump()
        {
            bool jumpPressed = _jumpBufferCounter > 0.0f;
            if(!jumpPressed)
            {
                return;
            }

            if(_isGrounded || (_currentJumpCount >= 0 && _currentJumpCount < _maxJumps))
            {
                _velocityY = _initialJumpVelocity;
                _currentJumpCount++;
                _jumpPressedThisFrame = true;
                _jumpBufferCounter = 0.0f;
                _jumpHoldCounter = _jumpHoldDuration;
            }
        }

        private bool IsGrounded()
        {
            float extraDetectedMovement = Mathf.Max(-_velocityY, 0.0f) * Time.fixedDeltaTime + 0.05f;
            foreach(Transform groundCheck in _groundChecks)
            {
                if(groundCheck == null)
                {
                    continue;
                }

                Vector2 groundCheckPos = new Vector2(groundCheck.position.x, groundCheck.position.y);
                if(Physics2D.CircleCast(groundCheckPos, _groundCheckRadius, -Vector2.up, extraDetectedMovement, _groundCheckLayerMask.value).collider != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
