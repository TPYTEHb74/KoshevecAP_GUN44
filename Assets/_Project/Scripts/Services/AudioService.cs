using UnityEngine;
using UnityEngine.Audio;

namespace Game.Services
{
    public class AudioService : MonoBehaviour
    {
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;

        public void SetMasterVolume(float normalizedValue) => SetMixerVolume("MasterVol", normalizedValue);
        public void SetMusicVolume(float normalizedValue) => SetMixerVolume("MusicVol", normalizedValue);
        public void SetSfxVolume(float normalizedValue) => SetMixerVolume("SFXVol", normalizedValue);

        public void PlaySfxRandomized(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _sfxSource == null)
            {
                return;
            }

            _sfxSource.pitch = Random.Range(0.9f, 1.1f);
            _sfxSource.PlayOneShot(clip, volume);
        }

        private void SetMixerVolume(string parameterName, float normalizedValue)
        {
            if (_mixer == null)
            {
                return;
            }

            float clamped = Mathf.Clamp(normalizedValue, 0.0001f, 1f);
            _mixer.SetFloat(parameterName, Mathf.Log10(clamped) * 20f);
        }
    }
}
