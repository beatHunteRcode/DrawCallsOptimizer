using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneInteractor
{
    ObjectsInteractor objectsInteractor = ScriptableObject.CreateInstance<ObjectsInteractor>();

    public GameObject CreateSceneBoundsObject()
    {
        GameObject gameObject = new GameObject();
        gameObject.name = "SceneBounds";
        gameObject.AddComponent<BoxCollider>();
        Bounds sceneBounds = objectsInteractor.GetSceneBounds();
        gameObject.transform.position = sceneBounds.center;
        gameObject.transform.localScale = sceneBounds.size;
        return gameObject;
    }
}
