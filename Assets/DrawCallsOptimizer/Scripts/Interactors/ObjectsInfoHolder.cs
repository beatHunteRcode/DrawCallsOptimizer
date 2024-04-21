using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Singleton]
public class ObjectsInfoHolder : ScriptableObject
{
    public static List<GameObject> originalObjects = new();
    public static List<GameObject> objectsCreatedByScript = new();
}
