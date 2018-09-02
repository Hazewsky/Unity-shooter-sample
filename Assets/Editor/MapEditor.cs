using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapEditor : Editor {
    public override void OnInspectorGUI()
    {
        //target - from customEditor
        MapGenerator map = target as MapGenerator;

        //if value in inspector changed
        if (DrawDefaultInspector())
        {
            // redraws every second base.OnInspectorGUI();
            map.GenerateMap();
        }
        //Button
        if(GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
      
    }
}
