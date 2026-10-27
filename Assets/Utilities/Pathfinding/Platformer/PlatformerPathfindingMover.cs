using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities.PauseHandling;



#if UNITY_EDITOR
using UnityEditor;
#endif
using Utilities.IOC;

namespace Utilities.Pathfinding.Platformer
{
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlatformerPathfindingMover : MonoBehaviour, IPausable
    {
        public enum PlatformerPathfindingMoverState
        {
            None = -1,
            Normal,
            Jumping
        }
        #region Constants
        private const float DifferenceEpsilon = 0.2f;
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
        [SerializeField] private bool _allowJumping = true;
        [Min(0.1f)]
        [SerializeField] private float _jumpHeight = 2.0f;
        [Min(0.1f)]
        [SerializeField] private float _jumpSpeed = 0.5f;
        [SerializeField] private AnimationCurve _jumpCurve;
        #endregion
        #region Private Fields
        private BoxCollider2D _boxCollider;
        private Rigidbody2D _rigidbody;

        private bool _shouldStop = false;
        private PlatformerPathfindingMoverState _currentState = PlatformerPathfindingMoverState.None;
        private Vector2 _currentMovement = Vector2.zero;
        private bool _isGrounded = false;
        private float _velocityY = 0.0f;
        
        private Vector2 _startPosition = Vector2.zero;
        private Vector2 _destination = Vector2.zero;
        
        private float _inputX = 0.0f;
        private float _currentInputX = 0.0f;
        private float _currentInputVelocity = 0.0f;
        private int _currentWaypointIndex;
        private List<IAstarNode> _currentPath = null;
        private int _currentPathCount = 0;
        private PlatformerPathfindingManager _platformPathfindingManager = null;

        private Vector2 _startJumpPosition = Vector2.zero;
        private Vector2 _endJumpPosition = Vector2.zero;
        private Vector2 _currentJumpPosition = Vector2.zero;
        private float _jumpDelta = 0.0f;
        private bool _isAlreadyReachedDestination = false;
        private bool _isPaused = false;
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
        public bool AllowJumping { get => _allowJumping; set => _allowJumping = value; }

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _currentMovement = transform.position;
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _boxCollider.isTrigger = true;
            _currentState = PlatformerPathfindingMoverState.Normal;
            ServiceLocator sceneServiceLocator = ServiceLocator.ForSceneOf(this);
            _platformPathfindingManager = sceneServiceLocator.Get<PlatformerPathfindingManager>();
            GamePauseManager.Register(this);
        }

        private void FixedUpdate()
        {
            if(_isPaused) return;

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

            if (_currentPath != null)
            {
                Gizmos.color = Color.blueViolet;
                for (int i = 0; i < _currentPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(_currentPath[i].WorldPos, _currentPath[i + 1].WorldPos);
                }
            }
        }
        #endregion

        #region Class Functionality

        public void SetDestination(Vector2 destination)
        {
            if (_platformPathfindingManager != null)
            {
                _platformPathfindingManager.FindPathNodes(transform.position, destination, OnPathFound, OnPathNotFound);
            }
        }

        public void ForcefullyStop()
        {
            _currentPath = null;
            _shouldStop = true;
        }

        private void OnPathFound(PlatformerPathfindingPathResult result)
        {   
            StartCoroutine(ResetMovementParameters(result)); 
        }

        private void OnPathNotFound()
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
                
                Vector2 currentWaypoint = _currentPath[_currentWaypointIndex].WorldPos;
                Vector2 difference = currentWaypoint - _rigidbody.position;
                _inputX = Mathf.Sign(difference.x);

                if(!_allowJumping)
                {
                    difference.y = 0.0f;
                }

                if(Vector2.SqrMagnitude(difference) <= _waypointCheckDistance * _waypointCheckDistance)
                {
                    PlatformerGraphAstarNode currentNode = _currentPath[_currentWaypointIndex] as PlatformerGraphAstarNode;
                    
                    if(_allowJumping && currentNode.IsJumpable && _currentWaypointIndex + 1 < _currentPath.Count)
                    {
                        PlatformerGraphAstarNode nextNode = _currentPath[_currentWaypointIndex + 1] as PlatformerGraphAstarNode;
                        float horizontalDist = Mathf.Abs(difference.x);
                        if (horizontalDist <= _waypointCheckDistance && nextNode.IsJumpable)
                        {
                            shouldJump = true;
                            _startJumpPosition = _currentPath[_currentWaypointIndex].WorldPos;
                            _endJumpPosition = _currentPath[_currentWaypointIndex + 1].WorldPos;
                        }
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
            else if(shouldJump == true && _allowJumping == true)
            {
                _isGrounded = false;
                _inputX = 0.0f;
                _currentInputX = 0.0f;
                _currentJumpPosition = _startJumpPosition;
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
                _currentMovement = _endJumpPosition;
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
                if (Physics2D.OverlapCircle(groundCheckPos, _groundCheckRadius, _groundCheckLayerMask.value) != null ||
                    Physics2D.CircleCast(groundCheckPos, _groundCheckRadius, -Vector2.up, extraDetectedMovement, _groundCheckLayerMask.value).collider != null)
                {
                    return true;
                }
            }

            return false;
        }

        public void OnPause()
        {
            _isPaused = true;
        }

        public void OnResume()
        {
            _isPaused = false;
        }

        private IEnumerator ResetMovementParameters(PlatformerPathfindingPathResult result)
        {
            yield return new WaitWhile(() => _currentState == PlatformerPathfindingMoverState.Jumping);

            _startPosition = result.Start;
            _destination = result.Destination;
            _currentPath = result.Path;
            _currentPathCount = _currentPath.Count;
            _currentWaypointIndex = GetClosestMovementIndex(result.Path);
            _isAlreadyReachedDestination = false;
            _shouldStop = false;

            yield return null;
        }

        private int GetClosestMovementIndex(List<IAstarNode> path)
        {
            int result = -1;
            float minSqrDistance = float.MaxValue;
            int i = 0;
            foreach (IAstarNode node in path)
            {
                float sqrDistance = Vector2.SqrMagnitude(node.WorldPos - _rigidbody.position);
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    result = i;
                }
                i++;
            }

            return result;
        }

        #endregion
    }
}
