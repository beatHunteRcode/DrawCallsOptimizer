using System.Linq;
using UnityEngine;

public class PolygonCountObjectAnalyzer : MonoBehaviour
{
    SceneInteractor sceneInteractor;
    ObjectsInteractor objectsInteractor;

    void Start()
    {
        sceneInteractor = ScriptableObject.CreateInstance<SceneInteractor>();
        objectsInteractor = ScriptableObject.CreateInstance<ObjectsInteractor>();

        //GameObject[] allSceneObjects = objectsInteractor.GetAllGameObjectsOnScene();
        //foreach (GameObject obj in allSceneObjects)
        //{
        //    if (obj.GetComponent<MeshFilter>() != null)
        //    {
        //        PrintInfoAboutObjectMesh(obj);
        //    }
        //}

        CreateSceneBoundsObject();
    }

    private void PrintInfoAboutObjectMesh(GameObject obj)
    {
        print(
            obj.name + ":\n" +
            "\tVertices:" + objectsInteractor.GetVerticesCount(obj) + "\n" +
            "\tTriangles:" + objectsInteractor.GetTrianglesCount(obj) + "\n"
        );
    }

    private void CreateSceneBoundsObject()
    {
        GameObject gameObject = new GameObject();
        gameObject.name = "SceneBounds";
        gameObject.AddComponent<BoxCollider>();
        Bounds sceneBounds = sceneInteractor.GetSceneBounds();
        gameObject.transform.position = sceneBounds.center;
        gameObject.transform.localScale = sceneBounds.size;
    }
}
