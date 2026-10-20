using Game.EnemyHandling;
using UnityEngine;
using UnityHFSM;

namespace Game
{
    public class NonExplodingEnemyAttackState : StateBase
    {
        private NonExplodingEnemy _enemy;
        public NonExplodingEnemyAttackState(NonExplodingEnemy enemy) : base(false, false)
        {
            _enemy = enemy;
        }

        public override void Init()
        {

        }

        public override void OnEnter()
        {
            _enemy.AttackMechanic.ShouldAttack = true;
        }

        public override void OnLogic()
        {

        }

        public override void OnExit()
        {
            _enemy.AttackMechanic.ShouldAttack = false;
        }
    }
}
