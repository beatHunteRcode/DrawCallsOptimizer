using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectCombiner))]
public class ObjectCombinerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ObjectCombiner objectCombiner = (ObjectCombiner)target;

        objectCombiner.TrianglesLimit = EditorGUILayout.IntField("Polygons limit", objectCombiner.TrianglesLimit);

        if (GUILayout.Button("Combine Objects by Polygons"))
        {
            objectCombiner.CombineAllSceneObjectsByTriangles();
        }

        if (GUILayout.Button("Combine Objects by Materials"))
        {
            objectCombiner.CombineAllSceneObjectsByMaterials();
        }
    }
}
