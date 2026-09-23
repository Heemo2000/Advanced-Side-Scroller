#region Headers

using UnityEngine;

using Game.SoundHandling;
using Game.SoundHandling.GenericSoundManagement;

using Utilities.IOC;
using Utilities.PauseHandling;

#endregion

namespace Game.WeaponHandling
{
    public class Pistol :  Weapon, IPausable
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
        
        [Header("Sound Settings:")]
        [SerializeField] private SoundData _emptyAmmoSound;
        [SerializeField] private SoundData _shootingSound;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float _minShootingPitch = 0.6f;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float _maxShootingPitch = 1.5f;

        #endregion
        #region Properties

        #endregion

        #region Private Fields
        private bool _isReloading = false;
        private int _currentAmmoCount = 0;
        private float _currentFireTime = 0.0f;
        private BulletPoolManager _bulletPoolManager = null;
        private bool _isPaused = false;
        private SoundManager _soundManager = null;
        
        #endregion

        #region Unity Methods
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _gunReloadingSoundPlayer.OnBeforeAnyAudioPlayed += SetIsReloadingFlagToTrue;
            _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed += SetIsReloadingFlagToFalse;
            _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed += RefillMagazine;
            _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed += InvokeReloadEvent;
            _currentAmmoCount = _ammoCount;

            GamePauseManager.Register(this);
        }


        private void OnDestroy()
        {
            if(_gunReloadingSoundPlayer != null)
            {
                _gunReloadingSoundPlayer.OnBeforeAnyAudioPlayed -= SetIsReloadingFlagToTrue;
                _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed -= SetIsReloadingFlagToFalse;
                _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed -= RefillMagazine;
                _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed -= InvokeReloadEvent;
            }
        }
        #endregion

        #region Class Functionality

        public void OnPause()
        {
            _isPaused = true;
        }

        public void OnResume()
        {
            _isPaused = false;
        }

        public override void SingleUse()
        {
            if (_soundManager == null)
            {
                _soundManager = ServiceLocator.Global.Get<SoundManager>();
            }

            _soundManager.CreateSoundBuilder().WithPosition(transform.position).Play(_emptyAmmoSound);
        }

        public override void ContinousUse()
        {
            if (_isPaused)
            {
                return;
            }

            if (_soundManager == null)
            {
                _soundManager = ServiceLocator.Global.Get<SoundManager>();
            }

            if (_bulletPoolManager == null)
            {
                _bulletPoolManager = ServiceLocator.ForSceneOf(this).Get<BulletPoolManager>();
            }

            if (_bulletPoolManager == null)
            {
                return;
            }

            if (!_bulletPoolManager.IsPoolExists(_bulletPrefab))
            {
                _bulletPoolManager.CreateBulletPool(_bulletPrefab);
            }

            if (_currentFireTime < Time.time)
            {
                _currentFireTime = Time.time + _fireInterval;
                if (_currentAmmoCount > 0)
                {
                    Bullet bullet = _bulletPoolManager.SpawnBullet(_bulletPrefab.GetInstanceID(), _firePoint.position);
                    bullet.Initialize(_firePoint.position);
                    bullet.transform.right = _firePoint.right;
                    _soundManager.CreateSoundBuilder().WithPosition(transform.position).WithRandomPitch(_minShootingPitch, _maxShootingPitch).Play(_shootingSound);
                    _currentAmmoCount--;
                    OnContinousUse?.Invoke();
                }
                else
                {

                    Reload();
                }
            }
        }

        public override void Reload()
        {
            if (!_gunReloadingSoundPlayer.IsPlaying)
            {
                _gunReloadingSoundPlayer.Play();
            }
        }

        private void InvokeReloadEvent()
        {
            OnReload?.Invoke();
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
