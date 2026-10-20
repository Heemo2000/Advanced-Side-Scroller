using UnityEngine;
using UnityHFSM;

namespace Game.EnemyHandling
{
    public class ExplodingEnemyPatrolState : StateBase
    {
        private ExplodingEnemy _enemy;
        public ExplodingEnemyPatrolState(ExplodingEnemy enemy) : base(false, false)
        {
            _enemy = enemy;
        }

        public override void Init()
        {

        }

        public override void OnEnter()
        {

        }

        public override void OnLogic()
        {

        }

        public override void OnExit()
        {

        }
    }
}
