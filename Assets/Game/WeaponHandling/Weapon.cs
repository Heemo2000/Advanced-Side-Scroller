using System;
using UnityEngine;

namespace Game.WeaponHandling
{
    public abstract class Weapon : MonoBehaviour
    {
        #region Events

        public Action OnReload;
        public Action OnUse;

        #endregion
        #region Class Functionality
        public virtual void Use()
        {
            OnUse?.Invoke();
        }

        public virtual void Reload()
        {
            OnReload?.Invoke();
        }
        #endregion
    }
}
