using System;
using UnityEngine;
using UnityHFSM;
using Utilities.Pathfinding.Platformer;


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
        [SerializeField] private float _explodeDistance = 5.0f;
        [SerializeField] private float _explodeRange = 5.0f;
        [SerializeField] private float _explodeDamage = 20.0f;
        [SerializeField] private float _beforeExplodeTime = 5.0f;

        [Header("Target Settings:")]
        [SerializeField] private Transform _target;
        #endregion

        #region Private Fields
        private StateMachine _fsm;
        private ExplodingEnemyPatrolState _patrolState;
        private ExplodingEnemyChaseState _chaseState;
        private ExplodingEnemyExplodeState _explodeState;
        private PlatformerPathfindingMover _mover;

        #endregion

        #region Properties
        public Transform[] PatrolPoints { get => _patrolPoints; }
        public float PatrolSpeed { get => _patrolSpeed; }
        public float ChaseSpeed { get => _chaseSpeed; }
        public Transform Target { get => _target; set => _target = value; }
        public PlatformerPathfindingMover Mover { get => _mover; }
        public float PatrolStopTime { get => _patrolStopTime; }
        public float PathSearchingInterval { get => _pathSearchingInterval; }
        public float ExplodeDamage { get => _explodeDamage; }
        public float ExplodeRange { get => _explodeRange; }
        public float BeforeExplodeTime { get => _beforeExplodeTime; }
        #endregion

        #region Events
        public event Action OnBeforeExplode;
        public event Action OnExplode;
        #endregion

        #region Unity Methods

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _mover = GetComponent<PlatformerPathfindingMover>();

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

        public void InvokeBeforeExplodeEvent()
        {
            OnBeforeExplode?.Invoke();
        }

        public void InvokeExplodeEvent()
        {
            OnExplode?.Invoke();
        }

        #endregion
    }
}
