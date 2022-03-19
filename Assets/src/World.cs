using System;
using System.Collections;
using System.Collections.Generic;
using src.MetaBlocks;
using src.Model;
using src.Service;
using UnityEngine;

namespace src
{
    public class World : MonoBehaviour
    {
        public Material material;

        //TODO take care of size/time limit
        //Diactivated chunks are added to this collection for possible future reuse
        private Dictionary<Vector3Int, Chunk> garbageChunks = new Dictionary<Vector3Int, Chunk>();
        private readonly Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

        //TODO check volatile documentation use sth like lock or semaphore
        private volatile bool creatingChunks = false;
        private HashSet<Chunk> chunkRequests = new HashSet<Chunk>();

        public GameObject debugScreen;
        public GameObject inventory;
        public GameObject cursorSlot;
        public GameObject help;
        public Player player;

        private void Start()
        {
            GameManager.INSTANCE.stateChange.AddListener(state =>
            {
                inventory.SetActive(state == GameManager.State.INVENTORY);
                cursorSlot.SetActive(state == GameManager.State.INVENTORY);
                help.SetActive(state == GameManager.State.HELP);
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
                debugScreen.SetActive(!debugScreen.activeSelf);
        }

        private Chunk PopRequest()
        {
            var iter = chunkRequests.GetEnumerator();
            iter.MoveNext();
            var chunk = iter.Current;
            chunkRequests.Remove(chunk);
            return chunk;
        }

        public bool Initialize(Vector3Int currChunk, bool clean)
        {
            if (clean) chunkRequests.Clear();
            else
            {
                while (0 != chunkRequests.Count)
                {
                    var chunk = PopRequest();
                    if (chunks.ContainsKey(chunk.coordinate))
                        chunks.Remove(chunk.coordinate); //FIXME Memory leak?
                }
            }

            if (creatingChunks) return false;
            if (clean)
            {
                foreach (var chunk in chunkRequests)
                {
                    if (chunk.chunkObject != null)
                    {
                        chunk.Destroy();
                        chunk.chunkObject = null;
                    }
                }

                chunkRequests.Clear();

                foreach (var chunk in garbageChunks.Values)
                    if (chunk.chunkObject != null)
                        chunk.Destroy();
                garbageChunks.Clear();

                foreach (var chunk in chunks.Values)
                    if (chunk.chunkObject != null)
                        chunk.Destroy();
                chunks.Clear();
            }

            OnPlayerChunkChanged(currChunk, true);
            return true;
        }

        private void OnPlayerChunkChanged(Vector3Int currChunk, bool instantly)
        {
            RequestChunkCreation(currChunk);

            if (!creatingChunks && chunkRequests.Count > 0)
                StartCoroutine(CreateChunks(instantly ? 50 : 1));
        }

        public void OnPlayerChunkChanged(Vector3Int currChunk)
        {
            this.OnPlayerChunkChanged(currChunk, false);
        }

        IEnumerator CreateChunks(int chunksPerFrame)
        {
            // creatingChunks = true;
            int todo = chunksPerFrame;
            while (chunkRequests.Count != 0)
            {
                var chunk = PopRequest();
                if (chunk.IsActive() && !chunk.IsInited())
                {
                    chunk.Init();
                    todo--;
                    if (todo <= 0)
                    {
                        todo = chunksPerFrame;
                        yield return null;
                    }
                }
            }

            creatingChunks = false;
        }

        public int CountChunksToCreate()
        {
            return chunkRequests.Count;
        }

        private void RequestChunkCreation(Vector3Int centerChunk)
        {
            var to = centerChunk + Player.ViewDistance;
            var from = centerChunk - Player.ViewDistance;

            var unseens = new HashSet<Vector3Int>(chunks.Keys);
            for (int x = from.x; x <= to.x; x++)
            {
                for (int y = from.y; y <= to.y; y++)
                {
                    for (int z = from.z; z <= to.z; z++)
                    {
                        Vector3Int key = new Vector3Int(x, y, z);
                        Chunk chunk;
                        if (!chunks.TryGetValue(key, out chunk))
                        {
                            if (garbageChunks.TryGetValue(key, out chunk))
                            {
                                chunk.SetActive(true);
                                chunks[key] = chunk;
                                garbageChunks.Remove(key);
                            }
                            else
                            {
                                chunks[key] = chunk = new Chunk(key, this);
                                chunk.SetActive(true);
                            }
                        }

                        if (!chunk.IsInited()) chunkRequests.Add(chunk);
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
                if (ch.IsInited())
                    garbageChunks[key] = ch;
            }

            if (garbageChunks.Count > 5000)
            {
                while (garbageChunks.Count > 2500)
                {
                    var iter = garbageChunks.Keys.GetEnumerator();
                    iter.MoveNext();
                    var key = iter.Current;
                    garbageChunks[key].Destroy();
                    garbageChunks.Remove(key);
                }
            }
        }

        private void DestroyGarbageChunkIfExists(Vector3Int chunkPos)
        {
            if (!garbageChunks.TryGetValue(chunkPos, out var chunk)) return;
            chunk.Destroy();
            garbageChunks.Remove(chunkPos);
        }

        public Chunk GetChunkIfInited(Vector3Int chunkPos)
        {
            if (chunks.TryGetValue(chunkPos, out var chunk) && chunk.IsInited() && chunk.IsActive())
                return chunk;
            return null;
        }
        
        public void PutBlock(VoxelPosition vp, BlockType type)
        {
            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk == null) return;
            if (type is MetaBlockType blockType)
                chunk.PutMeta(vp, blockType, player.placeLand);
            else
                chunk.PutVoxel(vp, type, player.placeLand);
        }
        
        public bool PutMetaWithProps(VoxelPosition vp, MetaBlockType type, object props, Land ownerLand = null)
        {
            var pos = vp.ToWorld();
            if (ownerLand == null && !player.CanEdit(pos, out ownerLand, true) || !WorldService.INSTANCE.IsSolid(vp))
                return false;

            var chunk = GetChunkIfInited(vp.chunk);
            if (chunk != null)
            {
                chunk.PutMeta(vp, type, ownerLand);
                chunk.GetMetaAt(vp).SetProps(props, ownerLand);
            }
            else
            {
                DestroyGarbageChunkIfExists(vp.chunk);
                WorldService.INSTANCE.AddMetaBlock(vp, type, ownerLand)[vp.local].SetProps(props, ownerLand);
            }

            return true;
        }

        public void PutBlocks(Dictionary<VoxelPosition, Tuple<BlockType, Land>> blocks)
        {
            var chunks = new Dictionary<Chunk, Dictionary<VoxelPosition, Tuple<BlockType, Land>>>();
            foreach (var vp in blocks.Keys)
            {
                var chunk = GetChunkIfInited(vp.chunk);
                if (chunk == null)
                {
                    DestroyGarbageChunkIfExists(vp.chunk);
                    WorldService.INSTANCE.AddChange(vp, blocks[vp].Item1,
                        blocks[vp].Item2); // TODO: re-draw neighbors?
                    continue;
                }

                if (!chunks.TryGetValue(chunk, out var chunkData))
                {
                    chunkData = new Dictionary<VoxelPosition, Tuple<BlockType, Land>>();
                    chunks.Add(chunk, chunkData);
                }

                chunkData.Add(vp, blocks[vp]);
            }

            foreach (var chunk in chunks.Keys)
                chunk.PutVoxels(chunks[chunk]);
        }

        public void DeleteBlocks(Dictionary<VoxelPosition, Land> blocks)
        {
            var chunks = new Dictionary<Chunk, Dictionary<VoxelPosition, Land>>();
            // var nullChunkNeighbors = new List<Chunk>();

            foreach (var vp in blocks.Keys)
            {
                var chunk = GetChunkIfInited(vp.chunk);
                if (chunk == null)
                {
                    WorldService.INSTANCE.AddChange(vp, blocks[vp]); // TODO: re-draw neighbors?
                    continue;
                }

                if (!chunks.TryGetValue(chunk, out var chunkData))
                {
                    chunkData = new Dictionary<VoxelPosition, Land>();
                    chunks.Add(chunk, chunkData);
                }

                chunkData.Add(vp, blocks[vp]);
            }

            foreach (var chunk in chunks.Keys)
                chunk.DeleteVoxels(chunks[chunk]);
        }

        public bool IsSolidAt(Vector3Int pos)
        {
            var vp = new VoxelPosition(pos);
            Chunk chunk;
            if (chunks.TryGetValue(vp.chunk, out chunk) && chunk.IsInited())
                return chunk.GetBlock(vp.local).isSolid;
            return WorldService.INSTANCE.IsSolid(vp);
        }

        public static World INSTANCE
        {
            get { return GameObject.Find("World").GetComponent<World>(); }
        }
    }
}