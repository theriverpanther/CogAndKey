using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetVolume : MonoBehaviour
{
    // The audio mixer the slider should change
    public AudioMixer mixer;

    /// <summary>
    /// Uses volume to the factor of 10 (since in decibels for mixer) 
    /// and puts it into the proper scale
    /// </summary>
    /// <param name="value">The value from the slider</param>
    public void SetLevel(float value)
    {
        mixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
    }
}
