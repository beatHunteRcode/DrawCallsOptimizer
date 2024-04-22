using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ObjectsInfoHolder
{
    public static List<GameObject> originalObjects = new();
    public static List<GameObject> objectsCreatedByScript = new();
}
