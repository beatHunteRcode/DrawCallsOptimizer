using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

public class SceneInteractor : ScriptableObject
{
    private ObjectCombiner objectCombiner;

    private List<GameObject> chunks = new();

    Dictionary<GameObject, List<GameObject>> chunksToObjects = new();

    private List<GameObject> allObjectsNeedToOptimize = new();
    private List<GameObject> objectsClones = new();

    public void Init()
    {
        objectCombiner = CreateInstance<ObjectCombiner>();
        objectCombiner.Init();
    }

    public GameObject[] GetAllGameObjectsOnScene()
    {
        return FindObjectsOfType<GameObject>();
    }

    public GameObject[] GetAllActiveStaticGameObjectsOnScene()
    {
        return FindObjectsOfType<GameObject>().Where(obj => obj.activeSelf == true && obj.isStatic).ToArray();
    }

    public List<GameObject> GetAllObjectsWithMeshFilter(List<GameObject> incomingObjects)
    {
        return incomingObjects.Where(obj => obj.GetComponent<MeshFilter>() != null).ToList();
    }

    public Bounds GetSceneBounds()
    {
        List<GameObject> allSceneObjects = new();
        if (allObjectsNeedToOptimize.Count == 0)
        {
            allSceneObjects = GetAllObjectsWithMeshFilter(GetAllGameObjectsOnScene().ToList());
        }
        else
        {
            allSceneObjects = allObjectsNeedToOptimize;
        }

        Bounds sceneBounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (GameObject obj in allSceneObjects)
        {
            Bounds objBounds = obj.GetComponent<Renderer>().bounds;
            sceneBounds.Encapsulate(objBounds);
        }

        return sceneBounds;
    }

    public GameObject CreateSceneBoundsObject()
    {
        GameObject gameObject = new GameObject();
        gameObject.name = "SceneBounds";
        gameObject.AddComponent<BoxCollider>();
        Bounds sceneBounds = GetSceneBounds();
        gameObject.transform.position = sceneBounds.center;
        gameObject.transform.localScale = sceneBounds.size;
        return gameObject;
    }

    public void DestroySceneBoundsObject(GameObject sceneBoundsObject)
    {
        Destroy(sceneBoundsObject);
    }

    public void DivideIntoChunks(GameObject targetBoundsCube, Vector3Int sectionCount, Material chunkMaterial)
    {
        chunks.Clear();

        if (targetBoundsCube != null)
        {
            Vector3 sizeOfOriginalBoundsCube = targetBoundsCube.transform.lossyScale;
            Vector3 chunkSize = new Vector3(
                sizeOfOriginalBoundsCube.x / sectionCount.x,
                sizeOfOriginalBoundsCube.y / sectionCount.y,
                sizeOfOriginalBoundsCube.z / sectionCount.z
                );
            Vector3 fillStartPosition = targetBoundsCube.transform.TransformPoint(new Vector3(-0.5f, 0.5f, -0.5f))
                                + targetBoundsCube.transform.TransformDirection(new Vector3(chunkSize.x, -chunkSize.y, chunkSize.z) / 2.0f);

            Transform parentTransform = new GameObject(targetBoundsCube.name).transform;

            GameObject chunk;
            int chunkNumber = 0;

            for (int i = 0; i < sectionCount.x; i++)
            {
                for (int j = 0; j < sectionCount.y; j++)
                {
                    for (int k = 0; k < sectionCount.z; k++)
                    {
                        chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        chunkNumber++;

                        chunk.name = "Chunk_" + chunkNumber;

                        chunk.transform.localScale = chunkSize;
                        chunk.transform.position = fillStartPosition +
                                                       targetBoundsCube.transform.TransformDirection(new Vector3((chunkSize.x) * i, -(chunkSize.y) * j, (chunkSize.z) * k));
                        chunk.transform.rotation = targetBoundsCube.transform.rotation;

                        chunk.transform.SetParent(parentTransform);
                        chunk.AddComponent<BoxCollider>();

                        chunk.GetComponent<Renderer>().material = chunkMaterial;
                        chunk.GetComponent<MeshRenderer>().enabled = false;

                        chunks.Add(chunk);
                    }
                }
            }

            DestroyImmediate(targetBoundsCube);
        }
    }

