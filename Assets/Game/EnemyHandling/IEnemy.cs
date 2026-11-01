using UnityEngine;

namespace Game.EnemyHandling
{
    public interface IEnemy
    {
        public Transform Target { get; set; }
    }
}
