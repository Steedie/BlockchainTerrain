using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioMixer audioMixer;

    [Header("Menu")]
    public AudioSource musicAudioSource;
    public float maxMusicVolume = .15f;
    public float musicFadeSpeed = .5f;
    public bool playMusic = true;
    public int activeAudio = 0;

    [Space]

    public Transform soundEffectsTransform;
    public int maxAudioSources = 5;
    public List<AudioSource> audioSources = new List<AudioSource>();

    [Space]

    public List<AudioClip> blockBreakingClips = new List<AudioClip>();

    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        HandleMusicVolume();
    }

    private void HandleMusicVolume()
    {
        if (playMusic)
        {
            if (musicAudioSource.volume != maxMusicVolume)
                musicAudioSource.volume = Mathf.Lerp(musicAudioSource.volume, maxMusicVolume + .025f, Time.fixedDeltaTime * musicFadeSpeed);
        }
        else if (musicAudioSource.volume > 0)
        {
            musicAudioSource.volume = Mathf.Lerp(musicAudioSource.volume, -.025f, Time.fixedDeltaTime * musicFadeSpeed);
        }

        musicAudioSource.volume = Mathf.Clamp(musicAudioSource.volume, 0, maxMusicVolume);
    }

    public void PlayAudio(AudioClip clip, Vector3 position)
    {
        AudioSource thisAudioSource = null;

        if (audioSources.Count < maxAudioSources)
        {
            GameObject footstep = new GameObject($"audio ({clip.name})");
            footstep.transform.parent = soundEffectsTransform;
            thisAudioSource = footstep.AddComponent<AudioSource>();
            thisAudioSource.volume = .05f;
            thisAudioSource.spatialBlend = 1f;
            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups("Master");
            thisAudioSource.outputAudioMixerGroup = audioMixerGroups[0];
            audioSources.Add(footstep.GetComponent<AudioSource>());
        }
        else
        {
            thisAudioSource = audioSources[activeAudio];
            audioSources[activeAudio].gameObject.name = $"audio ({clip.name})";
            //audioSources[0] = audioSources[audioSources.Count - 1];

            activeAudio++;
            if (activeAudio >= maxAudioSources)
                activeAudio = 0;
        }

        thisAudioSource.transform.position = position;
        thisAudioSource.clip = clip;
        
        thisAudioSource.Play();
    }
}
