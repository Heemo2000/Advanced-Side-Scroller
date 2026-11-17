using UnityEngine;

namespace Game.WeaponHandling.FlameThrowingManagement
{
    public interface IFlameCatchable
    {
        void SpawnFire(Vector3 position, Vector3 normal);
    }
}
