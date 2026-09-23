using Game.SoundHandling;
using Game.SoundHandling.GenericSoundManagement;
using Game.WeaponHandling;
using UnityEngine;
using Utilities.IOC;
using Utilities.PauseHandling;

namespace Game
{
    public class Shotgun : Weapon, IPausable
    {
        #region Constants
        private static float MaxGizmoLineDistance = 3.0f; 
        #endregion
        #region Serialized Fields

        [Header("Reload Settings:")]
        [SerializeField] private MultipleSoundPlayer _gunReloadingSoundPlayer;

        [Header("Bullet Settings:")]
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private Transform _firePoint;
        [Min(3)]
        [SerializeField] private int _maxShotsComingOut = 3;
        [Range(60.0f, 120.0f)]
        [SerializeField] private float _spreadRange = 90.0f;
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
        private BulletPoolManager _bulletPoolManager = null;
        private bool _isPaused = false;
        private SoundManager _soundManager = null;
        private float _currentDelta = 0.0f;
        private float _deltaToAdd = 0.0f;
        private float _currentAngle = 0.0f;
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
            if (_gunReloadingSoundPlayer != null)
            {
                _gunReloadingSoundPlayer.OnBeforeAnyAudioPlayed -= SetIsReloadingFlagToTrue;
                _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed -= SetIsReloadingFlagToFalse;
                _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed -= RefillMagazine;
                _gunReloadingSoundPlayer.OnAfterAllAudiosPlayed -= InvokeReloadEvent;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 top = _firePoint.position + Quaternion.AngleAxis(_spreadRange/2.0f, Vector3.forward) * _firePoint.right * MaxGizmoLineDistance;
            Vector3 down = _firePoint.position + Quaternion.AngleAxis(-_spreadRange / 2.0f, Vector3.forward) * _firePoint.right * MaxGizmoLineDistance;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_firePoint.position, top);
            Gizmos.DrawLine(_firePoint.position, down);
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
            if (_isPaused)
            {
                return;
            }

            if (_soundManager == null)
            {
                _soundManager = ServiceLocator.Global.Get<SoundManager>();
            }

            if (_currentAmmoCount == 0)
            {
                _soundManager.CreateSoundBuilder().WithPosition(transform.position).Play(_emptyAmmoSound);
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

            if (_currentAmmoCount > 0)
            {
                _currentDelta = 0.0f;
                _deltaToAdd = 1.0f / (float)(_maxShotsComingOut);
                _currentAngle = _spreadRange / 2.0f;

                while(_currentDelta <= 1.0f)
                {
                    _currentAngle = Mathf.Lerp(_spreadRange / 2.0f, -_spreadRange / 2.0f, _currentDelta);
                    
                    Bullet bullet = _bulletPoolManager.SpawnBullet(_bulletPrefab.GetInstanceID(), _firePoint.position);
                    bullet.Initialize(_firePoint.position);
                    bullet.transform.right = Quaternion.AngleAxis(_currentAngle, Vector3.forward) * _firePoint.right;
                    
                    _currentDelta += _deltaToAdd;
                }

                _soundManager.CreateSoundBuilder().WithPosition(transform.position).WithRandomPitch(_minShootingPitch, _maxShootingPitch).Play(_shootingSound);
                _currentAmmoCount--;
                OnSingleUse?.Invoke();
            }
            else
            {
                Reload();
            }
        }

        public override void ContinousUse()
        {
            
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
