using UnityEngine;
using UnityHFSM;
using Utilities.Pathfinding.Platformer;
using Game.CoreHandling;
using Utilities.IOC;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Game.EnemyHandling
{
    public class NonExplodingEnemy : MonoBehaviour, IEnemy
    {
        #region Static Variables

        private static string PatrolStateName = "Patrol";
        private static string ChaseStateName = "Chase";
        private static string AttackStateName = "Attack";

        #endregion

        #region Serialized Fields
        [Header("Patrol Settings:")]
        [Min(0.1f)]
        [SerializeField] private Transform[] _patrolPoints;
        [SerializeField] private float _patrolSpeed = 5.0f;
        [SerializeField] private float _patrolStopTime = 2.0f;

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

        [Header("Target Settings:")]
        [SerializeField] private Transform _target;

        [Header("Death Settings:")]
        [SerializeField] private ParticleSystem _deathEffect;
        #endregion

        #region Private Fields
        private StateMachine _fsm;
        private NonExplodingEnemyPatrolState _patrolState;
        private NonExplodingEnemyChaseState _chaseState;
        private NonExplodingEnemyAttackState _attackState;
        private PlatformerPathfindingMover _mover;
        private Health _health;
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
        #endregion

        #region Unity Methods

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _mover = GetComponent<PlatformerPathfindingMover>();
            _health = GetComponent<Health>();

            _fsm = new StateMachine();
            _patrolState = new NonExplodingEnemyPatrolState(this);
            _chaseState = new NonExplodingEnemyChaseState(this);
            _attackState = new NonExplodingEnemyAttackState(this);

            _fsm.AddState(PatrolStateName, _patrolState);
            _fsm.AddState(ChaseStateName, _chaseState);
            _fsm.AddState(AttackStateName, _attackState);

            _fsm.AddTransition(new Transition(PatrolStateName, ChaseStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) <= _chaseDistance * _chaseDistance));
            _fsm.AddTransition(new Transition(ChaseStateName, PatrolStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) > _chaseDistance * _chaseDistance));

            _fsm.AddTransition(new Transition(ChaseStateName, AttackStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) <= _attackDistance * _attackDistance));
            _fsm.AddTransition(new Transition(AttackStateName, ChaseStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) > _attackDistance * _attackDistance));

        }

        private void OnEnable()
        {
            _fsm.SetStartState(PatrolStateName);
        }

        private void Start()
        {
            _health.OnDeath += OnDeath;
        }

        // Update is called once per frame
        void Update()
        {
            _fsm.OnLogic();
        }

        private void OnDestroy()
        {
            _health.OnDeath -= OnDeath;
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

        private void OnDeath(HealthDamageType damageType)
        {
            ParticleSystemObjectPoolManager poolManager = ServiceLocator.ForSceneOf(this).Get<ParticleSystemObjectPoolManager>();
            poolManager.SpawnEffect(_deathEffect, transform.position);
        }

        #endregion
    }
}
