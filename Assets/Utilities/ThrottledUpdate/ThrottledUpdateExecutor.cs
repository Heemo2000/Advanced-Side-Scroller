using System;
using UnityEngine;

namespace Utilities.ThrottledUpdate
{
    public class ThrottledUpdateExecutor
    {
        private float _currentTime = 0.0f;

        public void Execute(float updateTime, params Action[] actions)
        {
            _currentTime += Time.deltaTime;
            if (_currentTime >= updateTime)
            {
                _currentTime = 0.0f;
                foreach (Action action in actions)
                {
                    action?.Invoke();
                }
            }
        }
    }
}
