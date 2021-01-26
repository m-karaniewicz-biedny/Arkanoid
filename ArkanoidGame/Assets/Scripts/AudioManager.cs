using UnityEngine.Audio;
using UnityEngine;
using System;

[Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0, 1)]
    public float volume = 1;
    [Range(0.1f, 3)]
    public float pitch = 1;

    internal AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Range(0,1)]
    public float globalVolumeScale = 0.5f;

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

        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.playOnAwake = false;
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }

    private void AdjustGlobalVolume(float volumeMultiplier)
    {
        globalVolumeScale = volumeMultiplier;
        foreach (Sound s in sounds)
        {
            s.source.volume *= volumeMultiplier;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s==null)
        {
            Debug.LogError($"No sound named \"{name}\".");
            return;
        }

        Debug.Log($"Playing {name}");
        s.source.Play();
    }
}
