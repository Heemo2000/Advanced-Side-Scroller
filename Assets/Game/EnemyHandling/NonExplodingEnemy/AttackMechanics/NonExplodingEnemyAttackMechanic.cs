using UnityEngine;

namespace Game.EnemyHandling
{
    public abstract class NonExplodingEnemyAttackMechanic : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private bool _shouldAttack = false;
        [SerializeField] private Transform _target;
        #endregion

        #region Properties
        public bool ShouldAttack { get => _shouldAttack; set => _shouldAttack = value; }
        public Transform Target { get => _target; set => _target = value; }
        #endregion

        #region Unity Methods
        // Update is called once per frame
        protected virtual void Update()
        {
            if(_shouldAttack)
            {
                Attack();
            }
        }

        #endregion

        #region Abstract Methods
        protected abstract void Attack();
        #endregion
    }
}