    public void MapObjectsToChunks()
    {
        chunksToObjects.Clear();
        List<GameObject> allSceneGameObjectsNeedToAnalyse = GetAllObjectsWithMeshFilter(GetAllActiveStaticGameObjectsOnScene().ToList());

        foreach (GameObject chunk in chunks)
        {
            List<GameObject> objectsInChunk = new();
            foreach (GameObject obj in allSceneGameObjectsNeedToAnalyse)
            {
                if (chunk.GetComponent<Renderer>().bounds.Contains(obj.transform.position) && !chunk.Equals(obj))
                {
                    objectsInChunk.Add(obj);
                }
            }

            if (objectsInChunk.Count > 0)
            {
                chunksToObjects.Add(chunk, objectsInChunk);
            }
        }
    }

    public void OptimizeChunksByObjectsPolygonsCount(int objectsPolygonsThreshold, bool isSaveObjectsNeeded = true)
    {
        if (chunksToObjects.Count == 0)
        {
            GameObject objectWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByPolygons(objectsPolygonsThreshold: objectsPolygonsThreshold, isSaveObjectsNeeded: isSaveObjectsNeeded);
            if (objectWithAnotherObjectsMeshes != null)
            {
                objectWithAnotherObjectsMeshes.name = GetObjectName(
                    obj: objectWithAnotherObjectsMeshes,
                    chunkName: "",
                    combineMethodString: CombineMethod.BY_POLYGONS
                );
            }
        }
        foreach (KeyValuePair<GameObject, List<GameObject>> chunkWithObjects in chunksToObjects)
        {
            GameObject chunk = chunkWithObjects.Key;
            List<GameObject> objects = chunkWithObjects.Value;

            GameObject objectWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByPolygons(objects, objectsPolygonsThreshold, isSaveObjectsNeeded);
            if (objectWithAnotherObjectsMeshes != null)
            {
                objectWithAnotherObjectsMeshes.name = GetObjectName(
                    obj: objectWithAnotherObjectsMeshes,
                    chunkName: chunk.name,
                    combineMethodString: CombineMethod.BY_POLYGONS
                );
            }
        }
    }

    public void OptimizeChunksByObjectsMaterials(int objectsWithSameMaterialThreshold, bool isSaveObjectsNeeded = true)
    {
        if (chunksToObjects.Count == 0)
        {
            List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByMaterials(objectsWithSameMaterialThreshold, isSaveObjectsNeeded: isSaveObjectsNeeded);
            foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
            {
                obj.name = GetObjectName(
                    obj: obj,
                    chunkName: "",
                    combineMethodString: CombineMethod.BY_MATERIALS
                );
            }
        }
        foreach (KeyValuePair<GameObject, List<GameObject>> chunkWithObjects in chunksToObjects)
        {
            GameObject chunk = chunkWithObjects.Key;
            List<GameObject> objects = chunkWithObjects.Value;

            List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByMaterials(objectsWithSameMaterialThreshold, objects, isSaveObjectsNeeded);
            foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
            {
                obj.name = GetObjectName(
                    obj: obj,
                    chunkName: chunk.name,
                    combineMethodString: CombineMethod.BY_MATERIALS
                );
            }
        }
    }

    public void OptimizeChunksByObjectsTags(string[] tagsToCombine, bool isSaveObjectsNeeded = true)
    {
        if (chunksToObjects.Count == 0)
        {
            List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByTags(tagsToCombine, isSaveObjectsNeeded: isSaveObjectsNeeded);
            foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
            {
                obj.name = GetObjectName(
                    obj: obj,
                    chunkName: "",
                    combineMethodString: CombineMethod.BY_TAGS
                );
            }
        }
        foreach (KeyValuePair<GameObject, List<GameObject>> chunkWithObjects in chunksToObjects)
        {
            GameObject chunk = chunkWithObjects.Key;
            List<GameObject> objects = chunkWithObjects.Value;

            List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByTags(tagsToCombine, objects, isSaveObjectsNeeded);
            foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
            {
                obj.name = GetObjectName(
                    obj: obj,
                    chunkName: chunk.name,
                    combineMethodString: CombineMethod.BY_TAGS
                );
            }
        }
    }

