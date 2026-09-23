using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.IOC;

namespace Utilities.PauseHandling
{
    public class GamePauseManager : MonoBehaviour
    {
        #region Serialized Fields 
        #endregion

        #region Events
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        #endregion

        #region Static Fields 
        private static List<IPausable> PendingRegistrations;
        #endregion

        #region Private Fields
        private bool _isInitializing = false;
        private bool _gamePaused = false;
        private List<IPausable> _pausables;
        #endregion

        #region Properties
        public bool GamePaused { get => _gamePaused; }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            _isInitializing = true;
            _pausables = new List<IPausable>();
            _gamePaused = false;
        }

        void Start()
        {
            ServiceLocator.Global.Register(this);
            OnGameResumed += OnResumed;
            OnGamePaused += OnPaused;
            DoPendingRegistrations();
            ResumeGame();
            _isInitializing = false;
        }

        private void Update()
        {
            if (PendingRegistrations != null && PendingRegistrations.Count > 0)
            {
                //Debug.Log("Adding remaining pausables");
                foreach (IPausable pausable in PendingRegistrations)
                {
                    RegisterInstance(pausable);
                }

                PendingRegistrations.Clear();
            }
            else
            {
                //Debug.Log("Either PendingRegistrations is null or is empty");
                if (PendingRegistrations != null)
                {
                    //Debug.Log("Pending Registrations Count: " + PendingRegistrations.Count);
                }
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Global.Remove(this);

            OnGameResumed -= OnResumed;
            OnGamePaused -= OnPaused;
            CleanupStaticVariables();
        }

        #endregion

        #region Class Functionality


        /// <summary>
        /// Initializes the Pending Registrations list for static registration.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializePendingRegistrations()
        {
            PendingRegistrations = new List<IPausable>();
        }


        /// <summary>
        /// Registers the IPausable's on MonoBehaviour either through service locator or through static way.
        /// Requires atleast one IPausable on MonoBehaviour.
        /// </summary>
        /// <param name="mb"></param>
        public static void Register(MonoBehaviour mb)
        {

            IPausable[] pausables = mb.GetComponents<IPausable>();
            if (pausables == null)
            {
                //Debug.LogError("Can't find IPausable interfaces on " +  mb.gameObject.name);
                return;
            }


            GamePauseManager instance = ServiceLocator.Global.Get<GamePauseManager>();

            foreach (IPausable pausable in pausables)
            {
                if (instance != null)
                {
                    instance.RegisterInstance(pausable);
                }
                else
                {
                    GamePauseManager.RegisterStatically(pausable);
                }
            }
        }


        /// <summary>
        /// Registers the IPausable, and calls OnPause or OnResume based on whether a game is paused or not.
        /// </summary>
        /// <param name="pausable"></param>
        private void RegisterInstance(IPausable pausable)
        {

            //Debug.Log("Registering locally");

            _pausables.Add(pausable);

            if (_gamePaused)
            {
                pausable.OnPause();
            }
            else
            {
                pausable.OnResume();
            }

            //Debug.Log("Pausables Count: " + _pausables.Count);
        }

        /// <summary>
        /// Adds the IPausable reference statically.
        /// </summary>
        /// <param name="pausable"></param>
        private static void RegisterStatically(IPausable pausable)
        {
            //Debug.Log("Registering Statically");

            PendingRegistrations.Add(pausable);

            //Debug.Log("Pending Registrations Count after registering: " +  PendingRegistrations.Count);
        }

        /// <summary>
        /// Resumes the game.
        /// </summary>
        public void ResumeGame()
        {
            OnGameResumed?.Invoke();
        }

        /// <summary>
        /// Used to toggle between a game paused state or game resume state.
        /// </summary>
        public void Toggle()
        {
            if (_isInitializing)
            {
                return;
            }

            if (_gamePaused)
            {
                OnGameResumed?.Invoke();
            }
            else
            {
                OnGamePaused?.Invoke();
            }
        }


        /// <summary>
        /// Cleanes all the static variables during this instance destruction phase.
        /// </summary>
        private void CleanupStaticVariables()
        {
            if (PendingRegistrations == null)
            {
                return;
            }

            PendingRegistrations.Clear();
            PendingRegistrations = null;
        }

        /// <summary>
        /// Adds all the IPausable's from static list to actual _pausables list.
        /// </summary>
        private void DoPendingRegistrations()
        {
            if (PendingRegistrations == null)
            {
                return;
            }

            _pausables.AddRange(PendingRegistrations);
            PendingRegistrations.Clear();
        }


        /// <summary>
        /// Method which gets called when a game is resumed.
        /// It makes all IPausable's call OnResume.
        /// </summary>
        private void OnResumed()
        {
            _gamePaused = false;

            foreach (IPausable pausable in _pausables)
            {
                pausable.OnResume();
            }

            //Debug.Log("Pausable Count while resuming: " + _pausables.Count);
        }

        /// <summary>
        /// Method which gets called when a game is paused.
        /// It makes all IPausable's call OnPause.
        /// </summary>
        private void OnPaused()
        {
            _gamePaused = true;

            foreach (IPausable pausable in _pausables)
            {
                pausable.OnPause();
            }
            //Debug.Log("Pausable Count while pausing: " + _pausables.Count);
        }

        #endregion
    }
}
