using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Lighting))]
public class LightingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Lighting lighting = (Lighting)target;


        if (GUILayout.Button("Update Shader"))
        {
            lighting.UpdateShader();
        }

        DrawDefaultInspector();
    }
}
