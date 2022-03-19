using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using src.Canvas;
using src.MetaBlocks;
using src.MetaBlocks.ImageBlock;
using src.MetaBlocks.LinkBlock;
using src.MetaBlocks.MarkerBlock;
using src.MetaBlocks.NftBlock;
using src.MetaBlocks.TdObjectBlock;
using src.MetaBlocks.VideoBlock;
using src.Model;
using src.Service.Migration;
using src.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace src.Service
{
    //TODO refactor into several services
    public class WorldService
    {
        public static WorldService INSTANCE = new WorldService();
        private Dictionary<Vector3Int, Dictionary<Vector3Int, uint>> changes = null;
        private Dictionary<Vector3Int, Dictionary<Vector3Int, MetaBlock>> metaBlocks = null;
        private Dictionary<Vector3Int, MetaBlock> markerBlocks = new Dictionary<Vector3Int, MetaBlock>();
        private HashSet<Land> changedLands = new HashSet<Land>();
        private readonly LandRegistry landRegistry = new LandRegistry();
        public readonly UnityEvent<object> blockPlaced = new UnityEvent<object>();

        public void GetMetaBlocksForChunk(Vector3Int coordinate, Action<Dictionary<Vector3Int, MetaBlock>> consumer)
        {
            WorldSliceService.INSTANCE.GetChunk(coordinate,
                chunk => consumer.Invoke(chunk?.metaBlocks ?? new Dictionary<Vector3Int, MetaBlock>()));
        }

        public void FillChunk(Vector3Int coordinate, uint[,,] voxels, Action done)
        {
            ChunkInitializer.InitializeChunk(coordinate, voxels);
            WorldSliceService.INSTANCE.GetChunk(coordinate, data =>
            {
                if (data?.blocks == null)
                {
                    done.Invoke();
                    return;
                }

                foreach (var change in data.blocks)
                {
                    var voxel = change.Key;
                    voxels[voxel.x, voxel.y, voxel.z] = change.Value;
                }

                done.Invoke();
            });
            // Dictionary<Vector3Int, uint> chunkChanges;
            // if (changes.TryGetValue(coordinate, out chunkChanges))
            // {
            //     foreach (var change in chunkChanges)
            //     {
            //         var voxel = change.Key;
            //         voxels[voxel.x, voxel.y, voxel.z] = change.Value;
            //     }
            // }
        }

        public void IsSolid(VoxelPosition voxelPosition, Action<bool> consumer)
        {
            WorldSliceService.INSTANCE.GetChunk(voxelPosition.chunk, chunk =>
            {
                consumer.Invoke(chunk != null && chunk.blocks != null &&
                                chunk.blocks.TryGetValue(voxelPosition.local, out var typeId) &&
                                Blocks.GetBlockType(typeId).isSolid);
            });
        }

        public bool IsSolidIfLoaded(VoxelPosition vp)
        {
            var chunkData = WorldSliceService.INSTANCE.GetChunkIfLoaded(vp.chunk);

            return (chunkData?.blocks != null && chunkData.blocks.TryGetValue(vp.local, out var typeId))
                ? Blocks.GetBlockType(typeId).isSolid
                : vp.chunk.y <= 0;
            // Dictionary<Vector3Int, uint> chunkChanges;
            // if (changes.TryGetValue(vp.chunk, out chunkChanges))
            // {
            //     uint type;
            //     if (chunkChanges.TryGetValue(vp.local, out type))
            //     {
            //         return GetBlockType(type).isSolid;
            //     }
            // }
            //
            // return vp.chunk.y <= 0;
        }


        public List<Land> GetPlayerLands()
        {
            return landRegistry.GetLandsForOwner(Settings.WalletId());
        }

        public IEnumerator Initialize(Loading loading, Action onDone, Action onFailed)
        {
            if (IsInitialized()) yield break;
            var migrationService = new MigrationService();
            if (!migrationService.GetLatestVersion().Equals("0.2.0"))
                throw new Exception("Unsupported migration latest version.");
            var changes = new Dictionary<Vector3Int, Dictionary<Vector3Int, uint>>();
            var metaBlocks = new Dictionary<Vector3Int, Dictionary<Vector3Int, MetaBlock>>();

            var failed = false;

            yield return LoadDetails(loading, (land, details) =>
            {
                details = migrationService.Migrate(land, details);
                land.properties = details.properties;
                if (details.metadata != null)
                    ReadMetadata(land, details, metaBlocks);
                if (details.changes != null)
                    ReadChanges(land, details, changes);
            }, () =>
            {
                failed = true;
                onFailed();
            });

            if (failed) yield break;

            this.changes = changes;
            this.metaBlocks = metaBlocks;
            onDone.Invoke();
        }

        private void ReadChanges(Land land, LandDetails details,
            Dictionary<Vector3Int, Dictionary<Vector3Int, uint>> changes)
        {
            var landStart = land.startCoordinate.ToVector3();
            foreach (var entry in details.changes)
            {
                var change = entry.Value;
                var pos = LandDetails.ParseKey(entry.Key) + landStart;
                if (land.Contains(pos))
                {
                    var type = Blocks.GetBlockType(change.name);
                    if (type == null) continue;

                    var position = new VoxelPosition(pos);
                    Dictionary<Vector3Int, uint> chunk;
                    if (!changes.TryGetValue(position.chunk, out chunk))
                        changes[position.chunk] = chunk = new Dictionary<Vector3Int, uint>();
                    chunk[position.local] = type.id;
                }
            }
        }

        private void ReadMetadata(Land land, LandDetails details,
            Dictionary<Vector3Int, Dictionary<Vector3Int, MetaBlock>> metaBlocks)
        {
            var landStart = land.startCoordinate.ToVector3();
            foreach (var entry in details.metadata)
            {
                var meta = entry.Value;
                var pos = LandDetails.ParseKey(entry.Key) + landStart;
                var position = new VoxelPosition(pos);
                if (land.Contains(pos))
                {
                    var block = MetaBlock.Parse(land, meta);
                    if (block == null) continue;
                    Dictionary<Vector3Int, MetaBlock> chunk;
                    if (!metaBlocks.TryGetValue(position.chunk, out chunk))
                        metaBlocks[position.chunk] = chunk = new Dictionary<Vector3Int, MetaBlock>();
                    chunk[position.local] = block;
                }
            }
        }

        private IEnumerator LoadDetails(Loading loading, Action<Land, LandDetails> consumer, Action onFailed)
        {
            loading.UpdateText("Loading Lands...");

            var failed = false;
            yield return landRegistry.ReloadLands(() =>
            {
                failed = true;
                onFailed();
            });
            // if (failed) yield break;

            //
            // var playerLands = landRegistry.GetLandsForOwner(Settings.WalletId());
            // var landsCount = playerLands.Count;
            // var enums = new IEnumerator[landsCount];
            //
            // var index = 0;
            // foreach (var land in playerLands)
            // {
            //     if (!string.IsNullOrWhiteSpace(land.ipfsKey))
            //         enums[index] = IpfsClient.INSATANCE.GetLandDetails(land, details => consumer(land, details));
            //     index++;
            // }
            //
            // for (int i = 0; i < landsCount; i++)
            //     if (enums[i] != null)
            //     {
            //         loading.UpdateText(string.Format("Loading Lands\n{0}/{1}", i, enums.Length));
            //         yield return enums[i];
            //     }
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

            Stream(filteredLands, changes, (key, type, land) =>
            {
                var change = new Block();
                change.name = Blocks.GetBlockType(type).name;
                landDetailsMap[land.id].changes[key] = change;
            }, m => true); //Filter can check if the block is default
            Stream(filteredLands, metaBlocks, (key, metaBlock, land) =>
            {
                var properties = metaBlock.GetProps();
                if (properties == null) return;
                var metadata = new MetaBlockData();
                metadata.properties = JsonConvert.SerializeObject(properties);
                metadata.type = metaBlock.type.name;
                landDetailsMap[land.id].metadata[key] = metadata;
            }, m => m.GetProps() != null && !m.type.inMemory);

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

        public Dictionary<Vector3Int, MetaBlock> AddMetaBlock(VoxelPosition pos, MetaBlockType type, Land land)
        {
            Dictionary<Vector3Int, MetaBlock> metas;
            if (!metaBlocks.TryGetValue(pos.chunk, out metas))
            {
                metas = new Dictionary<Vector3Int, MetaBlock>();
                metaBlocks[pos.chunk] = metas;
            }

            metas[pos.local] = type.Instantiate(land, "");

            if (type is MarkerBlockType)
                markerBlocks.Add(pos.ToWorld(), metas[pos.local]);

            changedLands.Add(land);
            blockPlaced.Invoke(new BlockPlaceEvent(pos.ToWorld(), type.name));

            return metas;
        }

        public void AddChange(VoxelPosition pos, BlockType type, Land land)
        {
            Dictionary<Vector3Int, uint> vc;
            if (!changes.TryGetValue(pos.chunk, out vc))
            {
                vc = new Dictionary<Vector3Int, uint>();
                changes[pos.chunk] = vc;
            }

            vc[pos.local] = type.id;
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
            return changes != null;
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