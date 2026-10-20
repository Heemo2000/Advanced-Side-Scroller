using UnityEngine;
using UnityHFSM;

namespace Game.EnemyHandling
{
    public class ExplodingEnemy : MonoBehaviour
    {
        #region Static Variables

        private static string PatrolStateName = "Patrol";
        private static string ChaseStateName = "Chase";
        private static string ExplodeStateName = "Explode";

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
        [SerializeField] private float _explodeDistance = 5.0f;

        [Header("Target Settings:")]
        [SerializeField] private Transform _target;
        #endregion

        #region Private Fields
        private StateMachine _fsm;
        private ExplodingEnemyPatrolState _patrolState;
        private ExplodingEnemyChaseState _chaseState;
        private ExplodingEnemyExplodeState _explodeState;

        #endregion

        #region Properties
        public Transform[] PatrolPoints { get => _patrolPoints; }
        public float PatrolSpeed { get => _patrolSpeed; }
        public float ChaseSpeed { get => _chaseSpeed; }
        public Transform Target { get => _target; }


        #endregion

        #region Unity Methods

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _fsm = new StateMachine();
            _patrolState = new ExplodingEnemyPatrolState(this);
            _chaseState = new ExplodingEnemyChaseState(this);
            _explodeState = new ExplodingEnemyExplodeState(this);

            _fsm.AddState(PatrolStateName, _patrolState);
            _fsm.AddState(ChaseStateName, _chaseState);
            _fsm.AddState(ExplodeStateName, _explodeState);

            _fsm.AddTransition(new Transition(PatrolStateName, ChaseStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) <= _chaseDistance * _chaseDistance));
            _fsm.AddTransition(new Transition(ChaseStateName, PatrolStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) > _chaseDistance * _chaseDistance));

            _fsm.AddTransition(new Transition(ChaseStateName, ExplodeStateName, t => Vector2.SqrMagnitude(_target.position - transform.position) <= _explodeDistance * _explodeDistance));
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
