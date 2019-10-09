using System.Collections;
using UnityEngine;

namespace ProjectQuorum.Audio
{

    public class AudioMgr : MonoBehaviour
    {
        public static AudioMgr Instance;

        [Header("Audio Settings")]
        public bool PlayMusicOnAwake = true;

        public float MusicVolume = 0.4f;
        public float SoundVolume = 1f;

        public bool MusicMuted = false;
        public bool FXMuted = false;


        [Header("Music Objects")]
        public GameObject MusicObject;

        public AudioClip IntroClip;

        public AudioClip MainLoopClip;

        [SerializeField]
        private AudioSource[] _musicSources;

        [Header("FX Objects")]
        public GameObject FXObject;

        [SerializeField]
        private AudioSource[] _fxSources;

        public void OnValidate()
        {
            _musicSources = MusicObject.GetComponents<AudioSource>();
            _fxSources = FXObject.GetComponents<AudioSource>();
        }

        public void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("Only one AudioMgr instance should exist!");
            }
            Instance = this;
        }

        public void Start()
        {
            if (PlayMusicOnAwake)
            {
                PlayMusic();
            }
        }

        private AudioSource GetFreeAudioSource(AudioSource[] sources)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                if (!sources[i].isPlaying)
                {
                    return sources[i];
                }
            }
            return null;
        }

        public void PlayMusic()
        {
            StartCoroutine(DoPlayMusic());
        }

        private IEnumerator DoPlayMusic()
        {
            if (IntroClip != null)
            {
                AudioSource intro = GetFreeAudioSource(_musicSources);
                if (intro != null)
                {
                    intro.clip = IntroClip;
                    intro.loop = false;
                    intro.volume = MusicVolume;
                    intro.Play();
                }

                // Subtract a frame for timing.
                yield return new WaitForSeconds(IntroClip.length - 4f);

                intro.volume = 0f;
            }

            AudioSource main = GetFreeAudioSource(_musicSources);
            if (main != null)
            {
                main.clip = MainLoopClip;
                main.loop = true;
                main.volume = MusicVolume;
                main.Play();
            }
        }

        public bool PlayFX(AudioClip clip, float volume = 1f)
        {
            if (FXMuted)
            {
                return false;
            }

            AudioSource source = GetFreeAudioSource(_fxSources);
            if (source != null)
            { 
                source.clip = clip;
                source.volume = volume * SoundVolume;
                source.Play();
                return true;
            }
            return false;
        }

        public void SetMusicVolume(float newVolume)
        {
            MusicVolume = newVolume;
            SetMusicClipsVolume(MusicVolume);
        }

        private void SetMusicClipsVolume(float newVolume)
        { 
            for (int i = 0; i < _musicSources.Length; i++)
            {
                if (_musicSources[i].isPlaying)
                {
                    _musicSources[i].volume = newVolume;
                }
            }  
        }

        public void SetSoundVolume(float newVolume)
        {
            SoundVolume = newVolume;
        }

        public void MuteMusic(bool mute)
        {
            MusicMuted = mute;
            if (mute)
            {
                SetMusicClipsVolume(0f);
            }
            else
            {
                SetMusicClipsVolume(MusicVolume);
            }
        }

        public void MuteFX(bool mute)
        {
            FXMuted = mute;
        }
    }

}