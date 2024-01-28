using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    [SerializeField] private AudioMixerGroup defaultMixerOutput;

    List<AudioSource> audioSources;

    AudioSource         music;

    public static SoundManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SoundManager>();
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if ((_instance == null) || (_instance == this))
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        // Find all audio sources
        audioSources = new List<AudioSource>(GetComponentsInChildren<AudioSource>());
        if (audioSources == null)
        {
            audioSources = new List<AudioSource>();
        }
    }

    private AudioSource _PlaySound(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        var audioSource = GetSource();

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.outputAudioMixerGroup = defaultMixerOutput;

        audioSource.Play();

        return audioSource;
    }

    private AudioSource GetSource()
    {
        if (audioSources == null)
        {
            audioSources = new List<AudioSource>();
            return NewSource();
        }

        foreach (var source in audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        return NewSource();
    }

    private AudioSource NewSource()
    {
        GameObject go = new GameObject();
        go.name = "Audio Source";
        go.transform.SetParent(transform);

        var audioSource = go.AddComponent<AudioSource>();

        audioSources.Add(audioSource);

        return audioSource;
    }

    static public AudioSource PlaySound(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
    {
        return _instance._PlaySound(clip, volume, pitch);
    }

    static public void PlayMusic(AudioClip clip)
    {
        SoundManager mng = _instance;

        if (mng.music == null)
        {
            mng.music = PlaySound(clip);
            mng.music.loop = true;
        }
        else
        {
            if (clip != mng.music.clip)
            {
                mng.StartCoroutine(_instance.Crossfade(clip));
            }
        }
    }

    static public void StopMusic()
    {
        SoundManager mng = _instance;

        if (mng.music != null)
        {
            mng.StartCoroutine(_instance.Fadeout());
        }
    }

    IEnumerator Crossfade(AudioClip nextMusic)
    {
        AudioSource newMusic = _PlaySound(nextMusic, 0, 1.0f);
        newMusic.loop = true;

        while (music.volume > 0.0f)
        {
            music.volume = Mathf.Clamp01(music.volume - Time.deltaTime);
            newMusic.volume = 1.0f - music.volume;
            yield return null;
        }

        music.Stop();
        music.loop = false;
        music = newMusic;
    }
    IEnumerator Fadeout()
    {
        while (music.volume > 0.0f)
        {
            music.volume = Mathf.Clamp01(music.volume - Time.deltaTime);
            yield return null;
        }

        music.Stop();
        music.loop = false;
        music = null;
    }

    public static void ToggleSound(bool enable)
    {
        _instance.defaultMixerOutput.audioMixer.SetFloat("Volume", (enable) ? (0.0f) : (-80.0f));
    }
}
