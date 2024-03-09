using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectsInteractor : ScriptableObject
{
    public GameObject[] GetAllGameObjectsOnScene()
    {
        return FindObjectsOfType<GameObject>();
    }

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
        List<Transform> children = new List<Transform>();
        foreach (Transform child in obj.transform.GetComponentsInChildren<Transform>())
        {
            if (!child.Equals(obj))
            {
                children.Add(child);
            }
        }
        return children.ToArray();
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
}
