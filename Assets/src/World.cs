using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public readonly VoxelService service = new VoxelService();
    //TODO take care of size/time limit
    //Diactivated chunks are added to this collection for possible future reuse
    private ConcurrentDictionary<Vector3Int, Chunk> garbageChunks
        = new ConcurrentDictionary<Vector3Int, Chunk>();
    private readonly ConcurrentDictionary<Vector3Int, Chunk> chunks = new ConcurrentDictionary<Vector3Int, Chunk>();
    //TODO check volatile documentation use sth like lock or semaphore
    private volatile bool creatingChunks = false;
    private ConcurrentBag<Vector3Int> chunkRequests = new ConcurrentBag<Vector3Int>();
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(service.Initialize());
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnPlayerChunkChanged(Vector3Int currChunk)
    {
        RequestChunkCreation(currChunk);

        if (!creatingChunks && !chunkRequests.IsEmpty)
            StartCoroutine("CreateChunks");
    }

    IEnumerator CreateChunks()
    {
        creatingChunks = true;
        Vector3Int key;
        while (chunkRequests.TryTake(out key))
        {
            if (chunks.ContainsKey(key))
                continue;
            Chunk ch = new Chunk(key, this);
            chunks[key] = ch;
            yield return new WaitForSeconds(.2f); ;
        }
        creatingChunks = false;
    }

    private void RequestChunkCreation(Vector3Int centerChunk)
    {
        var to = centerChunk + Player.viewDistance;
        var from = centerChunk - Player.viewDistance;

        var unseens = new HashSet<Vector3Int>(chunks.Keys);

        for (int x = from.x; x <= to.x; x++)
        {
            for (int y = from.y; y <= to.y; y++)
            {
                for (int z = from.z; z <= to.z; z++)
                {
                    Vector3Int key = new Vector3Int(x, y, z);
                    if (!chunks.ContainsKey(key))
                        chunkRequests.Add(key);
                    unseens.Remove(key);
                }
            }
        }


        foreach (Vector3Int key in unseens)
        {
            Chunk ch;
            chunks.TryRemove(key, out ch);
            Destroy(ch.chunkObject);
            //ch.isActive = false;
            //garbageChunks[key] = ch;
        }

    }

    public bool IsSolidAt(Vector3Int pos)
    {
        var vp = new VoxelPosition(pos);
        Chunk chunk;
        if (chunks.TryGetValue(vp.chunk, out chunk))
            return chunk.GetBlock(vp.local).isSolid;

        return service.IsSolid(vp);
    }
}
