using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FpController))]
public class TesterScript : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        FpController myScript = (FpController)target;
        if(GUILayout.Button("Subtract 10 Health"))
        {
            myScript.LoseHealth();
        }
    }
}
