using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ObjectCombiner))]
public class ObjectCombinerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ObjectCombiner objectCombiner = (ObjectCombiner)target;

        objectCombiner.trianglesLimit = EditorGUILayout.IntField("Polygons limit", objectCombiner.trianglesLimit);

        if (GUILayout.Button("Combine Objects by Polygons"))
        {
            objectCombiner.CombineAllSceneObjectsByTriangles();
        }

        if (GUILayout.Button("Combine Objects by Materials"))
        {
            objectCombiner.CombineAllSceneObjectsByMaterials();
        }

        SerializedProperty tagsToCombineProperty = serializedObject.FindProperty("tagsToCombine");
        EditorGUILayout.PropertyField(tagsToCombineProperty, new GUIContent("Tags to Combine"));

        if (GUILayout.Button("Combine Objects by Tags"))
        { 
            objectCombiner.CombineAllSceneObjectsByTags();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
