using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Net.NetworkInformation;

public class ObjectCombiner : MonoBehaviour
{
    public int trianglesLimit = 0;
    public float distanceLimit = 0f;

    private const string PREFIX = "PARENT";

    [TagSelector]
    public string[] tagsToCombine = new string[] {};
    public GameObject[] collectionOfObjectsToCombineByDistance = new GameObject[] {};

    private ObjectsInteractor objectsInteractor;

    void Start()
    {
        objectsInteractor = ScriptableObject.CreateInstance<ObjectsInteractor>();
    }

    public void CombineObjectsByPolygons()
    {
        List<GameObject> allSceneObjects = objectsInteractor.GetAllGameObjectsOnScene().ToList();
        List<GameObject> objectsWithMeshFilter = objectsInteractor.GetAllObjectsWithMeshFilter(allSceneObjects);
        int allObjectsPolygonsCount = CountAllObjectsPolygons(objectsWithMeshFilter);

        if (allObjectsPolygonsCount >= trianglesLimit)
        {
            GameObject parentObj = new GameObject();
            MeshCombiner meshCombiner = parentObj.AddComponent<MeshCombiner>();
            foreach (GameObject obj in objectsWithMeshFilter)
            {   
                obj.transform.SetParent(parentObj.transform);
                objectsInteractor.AddMaterialToObjectIfNeeded(obj.GetComponent<Renderer>().sharedMaterial, parentObj);
            }
            meshCombiner.DestroyCombinedChildren = true;
            if (parentObj.GetComponent<Renderer>().materials.Length > 1) 
            {
                meshCombiner.CreateMultiMaterialMesh = true;
            }
            meshCombiner.CombineMeshes(false);
            parentObj.name = PREFIX;
            Destroy(meshCombiner);
        }
    }

    public void CombineObjectsByMaterials()
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
            MeshCombiner meshCombiner = parentObj.AddComponent<MeshCombiner>();
            meshCombiner.DestroyCombinedChildren = true;
            meshCombiner.CombineMeshes(false);
            parentObj.name = PREFIX + "_" + material.name;
            Destroy(meshCombiner);
        }
    }

    public void CombineObjectsByTags()
    {
        Dictionary<string, List<GameObject>> tagsToObjects = MapTagsToObjectsOnScene();
        foreach (KeyValuePair<string, List<GameObject>> tagAndObjects in tagsToObjects)
        {
            string tag = tagAndObjects.Key;
            List<GameObject> gameObjects = tagAndObjects.Value;
            GameObject parentObj = new GameObject();
            MeshCombiner meshCombiner = parentObj.AddComponent<MeshCombiner>();
            foreach (GameObject obj in gameObjects)
            {
                obj.transform.SetParent(parentObj.transform);
                objectsInteractor.AddMaterialToObjectIfNeeded(obj.GetComponent<Renderer>().sharedMaterial, parentObj);
            }
            meshCombiner.DestroyCombinedChildren = true;
            if (parentObj.GetComponent<Renderer>().materials.Length > 1)
            {
                meshCombiner.CreateMultiMaterialMesh = true;
            }
            meshCombiner.CombineMeshes(false);
            parentObj.name = PREFIX + "_" + tag;
            parentObj.tag = tag;
            Destroy(meshCombiner);
        }
    }

    public void CombineObjectsByDistance()
    {
        foreach (GameObject collection in collectionOfObjectsToCombineByDistance)
        {
            int iterationNumber = 0;
            while (objectsInteractor.GetChildrenRecursively(collection).Length > 0)
            {
                iterationNumber++;
                GameObjectsGraph graph = objectsInteractor.CreateValidGameObjectsGraph(collection, distanceLimit);

                GameObjectsGraph.Node nodeWithMaxValidNeighbors = null;
                foreach (GameObjectsGraph.Node node in graph.Nodes)
                {
                    if (nodeWithMaxValidNeighbors == null)
                    {
                        nodeWithMaxValidNeighbors = node;
                    }
                    if (node.NeighboursWithValidDistances.Count > nodeWithMaxValidNeighbors.NeighboursWithValidDistances.Count)
                    {
                        nodeWithMaxValidNeighbors = node;
                    }
                }

                if (nodeWithMaxValidNeighbors == null)
                {
                    print("There is no GameObjects with valid distances between each other in " + collection.name);
                    return;
                }

                GameObject parentObj = new GameObject();
                MeshCombiner meshCombiner = parentObj.AddComponent<MeshCombiner>();
                nodeWithMaxValidNeighbors.GameObject.transform.SetParent(parentObj.transform);

                foreach (KeyValuePair<GameObjectsGraph.Node, GameObjectsGraph.Edge> nodeWithDistance in nodeWithMaxValidNeighbors.NeighboursWithValidDistances)
                {
                    GameObjectsGraph.Node node = nodeWithDistance.Key;
                    node.GameObject.transform.SetParent(parentObj.transform);
                    objectsInteractor.AddMaterialToObjectIfNeeded(node.GameObject.GetComponent<Renderer>().sharedMaterial, parentObj);
                }

                meshCombiner.DestroyCombinedChildren = true;
                if (parentObj.GetComponent<Renderer>().materials.Length > 1)
                {
                    meshCombiner.CreateMultiMaterialMesh = true;
                }
                meshCombiner.CombineMeshes(false);
                parentObj.name = iterationNumber + "_" + PREFIX + "_" + collection.name + "_" + distanceLimit;
                Destroy(meshCombiner);
            }
            Destroy(collection);
        }
    }

    private Dictionary<Material, List<GameObject>> MapMaterialsToObjectsOnScene()
    {
        Dictionary<Material, List<GameObject>> materialsToObjects = new();
        List<GameObject> allSceneObjects = objectsInteractor.GetAllGameObjectsOnScene().ToList();
        List<GameObject> objectsWithMeshFilter = objectsInteractor.GetAllObjectsWithMeshFilter(allSceneObjects);
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
        List<GameObject> objectsWithMeshFilter = objectsInteractor.GetAllObjectsWithMeshFilter(allSceneObjects);
        foreach (GameObject obj in objectsWithMeshFilter)
        {
            string currentObjectTag = obj.tag;

            if (!tagsToCombine.Contains(currentObjectTag))
            {
                continue;
            }


            tagsToObjects.TryGetValue(currentObjectTag, out List<GameObject> objectsWithCurrentTag);
            if (objectsWithCurrentTag != null)
            {
                objectsWithCurrentTag.Add(obj);
            }
            else
            {
                objectsWithCurrentTag = new();
                objectsWithCurrentTag.Add(obj);
                tagsToObjects.Add(currentObjectTag, objectsWithCurrentTag);
            }
        }
        return tagsToObjects;
    }

    private int CountAllObjectsPolygons(List<GameObject> objects)
    {
        return objects.Sum(obj => objectsInteractor.GetTrianglesCount(obj));
    }

}
