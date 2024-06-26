using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityMeshSimplifier;
using Unity.VisualScripting;

public class ObjectCombiner : ScriptableObject
{
    private SceneInteractor sceneInteractor;
    private ObjectsInteractor objectsInteractor;

    public void Init()
    {
        sceneInteractor = CreateInstance<SceneInteractor>();
        objectsInteractor = CreateInstance<ObjectsInteractor>();
    }

    public GameObject CombineObjectsByPolygons(bool analyzeOnlyStaticObjects, List<GameObject> objects = null, int objectsPolygonsThreshold = 0, bool isSaveObjectsNeeded = true)
    {
        GameObject parentObj = null;

        if (objects == null)
        {
            List<GameObject> allSceneObjects;
            if (analyzeOnlyStaticObjects)
            {
                allSceneObjects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(sceneInteractor.GetAllActiveStaticGameObjectsOnScene().ToList());
            }
            else
            {
                allSceneObjects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(sceneInteractor.GetAllActiveGameObjectsOnScene().ToList());
            }
            objects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(allSceneObjects);
        }
        int allObjectsPolygonsCount = CountAllObjectsPolygons(objects);

        if (allObjectsPolygonsCount >= objectsPolygonsThreshold)
        {
            parentObj = new();
            MeshCombiner meshCombiner = parentObj.AddComponent<MeshCombiner>();
            foreach (GameObject obj in objects)
            {
                GameObject cloneObject;
                if (isSaveObjectsNeeded)
                {
                    cloneObject = sceneInteractor.GetClone(obj);
                }
                else
                {
                    cloneObject = obj;
                }
                cloneObject.transform.SetParent(parentObj.transform);
                objectsInteractor.AddMaterialToObjectIfNeeded(cloneObject.GetComponent<Renderer>().sharedMaterial, parentObj);
            }
            meshCombiner.DestroyCombinedChildren = true;
            if (parentObj.GetComponent<Renderer>().sharedMaterials.Length > 1) 
            {
                meshCombiner.CreateMultiMaterialMesh = true;
            }
            meshCombiner.CombineMeshes(false);
            DestroyImmediate(meshCombiner);
            if (analyzeOnlyStaticObjects)
            {
                parentObj.isStatic = true;
            }
        }

        return parentObj;
    }

    public List<GameObject> CombineObjectsByMaterials(bool analyzeOnlyStaticObjects, int objectsWithSameMaterialThreshold, List<GameObject> objects = null, bool isSaveObjectsNeeded = true)
    {
        List<GameObject> parentObjList = new();

        if (objects == null) {
            List<GameObject> allSceneObjects;
            if (analyzeOnlyStaticObjects)
            {
                allSceneObjects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(sceneInteractor.GetAllActiveStaticGameObjectsOnScene().ToList());
            }
            else
            {
                allSceneObjects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(sceneInteractor.GetAllActiveGameObjectsOnScene().ToList());
            }
            objects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(allSceneObjects);
        }

        Dictionary<Material[], List<GameObject>> materialsToObjects = MapMaterialsToObjects(objects);
        foreach (KeyValuePair<Material[], List<GameObject>> materialsAndObjects in materialsToObjects)
        {
            if (materialsAndObjects.Value.Count >= objectsWithSameMaterialThreshold)
            {
                GameObject parentObj = new();
                List<GameObject> gameObjects = materialsAndObjects.Value;
                foreach (GameObject obj in gameObjects)
                {
                    GameObject cloneObject;
                    if (isSaveObjectsNeeded)
                    {
                        cloneObject = sceneInteractor.GetClone(obj);
                    }
                    else
                    {
                        cloneObject = obj;
                    }
                    Transform[] cloneChildren = objectsInteractor.GetChildrenRecursively(cloneObject);
                    foreach (Transform child in cloneChildren)
                    {
                        if (SceneInteractor.lodObjectsNames.Contains(child.gameObject.name)) 
                        {
                            DestroyImmediate(child.gameObject);
                        }
                    }
                    cloneObject.transform.SetParent(parentObj.transform);
                }
                MeshCombiner meshCombiner = parentObj.AddComponent<MeshCombiner>();
                meshCombiner.DestroyCombinedChildren = true;
                if (materialsAndObjects.Key.Length > 1)
                {
                    meshCombiner.CreateMultiMaterialMesh = true;
                }
                meshCombiner.CombineMeshes(false);
                DestroyImmediate(meshCombiner);
                if (analyzeOnlyStaticObjects)
                {
                    parentObj.isStatic = true;
                }

                LODGeneratorHelper lodGeneratorHelper = parentObj.AddComponent<LODGeneratorHelper>();
                SimplificationOptions simplificationOptions = lodGeneratorHelper.SimplificationOptions;
                simplificationOptions.PreserveBorderEdges = true;
                simplificationOptions.PreserveUVSeamEdges = true;
                simplificationOptions.PreserveUVFoldoverEdges = true;
                simplificationOptions.PreserveSurfaceCurvature = true;
                simplificationOptions.EnableSmartLink = true;

                lodGeneratorHelper.Levels[0].Quality = 1f;
                lodGeneratorHelper.Levels[1].Quality = 0.5f;
                lodGeneratorHelper.Levels[2].Quality = 0.1f;

                LODGenerator.GenerateLODs(lodGeneratorHelper);
                DestroyImmediate(lodGeneratorHelper);

                parentObjList.Add(parentObj);
            }
        }

        return parentObjList;
    }

