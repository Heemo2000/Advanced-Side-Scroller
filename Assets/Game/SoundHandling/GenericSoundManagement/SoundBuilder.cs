using UnityEngine;

namespace Game.SoundHandling.GenericSoundManagement
{
    public class SoundBuilder
    {
        readonly SoundManager soundManager;
        Vector3 position = Vector3.zero;

        public SoundBuilder(SoundManager soundManager)
        {
            this.soundManager = soundManager;
        }

        public SoundBuilder WithPosition(Vector3 position)
        {
            this.position = position;
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
            soundEmitter.transform.position = position;
            soundEmitter.transform.parent = soundManager.transform;

            if (soundData.frequentSound)
            {
                soundEmitter.Node = soundManager.FrequentSoundEmitters.AddLast(soundEmitter);
            }

            soundEmitter.Play();
        }
    }
}
