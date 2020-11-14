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
        if (Instance == this)
            Destroy(this);
    }

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        _audio.Play();
    }

    public void SetVolume(float val)
    {
        PlayerPrefs.SetFloat(VolumeKey, val);
    }
    
}