    public List<GameObject> CombineObjectsByTags(bool analyzeOnlyStaticObjects, string[] tagsToCombine, List<GameObject> objects = null, bool isSaveObjectsNeeded = true)
    {
        List<GameObject> parentObjList = new();

        if (objects == null)
        {
            List<GameObject> allSceneObjects;
            if (analyzeOnlyStaticObjects)
            {
                allSceneObjects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(sceneInteractor.GetAllActiveStaticGameObjectsOnScene().ToList());
            }
            else
            {
                allSceneObjects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(sceneInteractor.GetAllActiveGameObjectsOnScene().ToList());
            }
            objects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(allSceneObjects);
        }

        Dictionary<string, List<GameObject>> tagsToObjects = MapTagsToObjectsOnScene(tagsToCombine, objects);
        foreach (KeyValuePair<string, List<GameObject>> tagAndObjects in tagsToObjects)
        {
            string tag = tagAndObjects.Key;
            List<GameObject> gameObjects = tagAndObjects.Value;
            if (gameObjects.Count > 1)
            {
                GameObject parentObj = new();
                MeshCombiner meshCombiner = parentObj.AddComponent<MeshCombiner>();
                foreach (GameObject obj in gameObjects)
                {
                    GameObject cloneObject;
                    if (isSaveObjectsNeeded)
                    {
                        cloneObject = sceneInteractor.GetClone(obj);
                    }
                    else
                    {
                        cloneObject = obj;
                    }
                    cloneObject.transform.SetParent(parentObj.transform);
                    objectsInteractor.AddMaterialToObjectIfNeeded(obj.GetComponent<Renderer>().sharedMaterial, parentObj);
                }
                meshCombiner.DestroyCombinedChildren = true;
                if (parentObj.GetComponent<Renderer>().sharedMaterials.Length > 1)
                {
                    meshCombiner.CreateMultiMaterialMesh = true;
                }
                meshCombiner.CombineMeshes(false);
                parentObj.tag = tag;
                DestroyImmediate(meshCombiner);
                if (analyzeOnlyStaticObjects)
                {
                    parentObj.isStatic = true;
                }
                parentObjList.Add(parentObj);
            }
        }

        return parentObjList;
    }

    public List<GameObject> CombineObjectsByDistance(float distanceLimit, bool analyzeOnlyStaticObjects, List<GameObject> objects = null, bool isSaveObjectsNeeded = true)
    {
        List<GameObject> parentObjList = new();
        List<Transform> objectsTransforms = new();

        if (objects == null)
        {
            List<GameObject> allSceneObjects;
            if (analyzeOnlyStaticObjects)
            {
                allSceneObjects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(sceneInteractor.GetAllActiveStaticGameObjectsOnScene().ToList());
            }
            else
            {
                allSceneObjects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(sceneInteractor.GetAllActiveGameObjectsOnScene().ToList());
            }
            objects = sceneInteractor.GetAllObjectsWithMeshFilterOrLODGroup(allSceneObjects);
        }

        foreach (GameObject obj in objects)
        {
            try
            {
                Transform objTransform = obj.transform;
                objectsTransforms.Add(objTransform);
            }
            catch (MissingReferenceException)
            {
                continue;
            }
        }

        while (objectsTransforms.Count > 1)
        {
            GameObjectsGraph graph = objectsInteractor.CreateValidGameObjectsGraph(objectsTransforms.ToArray(), distanceLimit);

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

            if (nodeWithMaxValidNeighbors != null)
            {
                GameObject parentObj = new GameObject();
                MeshCombiner meshCombiner = parentObj.AddComponent<MeshCombiner>();
                GameObject cloneNodeObjectWithMaxValidNeighbors = sceneInteractor.GetClone(nodeWithMaxValidNeighbors.GameObject.gameObject);
                cloneNodeObjectWithMaxValidNeighbors.transform.SetParent(parentObj.transform);

                foreach (KeyValuePair<GameObjectsGraph.Node, GameObjectsGraph.Edge> nodeWithDistance in nodeWithMaxValidNeighbors.NeighboursWithValidDistances)
                {
                    GameObjectsGraph.Node node = nodeWithDistance.Key;
                    GameObject cloneObject;
                    if (isSaveObjectsNeeded)
                    {
                        cloneObject = sceneInteractor.GetClone(node.GameObject.gameObject);
                    }
                    else
                    {
                        cloneObject = node.GameObject.gameObject;
                    }
                    cloneObject.transform.SetParent(parentObj.transform);
                    objectsInteractor.AddMaterialToObjectIfNeeded(cloneObject.GetComponent<Renderer>().sharedMaterial, parentObj);
                    objectsTransforms.Remove(node.GameObject.transform);
                }

                meshCombiner.DestroyCombinedChildren = true;
                if (parentObj.GetComponent<Renderer>().sharedMaterials.Length > 1)
                {
                    meshCombiner.CreateMultiMaterialMesh = true;
                }
                meshCombiner.CombineMeshes(false);
                DestroyImmediate(meshCombiner);
                if (analyzeOnlyStaticObjects)
                {
                    parentObj.isStatic = true;
                }
                parentObjList.Add(parentObj);
                objectsTransforms.Remove(nodeWithMaxValidNeighbors.GameObject.transform);
            }
            objectsTransforms = objectsTransforms.Where(objTransform => objTransform != null).ToList();
        }

        return parentObjList;
    }