    public void OptimizeChunksByDistanceBetweenObjects(float distanceLimit, GameObject[] collectionsOfObjectsToCombineByDistance, bool isSaveObjectsNeeded = true)
    {
        if (collectionsOfObjectsToCombineByDistance.Length != 0)
        {
            int iterationNumber = 1;
            List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineCollectionsOfObjectsByDistance(distanceLimit, collectionsOfObjectsToCombineByDistance, isSaveObjectsNeeded);
            foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
            {
                obj.name = GetObjectName(
                    obj: obj,
                    chunkName: null,
                    combineMethodString: CombineMethod.BY_DISTANCE,
                    iterationNumber: iterationNumber
                );

                iterationNumber++;
            }
        }
        else
        {
            if (chunksToObjects.Count == 0)
            {
                List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByDistance(distanceLimit, isSaveObjectsNeeded: isSaveObjectsNeeded);
                foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
                {
                    obj.name = GetObjectName(
                        obj: obj,
                        chunkName: null,
                        combineMethodString: CombineMethod.BY_DISTANCE,
                        iterationNumber: 1
                    );
                }
            }
            foreach (KeyValuePair<GameObject, List<GameObject>> chunkWithObjects in chunksToObjects)
            {
                int iterationNumber = 1;

                GameObject chunk = chunkWithObjects.Key;
                List<GameObject> objects = chunkWithObjects.Value;

                List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByDistance(distanceLimit, objects, isSaveObjectsNeeded: isSaveObjectsNeeded);
                foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
                {
                    obj.name = GetObjectName(
                        obj: obj,
                        chunkName: chunk.name,
                        combineMethodString: CombineMethod.BY_DISTANCE,
                        iterationNumber: iterationNumber
                    );
                    iterationNumber++;
                }
            }
        }
    }
    public GameObject DeactivateObjectAndGetClone(GameObject obj)
    {
        Transform objParent = obj.transform.parent;
        obj.transform.SetParent(null);
        GameObject objClone = Instantiate(obj);
        objectsClones.Add(objClone);
        obj.transform.SetParent(objParent);
        obj.SetActive(false);

        return objClone;
    }

    public void DestroyAllObjectsClones()
    {
        List<GameObject> remainingClones = objectsClones.Where(obj => obj != null).ToList();
        foreach(GameObject clone in remainingClones)
        {
            DestroyImmediate(clone);
        }
    }

    private string GetObjectName(GameObject obj, string chunkName, CombineMethod combineMethodString, int iterationNumber = 0)
    {
        StringBuilder nameBuilder = new();

        if (chunkName != null)
        {
            nameBuilder.Append(chunkName).Append("_");
        }
        switch (combineMethodString)
        {
            case CombineMethod.BY_POLYGONS:
                nameBuilder.Append("CombinedByPolygons");
                break;
            case CombineMethod.BY_MATERIALS:
                nameBuilder.Append("CombinedByMaterials").Append("_").Append(obj.GetComponent<MeshRenderer>().sharedMaterial.name.Replace(" (Instance)", ""));
                break;
            case CombineMethod.BY_TAGS:
                nameBuilder.Append("CombinedByTags").Append("_").Append(obj.tag);
                break;
            case CombineMethod.BY_DISTANCE:
                nameBuilder.Append("CombinedByDistance");
                break;
        }
        if (iterationNumber > 0)
        {
            nameBuilder.Append("_").Append(iterationNumber);
        }

        return nameBuilder.ToString();
    }

    private enum CombineMethod
    {
        BY_POLYGONS,
        BY_MATERIALS,
        BY_TAGS,
        BY_DISTANCE
    }
}
