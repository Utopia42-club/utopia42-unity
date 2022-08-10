using System;
using System.Collections;
using System.Collections.Generic;
using Source.Configuration;
using Source.MetaBlocks;
using Source.Model;
using Source.Service.Auth;
using Source.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Source.Service
{
    internal class WorldSliceService
    {
        /**
         * Written using expression so no one can change the properties
         */
        internal static Vector3Int SLICE_SIZE => Chunk.CHUNK_SIZE * 64;

        private readonly Dictionary<Vector3Int, SliceData> slices = new();

        private readonly Dictionary<Vector3Int, UnityEvent<SliceData>> loadingSlices = new();

        internal ChunkData GetChunkIfLoaded(Vector3Int coordinate)
        {
            var startOfSlice = GetStartOfSlice(coordinate);
            return slices.TryGetValue(startOfSlice, out var slice) ? slice.GetChunk(coordinate) : null;
        }

        internal void GetChunk(Vector3Int coordinate, Action<ChunkData> consumer)
        {
            var startOfSlice = GetStartOfSlice(coordinate);
            if (slices.TryGetValue(startOfSlice, out var slice))
                consumer(slice.GetChunk(coordinate));
            else if (loadingSlices.TryGetValue(startOfSlice, out var loadEvent))
                loadEvent.AddListener(loaded => consumer(loaded.GetChunk(coordinate)));
            else
                Load(startOfSlice, loaded => consumer(loaded.GetChunk(coordinate)));
        }


        private void Load(Vector3Int start, Action<SliceData> consumer)
        {
            if (!loadingSlices.TryGetValue(start, out var e))
                e = loadingSlices[start] = new UnityEvent<SliceData>();
            e.AddListener(consumer.Invoke);

            World.INSTANCE.StartCoroutine(DoLoad(new SerializableVector3Int(start)));
        }

        private IEnumerator DoLoad(SerializableVector3Int start)
        {
            var contract = AuthService.Instance.CurrentContract;
            string url =
                $"{Configurations.Instance.apiURL}/world/slice?network={contract.network.id}&contract={contract.id}";
            bool success = false;
            var timeout = 0.2f;
            while (!success)
            {
                yield return RestClient.Post<SerializableVector3Int, WorldSlice>
                (url, start, slice =>
                {
                    success = true;
                    OnLoad(slice);
                }, () =>
                {
                    Debug.LogError("Failed to load slice at: " + start);
                    success = false;
                });
                if (!success)
                {
                    Debug.Log($"Scheduling reload until {timeout} seconds, for slice at ({start})");
                    yield return new WaitForSeconds(timeout);
                    timeout *= 2;
                }
            }
        }

        private void OnLoad(WorldSlice slice)
        {
            var start = slice.startCoordinate.ToVector3();
            var sliceData = slices[start] = new SliceData(slice);
            loadingSlices[start].Invoke(sliceData);
            loadingSlices.Remove(start);
        }


        private Vector3Int GetStartOfSlice(Vector3 chunkCoordinate)
        {
            chunkCoordinate.Scale(Chunk.CHUNK_SIZE);
            var v = Vectors.TruncateFloor(chunkCoordinate.x / SLICE_SIZE.x, chunkCoordinate.y / SLICE_SIZE.y,
                chunkCoordinate.z / SLICE_SIZE.z);
            v.Scale(SLICE_SIZE);
            return v;
        }
    }

    internal class SliceData
    {
        private readonly Dictionary<Vector3Int, ChunkData> chunks = new Dictionary<Vector3Int, ChunkData>();

        public SliceData(WorldSlice slice)
        {
            var worldService = WorldService.INSTANCE;
            foreach (var chunkEntry in slice.blocks)
            {
                var chunkPos = LandDetails.ParseIntKey(chunkEntry.Key);
                var chunkBlocks = new Dictionary<Vector3Int, uint>();
                foreach (var blockEntry in chunkEntry.Value)
                    chunkBlocks[LandDetails.ParseIntKey(blockEntry.Key)] =
                        Blocks.GetBlockType(blockEntry.Value.name).id;

                chunks[chunkPos] = new ChunkData(chunkPos, chunkBlocks, null);
            }

            foreach (var chunkEntry in slice.metaBlocks)
            {
                var chunkPos = LandDetails.ParseIntKey(chunkEntry.Key);
                var chunkMetaBlocks = new Dictionary<MetaLocalPosition, MetaBlock>();
                foreach (var blockEntry in chunkEntry.Value)
                {
                    var localPosition = LandDetails.ParseKey(blockEntry.Key);
                    var metaBlock = MetaBlock.Parse(
                        WorldService.INSTANCE.GetLandForPosition(MetaPosition.ToWorld(chunkPos, localPosition)),
                        blockEntry.Value);

                    chunkMetaBlocks[localPosition] = metaBlock;
                }

                ChunkData chunkData;
                if (!chunks.TryGetValue(chunkPos, out chunkData))
                    chunks[chunkPos] = chunkData = new ChunkData(chunkPos, null, null);
                chunkData.metaBlocks = chunkMetaBlocks;
            }
        }

        internal ChunkData GetChunk(Vector3Int position)
        {
            return chunks.TryGetValue(position, out var chunk) ? chunk : null;
        }
    }
}