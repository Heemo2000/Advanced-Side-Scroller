using UnityEngine;
using UnityHFSM;

namespace Game.EnemyHandling
{
    public class ExplodingEnemyChaseState : StateBase
    {
        private readonly ExplodingEnemy _enemy;
        private float _currentTime = 0.0f;
        public ExplodingEnemyChaseState(ExplodingEnemy enemy) : base(false, false)
        {
            _enemy = enemy;
        }

        public override void Init()
        {

        }

        public override void OnEnter()
        {
            _enemy.Mover.MoveSpeed = _enemy.ChaseSpeed;
            _currentTime = _enemy.PathSearchingInterval;
            _enemy.Mover.SetDestination(_enemy.Target.position);
        }

        public override void OnLogic()
        {
            if (_currentTime >= 0.0f)
            {
                _currentTime -= Time.deltaTime;
                return;
            }
            _enemy.Mover.SetDestination(_enemy.Target.position);
            _currentTime = _enemy.PathSearchingInterval;
        }

        public override void OnExit()
        {

        }
    }
}
