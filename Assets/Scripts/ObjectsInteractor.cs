using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using UnityEngine;

public class ObjectsInteractor : ScriptableObject
{
    public GameObject[] GetAllGameObjectsOnScene()
    {
        return FindObjectsOfType<GameObject>();
    }

    public int GetVerticesCount(GameObject obj)
    {
        return obj.GetComponent<MeshFilter>().sharedMesh.vertices.Count();
    }

    public int GetTrianglesCount(GameObject obj)
    {
        return obj.GetComponent<MeshFilter>().sharedMesh.triangles.Count() / 3;
    }

    public void DeleteAllChildren(GameObject obj)
    {
        while (obj.transform.childCount > 0)
        {
            DestroyImmediate(obj.transform.GetChild(0).gameObject);
        }
    }
}
