using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGen))]
public class MapGenEditor : Editor
{

    public override void OnInspectorGUI()
    {
        MapGen mapGen = (MapGen)target;

        var style_delete = new GUIStyle(GUI.skin.button);
        style_delete.normal.textColor = Color.red;

        var style_terrain = new GUIStyle(GUI.skin.button);
        style_terrain.normal.textColor = Color.yellow;

        var style_visualise = new GUIStyle(GUI.skin.button);
        style_visualise.normal.textColor = Color.green;

        DrawDefaultInspector();

        //EditorGUI.DrawPreviewTexture(new Rect(275, 60, 100, 100), mapGen.texture);

        GUILayout.Label("Generate Terrain");

        if (GUILayout.Button("Generate Terrain", style_terrain))
        {
            mapGen.GenerateTerrain();
        }

        GUILayout.Label("Generate Trees");

        if (GUILayout.Button("Generate Trees", style_terrain))
        {
            mapGen.GenerateTrees();
        }

        if (GUILayout.Button("Delete Meshes", style_delete))
        {
            mapGen.DeleteMeshes();
        }

        GUILayout.Label("Visualise Noise");

        if (GUILayout.Button("Visualise", style_visualise))
        {
            mapGen.Visualise();
        }

        

        if (GUILayout.Button("Delete Visualiser", style_delete))
        {
            mapGen.DeleteVisualiser();
        }

        
    }
}
