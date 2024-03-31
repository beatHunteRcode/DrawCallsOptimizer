using System.Linq;
using UnityEngine;

public class PolygonCountObjectAnalyzer : MonoBehaviour
{
    ObjectsInteractor objectsInteractor;

    void Start()
    {
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
        Bounds sceneBounds = objectsInteractor.GetSceneBounds();
        gameObject.transform.position = sceneBounds.center;
        gameObject.transform.localScale = sceneBounds.size;
    }
}
