using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class World : MonoBehaviour
{
    public Material material;
    //TODO take care of size/time limit
    //Diactivated chunks are added to this collection for possible future reuse
    private Dictionary<Vector3Int, Chunk> garbageChunks
        = new Dictionary<Vector3Int, Chunk>();
    private readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    //TODO check volatile documentation use sth like lock or semaphore
    private volatile bool creatingChunks = false;
    private List<Chunk> chunkRequests = new List<Chunk>();
    // Start is called before the first frame update
    public GameObject debugScreen;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    public void Initialize(Vector3Int currChunk, bool clean)
    {
        if (clean)
        {
            chunkRequests.Clear();

            foreach (var chunk in garbageChunks.Values)
                Destroy(chunk.chunkObject);
            garbageChunks.Clear();

            foreach (var chunk in chunks.Values)
                Destroy(chunk.chunkObject);
            chunks.Clear();
        }

        OnPlayerChunkChanged(currChunk, true);
    }

    private void OnPlayerChunkChanged(Vector3Int currChunk, bool instantly)
    {
        RequestChunkCreation(currChunk);

        if (!creatingChunks && chunkRequests.Count > 0)
            StartCoroutine(CreateChunks(instantly));
    }

    public void OnPlayerChunkChanged(Vector3Int currChunk)
    {
        this.OnPlayerChunkChanged(currChunk, false);
    }

    IEnumerator CreateChunks(bool instantly)
    {
        creatingChunks = true;
        while (chunkRequests.Count != 0)
        {
            var chunk = chunkRequests[0];
            if (chunk.IsActive() && !chunk.IsInited())
            {
                chunk.Init();
                if (!instantly)
                    yield return null;
            }
            chunkRequests.RemoveAt(0);
        }
        creatingChunks = false;
        yield break;
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
                    {
                        Chunk chunk;
                        if (garbageChunks.TryGetValue(key, out chunk))
                        {
                            chunk.SetActive(true);
                            chunks[key] = chunk;
                            garbageChunks.Remove(key);
                        }
                        else
                        {
                            chunks[key] = chunk = new Chunk(key, this);
                            chunkRequests.Add(chunk);
                        }
                    }
                    unseens.Remove(key);
                }
            }
        }


        foreach (Vector3Int key in unseens)
        {
            Chunk ch;
            if (chunks.TryGetValue(key, out ch))
            {
                //Destroy(ch.chunkObject);
                chunks.Remove(key);
            }
            ch.SetActive(false);
            garbageChunks[key] = ch;
        }

        if(garbageChunks.Count > 512)
        {
            foreach(var entry in garbageChunks)
                Destroy(entry.Value.chunkObject);
            garbageChunks.Clear();
        }
    }

    public Chunk GetChunkIfInited(Vector3Int chunkPos)
    {
        Chunk chunk;
        if (chunks.TryGetValue(chunkPos, out chunk) && chunk.IsInited() && chunk.IsActive())
            return chunk;
        return null;
    }

    public bool IsSolidAt(Vector3Int pos)
    {
        var vp = new VoxelPosition(pos);
        Chunk chunk;
        if (chunks.TryGetValue(vp.chunk, out chunk) && chunk.IsInited())
            return chunk.GetBlock(vp.local).isSolid;
        return VoxelService.INSTANCE.IsSolid(vp);
    }

    public static World INSTANCE
    {
        get
        {
            return GameObject.Find("World").GetComponent<World>();
        }
    }
}
