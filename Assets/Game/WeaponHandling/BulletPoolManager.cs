#region Headers
using System.Collections.Generic;
using UnityEngine;
using Utilities.IOC;
using Utilities.ObjectPoolHandling;
using Utilities.ThrottledUpdate;
#endregion

namespace Game.WeaponHandling
{
    public class BulletPoolManager : MonoBehaviour
    {
        #region Constants
        private const float RegisterInstanceInterval = 1.0f;
        #endregion
        #region Serialized Fields
        [Min(10)]
        [SerializeField] private int _maxBulletCount = 100;
        #endregion
        #region Private Fields

        private Dictionary<int, ObjectPool<Bullet>> _bulletDict;
        private ThrottledUpdateExecutor _registerInstanceUpdateExecutor;
        private bool _registeredInstance = false;
        #endregion

        #region Unity Methods

        private void Awake()
        {
            _bulletDict = new Dictionary<int, ObjectPool<Bullet>>();
            _registerInstanceUpdateExecutor = new ThrottledUpdateExecutor();
        }

        private void Update()
        {
            _registerInstanceUpdateExecutor.Execute(RegisterInstanceInterval, RegisterInstance);
        }

        #endregion

        #region Class Functionality

        public bool IsPoolExists(Bullet prefab)
        {
            return _bulletDict.ContainsKey(prefab.GetInstanceID());
        }

        public void CreateBulletPool(Bullet prefab)
        {
            int prefabID = prefab.GetInstanceID();
            if (_bulletDict.ContainsKey(prefabID))
            {
                return;
            }

            _bulletDict[prefabID] = new ObjectPool<Bullet>(()=> CreateBullet(prefab),
                                                                         OnBulletGet,
                                                                         OnBulletReturnToPool,
                                                                         OnBulletDestroy,
                                                                         _maxBulletCount);
        }


        public Bullet SpawnBullet(int prefabID, Vector2 position)
        {
            if (_bulletDict.ContainsKey(prefabID))
            {
                if (_bulletDict[prefabID].IsSpaceThereInPool())
                {
                    Bullet bulletInstance = _bulletDict[prefabID].Get();
                    bulletInstance.transform.position = position;
                    
                    return bulletInstance;
                }

                Debug.Log($"No bullet in the pool with prefab ID: {prefabID}");
                return null;

            }

            Debug.Log($"No pool for the prefab ID: {prefabID}");
            return null;
        }


        public void ReturnToPool(int prefabID, Bullet bulletInstance)
        {
            if(_bulletDict.ContainsKey(prefabID))
            {
                _bulletDict[prefabID].ReturnToPool(bulletInstance);
                return;
            }

            Debug.LogError($"No pool exists for bullet prefab ID: {prefabID} ({bulletInstance.GetType().FullName})");
            Destroy(bulletInstance);
        }


        private Bullet CreateBullet(Bullet prefab)
        {
            Bullet bullet = Instantiate(prefab, transform);
            bullet.gameObject.SetActive(false);
            return bullet;
        }

        private void OnBulletGet(Bullet bullet)
        {
            bullet.gameObject.SetActive(true);
        }

        private void OnBulletReturnToPool(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
        }

        private void OnBulletDestroy(Bullet bullet)
        {
            Destroy(bullet);
        }

        private void RegisterInstance()
        {
            if(_registeredInstance)
            { 
                return; 
            }

            if(ServiceLocator.ForSceneOf(this) != null)
            {
                _registeredInstance = true;
                ServiceLocator.ForSceneOf(this).Register(this);
            }
        }
        
        #endregion
    }
}
