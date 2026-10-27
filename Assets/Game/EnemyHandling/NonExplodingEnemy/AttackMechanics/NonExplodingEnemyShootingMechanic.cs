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
            if (base.ShouldAttack)
            {
                _weaponManager.AimDirection = (base.Target.position - transform.position).normalized;
            }
            else
            {
                _weaponManager.AimDirection = Vector2.right;
            }
        }

        protected override void Attack()
        {
            _weaponManager.SingleUse();
            _weaponManager.ContinousUse();
        }
    }
}
