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
        sceneInteractor.MapObjectsToChunks();
        PerformOptimizations(saveAnalyzedObjectsBeforeOptimizations);
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

    private void PerformOptimizations(bool isSaveObjectsNeeded)
    {
        if (combineObjectsByPolygonsNeeded)
        {
            sceneInteractor.OptimizeChunksByObjectsPolygonsCount(chunkObjectsPolygonsThreshold, isSaveObjectsNeeded);
        }

        if (combineObjectsByMaterialsNeeded)
        {
            sceneInteractor.OptimizeChunksByObjectsMaterials(chunkObjectsWithSameMaterialThreshold, isSaveObjectsNeeded);
        }

        if (combineObjectsByTagsNeeded)
        {
            sceneInteractor.OptimizeChunksByObjectsTags(tagsToCombine, isSaveObjectsNeeded);
        }

        if (combineObjectsByDistanceNeeded)
        {
            sceneInteractor.OptimizeChunksByDistanceBetweenObjects(distanceLimit, collectionsOfObjectsToCombineByDistance, isSaveObjectsNeeded);
        }
    }
}
