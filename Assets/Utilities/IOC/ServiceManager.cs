using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities.IOC
{
    public class ServiceManager
    {
        #region Read Only Variables
        readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        #endregion

        #region Properties
        public IEnumerable<object> RegisteredServices => _services.Values;

        #endregion

        #region Class Functionality
        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object obj))
            {
                service = obj as T;
                return true;
            }

            service = null;
            return false;
        }

        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object obj))
            {
                return obj as T;
            }

            Debug.LogError($"ServiceManager.Get: Service of type {type.FullName} not registered");
            return null;
        }

        public ServiceManager Register<T>(T service)
        {
            Type type = typeof(T);

            if (!_services.TryAdd(type, service))
            {
                Debug.LogError($"ServiceManager.Register: Service of type {type.FullName} already registered");
            }

            return this;
        }

        public ServiceManager Register(Type type, object service)
        {
            if (!type.IsInstanceOfType(service))
            {
                throw new ArgumentException("Type of service does not match type of service interface", nameof(service));
            }

            if (!_services.TryAdd(type, service))
            {
                Debug.LogError($"ServiceManager.Register: Service of type {type.FullName} already registered");
            }

            return this;
        }

        public bool Remove<T>(T service) where T: class
        {
            if(!_services.ContainsKey(service.GetType()))
            {
                Debug.LogError("Service to remove does not exists in the first place");
                return false;
            }

            _services.Remove(service.GetType());
            return true;
        }

        public bool IsServiceExists<T>(T service) where T: class
        {
            return _services.ContainsKey(service.GetType());
        }

        #endregion
    }
}
