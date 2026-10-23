#region Headers
using System;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace Utilities.ObjectPoolHandling
{
    public class ObjectPool<T>
    {
        #region Private Fields
        private Func<T> _onCreate;
        private Action<T> _onGetObj;
        private Action<T> _onObjReturnToPool;
        private Action<T> _onDestroyObj;

        private Queue<T> _poolQueue;
        #endregion

        #region Class Functionality
        public ObjectPool(Func<T> onCreate, Action<T> onGetObj, Action<T> onObjReturnToPool, Action<T> onDestroyObj, int initialGenerateCount)
        {
            _onCreate = onCreate;
            _onGetObj = onGetObj;
            _onObjReturnToPool = onObjReturnToPool;
            _onDestroyObj = onDestroyObj;
            
            _poolQueue = new Queue<T>();

            for(int i = 0; i < initialGenerateCount; i++)
            {
                T obj = onCreate();
                _poolQueue.Enqueue(obj);
            }
        }

        public T Get()
        {
            if(_poolQueue.Count == 0)
            {
                Debug.LogError("Pool is empty, return default of type " + typeof(T).FullName);
                T newObj = _onCreate();
                _onGetObj?.Invoke(newObj);
                return newObj;
            }

            T obj = _poolQueue.Dequeue();
            _onGetObj?.Invoke(obj);
            return obj;
        }


        public bool IsSpaceThereInPool()
        {
            return _poolQueue.Count > 0;
        }


        public void ReturnToPool(T obj)
        {
            _onObjReturnToPool?.Invoke(obj);
            _poolQueue.Enqueue(obj);
        }

        public void Destroy()
        {
            while (_poolQueue.Count > 0)
            {
                T obj = _poolQueue.Dequeue();
                _onDestroyObj?.Invoke(obj);
            }
        }

        #endregion
    }
}
