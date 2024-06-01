using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneAnalyzer))]
public class SceneAnalyzerEditor : Editor
{
    private const int LARGE_SPACER_WIDTH = 3;

    public override void OnInspectorGUI()
    {
        SceneAnalyzer sceneAnalyzer = (SceneAnalyzer)target;

        EditorGUILayout.LabelField("Select objects combine methods");

        using (new ScopedIndent(1))
        {
            DrawCombineObjectsByPolygonsGroup(sceneAnalyzer);
            DrawSpacer(1);
        }

        using (new ScopedIndent(1))
        {
            DrawCombineObjectsByMaterialsGroup(sceneAnalyzer);
            DrawSpacer(1);
        }

        using (new ScopedIndent(1))
        {
            DrawCombineObjectsByTagsGroup(sceneAnalyzer);
            DrawSpacer(1);
        }

        using (new ScopedIndent(1))
        {
            DrawCombineObjectsByDistanceGroup(sceneAnalyzer);
            DrawSpacer(1);
        }

        DrawSpacer(LARGE_SPACER_WIDTH);

#pragma warning disable 0618
        sceneAnalyzer.boundsObject = EditorGUILayout.ObjectField("Bounds Object", sceneAnalyzer.boundsObject, typeof(GameObject)) as GameObject;

        sceneAnalyzer.sectionCount = EditorGUILayout.Vector3IntField("Chunks Count", sceneAnalyzer.sectionCount);

#pragma warning disable 0618
        sceneAnalyzer.chunkMaterial = EditorGUILayout.ObjectField("Chunk Material", sceneAnalyzer.chunkMaterial, typeof(Material)) as Material;

        DrawSpacer(LARGE_SPACER_WIDTH);

        using (new FixedWidthLabel("Analyze only Static objects"))
        {
            sceneAnalyzer.analyzeOnlyStaticObjects = EditorGUILayout.Toggle(sceneAnalyzer.analyzeOnlyStaticObjects);
        }

        using (new FixedWidthLabel("Destroy Scene Bounds object after optimizations\t"))
        {
            sceneAnalyzer.destroySceneBoundsObjectAfterOptimizations = EditorGUILayout.Toggle(sceneAnalyzer.destroySceneBoundsObjectAfterOptimizations);
        }

        using (new FixedWidthLabel("Save analyzed Objects before optimizations\t"))
        {
            sceneAnalyzer.saveAnalyzedObjectsBeforeOptimizations = EditorGUILayout.Toggle(sceneAnalyzer.saveAnalyzedObjectsBeforeOptimizations);
        }

        DrawSpacer(LARGE_SPACER_WIDTH);

        if (GUILayout.Button("Optimize Objects on Scene"))
        {
            ObjectsInfoHolder.objectsCreatedByScript.Clear();
            sceneAnalyzer.OptimizeObjectsOnScene();
        }

        if (ObjectsInfoHolder.originalObjects.Count > 0)
        {
            if (GUILayout.Button("Enable all original Objects"))
            {
                sceneAnalyzer.EnableAllOriginalOptimizedObjects();
                ObjectsInfoHolder.originalObjects.Clear();
            }
        }

        if (ObjectsInfoHolder.objectsCreatedByScript.Count > 0)
        {
            if (GUILayout.Button("Destroy all Objects created by SceneAnalyzer script"))
            {
                sceneAnalyzer.DestroyAllObjectsCreatedByScript();
                ObjectsInfoHolder.objectsCreatedByScript.Clear();
            }
        }

        DrawSpacer(LARGE_SPACER_WIDTH);

        DrawTerrainsOptimizationGroup(sceneAnalyzer);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCombineObjectsByPolygonsGroup(SceneAnalyzer sceneAnalyser)
    {
        using (new FixedWidthLabel("Combine Objects by Polygons\t"))
        {
            sceneAnalyser.combineObjectsByPolygonsNeeded = EditorGUILayout.Toggle(sceneAnalyser.combineObjectsByPolygonsNeeded);
        }

        if (sceneAnalyser.combineObjectsByPolygonsNeeded)
        {
            using (new ScopedIndent(2))
            {
                EditorGUILayout.HelpBox(Documentation.COMBINE_OBJECTS_BY_POLYGONS_TEXT, MessageType.Info);

                using (new FixedWidthLabel("Chunk Objects Polygons Threshold\t"))
                {
                    sceneAnalyser.chunkObjectsPolygonsThreshold = EditorGUILayout.IntField(sceneAnalyser.chunkObjectsPolygonsThreshold);
                }
            }
        }
    }

    private void DrawCombineObjectsByMaterialsGroup(SceneAnalyzer sceneAnalyser)
    {
        using (new FixedWidthLabel("Combine Objects by Materials\t"))
        {
            sceneAnalyser.combineObjectsByMaterialsNeeded = EditorGUILayout.Toggle(sceneAnalyser.combineObjectsByMaterialsNeeded);
        }
       
        if (sceneAnalyser.combineObjectsByMaterialsNeeded)
        {
            using (new ScopedIndent(2))
            {
                EditorGUILayout.HelpBox(Documentation.COMBINE_OBJECTS_BY_MATERIALS_TEXT, MessageType.Info);

                using (new FixedWidthLabel("Chunk Objects With Same Material Threshold\t"))
                {
                    sceneAnalyser.chunkObjectsWithSameMaterialThreshold = EditorGUILayout.IntField(sceneAnalyser.chunkObjectsWithSameMaterialThreshold);
                }
            }
        }
    }

    private void DrawCombineObjectsByTagsGroup(SceneAnalyzer sceneAnalyser)
    {
        using (new FixedWidthLabel("Combine Objects by Tags\t"))
        {
            sceneAnalyser.combineObjectsByTagsNeeded = EditorGUILayout.Toggle(sceneAnalyser.combineObjectsByTagsNeeded);
        }

        if (sceneAnalyser.combineObjectsByTagsNeeded)
        {
            using (new ScopedIndent(2))
            {
                EditorGUILayout.HelpBox(Documentation.COMBINE_OBJECTS_BY_TAGS_TEXT, MessageType.Info);

                SerializedProperty tagsToCombineProperty = serializedObject.FindProperty("tagsToCombine");
                EditorGUILayout.PropertyField(tagsToCombineProperty, new GUIContent("Tags to Combine"));
            }
        }
    }

    private void DrawCombineObjectsByDistanceGroup(SceneAnalyzer sceneAnalyser)
    {
        using (new FixedWidthLabel("Combine Objects by Distance\t"))
        {
            sceneAnalyser.combineObjectsByDistanceNeeded = EditorGUILayout.Toggle(sceneAnalyser.combineObjectsByDistanceNeeded);
        }

        if (sceneAnalyser.combineObjectsByDistanceNeeded)
        {
            using (new ScopedIndent(2))
            {
                EditorGUILayout.HelpBox(Documentation.COMBINE_OBJECTS_BY_DISTANCE_TEXT, MessageType.Info);

                using (new FixedWidthLabel("Objects Distance Threshold\t"))
                {
                    sceneAnalyser.distanceLimit = EditorGUILayout.FloatField(sceneAnalyser.distanceLimit);
                }

                SerializedProperty collectionOfObjectsToCombineByDistance = serializedObject.FindProperty("collectionsOfObjectsToCombineByDistance");
                EditorGUILayout.PropertyField(collectionOfObjectsToCombineByDistance, new GUIContent("Collections of Objects to Combine"));
            }
        }
    }

    private void DrawTerrainsOptimizationGroup(SceneAnalyzer sceneAnalyser)
    {
        SerializedProperty collectionOfTerrainsToOptimize = serializedObject.FindProperty("terrainsToOptimize");
        EditorGUILayout.PropertyField(collectionOfTerrainsToOptimize, new GUIContent("Terrains to Optimize"));

        if (!Array.Exists(sceneAnalyser.terrainsToOptimize, terrain => terrain == null) && sceneAnalyser.terrainsToOptimize.Length > 0)
        {
            if (GUILayout.Button("Create Tree Objects From Terrains"))
            {
                sceneAnalyser.CreateTreeObjectsFromTerrain(sceneAnalyser.terrainsToOptimize);
            }
            if (GUILayout.Button("Delete All Trees From Terrains"))
            {
                sceneAnalyser.DeleteAllTreesFromTerrain(sceneAnalyser.terrainsToOptimize);
            }
            if (GUILayout.Button("Restore All Terrains Trees"))
            {
                sceneAnalyser.RestoreAllTerrainTrees(sceneAnalyser.terrainsToOptimize);
            }
        }
    }

    private void DrawSpacer(int spacerWidth)
    {
        for (int i = 0; i < spacerWidth; i++)
        {
            EditorGUILayout.Space();
        }
    }
}

public class ScopedIndent : IDisposable
{
    private readonly int delta;
    public ScopedIndent(int delta)
    {
        this.delta = delta;
        EditorGUI.indentLevel += delta;
    }

    public void Dispose()
    {
        EditorGUI.indentLevel -= delta;
    }
}
