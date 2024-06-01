using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

[ExecuteInEditMode]
public class SceneAnalyzer : MonoBehaviour
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

    public GameObject boundsObject;
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
        bool deleteSceneBoundsObject = boundsObject == null;
        if (boundsObject == null)
        {
            boundsObject = CreateSceneBoundsObject();
        }
        ObjectsInfoHolder.boundsObjectName = boundsObject.name;
        sceneInteractor.DivideIntoChunks(boundsObject, sectionCount, chunkMaterial, deleteSceneBoundsObject);
        sceneInteractor.MapObjectsToChunks(analyzeOnlyStaticObjects, boundsObject);
        PerformOptimizations();
        DeactivateAllObjects(ObjectsInfoHolder.originalObjects);
        if (destroySceneBoundsObjectAfterOptimizations)
        {
            DestroyImmediate(boundsObject);
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

    private void DeactivateAllObjects(List<GameObject> objects)
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(false);
        }
    }
}
