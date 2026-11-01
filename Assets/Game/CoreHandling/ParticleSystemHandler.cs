using UnityEngine;
using Utilities.IOC;

namespace Game.CoreHandling
{
    public class ParticleSystemHandler : MonoBehaviour
    {
        #region Private Fields
        private ParticleSystem _particleSystem;
        private ParticleSystemObjectPoolManager _poolManager;

        #endregion

        #region Properties
        public int OriginalParticlePrefabID { get; set; } = -1;
        #endregion

        #region Unity Methods        
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            var main = _particleSystem.main;
            main.stopAction = ParticleSystemStopAction.Callback;
        }

        private void OnParticleSystemStopped()
        {
            if(_poolManager == null)
            {
                _poolManager = ServiceLocator.ForSceneOf(this).Get<ParticleSystemObjectPoolManager>();
            }
            _poolManager.ReturnToPool(this);
        }
        #endregion

        #region Class Functionality

        public void Stop()
        {
            _particleSystem.Stop();
        }

        public void Pause()
        {
            _particleSystem.Pause();
        }

        public void Play()
        {
            _particleSystem.Play();
        }

        #endregion
    }
}
