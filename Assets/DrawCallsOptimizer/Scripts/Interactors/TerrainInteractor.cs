using System.Collections.Generic;
using UnityEngine;

public class TerrainInteractor : ScriptableObject
{
    private Dictionary<Terrain, TreeInstance[]> savedTerrainTrees = new();

    public void CreateTreeObjectsFromTerrain(Terrain[] terrains)
    {
        foreach (Terrain terrain in terrains)
        {
            TerrainData terrainData = terrain.terrainData;
            float width = terrainData.size.x;
            float height = terrainData.size.z;
            float y = terrainData.size.y;
            GameObject parent = new(terrain.gameObject.name + "_CreatedObjects");
            foreach (TreeInstance tree in terrainData.treeInstances)
            {
                if (tree.prototypeIndex >= terrainData.treePrototypes.Length)
                    continue;
                var treeProrotypePrefab = terrainData.treePrototypes[tree.prototypeIndex].prefab;
                Vector3 position = new Vector3(
                    tree.position.x * width,
                    tree.position.y * y,
                    tree.position.z * height) + terrain.transform.position;
                Vector3 scale = new Vector3(tree.widthScale, tree.heightScale, tree.widthScale);
                GameObject treeObject = Instantiate(treeProrotypePrefab, position, Quaternion.Euler(0f, Mathf.Rad2Deg * tree.rotation, 0f), parent.transform) as GameObject;
                treeObject.transform.localScale = scale;
            }
        }
    }

    public void DeleteAllTreesFromTerrain(Terrain[] terrains)
    {
        foreach (Terrain terrain in terrains)
        {
            if (savedTerrainTrees.TryGetValue(terrain, out var trees))
            {
                trees = new TreeInstance[terrain.terrainData.treeInstances.Length];
                terrain.terrainData.treeInstances.CopyTo(trees, 0);
            }
            else
            {
                TreeInstance[] savedTrees = new TreeInstance[terrain.terrainData.treeInstances.Length];
                terrain.terrainData.treeInstances.CopyTo(savedTrees, 0);
                savedTerrainTrees.Add(terrain, savedTrees);
            }

            while (terrain.terrainData.treeInstances.Length > 0)
            {
                for (int i = 0; i < terrain.terrainData.treeInstances.Length; i++)
                {
                    terrain.terrainData.treeInstances = terrain.terrainData.treeInstances.RemoveAt(i);
                }
            }
        }
    }

    public void RestoreAllTerrainTrees(Terrain[] terrains)
    {
        foreach (Terrain terrain in terrains)
        {
            if (savedTerrainTrees.TryGetValue(terrain, out var trees))
            {
                terrain.terrainData.treeInstances = trees;
            }
        }
    }
}
