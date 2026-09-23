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
            foreach (Weapon weapon in _weapons)
            {
                weapon.gameObject.SetActive(false);
            }

            _currentWeaponIndex = 0;
            _weapons[0].gameObject.SetActive(true);
        }

        private void Update()
        {
            HandleWeaponRotation();
        }

        #endregion

        #region Class Functionality

        public void SelectPreviousWeapon()
        {
            _weapons[_currentWeaponIndex].gameObject.SetActive(false);
            
            _currentWeaponIndex--;
            if(_currentWeaponIndex < 0)
            {
                _currentWeaponIndex = _weapons.Length - 1;
            }
            
            _weapons[_currentWeaponIndex].gameObject.SetActive(true);
        }

        public void SelectNextWeapon()
        {
            _weapons[_currentWeaponIndex].gameObject.SetActive(false);

            _currentWeaponIndex++;
            if (_currentWeaponIndex >= _weapons.Length)
            {
                _currentWeaponIndex = 0;
            }
            
            _weapons[_currentWeaponIndex].gameObject.SetActive(true);
        }


        public void SingleUse()
        {
            if(_currentWeaponIndex >= _weapons.Length)
            {
                return;
            }

            _weapons[_currentWeaponIndex].SingleUse();
        }

        public void ContinousUse()
        {
            if (_currentWeaponIndex >= _weapons.Length)
            {
                return;
            }

            _weapons[_currentWeaponIndex].ContinousUse();
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
