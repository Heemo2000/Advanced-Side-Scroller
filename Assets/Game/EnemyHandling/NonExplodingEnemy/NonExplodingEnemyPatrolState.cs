using UnityEngine;
using UnityHFSM;

namespace Game.EnemyHandling
{
    public class NonExplodingEnemyPatrolState : StateBase
    {
        private readonly NonExplodingEnemy _enemy;
        private int _patrolIndex = 0;
        private bool _shouldStop = false;
        private float _currentStopTime = 0.0f;
        public NonExplodingEnemyPatrolState(NonExplodingEnemy enemy): base(false, false)
        {
            _enemy = enemy;
        }

        public override void Init()
        {
            
        }

        public override void OnEnter()
        {
            _enemy.Mover.OnReachingDestination += OnReachedDestination;
            _shouldStop = false;
            _patrolIndex = 0;
            _enemy.Mover.MoveSpeed = _enemy.PatrolSpeed;
            _enemy.Mover.SetDestination(_enemy.PatrolPoints[_patrolIndex].position);
        }

        public override void OnLogic()
        {
            if(!_shouldStop)
            {
                return;
            }

            if (_currentStopTime >= 0.0f)
            {
                _currentStopTime -= Time.deltaTime;
            }
            else
            {
                _patrolIndex = (_patrolIndex + 1) % _enemy.PatrolPoints.Length;
                _enemy.Mover.SetDestination(_enemy.PatrolPoints[_patrolIndex].position);
                _shouldStop = false;
            }
        }

        public override void OnExit()
        {
            _enemy.Mover.OnReachingDestination -= OnReachedDestination;
        }

        private void OnReachedDestination()
        {
            _shouldStop = true;
            _currentStopTime = _enemy.PatrolStopTime;
        }
    }    
}
