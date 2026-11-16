using System;
using UnityEngine;

namespace Game.WeaponHandling
{
    public abstract class Weapon : MonoBehaviour
    {
        #region Events

        public Action OnReload;
        public Action OnSingleUse;
        public Action OnContinousUse;
        public Action OnLeaveTrigger;

        #endregion
        #region Class Functionality
        public virtual void SingleUse()
        {
            OnSingleUse?.Invoke();
        }

        public virtual void ContinousUse()
        {
            OnContinousUse?.Invoke();
        }

        public virtual void LeaveTrigger()
        {
            OnLeaveTrigger?.Invoke();
        }

        public virtual void Reload()
        {
            OnReload?.Invoke();
        }
        #endregion
    }
}
