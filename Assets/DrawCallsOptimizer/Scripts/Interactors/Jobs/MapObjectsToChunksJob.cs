using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

public struct MapObjectsToChunksJob : IJobParallelFor
{
    [ReadOnly]
    public NativeArray<JobGameObject> chunks;
    [ReadOnly]
    public NativeArray<JobGameObject> allSceneGameObjectsNeedToAnalyse;
    [NativeDisableParallelForRestriction]
    public NativeParallelHashMap<int, UnsafeList<int>> chunksIdsToObjectsIds; // int = InstanceId объекта на сцене

    public void Execute(int index)
    {
        UnsafeList<int> objectsIdsInChunk = new UnsafeList<int>(10, Allocator.TempJob);

        foreach (JobGameObject sceneObj in allSceneGameObjectsNeedToAnalyse)
        {
            if (chunks[index].bounds.Contains(sceneObj.position) && !chunks[index].instanceId.Equals(sceneObj.instanceId))
            {
                objectsIdsInChunk.Add(sceneObj.instanceId);
            }
        }

        if (objectsIdsInChunk.Capacity > 0)
        {
            chunksIdsToObjectsIds.Add(chunks[index].instanceId, objectsIdsInChunk);
        }
    }

    public void DisposeAllContainers()
    {
        chunks.Dispose();
        allSceneGameObjectsNeedToAnalyse.Dispose();
        chunksIdsToObjectsIds.Dispose();
    }
}
