using UnityEditor;
using UnityEngine;

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
    }
}
