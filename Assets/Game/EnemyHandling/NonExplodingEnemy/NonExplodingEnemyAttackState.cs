using Game.EnemyHandling;
using UnityEngine;
using UnityHFSM;

namespace Game
{
    public class NonExplodingEnemyAttackState : StateBase
    {
        private readonly NonExplodingEnemy _enemy;
        private float _currentTime = 0.0f;
        public NonExplodingEnemyAttackState(NonExplodingEnemy enemy) : base(false, false)
        {
            _enemy = enemy;
        }

        public override void Init()
        {

        }

        public override void OnEnter()
        {
            _enemy.Mover.ForcefullyStop();
            _enemy.AttackMechanic.ShouldAttack = true;
            _currentTime = _enemy.AttackTime;
            _enemy.AttackMechanic.ShouldAttack = true;

            Vector2 targetPosition = _enemy.Target.position;
            Vector2 position = _enemy.transform.position;
            Vector2 direction = (targetPosition - position).normalized;
            
            _enemy.Mover.SetDestination(targetPosition - direction * _enemy.AttackDistance * (1.0f - _enemy.MaxClosestDistancePercent));
        }

        public override void OnLogic()
        {
            if(_currentTime >= 0.0f)
            {
                _currentTime -= Time.deltaTime;
            }
            else
            {
                _enemy.AttackMechanic.ShouldAttack = !_enemy.AttackMechanic.ShouldAttack;
                if(_enemy.AttackMechanic.ShouldAttack)
                {
                    _currentTime = _enemy.AttackTime;
                }
                else
                {
                    _currentTime = _enemy.AttackWaitTime;
                }
            }
        }

        public override void OnExit()
        {
            _enemy.AttackMechanic.ShouldAttack = false;
        }
    }
}
