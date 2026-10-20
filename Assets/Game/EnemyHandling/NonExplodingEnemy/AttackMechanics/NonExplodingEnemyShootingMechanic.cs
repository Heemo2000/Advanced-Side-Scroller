using Game.WeaponHandling;
using UnityEngine;

namespace Game.EnemyHandling
{
    public class NonExplodingEnemyShootingMechanic : NonExplodingEnemyAttackMechanic
    {
        [SerializeField] private WeaponManager _weaponManager;
        protected override void Update()
        {
            base.Update();
            _weaponManager.AimDirection = (base.Target.position - transform.position).normalized;
        }

        protected override void Attack()
        {
            _weaponManager.SingleUse();
            _weaponManager.ContinousUse();
        }
    }
}
