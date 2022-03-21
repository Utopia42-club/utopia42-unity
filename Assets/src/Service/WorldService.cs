using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using src.Canvas;
using src.MetaBlocks;
using src.MetaBlocks.MarkerBlock;
using src.Model;
using src.Service.Migration;
using src.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace src.Service
{
    public class WorldService
    {
        public static WorldService INSTANCE = new WorldService();

        private readonly Dictionary<Vector3Int, ChunkData> changes = new Dictionary<Vector3Int, ChunkData>();
        private readonly LandRegistry landRegistry = new LandRegistry();
        private HashSet<Land> changedLands = new HashSet<Land>();
        public readonly UnityEvent<object> blockPlaced = new UnityEvent<object>();
        private Dictionary<Vector3Int, MetaBlock> markerBlocks = new Dictionary<Vector3Int, MetaBlock>();
        private bool initialized = false;

        public void GetChunkData(Vector3Int coordinate, Action<ChunkData> consumer)
        {
            WorldSliceService.INSTANCE.GetChunk(coordinate, data =>
            {
                var cloned = data?.Clone() ?? new ChunkData(coordinate, null, null);
                if (changes.TryGetValue(coordinate, out var c))
                    cloned.AddAll(c);
                consumer.Invoke(cloned);
            });
        }

        public void IsSolid(VoxelPosition voxelPosition, Action<bool> consumer)
        {
            if (changes.TryGetValue(voxelPosition.chunk, out var chunkChange)
                && chunkChange.blocks != null && chunkChange.blocks.TryGetValue(voxelPosition.chunk, out var block))
            {
                consumer.Invoke(Blocks.GetBlockType(block).isSolid);
                return;
            }

            WorldSliceService.INSTANCE.GetChunk(voxelPosition.chunk, chunk =>
            {
                var type = chunk?.GetBlockTypeAt(voxelPosition.local);
                consumer.Invoke(type?.isSolid ?? ChunkInitializer.IsDefaultSolidAt(voxelPosition));
            });
        }

        public bool IsSolidIfLoaded(VoxelPosition vp)
        {
            if (changes.TryGetValue(vp.chunk, out var chunkChange)
                && chunkChange.blocks != null && chunkChange.blocks.TryGetValue(vp.chunk, out var block))
                return Blocks.GetBlockType(block).isSolid;

            var type = WorldSliceService.INSTANCE.GetChunkIfLoaded(vp.chunk)?.GetBlockTypeAt(vp.local);
            return type?.isSolid ?? ChunkInitializer.IsDefaultSolidAt(vp);
        }


        public List<Land> GetPlayerLands()
        {
            return landRegistry.GetLandsForOwner(Settings.WalletId());
        }

        public IEnumerator Initialize(Loading loading, Action onDone, Action onFailed)
        {
            if (IsInitialized()) yield break;
            var failed = false;

            yield return landRegistry.ReloadLands(() =>
            {
                failed = true;
                onFailed();
            });

            if (failed) yield break;
            initialized = true;
            onDone.Invoke();
        }

        public Dictionary<long, LandDetails> GetLandsChanges(string wallet, List<Land> lands)
        {
            var filteredLands = new List<Land>();
            var landDetailsMap = new Dictionary<long, LandDetails>();
            for (int i = 0; i < lands.Count; i++)
            {
                var land = lands[i];
                if (changedLands.Contains(land))
                {
                    var details = new LandDetails();
                    details.changes = new Dictionary<string, Block>();
                    details.metadata = new Dictionary<string, MetaBlockData>();
                    details.v = "0.2.0";
                    details.wallet = wallet;
                    details.properties = land.properties;
                    landDetailsMap[land.id] = details;
                    filteredLands.Add(land);
                }
            }

            // Stream(filteredLands, changes, (key, type, land) =>
            // {
            // var change = new Block();
            // change.name = Blocks.GetBlockType(type).name;
            // landDetailsMap[land.id].changes[key] = change;
            // }, m => true); //Filter can check if the block is default
            // Stream(filteredLands, metaBlocks, (key, metaBlock, land) =>
            // {
            // var properties = metaBlock.GetProps();
            // if (properties == null) return;
            // var metadata = new MetaBlockData();
            // metadata.properties = JsonConvert.SerializeObject(properties);
            // metadata.type = metaBlock.type.name;
            // landDetailsMap[land.id].metadata[key] = metadata;
            // }, m => m.GetProps() != null && !m.type.inMemory);

            return landDetailsMap;
        }

        public List<Marker> GetMarkers()
        {
            return (from marker in markerBlocks
                let props = (MarkerBlockProperties) marker.Value.GetProps()
                select new Marker(props?.name, marker.Key)).ToList();
        }

        /*
         * values: chunk pos -> (voxel pos -> data)
         *
         *  For each item in the values that passes the filter, finds the corresponding land and calls the consumer with: position key, value, land
         */
        private void Stream<T>(List<Land> lands, Dictionary<Vector3Int, Dictionary<Vector3Int, T>> values,
            Action<string, T, Land> consumer, Func<T, bool> filter)
        {
            foreach (var chunkEntry in values)
            {
                var cpos = chunkEntry.Key;
                var chunkValues = chunkEntry.Value;
                foreach (var voxelEntry in chunkValues)
                {
                    if (filter(voxelEntry.Value))
                    {
                        var vpos = voxelEntry.Key;
                        var worldPos = VoxelPosition.ToWorld(cpos, vpos);
                        foreach (var land in lands)
                        {
                            if (land.Contains(worldPos))
                            {
                                var key = LandDetails.FormatKey(worldPos - land.startCoordinate.ToVector3());
                                consumer(key, voxelEntry.Value, land);
                                break;
                            }
                        }
                    }
                }
            }
        }


        public void MarkLandChanged(Land land)
        {
            changedLands.Add(land);
        }

        public void OnMetaRemoved(MetaBlock block, Vector3Int position)
        {
            if (block.type is MarkerBlockType)
                markerBlocks.Remove(position);
            if (block.land != null)
                changedLands.Add(block.land);
        }

        public MetaBlock AddMetaBlock(VoxelPosition pos, MetaBlockType type, Land land)
        {
            if (!changes.TryGetValue(pos.chunk, out var chunkChanges))
            {
                chunkChanges = new ChunkData(pos.chunk, null, null);
                changes[pos.chunk] = chunkChanges;
            }

            if (chunkChanges.metaBlocks == null)
                chunkChanges.metaBlocks = new Dictionary<Vector3Int, MetaBlock>();
            var block = chunkChanges.metaBlocks[pos.local] = type.Instantiate(land, "");

            if (type is MarkerBlockType)
                markerBlocks.Add(pos.ToWorld(), block);

            changedLands.Add(land);
            blockPlaced.Invoke(new BlockPlaceEvent(pos.ToWorld(), type.name));

            return block;
        }

        public void AddChange(VoxelPosition pos, BlockType type, Land land)
        {
            if (!changes.TryGetValue(pos.chunk, out var chunkChanges))
            {
                chunkChanges = new ChunkData(pos.chunk, null, null);
                changes[pos.chunk] = chunkChanges;
            }

            if (chunkChanges.blocks == null)
                chunkChanges.blocks = new Dictionary<Vector3Int, uint>();

            chunkChanges.blocks[pos.local] = type.id;
            changedLands.Add(land);
            blockPlaced.Invoke(new BlockPlaceEvent(pos.ToWorld(), type.name));
        }

        public void AddChange(VoxelPosition pos, Land land)
        {
            AddChange(pos, Blocks.AIR, land);
        }

        public LandProperties GetLandProperties(int landId)
        {
            if (landRegistry.GetLands().TryGetValue(landId, out Land land))
            {
                return land.properties;
            }

            return null;
        }

        public bool UpdateLandProperties(int landId, LandProperties properties)
        {
            if (landRegistry.GetLands().TryGetValue(landId, out Land land))
            {
                land.properties = properties;
                MarkLandChanged(land);
                return true;
            }

            return false;
        }

        internal void RefreshChangedLands(List<Land> ownerLands)
        {
            var oldChanges = changedLands;
            changedLands = new HashSet<Land>();
            if (ownerLands == null) return;
            foreach (var land in ownerLands)
            {
                foreach (var change in oldChanges)
                {
                    if (land.Equals(change) && Equals(change.ipfsKey, land.ipfsKey))
                        changedLands.Add(land);
                }
            }
        }

        public bool HasChange()
        {
            return changedLands.Count > 0;
        }

        public Dictionary<string, List<Land>> GetOwnersLands()
        {
            return landRegistry.GetOwnersLands();
        }

        public Land GetLandForPosition(Vector3 position)
        {
            var vp = new VoxelPosition(position);

            var lands = landRegistry.GetLandsForChunk(new Vector2Int(vp.chunk.x, vp.chunk.z));
            return lands?.FirstOrDefault(l => l.Contains(position));
        }

        public IEnumerator ReloadPlayerLands(Action onFailed)
        {
            yield return landRegistry.ReloadLandsForOwner(Settings.WalletId(), onFailed);
        }

        public bool IsInitialized()
        {
            return initialized;
        }

        public IEnumerator ReloadLands(Action onFailed)
        {
            yield return landRegistry.ReloadLands(onFailed);
        }

        public HashSet<Land> GetLandsForChunk(Vector2Int coordinate)
        {
            return landRegistry.GetLandsForChunk(coordinate);
        }


        [Serializable]
        private class BlockPlaceEvent
        {
            public SerializableVector3 position;
            public string type;

            public BlockPlaceEvent(Vector3Int position, string type)
            {
                this.position = new SerializableVector3(position);
                this.type = type;
            }
        }
    }
}