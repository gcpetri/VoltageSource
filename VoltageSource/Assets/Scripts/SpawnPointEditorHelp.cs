using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SpawnPointEditorHelp : MonoBehaviour
{
    [SerializeField] private Vector3 drawedCubeSize;
    [SerializeField] private Color cubeColor;
    private void OnDrawGizmos()
    {
        Gizmos.color = cubeColor;
        Gizmos.DrawCube(this.transform.position + (Vector3.up * (drawedCubeSize.y * 0.5f)), drawedCubeSize);
    }
}
