using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound {

    public string name;

    [HideInInspector]
    public AudioSource source;

    public AudioClip clip;
    //public AudioMixerGroup outputAudioMixerGroup;

    [Range(0, 1)] public float volume = 1;
    [Range(.1f, 3)] public float pitch = 1;

    public bool loop, playOnAwake;

}