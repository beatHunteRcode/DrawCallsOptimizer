using UnityEngine;

public struct JobGameObject
{
    public int instanceId;
    public Vector3 position;
    public Bounds bounds;

    public JobGameObject(GameObject obj)
    {
        instanceId = obj.GetInstanceID();
        position = obj.transform.position;
        bounds = obj.GetComponent<Renderer>().bounds;
    }
}
