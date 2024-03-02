using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(ObjectCombiner))]
public class ObjectCombinerEditor : Editor
{
    bool isCombineObjectsByPolygonsGroupOpened = false;
    bool isCombineObjectsByMaterialsGroupOpened = false;
    bool isCombineObjectsByTagsGroupOpened = false;

    public override void OnInspectorGUI()
    {
        ObjectCombiner objectCombiner = (ObjectCombiner)target;

        DrawCombineObjectsByPolygonsFoldoutGroup(objectCombiner);
        EditorGUILayout.Space();
        DrawCombineObjectsByMaterialsFoldoutGroup(objectCombiner);
        EditorGUILayout.Space();
        DrawCombineObjectsByTagsFoldoutGroup(objectCombiner);
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }


    private void DrawCombineObjectsByPolygonsFoldoutGroup(ObjectCombiner objectCombiner)
    {
        isCombineObjectsByPolygonsGroupOpened = EditorGUILayout.Foldout(isCombineObjectsByPolygonsGroupOpened, "Combine Objects by Polygons");

        if (isCombineObjectsByPolygonsGroupOpened && Selection.activeTransform)
        {
            objectCombiner.trianglesLimit = EditorGUILayout.IntField("Polygons limit", objectCombiner.trianglesLimit);

            if (GUILayout.Button("Combine Objects by Polygons"))
            {
                objectCombiner.CombineAllSceneObjectsByTriangles();
            }
        }

        if (!Selection.activeTransform)
        {
            isCombineObjectsByPolygonsGroupOpened = false;
        }
    }

    private void DrawCombineObjectsByMaterialsFoldoutGroup(ObjectCombiner objectCombiner)
    {

        isCombineObjectsByMaterialsGroupOpened = EditorGUILayout.Foldout(isCombineObjectsByMaterialsGroupOpened, "Combine Objects by Materials");

        if (isCombineObjectsByMaterialsGroupOpened && Selection.activeTransform)
        {
            if (GUILayout.Button("Combine Objects by Materials"))
            {
                objectCombiner.CombineAllSceneObjectsByMaterials();
            }
        }

        if (!Selection.activeTransform)
        {
            isCombineObjectsByMaterialsGroupOpened = false;
        }
    }

    private void DrawCombineObjectsByTagsFoldoutGroup(ObjectCombiner objectCombiner)
    {
        isCombineObjectsByTagsGroupOpened = EditorGUILayout.Foldout(isCombineObjectsByTagsGroupOpened, "Combine Objects by Tags");

        if (isCombineObjectsByTagsGroupOpened && Selection.activeTransform)
        {
            SerializedProperty tagsToCombineProperty = serializedObject.FindProperty("tagsToCombine");
            EditorGUILayout.PropertyField(tagsToCombineProperty, new GUIContent("Tags to Combine"));

            if (GUILayout.Button("Combine Objects by Tags"))
            {
                objectCombiner.CombineAllSceneObjectsByTags();
            }
        }

        if (!Selection.activeTransform)
        {
            isCombineObjectsByTagsGroupOpened = false;
        }
    }
}
