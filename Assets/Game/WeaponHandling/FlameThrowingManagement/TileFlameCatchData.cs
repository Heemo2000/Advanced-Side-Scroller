using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.WeaponHandling.FlameThrowingManagement
{
    [System.Serializable]
    public class TileFlameCatchData
    {
        public TileBase burntTile;
        [Min(0.1f)] public float burnTime;
        public TileBase originalTile;
    }
}
