using UnityEngine;

public class SceneAnalyser : MonoBehaviour
{
    SceneInteractor sceneInteractor;


    void Start()
    {
        sceneInteractor = new SceneInteractor();
    }

    public void CreateSceneBoundsObject()
    {
        sceneInteractor.CreateSceneBoundsObject();
    }
}
