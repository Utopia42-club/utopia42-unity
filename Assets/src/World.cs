using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public readonly VoxelService service = new VoxelService();
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
        StartCoroutine(service.Initialize(GameObject.Find("Loading")));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    public void OnPlayerChunkChanged(Vector3Int currChunk)
    {
        RequestChunkCreation(currChunk);

        if (!creatingChunks && chunkRequests.Count > 0)
            StartCoroutine("CreateChunks");
    }

    IEnumerator CreateChunks()
    {
        creatingChunks = true;
        while (chunkRequests.Count != 0)
        {
            var chunk = chunkRequests[0];
            if (chunk.IsActive() && !chunk.IsInited())
            {
                chunk.Init();
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
        return service.IsSolid(vp);
    }
}
