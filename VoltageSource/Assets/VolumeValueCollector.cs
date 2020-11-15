using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeValueCollector : MonoBehaviour
{
    
    
    public void SetVolume(float val)
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.SetVolume(val);
        }
    }
}
