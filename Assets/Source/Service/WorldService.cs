using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Source.Canvas;
using Source.MetaBlocks;
using Source.MetaBlocks.MarkerBlock;
using Source.Model;
using Source.Service.Ethereum;
using Source.Ui.Menu;
using Source.Utils;
using UnityEngine;
using UnityEngine.Events;
using Vector3 = UnityEngine.Vector3;

namespace Source.Service
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
                    cloned.ApplyChanges(c);
                consumer.Invoke(cloned);
            });
        }

        public void GetMetaBlock(MetaPosition vp, Action<MetaBlock> consumer)
        {
            if (changes.TryGetValue(vp.chunk, out var chunkData) && chunkData.metaBlocks != null &&
                chunkData.metaBlocks.TryGetValue(vp.local, out var metaBlock))
            {
                consumer.Invoke(metaBlock);
                return;
            }

            WorldSliceService.INSTANCE.GetChunk(vp.chunk, chunkData =>
            {
                if (chunkData?.metaBlocks != null &&
                    chunkData.metaBlocks.TryGetValue(vp.local, out var metaBlock))
                {
                    consumer.Invoke(metaBlock);
                    return;
                }
                consumer.Invoke(null);
            });
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
            return GetBlockTypeIfLoaded(vp)?.isSolid ?? ChunkInitializer.IsDefaultSolidAt(vp);
        }

        public void GetBlockType(VoxelPosition voxelPosition, Action<BlockType> consumer)
        {
            if (changes.TryGetValue(voxelPosition.chunk, out var chunkChange)
                && chunkChange.blocks != null && chunkChange.blocks.TryGetValue(voxelPosition.local, out var block))
            {
                consumer.Invoke(Blocks.GetBlockType(block));
                return;
            }

            WorldSliceService.INSTANCE.GetChunk(voxelPosition.chunk,
                chunk => { consumer.Invoke(chunk?.GetBlockTypeAt(voxelPosition.local)); });
        }


        public BlockType GetBlockTypeIfLoaded(VoxelPosition vp)
        {
            if (changes.TryGetValue(vp.chunk, out var chunkChange)
                && chunkChange.blocks != null && chunkChange.blocks.TryGetValue(vp.local, out var block))
                return Blocks.GetBlockType(block);

            var type = WorldSliceService.INSTANCE.GetChunkIfLoaded(vp.chunk)?.GetBlockTypeAt(vp.local);
            return type;
        }

        public List<Land> GetPlayerLands()
        {
            return landRegistry.GetLandsForOwner(Settings.WalletId());
        }

        public IEnumerator GetLandsChanges(string wallet, List<Land> lands,
            Action<Dictionary<long, LandDetails>> consumer, Action failure)
        {
            var filteredLands = lands.FindAll(l => changedLands.Contains(l));

            yield return LandDetailsService.INSTANCE.GetOrCreate(filteredLands,
                details => consumer(ClearDefaults(ApplyChanges(details))),
                failure);
        }

        private Dictionary<long, LandDetails> ClearDefaults(Dictionary<long, LandDetails> detailsMap)
        {
            foreach (var details in detailsMap.Values)
            {
                if (details.changes != null)
                {
                    var toClear = new List<string>(details.changes.Keys.Where(key =>
                    {
                        var position = new VoxelPosition(LandDetails.ParseIntKey(key));
                        return ChunkInitializer.GetDefaultAt(position, true, true)
                            .name.Equals(details.changes[key].name);
                    }));
                    foreach (var key in toClear)
                        details.changes.Remove(key);
                }
            }

            return detailsMap;
        }

        private Dictionary<long, LandDetails> ApplyChanges(Dictionary<long, LandDetails> detailsMap)
        {
            foreach (var changeEntry in changes)
            {
                var chunkPosition = new Vector2Int(changeEntry.Key.x, changeEntry.Key.z);
                var candidateLands = new List<Land>(landRegistry.GetLandsForChunk(chunkPosition));
                candidateLands = candidateLands.FindAll(l => detailsMap.ContainsKey(l.id));
                if (candidateLands.Count == 0) continue;

                // TODO [detach metablock]: refactor?
                var findIntDetails = new Func<Vector3Int, Tuple<LandDetails, string>>(changePos =>
                {
                    var pos = VoxelPosition.ToWorld(changeEntry.Key, changePos);
                    var land = candidateLands.Find(l => l.Contains(pos));
                    if (land != null && detailsMap.TryGetValue(land.id, out var details))
                    {
                        pos -= land.startCoordinate.ToVector3();
                        var key = LandDetails.FormatIntKey(pos);
                        return new Tuple<LandDetails, string>(details, key);
                    }

                    return null;
                });
                
                var findDetails = new Func<MetaLocalPosition, Tuple<LandDetails, string>>(changePos =>
                {
                    var pos = MetaPosition.ToWorld(changeEntry.Key, changePos);
                    var land = candidateLands.Find(l => l.Contains(pos));
                    if (land != null && detailsMap.TryGetValue(land.id, out var details))
                    {
                        pos -= land.startCoordinate.ToVector3();
                        var key = LandDetails.FormatKey(pos);
                        return new Tuple<LandDetails, string>(details, key);
                    }

                    return null;
                });

                if (changeEntry.Value.blocks != null)
                    foreach (var blockEntry in changeEntry.Value.blocks)
                    {
                        //FIXME do not save redundant data
                        var dt = findIntDetails(blockEntry.Key);
                        if (dt != null)
                        {
                            dt.Item1.changes[dt.Item2] =
                                new Block {name = Blocks.GetBlockType(blockEntry.Value).name};
                        }
                    }


                if (changeEntry.Value.metaBlocks != null)
                    foreach (var blockEntry in changeEntry.Value.metaBlocks)
                    {
                        var metaBlock = blockEntry.Value;
                        var metaBlockType = metaBlock.type;
                        if (metaBlockType != null && metaBlockType.inMemory)
                            continue;

                        var dt = findDetails(blockEntry.Key);
                        var props = metaBlock.GetProps();
                        if (dt != null)
                        {
                            if (metaBlockType == null || metaBlock == MetaBlock.DELETED_METABLOCK || props == null)
                                dt.Item1.metadata.Remove(dt.Item2);
                            else
                            {
                                dt.Item1.metadata[dt.Item2] = new MetaBlockData
                                {
                                    properties = JsonConvert.SerializeObject(props),
                                    type = metaBlockType.name
                                };
                            }
                        }
                    }
            }

            foreach (var entry in detailsMap)
                entry.Value.properties = landRegistry.GetLands()[entry.Key].properties;

            return detailsMap;
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
                                var key = LandDetails.FormatIntKey(worldPos - land.startCoordinate.ToVector3());
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

        public void OnMetaRemoved(MetaBlock block, MetaPosition position)
        {
            if (block.type is MarkerBlockType)
                markerBlocks.Remove(position.ToVoxelPosition().ToWorld());
            AddMetaBlock(position, null, block.land);
        }

        public MetaBlock AddMetaBlock(MetaPosition pos, MetaBlockType type, Land land)
        {
            if (!changes.TryGetValue(pos.chunk, out var chunkChanges))
            {
                chunkChanges = new ChunkData(pos.chunk, null, null);
                changes[pos.chunk] = chunkChanges;
            }

            if (chunkChanges.metaBlocks == null)
                chunkChanges.metaBlocks = new Dictionary<MetaLocalPosition, MetaBlock>();

            if (chunkChanges.metaBlocks.TryGetValue(pos.local, out var prev))
                prev.DestroyView();
            WorldSliceService.INSTANCE.GetChunk(pos.chunk, data =>
            {
                if (data != null && data.metaBlocks != null && data.metaBlocks.TryGetValue(pos.local, out var prev))
                    prev.DestroyView();
            });


            var block = chunkChanges.metaBlocks[pos.local] =
                type == null ? MetaBlock.DELETED_METABLOCK : type.Instantiate(land, "");

            // Debug.Log(block.type);
            if (type is MarkerBlockType)
                markerBlocks.Add(pos.ToVoxelPosition().ToWorld(), block);

            if (land != null)
                changedLands.Add(land);
            if (type != null)
                blockPlaced.Invoke(new BlockPlaceEvent(pos.ToWorld(), type.name));

            return type == null ? null : block;
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

            public BlockPlaceEvent(Vector3 position, string type)
            {
                this.position = new SerializableVector3(position);
                this.type = type;
            }
        }

        public IEnumerator ReloadLandOwnerAndNft(long id, Action success, Action failure)
        {
            var ids = new List<BigInteger>();
            ids.Add(id);
            yield return EthereumClientService.INSTANCE.GetLandsByIds(ids, lands =>
            {
                var land = landRegistry.Get(id);
                if (lands.Count == 1 && land != null)
                {
                    land.owner = lands[0].owner;
                    land.isNft = lands[0].isNft;
                }

                success();
            }, failure);
        }
    }
}