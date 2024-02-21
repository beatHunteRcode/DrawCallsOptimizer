using System.Linq;
using UnityEngine;

public class PolygonCountObjectAnalyzer : MonoBehaviour
{
    ObjectsInteractor objectsInteractor;

    void Start()
    {
        objectsInteractor = ScriptableObject.CreateInstance<ObjectsInteractor>();

        GameObject[] allSceneObjects = objectsInteractor.GetAllGameObjectsOnScene();
        foreach (GameObject obj in allSceneObjects)
        {
            if (obj.GetComponent<MeshFilter>() != null)
            {
                PrintInfoAboutObjectMesh(obj);
            }
        }
    }

    private void PrintInfoAboutObjectMesh(GameObject obj)
    {
        print(
            obj.name + ":\n" +
            "\tVertices:" + objectsInteractor.GetVerticesCount(obj) + "\n" +
            "\tTriangles:" + objectsInteractor.GetTrianglesCount(obj) + "\n"
        );
    }
}
