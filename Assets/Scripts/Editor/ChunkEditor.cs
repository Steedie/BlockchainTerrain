using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor
{

    public override void OnInspectorGUI()
    {
        Chunk chunk = (Chunk)target;


        /*if (GUILayout.Button("Generate Chunks"))
        {
            world.GenerateChunks();
        }*/

        DrawDefaultInspector();

        if (GUILayout.Button("Check Thread Status"))
        {
            //chunk.CheckThreadStatus();
        }

        if (GUILayout.Button("Abort Thread"))
        {
            //chunk.AbortThread();
        }
    }
}
