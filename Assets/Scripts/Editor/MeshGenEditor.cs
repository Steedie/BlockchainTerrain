using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshGen))]
public class MeshGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        MeshGen meshGen = (MeshGen)target;


        if (GUILayout.Button("Generate Chunk"))
        {
            meshGen.GenerateMesh();
        }

        if (GUILayout.Button("Destroy Mesh"))
        {
            meshGen.DestroyMesh();
        }

        DrawDefaultInspector();
    }
}
