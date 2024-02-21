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
        List<GameObject> objectsWithMeshFilter = GetAllObjectsWithMeshFilter(allSceneObjects);
        int allObjectsPolygonsCount = CountAllObjectsPolygons(objectsWithMeshFilter);

        if (allObjectsPolygonsCount >= TrianglesLimit)
        {
            GameObject parentObj = new GameObject();
            parentObj.AddComponent<MeshCombiner>();
            foreach (GameObject obj in objectsWithMeshFilter)
            {   
                obj.transform.SetParent(parentObj.transform);
                ObjectsInteractor.AddMaterialToObjectIfNeeded(obj.GetComponent<Renderer>().sharedMaterial, parentObj);
            }
            MeshCombiner meshCombiner = parentObj.GetComponent<MeshCombiner>();
            meshCombiner.DestroyCombinedChildren = true;
            if (parentObj.GetComponent<Renderer>().materials.Length > 1) 
            {
                meshCombiner.CreateMultiMaterialMesh = true;
            }
            meshCombiner.CombineMeshes(false);
            parentObj.name = "PARENT";
        }
    }

    public void CombineAllSceneObjectsByMaterials()
    {
        Dictionary<Material, List<GameObject>> materialsToObjects = MapMaterialsToObjectsOnScene();
        foreach (KeyValuePair<Material, List<GameObject>> materialAndObjects in materialsToObjects)
        {
            Material material = materialAndObjects.Key;
            List<GameObject> gameObjects = materialAndObjects.Value;
            GameObject parentObj = new GameObject();
            foreach (GameObject obj in gameObjects)
            {
                obj.transform.SetParent(parentObj.transform);
            }
            parentObj.AddComponent<MeshCombiner>();
            MeshCombiner meshCombiner = parentObj.GetComponent<MeshCombiner>();
            meshCombiner.DestroyCombinedChildren = true;
            meshCombiner.CombineMeshes(false);
            parentObj.name = "PARENT_" + material.name;
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

    private Dictionary<Material, List<GameObject>> MapMaterialsToObjectsOnScene()
    {
        Dictionary<Material, List<GameObject>> materialsToObjects = new();
        List<GameObject> allSceneObjects = ObjectsInteractor.GetAllGameObjectsOnScene().ToList();
        List<GameObject> objectsWithMeshFilter = GetAllObjectsWithMeshFilter(allSceneObjects);
        foreach (GameObject obj in objectsWithMeshFilter)
        {
            Material currentObjectMaterial = obj.GetComponent<Renderer>().sharedMaterial;

            if (currentObjectMaterial == null)
            {
                continue;
            }

            if (materialsToObjects.ContainsKey(currentObjectMaterial))
            {
                List<GameObject> objectsWithCurrentMaterial = materialsToObjects[currentObjectMaterial];
                objectsWithCurrentMaterial.Add(obj);
            }
            else
            {
                List<GameObject> objectsWithCurrentMaterial = new();
                objectsWithCurrentMaterial.Add(obj);
                materialsToObjects.Add(currentObjectMaterial, objectsWithCurrentMaterial);
            }
        }
        return materialsToObjects;
    }

    private int CountAllObjectsPolygons(List<GameObject> objects)
    {
        int polygonsCount = 0;
        foreach (GameObject obj in objects) {
            polygonsCount += ObjectsInteractor.GetTrianglesCount(obj);
        }
        return polygonsCount;
    }

}
