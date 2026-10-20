using UnityEngine;
using UnityHFSM;

namespace Game.EnemyHandling
{
    public class ExplodingEnemyChaseState : StateBase
    {
        private ExplodingEnemy _enemy;
        public ExplodingEnemyChaseState(ExplodingEnemy enemy) : base(false, false)
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
