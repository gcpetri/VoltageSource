using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private const string VolumeKey = "VolumeValue";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void SetVolume(float val)
    {
        PlayerPrefs.SetFloat(VolumeKey, val);
    }
    
}
