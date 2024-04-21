using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneAnalyser))]
public class SceneAnalyserEditor : Editor
{
    private const int LARGE_SPACER_WIDTH = 3;

    public override void OnInspectorGUI()
    {
        SceneAnalyser sceneAnalyser = (SceneAnalyser)target;

        EditorGUILayout.LabelField("Select objects combine methods");

        using (new ScopedIndent(1))
        {
            DrawCombineObjectsByPolygonsGroup(sceneAnalyser);
            DrawSpacer(1);
        }

        using (new ScopedIndent(1))
        {
            DrawCombineObjectsByMaterialsGroup(sceneAnalyser);
            DrawSpacer(1);
        }

        using (new ScopedIndent(1))
        {
            DrawCombineObjectsByTagsGroup(sceneAnalyser);
            DrawSpacer(1);
        }

        using (new ScopedIndent(1))
        {
            DrawCombineObjectsByDistanceGroup(sceneAnalyser);
            DrawSpacer(1);
        }

        DrawSpacer(LARGE_SPACER_WIDTH);

        sceneAnalyser.sectionCount = EditorGUILayout.Vector3IntField("Chunks Count", sceneAnalyser.sectionCount);

#pragma warning disable 0618
        sceneAnalyser.chunkMaterial = EditorGUILayout.ObjectField("Chunk Material", sceneAnalyser.chunkMaterial, typeof(Material)) as Material;

        DrawSpacer(LARGE_SPACER_WIDTH);

        using (new FixedWidthLabel("Analyze only Static objects"))
        {
            sceneAnalyser.analyzeOnlyStaticObjects = EditorGUILayout.Toggle(sceneAnalyser.analyzeOnlyStaticObjects);
        }

        using (new FixedWidthLabel("Destroy Scene Bounds object after optimizations\t"))
        {
            sceneAnalyser.destroySceneBoundsObjectAfterOptimizations = EditorGUILayout.Toggle(sceneAnalyser.destroySceneBoundsObjectAfterOptimizations);
        }

        using (new FixedWidthLabel("Save analyzed Objects before optimizations\t"))
        {
            sceneAnalyser.saveAnalyzedObjectsBeforeOptimizations = EditorGUILayout.Toggle(sceneAnalyser.saveAnalyzedObjectsBeforeOptimizations);
        }

        DrawSpacer(LARGE_SPACER_WIDTH);

        if (GUILayout.Button("Optimize Objects on Scene"))
        {
            ObjectsInfoHolder.objectsCreatedByScript.Clear();
            sceneAnalyser.OptimizeObjectsOnScene();
        }

        if (ObjectsInfoHolder.originalObjects.Count > 0)
        {
            if (GUILayout.Button("Enable all original Objects"))
            {
                sceneAnalyser.EnableAllOriginalOptimizedObjects();
                ObjectsInfoHolder.originalObjects.Clear();
            }
        }

        if (ObjectsInfoHolder.objectsCreatedByScript.Count > 0)
        {
            if (GUILayout.Button("Destroy all Objects created by SceneAnalyzer script"))
            {
                sceneAnalyser.DestroyAllObjectsCreatedByScript();
                ObjectsInfoHolder.objectsCreatedByScript.Clear();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCombineObjectsByPolygonsGroup(SceneAnalyser sceneAnalyser)
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

    private void DrawCombineObjectsByMaterialsGroup(SceneAnalyser sceneAnalyser)
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

    private void DrawCombineObjectsByTagsGroup(SceneAnalyser sceneAnalyser)
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

    private void DrawCombineObjectsByDistanceGroup(SceneAnalyser sceneAnalyser)
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
