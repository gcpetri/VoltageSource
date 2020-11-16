using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private const string VolumeKey = "VolumeValue";
    private AudioSource _audio;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
            return;
        }
        if (Instance != null && Instance != this)
            Destroy(this);
    }

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.volume = PlayerPrefs.GetFloat(VolumeKey);
        _audio.Play();
    }

    public void SetVolume(float val)
    {
        PlayerPrefs.SetFloat(VolumeKey, val);
        if(_audio)
            _audio.volume = val;
        
        if(!_audio)
            Debug.LogWarning("AudioManager is missing reference to audiosource");
    }


    public void StopAudio()
    {
        _audio.Stop();
    }
    
    public void StartAudio()
    {
        _audio.Play();
    }

}
