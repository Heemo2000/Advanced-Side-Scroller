using UnityEngine;

namespace Game.WeaponHandling
{
    public class WeaponManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private Weapon[] _weapons;
        #endregion

        #region Private Fields
        private int _currentWeaponIndex = -1;

        private Vector2 _aimDirection = Vector2.zero;
        #endregion

        #region Properties
        
        public Vector2 AimDirection { get { return _aimDirection; } set => _aimDirection = value; }
        #endregion

        #region Unity Methods

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _currentWeaponIndex = 0;
        }

        private void Update()
        {
            HandleWeaponRotation();
        }

        #endregion

        #region Class Functionality

        public void Use()
        {
            if(_currentWeaponIndex >= _weapons.Length)
            {
                return;
            }

            _weapons[_currentWeaponIndex].Use();
        }

        private void HandleWeaponRotation()
        {
            if (_currentWeaponIndex >= _weapons.Length)
            {
                return;
            }

            Weapon currentWeapon = _weapons[_currentWeaponIndex];
            currentWeapon.transform.right = _aimDirection;
        }
        #endregion
    }
}
