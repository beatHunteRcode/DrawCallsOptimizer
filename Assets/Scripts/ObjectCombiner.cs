using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.CompilerServices;
using System.Linq;

public class ObjectCombiner : MonoBehaviour
{
    public int TrianglesLimit = 0;

    private ObjectsInteractor ObjectsInteractor;

    void Start()
    {
        ObjectsInteractor = ScriptableObject.CreateInstance<ObjectsInteractor>();
    }

    public void CombineAllSceneObjectsByTriangles()
    {
        List<GameObject> allSceneObjects = ObjectsInteractor.GetAllGameObjectsOnScene().ToList();
        int allObjectsPolygonsCount = 0;
        List<GameObject> objectsWithMeshFilter = GetAllObjectsWithMeshFilter(allSceneObjects);

        if (allObjectsPolygonsCount >= TrianglesLimit)
        {
            GameObject parentObj = new GameObject();
            foreach (GameObject obj in objectsWithMeshFilter)
            {
                obj.transform.SetParent(parentObj.transform);
            }
            parentObj.AddComponent<MeshCombiner>();
            MeshCombiner meshCombiner = parentObj.GetComponent<MeshCombiner>();
            meshCombiner.CombineMeshes(false);
            parentObj.name = "PARENT";
            ObjectsInteractor.DeleteAllChildren(parentObj);
        }
    }

    public void CombineAllSceneObjectsByMaterials()
    {
        Dictionary<Material, List<GameObject>> materialsToObjects = new();
        List<GameObject> allSceneObjects = ObjectsInteractor.GetAllGameObjectsOnScene().ToList();
        List<GameObject> objectsWithMeshFilter = GetAllObjectsWithMeshFilter(allSceneObjects);
        foreach (GameObject obj in allSceneObjects)
        {
            Material currentObjectMaterial = obj.GetComponent<Material>();
            
            if (currentObjectMaterial == null)
            {
                continue;
            }

            if (materialsToObjects.ContainsKey(currentObjectMaterial))
            {
                List<GameObject> objectsWithCurrentMaterial = new();
                materialsToObjects.TryGetValue(currentObjectMaterial, out objectsWithCurrentMaterial);
                objectsWithCurrentMaterial.Add(obj);
            } else
            {
                materialsToObjects.Add(currentObjectMaterial, new List<GameObject>());
            }
        }


    }

    private List<GameObject> GetAllObjectsWithMeshFilter(List<GameObject> incomingObjects)
    {
        List<GameObject> objectsWithMeshFilter = new();

        foreach (GameObject obj in incomingObjects)
        {
            if (obj.GetComponent<MeshFilter>() != null)
            {
                objectsWithMeshFilter.Add(obj);
            }
        }

        return objectsWithMeshFilter;
    }
}
