using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(World))]
public class WorldEditor : Editor
{

    public override void OnInspectorGUI()
    {
        World world = (World)target;


        if (GUILayout.Button("Generate Chunks"))
        {
            world.GenerateChunks();
        }

        DrawDefaultInspector();

        if (GUILayout.Button("Check Thread Status"))
        {
            world.CheckThreadStatus();
        }

        if (GUILayout.Button("Abort Thread"))
        {
            world.AbortThread();
        }

        if (GUILayout.Button("Draw Chunks"))
        {
            world.ToggleDrawChunks();
        }

        if (GUILayout.Button("Get Block Info"))
        {
            world.GetBlockInfo();
        }
    }
}
