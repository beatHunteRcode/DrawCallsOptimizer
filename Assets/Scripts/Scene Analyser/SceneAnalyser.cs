using UnityEngine;

public class SceneAnalyser : MonoBehaviour
{
    SceneInteractor sceneInteractor;

    public Vector3Int SectionCount;

    void Start()
    {
        sceneInteractor = new SceneInteractor();
    }

    public GameObject CreateSceneBoundsObject()
    {
        return sceneInteractor.CreateSceneBoundsObject();
    }

    public void DivideIntoChunks(GameObject targetBoundsCube)
    {
        if (targetBoundsCube != null)
        {
            Vector3 sizeOfOriginalBoundsCube = targetBoundsCube.transform.lossyScale;
            Vector3 chunkSize = new Vector3(
                sizeOfOriginalBoundsCube.x / SectionCount.x,
                sizeOfOriginalBoundsCube.y / SectionCount.y,
                sizeOfOriginalBoundsCube.z / SectionCount.z
                );
            Vector3 fillStartPosition = targetBoundsCube.transform.TransformPoint(new Vector3(-0.5f, 0.5f, -0.5f))
                                + targetBoundsCube.transform.TransformDirection(new Vector3(chunkSize.x, -chunkSize.y, chunkSize.z) / 2.0f);

            Transform parentTransform = new GameObject(targetBoundsCube.name).transform;

            GameObject chunk;
            int chunkNumber = 0;

            for (int i = 0; i < SectionCount.x; i++)
            {
                for (int j = 0; j < SectionCount.y; j++)
                {
                    for (int k = 0; k < SectionCount.z; k++)
                    {
                        chunk = new GameObject();
                        chunkNumber++;

                        chunk.name = "Chunk_" + chunkNumber;

                        chunk.transform.localScale = chunkSize;
                        chunk.transform.position = fillStartPosition +
                                                       targetBoundsCube.transform.TransformDirection(new Vector3((chunkSize.x) * i, -(chunkSize.y) * j, (chunkSize.z) * k));
                        chunk.transform.rotation = targetBoundsCube.transform.rotation;

                        chunk.transform.SetParent(parentTransform);
                        chunk.AddComponent<BoxCollider>();
                    }
                }
            }

            Destroy(targetBoundsCube);
        }
    }
}
