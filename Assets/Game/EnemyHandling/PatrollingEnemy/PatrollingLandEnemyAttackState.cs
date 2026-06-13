using UnityEngine;
using UnityHFSM;
namespace Game.EnemyHandling
{
    public class PatrollingLandEnemyAttackState : StateBase
    {
        #region Private Fields
        private PatrollingLandEnemy _enemy;
        #endregion

        #region Class Functionality
        public PatrollingLandEnemyAttackState(PatrollingLandEnemy enemy) : base(false, false)
        {
            _enemy = enemy;
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
        #endregion
    }
}
