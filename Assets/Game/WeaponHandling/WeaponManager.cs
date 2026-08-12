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

        private Vector2 _aimPosition = Vector2.zero;
        #endregion

        #region Properties
        
        public Vector2 AimPosition { get { return _aimPosition; } }
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
            Vector2 currentWeaponPosition = new Vector2(currentWeapon.transform.position.x, currentWeapon.transform.position.y);
            Vector3 direction = (_aimPosition - currentWeaponPosition).normalized;

            _weapons[_currentWeaponIndex].transform.forward = direction;
        }
        #endregion
    }
}
