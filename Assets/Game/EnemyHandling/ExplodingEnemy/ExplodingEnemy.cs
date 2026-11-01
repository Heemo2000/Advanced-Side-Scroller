using System;

using UnityEngine;
using UnityHFSM;

using Utilities.Pathfinding.Platformer;
using Utilities.IOC;
using Utilities.Generic;

using Game.CoreHandling;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.EnemyHandling
{
    public class ExplodingEnemy : MonoBehaviour, IEnemy
    {
        #region Static Variables

        private static string PatrolStateName = "Patrol";
        private static string ChaseStateName = "Chase";
        private static string ExplodeStateName = "Explode";

        #endregion

        #region Serialized Fields
        [Header("Patrol Settings:")]
        [Tooltip("These are the points which enemy uses to patrol around the particular area")]
        [SerializeField] private Transform[] _patrolPoints;
        [Min(0.1f)]
        [SerializeField] private float _patrolSpeed = 5.0f;

        [Min(0.1f)]
        [Tooltip("This is a variable which tells how much an enemy waits at a particular patrol point")]
        [SerializeField] private float _patrolStopTime = 2.0f;

        [Header("Chase Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _chaseDistance = 10.0f;
        [Min(0.1f)]
        [SerializeField] private float _chaseSpeed = 10.0f;
        [Min(0.1f)]
        [Tooltip("This tells after how much time should an enemy recalculate path when chasing the player")]
        [SerializeField] private float _pathSearchingInterval = 0.5f;

        [Header("Attack Settings:")]
        [SerializeField] private float _explodeDistance = 5.0f;
        [SerializeField] private float _explodeRange = 5.0f;
        [SerializeField] private float _explodeDamage = 20.0f;
        [SerializeField] private float _beforeExplodeTime = 5.0f;
        [Range(0.1f, 1.0f)]
        [Tooltip("This tells how much to stay close to the player when in attack state")]
        [SerializeField] private float _maxClosestDistancePercent = 0.4f;

        [Header("Target Settings:")]
        [SerializeField] private Transform _target;

        [Header("Death Settings:")]
        [SerializeField] private ParticleSystem _explodeEffect;
        #endregion

        #region Private Fields
        private StateMachine _fsm;
        private BoxCollider2D _collider;
        private ExplodingEnemyPatrolState _patrolState;
        private ExplodingEnemyChaseState _chaseState;
        private ExplodingEnemyExplodeState _explodeState;
        private PlatformerPathfindingMover _mover;
        private BoxCollider2D _targetCollider;
        private Health _health;
        #endregion

        #region Properties
        public Transform[] PatrolPoints { get => _patrolPoints; set => _patrolPoints = value; }
        public float PatrolSpeed { get => _patrolSpeed; }
        public float ChaseSpeed { get => _chaseSpeed; }
        public Transform Target { get => _target; set => _target = value; }
        public PlatformerPathfindingMover Mover { get => _mover; }
        public float PatrolStopTime { get => _patrolStopTime; }
        public float PathSearchingInterval { get => _pathSearchingInterval; }
        public float ExplodeDamage { get => _explodeDamage; }
        public float ExplodeRange { get => _explodeRange; }
        public float BeforeExplodeTime { get => _beforeExplodeTime; }
        public Health Health { get => _health; }
        public float MaxClosestDistancePercent { get => _maxClosestDistancePercent; }
        public float ExplodeDistance { get => _explodeDistance;}
        #endregion

        #region Events
        public event Action OnExplosionTimerStarts;
        public event Action<float> OnBeforeExplode;
        public event Action OnExplode;
        #endregion

        #region Unity Methods

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _mover = GetComponent<PlatformerPathfindingMover>();
            _health = GetComponent<Health>();
            _collider = GetComponent<BoxCollider2D>();

            _fsm = new StateMachine();
            _patrolState = new ExplodingEnemyPatrolState(this);
            _chaseState = new ExplodingEnemyChaseState(this);
            _explodeState = new ExplodingEnemyExplodeState(this);

            _fsm.AddState(PatrolStateName, _patrolState);
            _fsm.AddState(ChaseStateName, _chaseState);
            _fsm.AddState(ExplodeStateName, _explodeState);

            _fsm.AddTransition(new Transition(PatrolStateName, ChaseStateName, PatrolToChaseTransition));
            _fsm.AddTransition(new Transition(ChaseStateName, PatrolStateName, ChaseToPatrolTransition));
            _fsm.AddTransition(new Transition(ChaseStateName, ExplodeStateName, ChaseToExplodeTransition));
        }

        private void OnEnable()
        {
            _fsm.SetStartState(PatrolStateName);
            _fsm.Init();
        }

        private void Start()
        {
            OnExplode += SpawnExplodeEffectAndDestroyEnemy;
        }

        // Update is called once per frame
        void Update()
        {
            _fsm.OnLogic();
        }

        private void OnDestroy()
        {
            OnExplode -= SpawnExplodeEffectAndDestroyEnemy;
        }

        private void OnDrawGizmosSelected()
        {
            #if UNITY_EDITOR

            Handles.color = Color.yellow;
            Handles.DrawWireDisc(transform.position, Vector3.forward, _chaseDistance);

            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.forward, _explodeDistance);
            #endif
        }

        #endregion

        #region Class Functionality

        public void InvokeExplosionTimerStartsEvent()
        {
            OnExplosionTimerStarts?.Invoke();
        }

        public void InvokeBeforeExplodeEvent(float remainingTime)
        {
            OnBeforeExplode?.Invoke(remainingTime);
        }

        public void InvokeExplodeEvent()
        {
            OnExplode?.Invoke();
        }


        private bool PatrolToChaseTransition(Transition<string> t)
        {
            if(_target == null)
            {
                return false;
            }

            if(_targetCollider == null && !_target.TryGetComponent<BoxCollider2D>(out _targetCollider))
            {
                return false;
            }

            float directionX = Mathf.Sign(_target.position.x - transform.position.x);
            return (_mover.InputX == 0.0f || _mover.InputX == directionX) && HelperFunctions.IsDetectable(_targetCollider, _collider, _chaseDistance, ~((1 << _target.gameObject.layer) | (1 << gameObject.layer)));
        }

        private bool ChaseToPatrolTransition(Transition<string> t)
        {
            if (_target == null)
            {
                return false;
            }

            if (_targetCollider == null && !_target.TryGetComponent<BoxCollider2D>(out _targetCollider))
            {
                return false;
            }

            return !HelperFunctions.IsDetectable(_targetCollider, _collider, _chaseDistance);
        }

        private bool ChaseToExplodeTransition(Transition<string> t)
        {
            if (_target == null)
            {
                return false;
            }
            if (_targetCollider == null && !_target.TryGetComponent<BoxCollider2D>(out _targetCollider))
            {
                return false;
            }

            return HelperFunctions.IsDetectable(_targetCollider, _collider, _explodeDistance);
        }

        private void SpawnExplodeEffectAndDestroyEnemy()
        {
            ParticleSystemObjectPoolManager poolManager = ServiceLocator.ForSceneOf(this).Get<ParticleSystemObjectPoolManager>();
            poolManager.SpawnEffect(_explodeEffect, transform.position);

            Destroy(gameObject);
        }
        #endregion
    }
}
