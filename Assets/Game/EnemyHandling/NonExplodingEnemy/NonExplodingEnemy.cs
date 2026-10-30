using UnityEngine;
using UnityHFSM;
using Utilities.Pathfinding.Platformer;
using Utilities.IOC;
using Utilities.PauseHandling;
using Utilities.Generic;
using Game.CoreHandling;
using System;




#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Game.EnemyHandling
{
    public class NonExplodingEnemy : MonoBehaviour, IEnemy, IPausable
    {
        #region Static Variables

        private static string PatrolStateName = "Patrol";
        private static string SuspiciousStateName = "Suspicious";
        private static string ChaseStateName = "Chase";
        private static string AttackStateName = "Attack";

        #endregion

        #region Serialized Fields
        [Header("Patrol Settings:")]
        [SerializeField] private Transform[] _patrolPoints;
        [SerializeField] private float _patrolSpeed = 5.0f;
        [SerializeField] private float _patrolStopTime = 2.0f;

        [Header("Suspicion Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _minSuspicionCheckDistance = 1.0f;
        [Min(0.1f)]
        [SerializeField] private float _maxSuspicionCheckDistance = 3.0f;
        [Min(0.1f)]
        [SerializeField] private float _suspicionCheckTime = 5.0f;
        
        [Header("Chase Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _chaseDistance = 10.0f;
        [Min(0.1f)]
        [SerializeField] private float _chaseSpeed = 10.0f;
        [Min(0.1f)]
        [SerializeField] private float _pathSearchingInterval = 0.5f;

        [Header("Attack Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _attackDistance = 5.0f;
        [SerializeField] private NonExplodingEnemyAttackMechanic _attackMechanic;
        [Min(0.1f)]
        [SerializeField] private float _attackTime = 2.0f;
        [Min(0.1f)]
        [SerializeField] private float _attackWaitTime = 3.0f;
        [Range(0.1f, 1.0f)]
        [SerializeField] private float _maxClosestDistancePercent = 0.4f;

        [Header("Target Settings:")]
        [SerializeField] private Transform _target;

        [Header("Death Settings:")]
        [SerializeField] private ParticleSystem _deathEffect;

        [Header("State change Status:")]
        [SerializeField] private string _currentState = "";
        #endregion

        #region Private Fields
        private StateMachine _fsm;
        private NonExplodingEnemyPatrolState _patrolState;
        private NonExplodingEnemySuspiciousState _suspiciousState;
        private NonExplodingEnemyChaseState _chaseState;
        private NonExplodingEnemyAttackState _attackState;
        private PlatformerPathfindingMover _mover;
        private Health _health;
        private BoxCollider2D _boxCollider;
        private bool _shouldBecomeSuspicious = false;
        private Vector2 _hitPosition = Vector2.zero;


        private bool _isPaused = false;
        private BoxCollider2D _targetBoxCollider;
        #endregion

        #region Properties
        public Transform[] PatrolPoints { get => _patrolPoints; }
        public float PatrolSpeed { get => _patrolSpeed; }
        public float PatrolStopTime { get => _patrolStopTime; }
        public float ChaseSpeed { get => _chaseSpeed; }
        public Transform Target { get => _target; set => _target = value; }
        public PlatformerPathfindingMover Mover { get => _mover; }
        public float PathSearchingInterval { get => _pathSearchingInterval; }
        public NonExplodingEnemyAttackMechanic AttackMechanic { get => _attackMechanic; }
        public float AttackTime { get => _attackTime; }
        public float AttackWaitTime { get => _attackWaitTime; set => _attackWaitTime = value; }
        public float AttackDistance { get => _attackDistance; }
        public float MaxClosestDistancePercent { get => _maxClosestDistancePercent; }
        public Health Health { get => _health; }
        public bool ShouldBecomeSuspicious { get => _shouldBecomeSuspicious; set => _shouldBecomeSuspicious = value; }
        public Vector2 HitPosition { get => _hitPosition; set => _hitPosition = value; }
        public float SuspicionCheckTime { get => _suspicionCheckTime; }
        public float MinSuspicionCheckDistance { get => _minSuspicionCheckDistance; }
        public float MaxSuspicionCheckDistance { get => _maxSuspicionCheckDistance; }
        #endregion

        #region Unity Methods

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _mover = GetComponent<PlatformerPathfindingMover>();
            _health = GetComponent<Health>();
            _boxCollider = GetComponent<BoxCollider2D>();

            _fsm = new StateMachine();
            _patrolState = new NonExplodingEnemyPatrolState(this);
            _suspiciousState = new NonExplodingEnemySuspiciousState(this);
            _chaseState = new NonExplodingEnemyChaseState(this);
            _attackState = new NonExplodingEnemyAttackState(this);

            _fsm.AddState(PatrolStateName, _patrolState);
            _fsm.AddState(SuspiciousStateName, _suspiciousState);
            _fsm.AddState(ChaseStateName, _chaseState);
            _fsm.AddState(AttackStateName, _attackState);

            _fsm.AddTransition(new Transition(PatrolStateName, ChaseStateName, PatrolToChaseState));
            _fsm.AddTransition(new Transition(PatrolStateName, SuspiciousStateName, PatrolToSuspiciousState));
            
            _fsm.AddTransition(new Transition(SuspiciousStateName, PatrolStateName, SuspiciousToPatrolState));
            _fsm.AddTransition(new Transition(SuspiciousStateName, ChaseStateName, SuspiciousToChaseState));
            _fsm.AddTransition(new Transition(ChaseStateName, PatrolStateName, ChaseToPatrolState));
            _fsm.AddTransition(new Transition(ChaseStateName, AttackStateName, ChaseToAttackState));

            _fsm.AddTransition(new Transition(AttackStateName, ChaseStateName, AttackToChaseState));
            _fsm.AddTransition(new Transition(AttackStateName, PatrolStateName, AttackToPatrolState));
        }

        

        private void OnEnable()
        {            
            _fsm.SetStartState(PatrolStateName);
            _currentState = PatrolStateName;
            _fsm.Init();
        }

        private void OnValidate()
        {
            if(_attackMechanic != null)
            {
                _attackMechanic.Target = _target;
            }
        }

        private void Start()
        {
            _fsm.StateChanged += OnStateChanged;
            _attackMechanic.Target = _target;
            _health.OnDeath += OnDeath;
            _health.OnHealthDamage += BecomeSuspicious;
            GamePauseManager.Register(this);
        }

        

        // Update is called once per frame
        void Update()
        {
            if(!_isPaused)
            {
                _fsm.OnLogic();
            }
        }

        private void OnDestroy()
        {
            _fsm.StateChanged -= OnStateChanged;
            _health.OnDeath -= OnDeath;
            _health.OnHealthDamage -= BecomeSuspicious;
        }

        private void OnDrawGizmosSelected()
        {
            #if UNITY_EDITOR
            
            Handles.color = Color.yellow;
            Handles.DrawWireDisc(transform.position, Vector3.forward, _chaseDistance);

            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.forward, _attackDistance);
            #endif
        }

        #endregion

        #region Class Functionality

        public void OnPause()
        {
            _isPaused = true;
        }

        public void OnResume()
        {
            _isPaused = false;
        }

        private void OnDeath(HealthDamageType damageType)
        {
            ParticleSystemObjectPoolManager poolManager = ServiceLocator.ForSceneOf(this).Get<ParticleSystemObjectPoolManager>();
            poolManager.SpawnEffect(_deathEffect, transform.position);
            Destroy(gameObject);
        }

        private bool PatrolToChaseState(Transition<string> t)
        {
            if(_target == null)
            {
                return false;
            }
            if(!_target.TryGetComponent<BoxCollider2D>(out _targetBoxCollider))
            {
                return false;
            }
            int directionX = (int)Mathf.Sign(_target.position.x - transform.position.x);
            return (_mover.InputX == 0.0f || directionX == _mover.InputX) && HelperFunctions.IsDetectable(_targetBoxCollider, _boxCollider, _chaseDistance, ~((1 << _target.gameObject.layer) | (1 << gameObject.layer)));
        }

        private bool PatrolToSuspiciousState(Transition<string> transition)
        {
            if (_target == null)
            {
                return false;
            }

            return _shouldBecomeSuspicious == true;
        }

        private bool SuspiciousToPatrolState(Transition<string> transition)
        {
            if (_target == null)
            {
                return false;
            }

            return _shouldBecomeSuspicious == false;
        }

        private bool SuspiciousToChaseState(Transition<string> transition)
        {
            if (_target == null)
            {
                return false;
            }

            if (!_target.TryGetComponent<BoxCollider2D>(out _targetBoxCollider))
            {
                return false;
            }

            int directionX = (int)Mathf.Sign(_target.position.x - transform.position.x);
            return (_mover.InputX == 0.0f || directionX == _mover.InputX) && HelperFunctions.IsDetectable(_targetBoxCollider, _boxCollider, _chaseDistance, ~((1 << _target.gameObject.layer) | (1 << gameObject.layer)));
        }
        private bool ChaseToPatrolState(Transition<string> t)
        {
            if (_target == null)
            {
                return false;
            }

            if (!_target.TryGetComponent<BoxCollider2D>(out _targetBoxCollider))
            {
                return false;
            }

            return !HelperFunctions.IsDetectable(_targetBoxCollider, _boxCollider, _chaseDistance);
        }

        private bool ChaseToAttackState(Transition<string> t)
        {
            if (_target == null)
            {
                return false;
            }

            if (!_target.TryGetComponent<BoxCollider2D>(out _targetBoxCollider))
            {
                return false;
            }

            return HelperFunctions.IsDetectable(_targetBoxCollider, _boxCollider, _attackDistance);
        }

        private bool AttackToChaseState(Transition<string> t)
        {
            if (_target == null)
            {
                return false;
            }
            
            if (!_target.TryGetComponent<BoxCollider2D>(out _targetBoxCollider))
            {
                return false;
            }

            return !HelperFunctions.IsDetectable(_targetBoxCollider, _boxCollider, _attackDistance) &&
                    HelperFunctions.IsDetectable(_targetBoxCollider, _boxCollider, _chaseDistance);
        }

        private bool AttackToPatrolState(Transition<string> t)
        {
            if (_target == null)
            {
                return false;
            }

            if (!_target.TryGetComponent<BoxCollider2D>(out _targetBoxCollider))
            {
                return false;
            }

            return !HelperFunctions.IsDetectable(_targetBoxCollider, _boxCollider, _attackDistance) &&
                   !HelperFunctions.IsDetectable(_targetBoxCollider, _boxCollider, _chaseDistance);
        }

        private void OnStateChanged(StateBase<string> state)
        {
            _currentState = state.name;
        }

        private void BecomeSuspicious(float damage, Vector2 position, Vector2 hitPosition, HealthDamageType damageType)
        {
            if(_currentState != PatrolStateName)
            {
                return;
            }

            _shouldBecomeSuspicious = true;
            _hitPosition = hitPosition;
        }

        #endregion
    }
}
