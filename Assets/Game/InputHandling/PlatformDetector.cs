#region Headers
using System.Runtime.InteropServices;
using UnityEngine;
#endregion

namespace Game.InputHandling
{
    public class PlatformDetector : MonoBehaviour
    {
        #region Class Functionality

        #if UNITY_WEBGL && !UNITY_EDITOR
            [DllImport("__Internal")]
        private static extern int IsMobileBrowser();
        #endif

        public static bool IsMobile()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                return IsMobileBrowser() == 1;
            #else
                return Application.isMobilePlatform;
            #endif
        }

        #endregion
    }
}
