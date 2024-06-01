using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public class SceneInteractor : ScriptableObject
{
    public static List<string> lodObjectsNames = new();

    private ObjectCombiner objectCombiner;

    private List<GameObject> chunks = new();

    Dictionary<GameObject, List<GameObject>> chunksToObjects = new();

    private List<GameObject> allObjectsNeedToOptimize = new();
    private List<GameObject> objectsClones = new();

    Dictionary<int, string> instanceIdsStringsToObjectsNames = new();

    public void Init()
    {
        objectCombiner = CreateInstance<ObjectCombiner>();
        objectCombiner.Init();
    }

    public GameObject[] GetAllGameObjectsOnScene()
    {
        return FindObjectsOfType<GameObject>();
    }

    public GameObject[] GetAllActiveGameObjectsOnScene()
    {
        return FindObjectsOfType<GameObject>().Where(obj => obj.activeSelf == true).ToArray();
    }

    public GameObject[] GetAllActiveStaticGameObjectsOnScene()
    {
        return FindObjectsOfType<GameObject>().Where(obj => obj.activeSelf == true && obj.isStatic).ToArray();
    }

    public GameObject[] GetAllActiveGameObjectsInBounds(Bounds bounds)
    {
        return FindObjectsOfType<GameObject>().Where(obj =>
            obj.activeSelf == true &&
            bounds.Contains(obj.transform.position)
        ).ToArray();
    }

    public GameObject[] GetAllActiveStaticGameObjectsInBounds(Bounds bounds)
    {
        return FindObjectsOfType<GameObject>().Where(obj =>
            obj.activeSelf == true &&
            obj.isStatic &&
            bounds.Contains(obj.transform.position)
        ).ToArray();
    }

    public List<GameObject> GetAllObjectsWithMeshFilterOrLODGroup(List<GameObject> incomingObjects)
    {
        return incomingObjects.Where(
            obj =>  obj.GetComponent<MeshFilter>() != null ||
                    obj.GetComponent<LODGroup>() != null
        ).ToList();
    }

    public Bounds GetSceneBounds()
    {
        List<GameObject> allSceneObjects = new();
        if (allObjectsNeedToOptimize.Count == 0)
        {
            allSceneObjects = GetAllObjectsWithMeshFilterOrLODGroup(GetAllGameObjectsOnScene().ToList());
        }
        else
        {
            allSceneObjects = allObjectsNeedToOptimize;
        }

        Bounds sceneBounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (GameObject obj in allSceneObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            LODGroup lodGroup = obj.GetComponent<LODGroup>();
            if (renderer != null)
            {
                Bounds objBounds = obj.GetComponent<Renderer>().bounds;
                sceneBounds.Encapsulate(objBounds);
            }
            else if (lodGroup != null)
            {
                LOD[] lods = lodGroup.GetLODs();
                foreach (LOD lod in lods)
                {
                    Renderer[] lodRenderers = lod.renderers;
                    foreach (Renderer lodRenderer in lodRenderers)
                    {
                        Bounds lodBounds = lodRenderer.bounds;
                        sceneBounds.Encapsulate(lodBounds);
                    }
                }
            }
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

    public void DivideIntoChunks(GameObject targetBoundsCube, Vector3Int sectionCount, Material chunkMaterial, bool deleteSceneBoundsObject)
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

            if (deleteSceneBoundsObject)
            {
                ObjectsInfoHolder.objectsCreatedByScript.Add(targetBoundsCube.gameObject);
            }

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

                        chunk.transform.SetParent(targetBoundsCube.transform);
                        chunk.GetComponent<BoxCollider>().isTrigger = true;

                        chunk.GetComponent<Renderer>().material = chunkMaterial;
                        chunk.GetComponent<MeshRenderer>().enabled = false;

                        chunks.Add(chunk);
                        ObjectsInfoHolder.objectsCreatedByScript.Add(chunk);
                    }
                }
            }
        }
    }

    public void MapObjectsToChunks(bool analyzeOnlyStaticObjects, GameObject boundsObject)
    {
        chunksToObjects.Clear();
        instanceIdsStringsToObjectsNames.Clear();
        lodObjectsNames.Clear();

        List<GameObject> allSceneGameObjectsNeedToAnalyse;
        if (analyzeOnlyStaticObjects)
        {
            Renderer renderer = boundsObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                allSceneGameObjectsNeedToAnalyse = GetAllObjectsWithMeshFilterOrLODGroup(GetAllActiveStaticGameObjectsInBounds(renderer.bounds).ToList());
                
            }
            else
            {
                allSceneGameObjectsNeedToAnalyse = GetAllObjectsWithMeshFilterOrLODGroup(GetAllActiveStaticGameObjectsOnScene().ToList());
            }
        }
        else
        {
            Renderer renderer = boundsObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                allSceneGameObjectsNeedToAnalyse = GetAllObjectsWithMeshFilterOrLODGroup(GetAllActiveGameObjectsInBounds(renderer.bounds).ToList());
            }
            else
            {
                allSceneGameObjectsNeedToAnalyse = GetAllObjectsWithMeshFilterOrLODGroup(GetAllActiveGameObjectsOnScene().ToList());
            }
        }

        allSceneGameObjectsNeedToAnalyse.Remove(boundsObject);

        List<GameObject> lodObjects = new();
        List<GameObject> objectsWithLods = new();
        foreach (GameObject obj in allSceneGameObjectsNeedToAnalyse)
        {
            LODGroup lodGroup = obj.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                objectsWithLods.Add(obj);
                LOD[] lods = lodGroup.GetLODs();
                for (int i = 1; i < lods.Length; i++)
                {
                    Renderer[] renderers = lods[i].renderers;
                    foreach (Renderer renderer in renderers)
                    {
                        lodObjectsNames.Add(renderer.gameObject.name);
                        lodObjects.Add(renderer.gameObject);
                    }
                }
            }
        }

        foreach (GameObject lodObject in lodObjects)
        {
            allSceneGameObjectsNeedToAnalyse.Remove(lodObject);
        }
        foreach (GameObject obj in objectsWithLods)
        {
            allSceneGameObjectsNeedToAnalyse.Remove(obj);
            ObjectsInfoHolder.originalObjects.Add(obj);
        }

        foreach (GameObject obj in allSceneGameObjectsNeedToAnalyse)
        {
            int instanceId = obj.GetInstanceID();
            if (!instanceIdsStringsToObjectsNames.ContainsKey(instanceId)) 
            {
                instanceIdsStringsToObjectsNames.Add(instanceId, obj.name);
            }
            obj.name = instanceId.ToString();
        }
        foreach (GameObject chunkObj in chunks)
        {
            int instanceId = chunkObj.GetInstanceID();
            if (!instanceIdsStringsToObjectsNames.ContainsKey(instanceId))
            {
                instanceIdsStringsToObjectsNames.Add(instanceId, chunkObj.name);
            }
            chunkObj.name = instanceId.ToString();
        }

        MapObjectsToChunksParallel(allSceneGameObjectsNeedToAnalyse);
    }

    public void OptimizeChunksByObjectsPolygonsCount(int objectsPolygonsThreshold, bool analyzeOnlyStaticObjects, bool isSaveObjectsNeeded = true)
    {
        objectsClones.Clear();
        ObjectsInfoHolder.originalObjects.Clear();

        if (chunksToObjects.Count == 0)
        {
            GameObject objectWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByPolygons(analyzeOnlyStaticObjects, objectsPolygonsThreshold: objectsPolygonsThreshold, isSaveObjectsNeeded: isSaveObjectsNeeded);
            if (objectWithAnotherObjectsMeshes != null)
            {
                objectWithAnotherObjectsMeshes.name = GetObjectName(
                    obj: objectWithAnotherObjectsMeshes,
                    chunkName: "",
                    combineMethodString: CombineMethod.BY_POLYGONS
                );
                ObjectsInfoHolder.objectsCreatedByScript.Add(objectWithAnotherObjectsMeshes);
            }
        }
        foreach (KeyValuePair<GameObject, List<GameObject>> chunkWithObjects in chunksToObjects)
        {
            GameObject chunk = chunkWithObjects.Key;
            List<GameObject> objects = chunkWithObjects.Value;

            GameObject objectWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByPolygons(analyzeOnlyStaticObjects, objects, objectsPolygonsThreshold, isSaveObjectsNeeded);
            if (objectWithAnotherObjectsMeshes != null)
            {
                objectWithAnotherObjectsMeshes.name = GetObjectName(
                    obj: objectWithAnotherObjectsMeshes,
                    chunkName: chunk.name,
                    combineMethodString: CombineMethod.BY_POLYGONS
                );
                ObjectsInfoHolder.objectsCreatedByScript.Add(objectWithAnotherObjectsMeshes);
            }
        }
    }

    public void OptimizeChunksByObjectsMaterials(int objectsWithSameMaterialThreshold, bool analyzeOnlyStaticObjects, bool isSaveObjectsNeeded = true)
    {

        if (chunksToObjects.Count == 0)
        {
            List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByMaterials(analyzeOnlyStaticObjects, objectsWithSameMaterialThreshold, isSaveObjectsNeeded: isSaveObjectsNeeded);
            foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
            {
                obj.name = GetObjectName(
                    obj: obj,
                    chunkName: "",
                    combineMethodString: CombineMethod.BY_MATERIALS
                );
                ObjectsInfoHolder.objectsCreatedByScript.Add(obj);
            }
        }
        foreach (KeyValuePair<GameObject, List<GameObject>> chunkWithObjects in chunksToObjects)
        {
            GameObject chunk = chunkWithObjects.Key;
            List<GameObject> objects = chunkWithObjects.Value;

            List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByMaterials(analyzeOnlyStaticObjects, objectsWithSameMaterialThreshold, objects, isSaveObjectsNeeded);
            foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
            {
                obj.name = GetObjectName(
                    obj: obj,
                    chunkName: chunk.name,
                    combineMethodString: CombineMethod.BY_MATERIALS
                );
                ObjectsInfoHolder.objectsCreatedByScript.Add(obj);
            }
        }
    }

    public void OptimizeChunksByObjectsTags(string[] tagsToCombine, bool analyzeOnlyStaticObjects, bool isSaveObjectsNeeded = true)
    {
        objectsClones.Clear();
        ObjectsInfoHolder.originalObjects.Clear();

        if (chunksToObjects.Count == 0)
        {
            List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByTags(analyzeOnlyStaticObjects, tagsToCombine, isSaveObjectsNeeded: isSaveObjectsNeeded);
            foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
            {
                obj.name = GetObjectName(
                    obj: obj,
                    chunkName: "",
                    combineMethodString: CombineMethod.BY_TAGS
                );
                ObjectsInfoHolder.objectsCreatedByScript.Add(obj);
            }
        }
        foreach (KeyValuePair<GameObject, List<GameObject>> chunkWithObjects in chunksToObjects)
        {
            GameObject chunk = chunkWithObjects.Key;
            List<GameObject> objects = chunkWithObjects.Value;

            List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByTags(analyzeOnlyStaticObjects, tagsToCombine, objects, isSaveObjectsNeeded);
            foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
            {
                obj.name = GetObjectName(
                    obj: obj,
                    chunkName: chunk.name,
                    combineMethodString: CombineMethod.BY_TAGS
                );
                ObjectsInfoHolder.objectsCreatedByScript.Add(obj);
            }
        }
    }

    public void OptimizeChunksByDistanceBetweenObjects(float distanceLimit, bool analyzeOnlyStaticObjects, GameObject[] collectionsOfObjectsToCombineByDistance, bool isSaveObjectsNeeded = true)
    {
        objectsClones.Clear();
        ObjectsInfoHolder.originalObjects.Clear();

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
                ObjectsInfoHolder.objectsCreatedByScript.Add(obj);

                iterationNumber++;
            }
        }
        else
        {
            if (chunksToObjects.Count == 0)
            {
                List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByDistance(distanceLimit, analyzeOnlyStaticObjects, isSaveObjectsNeeded: isSaveObjectsNeeded);
                foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
                {
                    obj.name = GetObjectName(
                        obj: obj,
                        chunkName: null,
                        combineMethodString: CombineMethod.BY_DISTANCE,
                        iterationNumber: 1
                    );
                    ObjectsInfoHolder.objectsCreatedByScript.Add(obj);
                }
            }
            foreach (KeyValuePair<GameObject, List<GameObject>> chunkWithObjects in chunksToObjects)
            {
                int iterationNumber = 1;

                GameObject chunk = chunkWithObjects.Key;
                List<GameObject> objects = chunkWithObjects.Value;

                List<GameObject> objectsWithAnotherObjectsMeshes = objectCombiner.CombineObjectsByDistance(distanceLimit, analyzeOnlyStaticObjects, objects, isSaveObjectsNeeded: isSaveObjectsNeeded);
                foreach (GameObject obj in objectsWithAnotherObjectsMeshes)
                {
                    obj.name = GetObjectName(
                        obj: obj,
                        chunkName: chunk.name,
                        combineMethodString: CombineMethod.BY_DISTANCE,
                        iterationNumber: iterationNumber
                    );
                    ObjectsInfoHolder.objectsCreatedByScript.Add(obj);
                    iterationNumber++;
                }
            }
        }
    }
    public GameObject GetClone(GameObject obj)
    {
        ObjectsInfoHolder.originalObjects.Add(obj);
        Transform objParent = obj.transform.parent;
        obj.transform.SetParent(null);
        GameObject objClone = Instantiate(obj);
        objectsClones.Add(objClone);
        obj.transform.SetParent(objParent);

        return objClone;
    }

    public void DestroyAllObjectsClones()
    {
        List<GameObject> remainingClones = objectsClones.Where(obj => obj != null).ToList();
        foreach (GameObject clone in remainingClones)
        {
            DestroyImmediate(clone);
        }
    }

    public void EnableAllOriginalOptimizedObjects()
    {
        if (ObjectsInfoHolder.originalObjects.Count == 0)
        {
            Debug.Log("There is no deactivated original Objects");
            return;
        }

        foreach (GameObject obj in ObjectsInfoHolder.originalObjects)
        {
            obj.SetActive(true);
        }
    }

    public void DestroyAllObjectsCreatedByScript()
    {
        foreach (GameObject obj in ObjectsInfoHolder.objectsCreatedByScript)
        {
            DestroyImmediate(obj);
        }
    }

    private void MapObjectsToChunksParallel(List<GameObject> objects)
    {
        MapObjectsToChunksJob mapObjectsToChunksJob = new MapObjectsToChunksJob();
        mapObjectsToChunksJob.chunks = ConvertToNativeArray(chunks.ToArray());
        mapObjectsToChunksJob.allSceneGameObjectsNeedToAnalyse = ConvertToNativeArray(objects.ToArray());
        mapObjectsToChunksJob.chunksIdsToObjectsIds = new NativeParallelHashMap<int, UnsafeList<int>>(objects.Count, Allocator.TempJob);

        JobHandle handle = mapObjectsToChunksJob.Schedule(chunks.Count, chunks.Count);
        JobHandle.ScheduleBatchedJobs();
        handle.Complete();

        chunksToObjects = ConvertInstanceIdsDictionaryToGameObjectsDictionary(mapObjectsToChunksJob.chunksIdsToObjectsIds);

        mapObjectsToChunksJob.DisposeAllContainers();
    }

    private void MapObjectsToChunksSequential(List<GameObject> objects)
    {
        foreach (GameObject chunk in chunks)
        {
            List<GameObject> objectsInChunk = new();
            foreach (GameObject obj in objects)
            {
                if (chunk.GetComponent<Renderer>().bounds.Contains(obj.transform.position) && !chunk.Equals(obj))
                {
                    objectsInChunk.Add(obj);
                }
            }

            if (objectsInChunk.Count > 0)
            {
                chunksToObjects.TryAdd(chunk, objectsInChunk);
            }
        }
    }

    private string GetObjectName(GameObject obj, string chunkName, CombineMethod combineMethodString, int iterationNumber = 0)
    {
        StringBuilder nameBuilder = new();

        if (ObjectsInfoHolder.boundsObjectName != null)
        {
            nameBuilder.Append(ObjectsInfoHolder.boundsObjectName).Append("_");
        }
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
                nameBuilder.Append("CombinedByMaterials");
                foreach (Material mat in obj.GetComponent<MeshRenderer>().sharedMaterials)
                {
                    nameBuilder.Append("_");
                    nameBuilder.Append(mat.name.Replace(" (Instance)", ""));
                }
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

    private NativeArray<JobGameObject> ConvertToNativeArray(GameObject[] objects)
    {
        List<JobGameObject> jobObjects = new();
        foreach (GameObject obj in objects)
        {
            JobGameObject objectForNativeArray = new JobGameObject(obj);
            jobObjects.Add(objectForNativeArray);
        }
        return new NativeArray<JobGameObject>(jobObjects.ToArray(), Allocator.TempJob);
    }

    private Dictionary<GameObject, List<GameObject>> ConvertInstanceIdsDictionaryToGameObjectsDictionary(NativeParallelHashMap<int, UnsafeList<int>> nativeHashMap) 
    {
        Dictionary<GameObject, List<GameObject>> result = new Dictionary<GameObject, List<GameObject>>();
        foreach (KeyValue<int, UnsafeList<int>> entry in nativeHashMap)
        {
            GameObject keyObject = GameObject.Find(entry.Key.ToString());
            if (keyObject != null)
            {
                if (instanceIdsStringsToObjectsNames.TryGetValue(keyObject.GetInstanceID(), out var keyObjectName))
                {
                    keyObject.name = keyObjectName;
                    List<GameObject> valueObjects = new();
                    foreach (int objectId in entry.Value)
                    {
                        GameObject valueObject = GameObject.Find(objectId.ToString());
                        if (valueObject != null)
                        {
                            if (instanceIdsStringsToObjectsNames.TryGetValue(valueObject.GetInstanceID(), out var valueObjectName))
                            {
                                valueObject.name = valueObjectName;
                                valueObjects.Add(valueObject);
                            }
                        }
                    }
                    result.Add(keyObject, valueObjects);
                }
            }
        }
        return result;
    }

    private enum CombineMethod
    {
        BY_POLYGONS,
        BY_MATERIALS,
        BY_TAGS,
        BY_DISTANCE
    }
}
