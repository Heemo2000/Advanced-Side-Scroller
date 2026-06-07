using UnityEngine;
using Utilities.Extensions;

namespace Utilities.IOC
{
    [RequireComponent(typeof(ServiceLocator))]
    public abstract class Bootstrapper : MonoBehaviour
    {
        private ServiceLocator _serviceLocator;
        private bool _hasBeenBootstrapped = false;
        internal ServiceLocator Container => _serviceLocator.OrNull() ? _serviceLocator : _serviceLocator = GetComponent<ServiceLocator>();

        private void Awake()
        {
            DoBootstrapOnDemand();
        }
        public void DoBootstrapOnDemand()
        {
            if(!_hasBeenBootstrapped)
            {
                _hasBeenBootstrapped = true;
                Configure();
            }
        }
        public abstract void Configure();
        
    }
}
