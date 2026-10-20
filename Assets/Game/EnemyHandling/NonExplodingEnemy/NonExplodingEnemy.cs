using UnityEngine;
using UnityHFSM;
using Utilities.Pathfinding.Platformer;
namespace Game.EnemyHandling
{
    public class NonExplodingEnemy : MonoBehaviour
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
        [SerializeField] private float _attackDistance = 5.0f;
        [SerializeField] private NonExplodingEnemyAttackMechanic _attackMechanic;

        [Header("Target Settings:")]
        [SerializeField] private Transform _target;
        #endregion

        #region Private Fields
        private StateMachine _fsm;
        private NonExplodingEnemyPatrolState _patrolState;
        private NonExplodingEnemyChaseState _chaseState;
        private NonExplodingEnemyAttackState _attackState;
        private PlatformerPathfindingMover _mover;
        #endregion

        #region Properties
        public Transform[] PatrolPoints { get => _patrolPoints; }
        public float PatrolSpeed { get => _patrolSpeed; }
        public float PatrolStopTime { get => _patrolStopTime; }
        public float ChaseSpeed { get => _chaseSpeed; }
        public Transform Target { get => _target; }
        public PlatformerPathfindingMover Mover { get => _mover; }
        public float PathSearchingInterval { get => _pathSearchingInterval; }
        public NonExplodingEnemyAttackMechanic AttackMechanic { get => _attackMechanic; }
        #endregion

        #region Unity Methods

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _mover = GetComponent<PlatformerPathfindingMover>();

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

        // Update is called once per frame
        void Update()
        {
            _fsm.OnLogic();
        }

        #endregion

        #region Class Functionality



        #endregion
    }
}
