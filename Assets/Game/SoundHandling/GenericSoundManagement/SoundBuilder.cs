using UnityEngine;

namespace Game.SoundHandling.GenericSoundManagement
{
    public class SoundBuilder
    {
        readonly SoundManager soundManager;
        private Vector3 _position = Vector3.zero;
        private float _pitch = 1.0f;
        public SoundBuilder(SoundManager soundManager)
        {
            this.soundManager = soundManager;
        }

        public SoundBuilder WithPosition(Vector3 position)
        {
            this._position = position;
            return this;
        }

        public SoundBuilder WithRandomPitch(float minPitch = 0.1f, float maxPitch = 0.2f)
        {
            this._pitch = Random.Range(minPitch, maxPitch + 0.1f);
            return this;
        }

        public void Play(SoundData soundData)
        {
            if (soundData == null)
            {
                Debug.LogError("SoundData is null");
                return;
            }

            if (!soundManager.CanPlaySound(soundData)) return;

            SoundEmitter soundEmitter = soundManager.Get();
            soundEmitter.Initialize(soundData);
            soundEmitter.transform.position = _position;
            soundEmitter.Data.pitch = _pitch;
            soundEmitter.transform.parent = soundManager.transform;

            if (soundData.frequentSound)
            {
                soundEmitter.Node = soundManager.FrequentSoundEmitters.AddLast(soundEmitter);
            }

            soundEmitter.Play();
        }
    }
}
