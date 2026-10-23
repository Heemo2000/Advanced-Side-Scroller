using System.Collections.Generic;
using UnityEngine;
using Utilities.IOC;
using Utilities.ObjectPoolHandling;
using Utilities.PauseHandling;
using Utilities.ThrottledUpdate;

namespace Game.CoreHandling
{
    public class ParticleSystemObjectPoolManager : MonoBehaviour, IPausable
    {
        #region Constants
        private const float RegisterInstanceInterval = 0.5f;
        #endregion

        #region Serialized Fields
        [Min(20)]
        [SerializeField] private int _initialPoolSize = 25;
        #endregion

        #region Private Fields
        private Dictionary<int, ObjectPool<ParticleSystemHandler>> _particlesLookup;
        private List<ParticleSystemHandler> _activeParticleHandlers;
        private ThrottledUpdateExecutor _registerInstanceExecutor;
        private bool _instanceRegistered = false;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            _particlesLookup = new Dictionary<int, ObjectPool<ParticleSystemHandler>>();
            _activeParticleHandlers = new List<ParticleSystemHandler>();
            _registerInstanceExecutor = new ThrottledUpdateExecutor();
        }

        private void Start()
        {
            GamePauseManager.Register(this);
        }

        private void Update()
        {
            if (!_instanceRegistered)
            {
                _registerInstanceExecutor.Execute(RegisterInstanceInterval, RegisterInstance);
            }
        }

        #endregion

        #region Class Functionality

        public void SpawnEffect(ParticleSystem particleSystem, Vector3 position)
        {
            if(!_particlesLookup.ContainsKey(particleSystem.GetInstanceID()))
            {
                AddObjectPool(particleSystem);
            }

            ParticleSystemHandler particleSystemHandler = _particlesLookup[particleSystem.GetInstanceID()].Get();
            _activeParticleHandlers.Add(particleSystemHandler);
            particleSystemHandler.transform.position = position;
            particleSystemHandler.Play();
        }

        public void ReturnToPool(ParticleSystemHandler particleSystem)
        {
            _activeParticleHandlers.Remove(particleSystem);

            if(particleSystem.OriginalParticlePrefabID == -1)
            {
                Destroy(particleSystem.gameObject);
                return;
            }

            _particlesLookup[particleSystem.OriginalParticlePrefabID].ReturnToPool(particleSystem);
        }

        private void AddObjectPool(ParticleSystem particleEffect)
        {
            _particlesLookup.Add(particleEffect.GetInstanceID(),
                                 new ObjectPool<ParticleSystemHandler>(()=> CreateParticle(particleEffect),
                                                                OnGetParticle,
                                                                OnReturnParticle,
                                                                OnDestroyParticle,
                                                                _initialPoolSize));
        }

        private void RegisterInstance()
        {
            ServiceLocator.ForSceneOf(this).Register(this);
            _instanceRegistered = true;
        }

        private ParticleSystemHandler CreateParticle(ParticleSystem particleEffectOriginal)
        {
            ParticleSystem particleSystem = Instantiate(particleEffectOriginal, transform);
            if (!particleSystem.gameObject.TryGetComponent<ParticleSystemHandler>(out ParticleSystemHandler handler))
            {
                handler = particleSystem.gameObject.AddComponent<ParticleSystemHandler>();
            }

            handler.OriginalParticlePrefabID = particleEffectOriginal.GetInstanceID();
            handler.gameObject.SetActive(false);
            return handler;
        }

        private void OnGetParticle(ParticleSystemHandler particleEffectHandler)
        {
            particleEffectHandler.gameObject.SetActive(true);
        }

        private void OnReturnParticle(ParticleSystemHandler particleSystemHandler)
        {
            particleSystemHandler.Stop();
            particleSystemHandler.gameObject.SetActive(false);
        }

        private void OnDestroyParticle(ParticleSystemHandler particleSystemHandler)
        {
            Destroy(particleSystemHandler.gameObject);
        }

        public void OnPause()
        {
            foreach(var handler in _activeParticleHandlers)
            {
                handler.Pause();
            }
        }

        public void OnResume()
        {
            foreach (var handler in _activeParticleHandlers)
            {
                handler.Play();
            }
        }

        #endregion
    }
}
