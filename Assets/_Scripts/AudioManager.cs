using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager> {

    //public AudioMixer masterMixer { get { return _masterMixer; } }

    //[SerializeField] private AudioMixer _masterMixer;

    [SerializeField] private Sound[] sounds;

    protected override void Awake() {
        base.Awake();

        foreach (Sound sound in sounds) {
            sound.source = gameObject.AddComponent<AudioSource>();
            AudioSource source = sound.source;

            source.clip = sound.clip;
            //source.outputAudioMixerGroup = sound.outputAudioMixerGroup;

            source.volume = sound.volume;
            source.pitch = sound.pitch;

            source.loop = sound.loop;
            source.playOnAwake = sound.playOnAwake;
        }
    }

    void Start() {
        Play("BGM");    
    }

    private Sound GetSound(string name) {
        Sound sound = Array.Find(sounds, s => s.name == name);

        if (sound == null) { Debug.LogWarning("Sound: " + name + " not found!"); return null; }
        else return sound;
    }

    /// <summary>
    /// Plays an audio file as a song.
    /// </summary>
    public void Play(string name) {
        Sound sound = GetSound(name);
        sound.source.Play();
    }

    /// <summary>
    /// Plays an audio file as a one-shot.
    /// </summary>
    public void PlayOneShot(string name) {
        Sound sound = GetSound(name);
        sound.source.PlayOneShot(sound.clip);
    }

    public void Stop(string name) {
        Sound sound = GetSound(name);
        sound.source.Stop();
    }

    public void StopAllSounds() {
        foreach (Sound sound in sounds) sound.source.Stop();
    }

}