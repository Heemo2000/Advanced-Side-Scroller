using UnityEngine;
using UnityHFSM;
using Utilities.Pathfinding.Platformer;
using Game.CoreHandling;
using Utilities.IOC;
using Utilities.PauseHandling;



#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Game.EnemyHandling
{
    public class NonExplodingEnemy : MonoBehaviour, IEnemy, IPausable
    {
        #region Static Variables

        private static string PatrolStateName = "Patrol";
        private static string ChaseStateName = "Chase";
        private static string AttackStateName = "Attack";

        #endregion

        #region Serialized Fields
        [Header("Patrol Settings:")]
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
        private NonExplodingEnemyChaseState _chaseState;
        private NonExplodingEnemyAttackState _attackState;
        private PlatformerPathfindingMover _mover;
        private Health _health;
        private bool _isPaused = false;
        
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

            _fsm.AddTransition(new Transition(PatrolStateName, ChaseStateName, PatrolToChaseState));
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
            return Vector2.SqrMagnitude(_target.position - transform.position) <= _chaseDistance * _chaseDistance;
        }

        private bool ChaseToPatrolState(Transition<string> t)
        {
            return Vector2.SqrMagnitude(_target.position - transform.position) > _chaseDistance * _chaseDistance;
        }

        private bool ChaseToAttackState(Transition<string> t)
        {
            return Vector2.SqrMagnitude(_target.position - transform.position) <= _attackDistance * _attackDistance;
        }

        private bool AttackToChaseState(Transition<string> t)
        {
            float sqrDistance = Vector2.SqrMagnitude(_target.position - transform.position);
            return sqrDistance > _attackDistance * _attackDistance && sqrDistance <= _chaseDistance * _chaseDistance;
        }

        private bool AttackToPatrolState(Transition<string> t)
        {
            float sqrDistance = Vector2.SqrMagnitude(_target.position - transform.position);
            return sqrDistance > _attackDistance * _attackDistance && 
                   sqrDistance > _chaseDistance * _chaseDistance;
        }

        private void OnStateChanged(StateBase<string> state)
        {
            _currentState = state.name;
        }

        #endregion
    }
}
