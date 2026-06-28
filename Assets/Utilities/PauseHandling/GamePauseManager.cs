using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.IOC;

namespace Utilities.PauseHandling
{
    public class GamePauseManager : MonoBehaviour
    {
        public event Action OnGamePaused;
        public event Action OnGameResumed;

        private static List<IPausable> PendingRegistrations;
        private List<IPausable> _pausables;
        private bool _gamePaused = false;

        private void Awake()
        {
            _pausables = new List<IPausable>();
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _gamePaused = false;
            ServiceLocator.ForSceneOf(this).Register(this);
            OnGamePaused += PausePausables;
            OnGameResumed += ResumePausables;
            CompletePendingPausableRegistrations();
            ResumeGame();
        }

        private void OnDestroy()
        {
            OnGamePaused -= PausePausables;
            OnGameResumed -= ResumePausables;
            ClearStaticVariables();
        }

        public void Register(IPausable pausable)
        {
            _pausables.Add(pausable);

            if(_gamePaused)
            {
                pausable.OnPause();
            }
            else
            {
                pausable.OnResume();
            }
        }

        public static void RegisterStatically(IPausable pausable)
        {
            if(PendingRegistrations == null)
            {
                PendingRegistrations = new List<IPausable>();
                Debug.Log("Creating new PendingRegistrations List");
            }

            Debug.Log("Adding new pausable to PendingRegistrations");
            PendingRegistrations.Add(pausable);
        }

        public void ResumeGame()
        {
            _gamePaused = false;
            OnGameResumed?.Invoke();
        }

        public void PauseGame()
        {
            _gamePaused = true;
            OnGamePaused?.Invoke();
        }

        public void Toggle()
        {
            _gamePaused = !_gamePaused;
            if (_gamePaused)
            {
                OnGamePaused?.Invoke();
            }
            else
            {
                OnGameResumed?.Invoke();
            }
        }

        private void CompletePendingPausableRegistrations()
        {
            if(PendingRegistrations != null)
            {
                foreach(var pausable in PendingRegistrations)
                {
                    Debug.Log("Adding " + pausable.GetType().FullName + " to Pending Registrations");
                    _pausables.Add(pausable);
                }
                PendingRegistrations.Clear();
            }
        }

        private static void ClearStaticVariables()
        {
            if(PendingRegistrations != null)
            {
                PendingRegistrations.Clear();
                PendingRegistrations = null;
            }
        }

        private void ResumePausables()
        {
            foreach(IPausable pausable in _pausables)
            {
                pausable.OnResume();
            }
        }

        private void PausePausables()
        {
            foreach (IPausable pausable in _pausables)
            {
                pausable.OnPause();
            }
        }

        
    }
}
