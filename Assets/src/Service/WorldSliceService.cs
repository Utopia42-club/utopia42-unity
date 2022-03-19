using System;
using System.Collections;
using System.Collections.Generic;
using src.MetaBlocks;
using src.Model;
using src.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace src.Service
{
    internal class WorldSliceService
    {
        internal static WorldSliceService INSTANCE = new WorldSliceService();

        /**
         * Written using expression so no one can change the properties
         */
        internal static Vector3Int SLICE_SIZE => Chunk.CHUNK_SIZE * 64;

        private readonly Dictionary<Vector3Int, SliceData> slices = new Dictionary<Vector3Int, SliceData>();

        private readonly Dictionary<Vector3Int, UnityEvent<SliceData>> loadingSlices =
            new Dictionary<Vector3Int, UnityEvent<SliceData>>();


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
            Debug.Log("Loading at: " + start);
            var e = loadingSlices[start] = new UnityEvent<SliceData>();
            e.AddListener(consumer.Invoke);

            string url = Constants.ApiURL + "/world/slice";
            var slice = new WorldSlice
            {
                startCoordinate = new SerializableVector3Int(start),
                endCoordinate = new SerializableVector3Int(start + SLICE_SIZE)
            };
            World.INSTANCE.StartCoroutine(RestClient.Post<WorldSlice, WorldSlice>(url, slice, OnLoad,
                () => { Debug.LogError("Failed!"); }));
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
                var chunkPos = LandDetails.ParseKey(chunkEntry.Key);
                var chunkBlocks = new Dictionary<Vector3Int, uint>();
                foreach (var blockEntry in chunkEntry.Value)
                    chunkBlocks[LandDetails.ParseKey(blockEntry.Key)] =
                        worldService.GetBlockType(blockEntry.Value.name).id;

                chunks[chunkPos] = new ChunkData(chunkPos, chunkBlocks, null);
            }

            foreach (var chunkEntry in slice.metaBlocks)
            {
                var chunkPos = LandDetails.ParseKey(chunkEntry.Key);
                var chunkMetaBlocks = new Dictionary<Vector3Int, MetaBlock>();
                foreach (var blockEntry in chunkEntry.Value)
                {
                    var localPosition = LandDetails.ParseKey(blockEntry.Key);
                    var metaBlock = MetaBlock.Parse(
                        WorldService.INSTANCE.GetLandForPosition(VoxelPosition.ToWorld(chunkPos, localPosition)),
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