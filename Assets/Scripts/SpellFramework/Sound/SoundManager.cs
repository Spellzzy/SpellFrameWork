using SpellFramework.Tools;
using UnityEngine;

namespace SpellFramework.Sound
{
    public class SoundManager : Singleton<SoundManager>
    {
        // 音效资源绑定对象
        private GameObject _soundManager;

        private AudioSource _bgMusicSource;
        private AudioSource _effectMusicSource;

        public float BgVolume = 0.8f;
        public float EffectVolume = 1.0f;

        public void Init()
        {
            _soundManager = new GameObject("SoundManager");
            _soundManager.AddComponent<AudioListener>();
            Object.DontDestroyOnLoad(_soundManager);
            
            // 背景音乐节点
            var bgTrans = _soundManager.transform.Find("bgMusic");
            if (bgTrans == null)
            {
                _bgMusicSource = CreateAudioSource("bgMusic");
                _bgMusicSource.loop = true;
                // _bgMusicSource.outputAudioMixerGroup = 
            }
            else
            {
                _bgMusicSource = bgTrans.GetComponent<AudioSource>();
            }

            // 音效播放节点
            var effectTrans = _soundManager.transform.Find("effect");
            if (effectTrans == null)
            {
                _effectMusicSource = CreateAudioSource("sound");
                _effectMusicSource.loop = false;
            }
            else
            {
                _effectMusicSource = effectTrans.GetComponent<AudioSource>();
            }

            UpdateAudio();
        }

        private void UpdateAudio()
        {
            _bgMusicSource.volume = BgVolume;
            _effectMusicSource.volume = EffectVolume;
        }

        private AudioSource CreateAudioSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_soundManager.transform);

            var audioSource = go.AddComponent<AudioSource>();
            audioSource.priority = 0;

            return audioSource;
        }

        public void PlayBackground(AudioClip clip)
        {
            _bgMusicSource.clip = clip;
            _bgMusicSource.Play();
        }

        public void StopBackground()
        {
            _bgMusicSource.Stop();;
        }
        
        public void PlaySound(AudioClip clip)
        {
            // _effectMusicSource.clip = clip;
            _effectMusicSource.PlayOneShot(clip);
        }

        public void StopSound()
        {
            _effectMusicSource.Stop();
        }

        public void StopAll()
        {
            StopBackground();
            StopSound();
        }

    }
}