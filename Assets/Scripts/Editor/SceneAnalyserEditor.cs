using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneAnalyser))]
public class SceneAnalyserEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SceneAnalyser sceneAnalyser = (SceneAnalyser)target;

        DrawCreateSceneBoundsObjectGroup(sceneAnalyser);
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCreateSceneBoundsObjectGroup(SceneAnalyser sceneAnalyser)
    {
        sceneAnalyser.SectionCount = EditorGUILayout.Vector3IntField("Chunks Count", sceneAnalyser.SectionCount);

        if (GUILayout.Button("Create SceneBounds GameObject"))
        {
            GameObject boundsCube = sceneAnalyser.CreateSceneBoundsObject();
            if (sceneAnalyser.SectionCount != Vector3Int.zero)
            {
                sceneAnalyser.DivideIntoChunks(boundsCube);
            }
        }
    }
}
