#region Headers
//Language Headers
using System;
using System.Collections;

//Engine Headers
using UnityEngine;

//Utility Headers
using Utilities.IOC;
using Utilities.PauseHandling;

#endregion
namespace Game.SoundHandling
{
    [RequireComponent(typeof(AudioSource))]
    public class MultipleSoundPlayer : MonoBehaviour, IPausable
    {
        #region Serialized Fields
        [SerializeField] private AudioClip[] clipsToPlay;
        #endregion

        #region Private Fields
        private AudioSource _audioSource;
        private Coroutine _playCoroutine;
        private bool _isPaused = false;
        private bool _isPlaying = false;
        #endregion

        #region Properties
        public bool IsPlaying { get => _isPlaying; }
        #endregion

        #region Events

        public Action OnBeforeAnyAudioPlayed;
        public Action OnAfterAllAudiosPlayed;

        #endregion

        #region Unity Methods
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            GamePauseManager.Register(this);
        }

        #endregion


        #region Class Functionality

        public void OnPause()
        {
            _isPaused = true;
            _audioSource.Pause();
        }

        public void OnResume()
        {
            _isPaused = false;
            _audioSource.UnPause();
        }

        public void Play()
        {
            if(_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
            }

            _playCoroutine = StartCoroutine(PlayClips());
        }

        private IEnumerator PlayClips()
        {
            _isPlaying = true;
            
            OnBeforeAnyAudioPlayed?.Invoke();

            foreach(var clip in clipsToPlay)
            {
                _audioSource.clip = clip;
                _audioSource.Play();
                yield return new WaitWhile(()=>  _audioSource.isPlaying || _isPaused);
                _audioSource.Stop();
            }

            OnAfterAllAudiosPlayed?.Invoke();

            _isPlaying = false;
        }

        #endregion
    }
}
