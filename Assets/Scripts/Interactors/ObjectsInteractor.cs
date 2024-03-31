using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ObjectsInteractor : ScriptableObject
{
    public GameObject[] GetAllGameObjectsOnScene()
    {
        return FindObjectsOfType<GameObject>();
    }

    public List<GameObject> GetAllObjectsWithMeshFilter(List<GameObject> incomingObjects)
    {
        return incomingObjects.Where(obj => obj.GetComponent<MeshFilter>() != null).ToList();
    }

    public Bounds GetSceneBounds()
    {
        List<GameObject> allSceneObjects = GetAllObjectsWithMeshFilter(GetAllGameObjectsOnScene().ToList());

        Bounds sceneBounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (GameObject obj in allSceneObjects)
        {
            Bounds objBounds = obj.GetComponent<Renderer>().bounds;
            sceneBounds.Encapsulate(objBounds);
        }

        return sceneBounds;
    }

    public int GetVerticesCount(GameObject obj)
    {
        return obj.GetComponent<MeshFilter>().sharedMesh.vertices.Count();
    }

    public int GetTrianglesCount(GameObject obj)
    {
        return obj.GetComponent<MeshFilter>().sharedMesh.triangles.Count();
    }

    public Transform[] GetChildrenRecursively(GameObject obj)
    {
        return obj.transform.GetComponentsInChildren<Transform>().Where(child => !child.Equals(obj.transform)).ToArray();
    }

    public void AddMaterialToObjectIfNeeded(Material newMaterial, GameObject obj)
    {
        Material[] allObjectMaterials = obj.GetComponent<Renderer>().materials;
        if (!allObjectMaterials.Contains(newMaterial))
        {
            Material[] newObjectMaterials = new Material[allObjectMaterials.Length + 1];
            Array.Copy(allObjectMaterials, newObjectMaterials, allObjectMaterials.Length);
            newObjectMaterials[allObjectMaterials.Length] = newMaterial;
            obj.GetComponent<Renderer>().materials = RemoveDuplicateMaterials(newObjectMaterials);
        }
    }

    public static Material[] RemoveDuplicateMaterials(Material[] materials)
    {
        HashSet<Material> set = new HashSet<Material>(materials);
        Material[] result = new Material[set.Count];
        set.CopyTo(result);
        return result;
    }

    public GameObjectsGraph CreateValidGameObjectsGraph(GameObject collection, float distanceLimit)
    {
        GameObjectsGraph graph = new GameObjectsGraph();
        Transform[] children = GetChildrenRecursively(collection);
        foreach (Transform iChild in children)
        {
            GameObjectsGraph.Node node = new GameObjectsGraph.Node(iChild);
            foreach (Transform jChild in children)
            {
                if (iChild == jChild)
                {
                    continue;
                }

                float distanceBetweenObjects = Vector3.Distance(iChild.position, jChild.position);

                if (distanceBetweenObjects <= distanceLimit)
                {
                    node.NeighboursWithValidDistances.Add(new GameObjectsGraph.Node(jChild), new GameObjectsGraph.Edge(distanceBetweenObjects));
                }
            }
            graph.Nodes.Add(node);
        }
        return graph;
    }
}
