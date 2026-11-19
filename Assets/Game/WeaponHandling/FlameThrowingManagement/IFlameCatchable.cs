using UnityEngine;

namespace Game.WeaponHandling.FlameThrowingManagement
{
    public interface IFlameCatchable
    {
        void SpawnFire(Vector3 endLinePosition, Vector3 hitPosition, Vector3 normal, float maxFireRegionRadius);
    }
}
