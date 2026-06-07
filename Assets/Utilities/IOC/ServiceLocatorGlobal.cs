using UnityEngine;

namespace Utilities.IOC
{
    public class ServiceLocatorGlobal : Bootstrapper
    {
        #region Serialized Fields
        [SerializeField] private bool _dontDestroyOnLoad;
        #endregion

        #region Classs Functionality
        public override void Configure()
        {
            base.Container.ConfigureAsGlobal(_dontDestroyOnLoad);
        }
        #endregion
    }
}
