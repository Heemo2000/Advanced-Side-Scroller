#region Headers
using UnityEngine;
#endregion

namespace Game.WeaponHandling
{
    public interface IWeapon
    {
        #region Class Functionality
        GameObject WeaponGameObject { get;}
        void Use();

        void Reload();

        #endregion
    }
}
