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
    }

    private void DrawCreateSceneBoundsObjectGroup(SceneAnalyser sceneAnalyser)
    {
        if (GUILayout.Button("Create SceneBounds GameObject"))
        {
            sceneAnalyser.CreateSceneBoundsObject();
        }
    }
}
