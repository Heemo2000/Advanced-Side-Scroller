using UnityEngine;
using UnityEngine.Audio;

namespace Game.SoundHandling.GenericSoundManagement
{
    [CreateAssetMenu(fileName = "Sound Data", menuName = "SOs/Sound Data")]
    public class SoundData : ScriptableObject
    {
        public AudioClip[] clips;
        public AudioMixerGroup mixerGroup;
        public bool loop;
        public bool playOnAwake;
        public bool frequentSound;

        public bool mute;
        public bool bypassEffects;
        public bool bypassListenerEffects;
        public bool bypassReverbZones;

        public int priority = 128;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range (0f, 3f)]
        public float pitch = 1f;
        public float panStereo;
        public float spatialBlend;
        public float reverbZoneMix = 1f;
        public float dopplerLevel = 1f;
        public float spread;

        public float minDistance = 1f;
        public float maxDistance = 500f;

        public bool ignoreListenerVolume;
        public bool ignoreListenerPause;

        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    }
}
