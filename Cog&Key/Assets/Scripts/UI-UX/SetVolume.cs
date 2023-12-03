using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetVolume : MonoBehaviour
{
    // The audio mixer the slider should change
    public AudioMixer mixer;

    public void Start()
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(PlayerPrefs.GetFloat("MusicVolume" + mixer.name, 1) * 20));
    }

    /// <summary>
    /// Uses volume to the factor of 10 (since in decibels for mixer) 
    /// and puts it into the proper scale
    /// </summary>
    /// <param name="value">The value from the slider</param>
    public void SetLevel(float value)
    {
        //(-80 + value * 80)
        mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MusicVolume" + mixer.name, value);
        PlayerPrefs.Save();
    }
}
