using UnityEngine;

namespace Utilities.Generic
{
    public class FPSModifier : MonoBehaviour
    {
        [Range(10, 60)]
        [SerializeField] private int _targetFrameRate = 30;

        private void Start()
        {
            Application.targetFrameRate = _targetFrameRate;
        }
        private void OnValidate()
        {
            if(!Application.isPlaying)
            {
                return;
            }

            Application.targetFrameRate = _targetFrameRate;
        }
    }
}
