using System;
using UnityEngine;

[ExecuteInEditMode]
public class SceneAnalyser : MonoBehaviour
{
    SceneInteractor sceneInteractor;

    public bool combineObjectsByPolygonsNeeded = false;
    public bool combineObjectsByMaterialsNeeded = false;
    public bool combineObjectsByTagsNeeded = false;
    public bool combineObjectsByDistanceNeeded = false;

    public Vector3Int sectionCount;

    public int chunkObjectsPolygonsThreshold;

    public int chunkObjectsWithSameMaterialThreshold;

    [TagSelector]
    public string[] tagsToCombine = new string[] { };

    public float distanceLimit = 0f;
    public GameObject[] collectionsOfObjectsToCombineByDistance = new GameObject[] { };

    public Material chunkMaterial;

    public bool analyzeOnlyStaticObjects = true;
    public bool destroySceneBoundsObjectAfterOptimizations = true;
    public bool saveAnalyzedObjectsBeforeOptimizations = true;

    void Start()
    {
        sceneInteractor = ScriptableObject.CreateInstance<SceneInteractor>();
        sceneInteractor.Init();
    }

    public void OptimizeObjectsOnScene()
    {
        GameObject boundsCube = CreateSceneBoundsObject();
        sceneInteractor.DivideIntoChunks(boundsCube, sectionCount, chunkMaterial);
        sceneInteractor.MapObjectsToChunks(analyzeOnlyStaticObjects);
        PerformOptimizations();
        if (destroySceneBoundsObjectAfterOptimizations)
        {
            DestroyImmediate(boundsCube);
        }
        if (saveAnalyzedObjectsBeforeOptimizations)
        {
            sceneInteractor.DestroyAllObjectsClones();
        }
    }

    private GameObject CreateSceneBoundsObject()
    {
        return sceneInteractor.CreateSceneBoundsObject();
    }

    private void PerformOptimizations()
    {
        if (combineObjectsByPolygonsNeeded)
        {
            sceneInteractor.OptimizeChunksByObjectsPolygonsCount(chunkObjectsPolygonsThreshold, analyzeOnlyStaticObjects, saveAnalyzedObjectsBeforeOptimizations);
        }

        if (combineObjectsByMaterialsNeeded)
        {
            sceneInteractor.OptimizeChunksByObjectsMaterials(chunkObjectsWithSameMaterialThreshold, analyzeOnlyStaticObjects, saveAnalyzedObjectsBeforeOptimizations);
        }

        if (combineObjectsByTagsNeeded)
        {
            sceneInteractor.OptimizeChunksByObjectsTags(tagsToCombine, analyzeOnlyStaticObjects, saveAnalyzedObjectsBeforeOptimizations);
        }

        if (combineObjectsByDistanceNeeded)
        {
            sceneInteractor.OptimizeChunksByDistanceBetweenObjects(distanceLimit, analyzeOnlyStaticObjects, collectionsOfObjectsToCombineByDistance, saveAnalyzedObjectsBeforeOptimizations);
        }
    }
}
