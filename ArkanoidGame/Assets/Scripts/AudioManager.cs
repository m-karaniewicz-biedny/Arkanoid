using UnityEngine.Audio;
using UnityEngine;
using System;

[Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0, 1)]
    public float sfxVolume = 1;

    public bool isMusic = false;

    [HideInInspector]
    public float lastStartTime;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public float globalSFXCooldown = 0.15f;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    public Sound[] sounds;

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        musicSource = new GameObject().AddComponent<AudioSource>();
        musicSource.transform.parent = transform;
        musicSource.loop = true;
        musicSource.volume = 0.5f;
        sfxSource = new GameObject().AddComponent<AudioSource>();
        sfxSource.transform.parent = transform;
        sfxSource.volume = 0.5f;
    }

    //System.Single instead of float to receive value from UI UnityEvent
    public void SetSFXVolume(System.Single volume)
    {
        sfxSource.volume = volume;
    }

    public void SetMusicVolume(System.Single volume)
    {
        musicSource.volume = volume;
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError($"No sound named \"{name}\".");
            return;
        }

        Debug.Log($"Playing {name}");
        if (!s.isMusic)
        {
            if(Time.time - s.lastStartTime > globalSFXCooldown)
            {
                sfxSource.PlayOneShot(s.clip, s.sfxVolume);
                s.lastStartTime = Time.time;
            }
        }
        else
        {
            musicSource.clip = s.clip;
            musicSource.Play();
        }


    }
}
