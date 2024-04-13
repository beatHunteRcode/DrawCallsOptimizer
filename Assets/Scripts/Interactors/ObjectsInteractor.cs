using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectsInteractor : ScriptableObject
{
    public int GetVerticesCount(GameObject obj)
    {
        return obj.GetComponent<MeshFilter>().sharedMesh.vertices.Count();
    }

    public int GetTrianglesCount(GameObject obj)
    {
        return obj.GetComponent<MeshFilter>().sharedMesh.triangles.Count() / 3;
    }

    public Transform[] GetChildrenRecursively(GameObject obj)
    {
        return obj.transform.GetComponentsInChildren<Transform>().Where(child => !child.Equals(obj.transform)).ToArray();
    }

    public void AddMaterialToObjectIfNeeded(Material newMaterial, GameObject obj)
    {
        Material[] allObjectMaterials = obj.GetComponent<Renderer>().sharedMaterials;
        if (!allObjectMaterials.Contains(newMaterial))
        {
            Material[] newObjectMaterials = new Material[allObjectMaterials.Length + 1];
            Array.Copy(allObjectMaterials, newObjectMaterials, allObjectMaterials.Length);
            newObjectMaterials[allObjectMaterials.Length] = newMaterial;
            obj.GetComponent<Renderer>().sharedMaterials = RemoveDuplicateMaterials(newObjectMaterials);
        }
    }

    public static Material[] RemoveDuplicateMaterials(Material[] materials)
    {
        HashSet<Material> set = new HashSet<Material>(materials);
        Material[] result = new Material[set.Count];
        set.CopyTo(result);
        return result;
    }

    public GameObjectsGraph CreateValidGameObjectsGraph(Transform[] objects, float distanceLimit)
    {
        GameObjectsGraph graph = new();
        foreach (Transform iChild in objects)
        {
            GameObjectsGraph.Node node = new GameObjectsGraph.Node(iChild);
            foreach (Transform jChild in objects)
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

    public GameObjectsGraph CreateValidGameObjectsGraphFromCollection(GameObject collection, float distanceLimit)
    {
        Transform[] children = GetChildrenRecursively(collection);
        return CreateValidGameObjectsGraph(children, distanceLimit);
    }
}
