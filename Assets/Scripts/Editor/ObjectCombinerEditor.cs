using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.UIElements;

[CustomEditor(typeof(ObjectCombiner))]
public class ObjectCombinerEditor : Editor
{
    bool isCombineObjectsByPolygonsGroupOpened = false;
    bool isCombineObjectsByMaterialsGroupOpened = false;
    bool isCombineObjectsByTagsGroupOpened = false;
    bool isCombineObjectsByDistanceGroupOpened = false;

    public override void OnInspectorGUI()
    {
        ObjectCombiner objectCombiner = (ObjectCombiner)target;

        DrawCombineObjectsByPolygonsFoldoutGroup(objectCombiner);
        EditorGUILayout.Space();
        DrawCombineObjectsByMaterialsFoldoutGroup(objectCombiner);
        EditorGUILayout.Space();
        DrawCombineObjectsByTagsFoldoutGroup(objectCombiner);
        EditorGUILayout.Space();
        DrawCombineObjectsByDistanceFoldoutGroup(objectCombiner);
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }


    private void DrawCombineObjectsByPolygonsFoldoutGroup(ObjectCombiner objectCombiner)
    {
        isCombineObjectsByPolygonsGroupOpened = EditorGUILayout.Foldout(isCombineObjectsByPolygonsGroupOpened, "Combine Objects by Polygons");

        if (isCombineObjectsByPolygonsGroupOpened && Selection.activeTransform)
        {
            EditorGUILayout.HelpBox("Combines all objects on scene if sum of all objects polygons (triangles) are greater than Polygons limit\n" +
                "1. Enter polygons limit\n" +
                "2. Hit \"Combine Objects by Polygons\" button", 
                MessageType.Info);


            objectCombiner.trianglesLimit = EditorGUILayout.IntField("Polygons limit", objectCombiner.trianglesLimit);

            if (GUILayout.Button("Combine Objects by Polygons"))
            {
                objectCombiner.CombineObjectsByTriangles();
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
            EditorGUILayout.HelpBox("All objects with specific materials will combine in one GameObject\n" +
                "1. Hit \"Combine Objects by Materials\" button",
                MessageType.Info);

            if (GUILayout.Button("Combine Objects by Materials"))
            {
                objectCombiner.CombineObjectsByMaterials();
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
            EditorGUILayout.HelpBox("All objects with specific tag will combine in one GameObject\n" +
                "1. Enter amount of Tags to combine\n" +
                "2. Select each specific Tag\n" +
                "3. Hit \"Combine Objects by Tags\" button",
                MessageType.Info);

            SerializedProperty tagsToCombineProperty = serializedObject.FindProperty("tagsToCombine");
            EditorGUILayout.PropertyField(tagsToCombineProperty, new GUIContent("Tags to Combine"));

            if (GUILayout.Button("Combine Objects by Tags"))
            {
                objectCombiner.CombineObjectsByTags();
            }
        }

        if (!Selection.activeTransform)
        {
            isCombineObjectsByTagsGroupOpened = false;
        }
    }

    private void DrawCombineObjectsByDistanceFoldoutGroup(ObjectCombiner objectCombiner)
    {
        isCombineObjectsByDistanceGroupOpened = EditorGUILayout.Foldout(isCombineObjectsByDistanceGroupOpened, "Combine Objects by Distance");

        if (isCombineObjectsByDistanceGroupOpened && Selection.activeTransform)
        {
            EditorGUILayout.HelpBox("Combines all child-objects in specific GameObject-collection if distances between all chils-objects pivots in collection are greater than Distance limit\n" +
                "1. Enter Distance limit\n" +
                "2. Enter amount of collections to work with\n" +
                "3. Select each specific collection (or Drag'n'Drop )\n" +
                "3. Hit \"Combine Objects by Distance\" button",
                MessageType.Info);

            objectCombiner.distanceLimit = EditorGUILayout.FloatField("Disnance limit", objectCombiner.distanceLimit);

            SerializedProperty collectionOfObjectsToCombineByDistance = serializedObject.FindProperty("collectionOfObjectsToCombineByDistance");
            EditorGUILayout.PropertyField(collectionOfObjectsToCombineByDistance, new GUIContent("Collections of Objects to Combine"));

            if (GUILayout.Button("Combine Objects by Distance"))
            {
                objectCombiner.CombineObjectsByDistance();
            }
        }

        if (!Selection.activeTransform)
        {
            isCombineObjectsByDistanceGroupOpened = false;
        }
    }
}
