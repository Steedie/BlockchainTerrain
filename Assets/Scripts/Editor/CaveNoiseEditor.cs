using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CaveNoise))]
public class CaveGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        CaveNoise caveNoise = (CaveNoise)target;


        if (GUILayout.Button("Generate"))
        {
            caveNoise.Make3dNoise();
        }

        DrawDefaultInspector();
    }
}
