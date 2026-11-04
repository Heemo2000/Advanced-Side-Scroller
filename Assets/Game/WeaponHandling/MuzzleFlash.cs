using Game.CoreHandling;
using System;
using UnityEngine;
using Utilities.IOC;

namespace Game.WeaponHandling
{
    public class MuzzleFlash : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private ParticleSystem _muzzleFlashEffect;
        [SerializeField] private Transform _firePoint;
        #endregion

        #region Private Fields
        private Weapon _weapon;
        private ParticleSystemObjectPoolManager _poolManager;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            _weapon = GetComponent<Weapon>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _weapon.OnSingleUse += SpawnEffect;
            _weapon.OnContinousUse += SpawnEffect;
        }

        private void OnDestroy()
        {
            _weapon.OnSingleUse -= SpawnEffect;
            _weapon.OnContinousUse -= SpawnEffect;
        }
        #endregion

        #region Class Functionality
        private void SpawnEffect()
        {
            if(_poolManager == null)
            {
                _poolManager = ServiceLocator.ForSceneOf(this).Get<ParticleSystemObjectPoolManager>();
            }

            if(_poolManager == null)
            {
                return;
            }

            _poolManager.SpawnEffect(_muzzleFlashEffect, 
                                     _firePoint.position, 
                                     _firePoint.right);
        }
        #endregion
    }
}