    public List<GameObject> CombineCollectionsOfObjectsByDistance(float distanceLimit, GameObject[] collectionsOfObjectsToCombineByDistance, bool isSaveObjectsNeeded = true)
    {
        List<GameObject> parentObjList = new();

        foreach (GameObject collection in collectionsOfObjectsToCombineByDistance)
        {
            int iterationNumber = 0;
            while (objectsInteractor.GetChildrenRecursively(collection).Length > 0)
            {
                iterationNumber++;
                GameObjectsGraph graph = objectsInteractor.CreateValidGameObjectsGraphFromCollection(collection, distanceLimit);

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

                GameObject parentObj = new GameObject();
                MeshCombiner meshCombiner = parentObj.AddComponent<MeshCombiner>();
                nodeWithMaxValidNeighbors.GameObject.transform.SetParent(parentObj.transform);

                foreach (KeyValuePair<GameObjectsGraph.Node, GameObjectsGraph.Edge> nodeWithDistance in nodeWithMaxValidNeighbors.NeighboursWithValidDistances)
                {
                    GameObjectsGraph.Node node = nodeWithDistance.Key;
                    GameObject cloneObject;
                    if (isSaveObjectsNeeded)
                    {
                        cloneObject = sceneInteractor.GetClone(node.GameObject.gameObject);
                    }
                    else
                    {
                        cloneObject = node.GameObject.gameObject;
                    }
                    cloneObject.transform.SetParent(parentObj.transform);
                    objectsInteractor.AddMaterialToObjectIfNeeded(cloneObject.GetComponent<Renderer>().sharedMaterial, parentObj);
                }

                meshCombiner.DestroyCombinedChildren = true;
                if (parentObj.GetComponent<Renderer>().materials.Length > 1)
                {
                    meshCombiner.CreateMultiMaterialMesh = true;
                }
                meshCombiner.CombineMeshes(false);
                Destroy(meshCombiner);
                parentObj.isStatic = true;
                parentObjList.Add(parentObj);
            }
            Destroy(collection);
        }

        return parentObjList;
    }

    public Dictionary<Material[], List<GameObject>> MapMaterialsToObjects(List<GameObject> objectsWithMeshFilter)
    {
        Dictionary<Material[], List<GameObject>> materialsToObjects = new();
        foreach (GameObject obj in objectsWithMeshFilter)
        {
            Material[] currentObjectMaterials = obj.GetComponent<Renderer>().sharedMaterials;

            if (currentObjectMaterials == null)
            {
                continue;
            }

            bool isAnyMaterialMatches = false;
            foreach(KeyValuePair<Material[], List<GameObject>> entry in materialsToObjects)
            {
                if (ListExtensions.ContainsAllItems(entry.Key, currentObjectMaterials))
                {
                    isAnyMaterialMatches = true;
                    List<GameObject> objectsWithCurrentMaterials = entry.Value;
                    objectsWithCurrentMaterials.Add(obj);
                }
            }

            if (!isAnyMaterialMatches)
            {
                List<GameObject> objectsWithCurrentMaterial = new();
                objectsWithCurrentMaterial.Add(obj);
                materialsToObjects.Add(currentObjectMaterials, objectsWithCurrentMaterial);
            }
        }
        return materialsToObjects;
    }

    private Dictionary<string, List<GameObject>> MapTagsToObjectsOnScene(string[] tagsToCombine, List<GameObject> objectsWithMeshFilter)
    {
        Dictionary<string, List<GameObject>> tagsToObjects = new();
        foreach (GameObject obj in objectsWithMeshFilter)
        {
            try
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
            catch (MissingReferenceException)
            {
                continue;
            }
        }
        return tagsToObjects;
    }

    private int CountAllObjectsPolygons(List<GameObject> objects)
    {
        int polygonsCount = 0;

        try
        {
            polygonsCount = objects.Sum(obj => objectsInteractor.GetTrianglesCount(obj));
        }
        catch (MissingReferenceException)
        {
            // ignored
        }

        return polygonsCount;
    }

}
