
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Utilities.IOC
{
    public class ServiceLocator : MonoBehaviour
    {
        #region Constants
        private const string ServiceLocatorGlobalName = "Service Locator[Global]";
        private const string ServiceLocatorSceneName = "Service Locator[Scene]";
        #endregion

        #region Private Fields

        private static ServiceLocator _global;
        private static Dictionary<Scene, ServiceLocator> _sceneServiceLocators = new Dictionary<Scene, ServiceLocator>();
        private ServiceManager _serviceManager = new ServiceManager();

        #endregion

        #region Properties
        
        public static ServiceLocator Global
        {
            get
            {
                if(_global == null)
                {
                    if(FindFirstObjectByType<ServiceLocatorGlobal>() is { } found)
                    {
                        found.DoBootstrapOnDemand();
                        return _global;
                    }

                    GameObject globalSLGameObject = new GameObject(ServiceLocatorGlobalName);
                    ServiceLocatorGlobal globalSLInstance = globalSLGameObject.AddComponent<ServiceLocatorGlobal>();
                    globalSLInstance.DoBootstrapOnDemand();

                    return _global;
                }

                return _global;
            }
        }
        
        #endregion

        #region Class Functionality

        public ServiceLocator Register<T>(T service)
        {
            _serviceManager.Register(service);
            return this;
        }

        public T Get<T>() where T: class
        {
            return _serviceManager.Get<T>();
        }

        public bool TryGet<T>(out T service) where T : class
        {
            return _serviceManager.TryGet<T>(out service);
        }

        public bool Remove<T>(T service) where T : class
        {
            return _serviceManager.Remove(service);
        }

        public bool IsServiceExists<T>(T service) where T : class
        {
            return _serviceManager.IsServiceExists<T>(service);
        }

        public static ServiceLocator For(MonoBehaviour mb)
        {
            ServiceLocator sceneSL = ForSceneOf(mb);
            if(sceneSL != null)
            {
                return sceneSL;
            }

            return Global;
        }

        public static ServiceLocator ForSceneOf(MonoBehaviour mb)
        {
            Scene scene = mb.gameObject.scene;

            if (_sceneServiceLocators.TryGetValue(scene, out ServiceLocator sl))
            {
                return sl;
            }

            Debug.LogError("Could not find service locator for the scene " + scene.name);

            return null;
        }

        internal void ConfigureAsGlobal(bool dontDestroyOnLoad)
        {
            if(_global == this)
            {
                Debug.LogError("ServiceLocator:ConfigureAsGlobal():: Already configured as global");
            }
            else if(_global != null)
            {
                Debug.LogError("ServiceLocator:ConfigureAsGlobal():: Another instance already configured as global");
            }
            else
            {
                _global = this;
                if(dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(_global.gameObject);
                }
            }
        }

        internal void ConfigureAsScene()
        {
            if(this == _global)
            {
                return;
            }

            Scene scene = gameObject.scene;

            if(_sceneServiceLocators.ContainsKey(scene))
            {
                Debug.LogError("ServiceLocator:ConfigureAsScene():: The service locator was already initialized for scene " + scene.name);
                return;
            }
            _sceneServiceLocators.Add(scene, this);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _global = null;
            _sceneServiceLocators = new Dictionary<Scene, ServiceLocator>();
        }

        #if UNITY_EDITOR

        [MenuItem("GameObject/ServiceLocator/Add Global")]        
        
        private static void AddGlobalServiceLocator()
        {
            GameObject globalSL = new GameObject(ServiceLocatorGlobalName, typeof(ServiceLocatorGlobal));
        }


        [MenuItem("GameObject/ServiceLocator/Add Scene")]
        private static void AddSceneServiceLocator()
        {
            GameObject sceneSL = new GameObject(ServiceLocatorSceneName, typeof(ServiceLocatorScene));
        }


        #endif

        #endregion

        #region Unity Methods

        private void OnDestroy()
        {
           if(_global == this)
           {
              _global = null;
           }
           else if(_sceneServiceLocators.ContainsKey(this.gameObject.scene))
           {
              _sceneServiceLocators.Remove(this.gameObject.scene);
           }
        }

        #endregion
    }
}
