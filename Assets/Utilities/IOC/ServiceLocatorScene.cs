using UnityEngine;

namespace Utilities.IOC
{
    public class ServiceLocatorScene : Bootstrapper
    {
        #region Class Functionality
        public override void Configure()
        {
            base.Container.ConfigureAsScene();
        }

        #endregion
    }
}
