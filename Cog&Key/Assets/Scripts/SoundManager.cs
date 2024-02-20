using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SoundKeywordPair
{
    public AudioClip soundClip;
    public string identifier;
}

public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundKeywordPair[] sounds;
    public static SoundManager Instance { get; private set; }

    private Dictionary<string, AudioClip> soundList;
    private List<AudioSource> activeSounds;

    void Awake() {
        if(Instance != null) {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        activeSounds = new List<AudioSource>();
        soundList = new Dictionary<string, AudioClip>();
        foreach(SoundKeywordPair sound in sounds) {
            soundList[sound.identifier] = sound.soundClip;
        }
    }

    void Update() {
        for(int i = activeSounds.Count - 1; i >= 0; i--) {
            if(!activeSounds[i].isPlaying) {
                Destroy(activeSounds[i]);
                activeSounds.RemoveAt(i);
            }
        }
    }

    public void PlaySound(string identifier, float volume = 0.3f) {
        if(!soundList.ContainsKey(identifier)) {
            Debug.Log("Sound clip not found.");
        }

        AudioSource soundClip = gameObject.AddComponent<AudioSource>();
        soundClip.clip = soundList[identifier];
        soundClip.volume = volume;
        soundClip.Play();
        activeSounds.Add(soundClip);
    }
}
