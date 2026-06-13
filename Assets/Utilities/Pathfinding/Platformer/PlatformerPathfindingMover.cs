using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Utilities.IOC;

namespace Utilities.Pathfinding.Platformer
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlatformerPathfindingMover : MonoBehaviour
    {
        public enum PlatformerPathfindingMoverState
        {
            None = -1,
            Normal,
            Jumping
        }
        #region Constants
        private const float DifferenceEpsilon = 0.1f;
        #endregion
        #region Serialized Fields
        [Header("Waypoint Settings:")]
        [Min(0.01f)]
        [SerializeField] private float _waypointCheckDistance = 0.3f;
        [Header("Normal Movement Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _moveSpeed = 10.0f;
        [SerializeField] private Transform[] _groundChecks;
        [Min(0.01f)]
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundCheckLayerMask;
        [Min(0.01f)]
        [SerializeField] private float _smoothingTime = 0.2f;
        [Min(0.1f)]
        [SerializeField] private float _normalGravity = 5.0f;
        

        [Header("Jumping Movement Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _jumpHeight = 2.0f;
        [Min(0.1f)]
        [SerializeField] private float _jumpSpeed = 0.5f;
        [SerializeField] private AnimationCurve _jumpCurve;
        #endregion
        #region Private Fields
        private BoxCollider _boxCollider;
        private Rigidbody _rigidbody;

        private bool _shouldStop = false;
        private PlatformerPathfindingMoverState _currentState = PlatformerPathfindingMoverState.None;
        private Vector2 _currentMovement = Vector2.zero;
        private bool _isGrounded = false;
        private float _velocityY = 0.0f;
        
        private Vector2 _destination = Vector2.zero;
        
        private float _inputX = 0.0f;
        private float _currentInputX = 0.0f;
        private float _currentInputVelocity = 0.0f;
        private int _currentWaypointIndex;
        private List<IAstarNode> _currentPath = null;
        private PlatformerPathfindingManager _platformPathfindingManager = null;

        private Vector2 _startJumpPosition = Vector2.zero;
        private Vector2 _endJumpPosition = Vector2.zero;
        private Vector2 _currentJumpPosition = Vector2.zero;
        private float _jumpDelta = 0.0f;
        private bool _isAlreadyReachedDestination = false;
        #endregion


        #region Events
        public event Action<float> OnMove;
        public event Action OnReachingDestination;
        public event Action OnJumping;
        public event Action OnJumpingComplete;
        #endregion
        #region Properties

        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
        public Vector2 Destination { get => _destination; }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _rigidbody = GetComponent<Rigidbody>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _currentMovement = transform.position;
            _rigidbody.isKinematic = true;
            _boxCollider.isTrigger = true;
            _currentState = PlatformerPathfindingMoverState.Normal;
            ServiceLocator sceneServiceLocator = ServiceLocator.ForSceneOf(this);
            _platformPathfindingManager = sceneServiceLocator.Get<PlatformerPathfindingManager>();
        }

        private void FixedUpdate()
        {
            HandleOverallMovement();
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 extraDetectedMovement = (Mathf.Max(-_velocityY, 0.0f) * Time.fixedDeltaTime + 0.05f) * -Vector2.up;
            foreach (Transform groundCheck in _groundChecks)
            {
                if (groundCheck == null)
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
        #endregion

        #region Class Functionality

        public void SetDestination(Vector2 destination)
        {

            _destination = destination;
            _currentPath = _platformPathfindingManager.FindPathNodes(transform.position, destination);
            _currentWaypointIndex = 0;
            _isAlreadyReachedDestination = false;
            _shouldStop = false;
        }

        public void ForcefullyStop()
        {
            _shouldStop = true;
        }

        private void HandleOverallMovement()
        {
            switch(_currentState)
            {
                case PlatformerPathfindingMoverState.Normal:
                    HandleNormalMovement();
                    break;

                case PlatformerPathfindingMoverState.Jumping:
                    HandleJumpingMovement();
                    break;
            }
        }


        private void HandleNormalMovement()
        {
            _isGrounded = IsGrounded();
            HandleGravity();
            bool shouldJump = false;
            if(_currentPath == null || _currentWaypointIndex >= _currentPath.Count || _shouldStop)
            {
                _inputX = 0.0f;
            }
            else
            {
                
                Vector3 currentWaypoint = _currentPath[_currentWaypointIndex].WorldPos;
                Vector2 difference = currentWaypoint - _rigidbody.position;
                _inputX = Mathf.Sign(difference.x);

                if(Vector2.SqrMagnitude(difference) <= _waypointCheckDistance * _waypointCheckDistance)
                {
                    PlatformerGraphAstarNode currentNode = _currentPath[_currentWaypointIndex] as PlatformerGraphAstarNode;
                    
                    if(currentNode.IsJumpable && _currentWaypointIndex + 1 < _currentPath.Count)
                    {
                        shouldJump = true;
                        _startJumpPosition = _currentPath[_currentWaypointIndex].WorldPos;
                        _endJumpPosition = _currentPath[_currentWaypointIndex + 1].WorldPos;
                    }
                    _currentWaypointIndex++;
                }
            }

            if(!shouldJump)
            {
                _currentInputX = Mathf.SmoothDamp(_currentInputX, _inputX, ref _currentInputVelocity, _smoothingTime);
                _currentMovement.x += _currentInputX * _moveSpeed * Time.fixedDeltaTime;
                _currentMovement.y += _velocityY * Time.fixedDeltaTime;
                _rigidbody.MovePosition(_currentMovement);
                OnMove?.Invoke(_currentInputX);

                if(Mathf.Abs(_currentInputX) <= DifferenceEpsilon)
                {
                    if(!_isAlreadyReachedDestination)
                    {
                        OnReachingDestination?.Invoke();
                        _isAlreadyReachedDestination = true;
                    }
                }
            }
            else
            {
                _isGrounded = false;
                _inputX = 0.0f;
                _currentInputX = 0.0f;
                
                _jumpDelta = 0.0f;
                _currentState = PlatformerPathfindingMoverState.Jumping;
            }
            
        }

        private void HandleJumpingMovement()
        {
            if(_jumpDelta < 1.0f)
            {
                _currentJumpPosition = Vector2.Lerp(_startJumpPosition, _endJumpPosition, _jumpDelta) + 
                                   Vector2.up * _jumpHeight * _jumpCurve.Evaluate(_jumpDelta);
                _jumpDelta += _jumpSpeed * Time.fixedDeltaTime;
                _rigidbody.MovePosition(_currentJumpPosition);
                OnJumping?.Invoke();
            }
            else
            {
                _currentMovement = _currentJumpPosition;
                _currentState = PlatformerPathfindingMoverState.Normal;
                _jumpDelta = 0.0f;
                OnJumpingComplete?.Invoke();
            }
        }

        private void HandleGravity()
        {
            if (_isGrounded)
            {
                _velocityY = 0.0f;
            }
            else
            {
                float oldVelocityY = _velocityY;
                float nextVelocityY = _velocityY - _normalGravity * Time.fixedDeltaTime;
                float newVelocityY = (oldVelocityY + nextVelocityY) / 2.0f;
                _velocityY = newVelocityY;
            }
        }

        private bool IsGrounded()
        {
            float extraDetectedMovement = Mathf.Max(-_velocityY, 0.0f) * Time.fixedDeltaTime + 0.05f;
            foreach (Transform groundCheck in _groundChecks)
            {
                if (groundCheck == null)
                {
                    continue;
                }

                Vector2 groundCheckPos = new Vector2(groundCheck.position.x, groundCheck.position.y);
                if (Physics2D.CircleCast(groundCheckPos, _groundCheckRadius, -Vector2.up, extraDetectedMovement, _groundCheckLayerMask.value).collider != null)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
