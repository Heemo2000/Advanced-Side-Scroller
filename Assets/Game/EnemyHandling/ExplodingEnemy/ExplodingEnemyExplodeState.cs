using Game.EnemyHandling;
using UnityEngine;
using UnityHFSM;

namespace Game
{
    public class ExplodingEnemyExplodeState : StateBase
    {
        private ExplodingEnemy _enemy;
        public ExplodingEnemyExplodeState(ExplodingEnemy enemy) : base(false, false)
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
