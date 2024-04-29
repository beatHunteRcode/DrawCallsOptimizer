using System;
using Unity.Jobs;
using UnityEngine;

[ExecuteInEditMode]
public class SceneAnalyser : MonoBehaviour
{
    SceneInteractor sceneInteractor;
    TerrainInteractor terrainInteractor;

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

    public Terrain[] terrainsToOptimize = new Terrain[] { };

    void Start()
    {
        sceneInteractor = ScriptableObject.CreateInstance<SceneInteractor>();
        sceneInteractor.Init();

        terrainInteractor = ScriptableObject.CreateInstance<TerrainInteractor>();
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

    public void EnableAllOriginalOptimizedObjects()
    {
        sceneInteractor.EnableAllOriginalOptimizedObjects();
    }

    public void DestroyAllObjectsCreatedByScript()
    {
        sceneInteractor.DestroyAllObjectsCreatedByScript();
    }

    public void CreateTreeObjectsFromTerrain(Terrain[] terrains)
    {
        terrainInteractor.CreateTreeObjectsFromTerrain(terrains);
    }

    public void DeleteAllTreesFromTerrain(Terrain[] terrains)
    {
        terrainInteractor.DeleteAllTreesFromTerrain(terrains);
    }

    public void RestoreAllTerrainTrees(Terrain[] terrains)
    {
        terrainInteractor.RestoreAllTerrainTrees(terrains);
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
