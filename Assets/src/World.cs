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
    private ConcurrentDictionary<string, Chunk> garbageChunks
        = new ConcurrentDictionary<string, Chunk>();
    private readonly ConcurrentDictionary<string, Chunk> chunks = new ConcurrentDictionary<string, Chunk>();
    //TODO check volatile documentation use sth like lock or semaphore
    private volatile bool creatingChunks = false;
    private ConcurrentBag<string> chunkRequests = new ConcurrentBag<string>();

    // Start is called before the first frame update
    void Start()
    {
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
        string key;
        while (chunkRequests.TryTake(out key))
        {
            if (chunks.ContainsKey(key))
                continue;
            Chunk ch = new Chunk(Vectors.ParseKey(key), this);
            chunks[key] = ch;
            yield return null;
        }
        creatingChunks = false;
    }

    private void RequestChunkCreation(Vector3Int centerChunk)
    {
        var to = centerChunk + Player.viewDistance;
        var from = centerChunk - Player.viewDistance;

        HashSet<string> unseens = new HashSet<string>(chunks.Keys);

        for (int x = from.x; x <= to.x; x++)
        {
            for (int y = from.y; y <= to.y; y++)
            {
                for (int z = from.z; z <= to.z; z++)
                {
                    string key = Vectors.FormatKey(x, y, z);
                    if (!chunks.ContainsKey(key))
                        chunkRequests.Add(key);
                    unseens.Remove(key);
                }
            }
        }


        foreach (string key in unseens)
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
        if (chunks.TryGetValue(Vectors.FormatKey(vp.chunk), out chunk))
            return chunk.GetBlock(vp.local).isSolid;

        return service.IsSolid(vp);
    }
}
