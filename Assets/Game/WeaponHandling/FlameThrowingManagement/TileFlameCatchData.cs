using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.WeaponHandling.FlameThrowingManagement
{
    [System.Serializable]
    public class TileFlameCatchData
    {
        public TileBase burntTile;
        [Min(0.1f)] public float burnTime;
        [Range(0.1f, 1.0f)] public float maxFirePercent = 0.5f;
        public AnimationCurve burnCurve;
        public TileBase originalTile;
    }
}
