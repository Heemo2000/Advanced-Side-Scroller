using UnityEngine;
using UnityHFSM;


namespace Game.EnemyHandling
{
    public class PatrollingLandEnemy : MonoBehaviour
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

        [Header("Chase Settings:")]
        [Min(0.1f)]
        [SerializeField] private float _chaseDistance = 10.0f;
        [Min(0.1f)]
        [SerializeField] private float _chaseSpeed = 10.0f;

        [Header("Attack Settings:")]
        [SerializeField] private float _attackDistance = 5.0f;

        [Header("Target Settings:")]
        [SerializeField] private Transform _target;
        #endregion

        #region Private Fields
        private StateMachine _fsm;
        private PatrollingLandEnemyPatrolState _patrolState;
        private PatrollingLandEnemyChaseState _chaseState;
        private PatrollingLandEnemyAttackState _attackState;

        public Transform[] PatrolPoints { get => _patrolPoints; }
        #endregion

        #region Properties



        #endregion

        #region Unity Methods

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _fsm = new StateMachine();
            _patrolState = new PatrollingLandEnemyPatrolState(this);
            _chaseState = new PatrollingLandEnemyChaseState(this);
            _attackState = new PatrollingLandEnemyAttackState(this);
            
            _fsm.AddState(PatrolStateName, _patrolState);
            _fsm.AddState(ChaseStateName, _chaseState);
            _fsm.AddState(AttackStateName, _attackState);

            _fsm.AddTransition(new Transition(PatrolStateName, ChaseStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) <= _chaseDistance * _chaseDistance));
            _fsm.AddTransition(new Transition(ChaseStateName, PatrolStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) > _chaseDistance * _chaseDistance));

            _fsm.AddTransition(new Transition(ChaseStateName, AttackStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) <= _attackDistance * _attackDistance));
            _fsm.AddTransition(new Transition(AttackStateName, ChaseStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) > _attackDistance * _attackDistance));

            _fsm.SetStartState(PatrolStateName);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            _fsm.OnLogic();
        }

        #endregion

        #region Class Functionality
        

        
        #endregion
    }
}
