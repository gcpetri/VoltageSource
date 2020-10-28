using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cutieCamera : MonoBehaviour
{
    public Camera cam;

    private void Start()
    {
        cam.depth = Camera.main.depth + 1;
    }
}
