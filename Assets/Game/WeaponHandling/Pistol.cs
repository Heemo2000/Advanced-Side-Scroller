#region Headers
using Game.SoundHandling;
using UnityEngine;
using Utilities.IOC;
#endregion

namespace Game.WeaponHandling
{
    public class Pistol : MonoBehaviour, IWeapon
    {
        #region Serialized Fields
        [Header("Reload Settings:")]
        [SerializeField] private MultipleSoundPlayer _gunReloadingSoundPlayer; 
        [Header("Bullet Settings:")]
        [SerializeField] private Bullet _bulletPrefab;
        [Min(0.1f)]
        [SerializeField] private float _fireInterval = 0.1f;
        [SerializeField] private Transform _firePoint;
        [Min(5)]
        [SerializeField] private int _ammoCount = 10;
        
        #endregion
        #region Properties
        public GameObject WeaponGameObject { get => gameObject; }

        #endregion

        #region Private Fields
        private bool _isReloading = false;
        private int _currentAmmoCount = 0;
        private BulletPoolManager _bulletPoolManager = null;
        #endregion

        #region Unity Methods
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _gunReloadingSoundPlayer.OnBeforeAnyAudioPlayed += SetIsReloadingFlagToTrue;
            _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed += SetIsReloadingFlagToFalse;
            _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed += RefillMagazine;
            _currentAmmoCount = _ammoCount;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnDestroy()
        {
            if(_gunReloadingSoundPlayer != null)
            {
                _gunReloadingSoundPlayer.OnBeforeAnyAudioPlayed -= SetIsReloadingFlagToTrue;
                _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed -= SetIsReloadingFlagToFalse;
                _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed -= RefillMagazine;
            }
        }
        #endregion

        #region Class Functionality

        public void Use()
        {
            if (_isReloading)
            {
                return;
            }

            if (_bulletPoolManager == null)
            {
                _bulletPoolManager = ServiceLocator.ForSceneOf(this).Get<BulletPoolManager>();
            }

            if(!_bulletPoolManager.IsPoolExists(_bulletPrefab))
            {
                _bulletPoolManager.CreateBulletPool(_bulletPrefab);
            }

            if(_currentAmmoCount > 0)
            {
                Bullet bullet = _bulletPoolManager.SpawnBullet(_bulletPrefab.GetInstanceID(), _firePoint.position);
                bullet.transform.right = _firePoint.right;
                _currentAmmoCount--;
            }
            else
            {
                Reload();
            }
        }

        public void Reload()
        {
            _gunReloadingSoundPlayer.Play();
        }

        private void SetIsReloadingFlagToFalse()
        {
            _isReloading = false;
        }

        private void SetIsReloadingFlagToTrue()
        {
            _isReloading = true;
        }

        private void RefillMagazine()
        {
            _currentAmmoCount = _ammoCount;
        }
        
        
        #endregion
    }
}
