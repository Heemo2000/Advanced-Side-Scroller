#region Headers
//Language headers
using System.Collections.Generic;

//Engine Headers
using UnityEngine;

//Utility Headers
using Utilities.ObjectPoolHandling;
using Utilities.IOC;
using Utilities.ThrottledUpdate;
#endregion
namespace Game.SoundHandling.GenericSoundManagement
{
    public class SoundManager : MonoBehaviour
    {
        #region Constants
        private const float RegisterInstanceTimeInterval = 0.5f;
        #endregion

        #region Serialized Fields
        [SerializeField] private SoundEmitter _soundEmitterPrefab;
        [SerializeField] private int _maxPoolSize = 30;
        [SerializeField] private int _maxSoundInstances;
        #endregion

        #region Private Fields
        private ObjectPool<SoundEmitter> _soundEmitterPool;
        private ThrottledUpdateExecutor _registerInstanceUpdateExecutor;
        private bool _registeredInstance = false;
        #endregion

        #region Readonly Fields
        readonly List<SoundEmitter> aliveSoundEmitters = new();
        
        public readonly LinkedList<SoundEmitter> FrequentSoundEmitters = new();
        #endregion


        #region Unity Methods

        private void Awake()
        {
            _registerInstanceUpdateExecutor = new ThrottledUpdateExecutor();
        }
        void Start()
        {
            InitializePool();
        }

        private void Update()
        {
            _registerInstanceUpdateExecutor.Execute(RegisterInstanceTimeInterval, RegisterInstance);
        }

        private void OnDestroy()
        {
            ServiceLocator.Global.Remove(this);
        }

        #endregion


        #region Class Functionality
        public SoundBuilder CreateSoundBuilder() => new SoundBuilder(this);

        public bool CanPlaySound(SoundData data)
        {
            if (!data.frequentSound) return true;

            if (FrequentSoundEmitters.Count >= _maxSoundInstances)
            {
                try
                {
                    FrequentSoundEmitters.First.Value.Stop();
                    return true;
                }
                catch
                {
                    Debug.Log("SoundEmitter is already released");
                }
                return false;
            }
            return true;
        }

        public SoundEmitter Get()
        {
            return _soundEmitterPool.Get();
        }

        public void ReturnToPool(SoundEmitter soundEmitter)
        {
            _soundEmitterPool.ReturnToPool(soundEmitter);
        }

        public void StopAll()
        {
            
            foreach (var soundEmitter in aliveSoundEmitters)
            {
                soundEmitter.Stop();
            }

            FrequentSoundEmitters.Clear();
        }

        void InitializePool()
        {
            _soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                _maxPoolSize);
        }

        SoundEmitter CreateSoundEmitter()
        {
            var soundEmitter = Instantiate(_soundEmitterPrefab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }

        void OnTakeFromPool(SoundEmitter soundEmitter)
        {
            soundEmitter.gameObject.SetActive(true);
            aliveSoundEmitters.Add(soundEmitter);
        }

        void OnReturnedToPool(SoundEmitter soundEmitter)
        {
            if (soundEmitter.Node != null)
            {
                FrequentSoundEmitters.Remove(soundEmitter.Node);
                soundEmitter.Node = null;
            }
            soundEmitter.gameObject.SetActive(false);
            aliveSoundEmitters.Remove(soundEmitter);
        }

        void OnDestroyPoolObject(SoundEmitter soundEmitter)
        {
            Destroy(soundEmitter.gameObject);
        }

        private void RegisterInstance()
        {
            if(!_registeredInstance)
            {
                ServiceLocator.Global.Register(this);
                _registeredInstance = true;
            }
        }

        #endregion
    }
}
