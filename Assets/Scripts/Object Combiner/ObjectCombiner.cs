using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class ObjectCombiner : MonoBehaviour
{
    public int trianglesLimit = 0;

    [TagSelector]
    public string[] tagsToCombine = new string[] {};

    private ObjectsInteractor objectsInteractor;

    void Start()
    {
        objectsInteractor = ScriptableObject.CreateInstance<ObjectsInteractor>();
    }

    public void CombineAllSceneObjectsByTriangles()
    {
        List<GameObject> allSceneObjects = objectsInteractor.GetAllGameObjectsOnScene().ToList();
        List<GameObject> objectsWithMeshFilter = GetAllObjectsWithMeshFilter(allSceneObjects);
        int allObjectsPolygonsCount = CountAllObjectsPolygons(objectsWithMeshFilter);

        if (allObjectsPolygonsCount >= trianglesLimit)
        {
            GameObject parentObj = new GameObject();
            parentObj.AddComponent<MeshCombiner>();
            foreach (GameObject obj in objectsWithMeshFilter)
            {   
                obj.transform.SetParent(parentObj.transform);
                objectsInteractor.AddMaterialToObjectIfNeeded(obj.GetComponent<Renderer>().sharedMaterial, parentObj);
            }
            MeshCombiner meshCombiner = parentObj.GetComponent<MeshCombiner>();
            meshCombiner.DestroyCombinedChildren = true;
            if (parentObj.GetComponent<Renderer>().materials.Length > 1) 
            {
                meshCombiner.CreateMultiMaterialMesh = true;
            }
            meshCombiner.CombineMeshes(false);
            parentObj.name = "PARENT";
            Destroy(parentObj.GetComponent<MeshCombiner>());
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
            Destroy(parentObj.GetComponent<MeshCombiner>());
        }
    }

    public void CombineAllSceneObjectsByTags()
    {
        Dictionary<string, List<GameObject>> tagsToObjects = MapTagsToObjectsOnScene();
        foreach (KeyValuePair<string, List<GameObject>> tagAndObjects in tagsToObjects)
        {
            string tag = tagAndObjects.Key;
            List<GameObject> gameObjects = tagAndObjects.Value;
            GameObject parentObj = new GameObject();
            parentObj.AddComponent<MeshCombiner>();
            foreach (GameObject obj in gameObjects)
            {
                obj.transform.SetParent(parentObj.transform);
                objectsInteractor.AddMaterialToObjectIfNeeded(obj.GetComponent<Renderer>().sharedMaterial, parentObj);
            }
            MeshCombiner meshCombiner = parentObj.GetComponent<MeshCombiner>();
            meshCombiner.DestroyCombinedChildren = true;
            if (parentObj.GetComponent<Renderer>().materials.Length > 1)
            {
                meshCombiner.CreateMultiMaterialMesh = true;
            }
            meshCombiner.CombineMeshes(false);
            parentObj.name = "PARENT_" + tag;
            parentObj.tag = tag;
            Destroy(parentObj.GetComponent<MeshCombiner>());
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
        List<GameObject> allSceneObjects = objectsInteractor.GetAllGameObjectsOnScene().ToList();
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

    private Dictionary<string, List<GameObject>> MapTagsToObjectsOnScene()
    {
        Dictionary<string, List<GameObject>> tagsToObjects = new();
        List<GameObject> allSceneObjects = objectsInteractor.GetAllGameObjectsOnScene().ToList();
        List<GameObject> objectsWithMeshFilter = GetAllObjectsWithMeshFilter(allSceneObjects);
        foreach (GameObject obj in objectsWithMeshFilter)
        {
            string currentObjectTag = obj.tag;

            if (!tagsToCombine.Contains(currentObjectTag))
            {
                continue;
            }

            if (tagsToObjects.ContainsKey(currentObjectTag))
            {
                List<GameObject> objectsWithCurrentTag = tagsToObjects[currentObjectTag];
                objectsWithCurrentTag.Add(obj);
            }
            else
            {
                List<GameObject> objectsWithCurrentTag = new();
                objectsWithCurrentTag.Add(obj);
                tagsToObjects.Add(currentObjectTag, objectsWithCurrentTag);
            }
        }
        return tagsToObjects;
    }


    private int CountAllObjectsPolygons(List<GameObject> objects)
    {
        int polygonsCount = 0;
        foreach (GameObject obj in objects) {
            polygonsCount += objectsInteractor.GetTrianglesCount(obj);
        }
        return polygonsCount;
    }

}