using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using Debug = UnityEngine.Debug;

namespace src.Service
{
    //TODO refactor into several services
    public class WorldService
    {
        public static WorldService INSTANCE = new WorldService();
        private Dictionary<uint, BlockType> types = new Dictionary<uint, BlockType>();
        private Dictionary<Vector3Int, Dictionary<Vector3Int, uint>> changes = null;
        private Dictionary<Vector3Int, Dictionary<Vector3Int, MetaBlock>> metaBlocks = null;
        private Dictionary<Vector3Int, MetaBlock> markerBlocks = new Dictionary<Vector3Int, MetaBlock>();
        private HashSet<Land> changedLands = new HashSet<Land>();
        private readonly LandRegistry landRegistry = new LandRegistry();
        public readonly UnityEvent<object> blockPlaced = new UnityEvent<object>();

        public WorldService()
        {
            types[0] = new BlockType(0, "air", false, 0, 0, 0, 0, 0, 0);
            types[1] = new BlockType(1, "grass", true, 10, 10, 10, 10, 7, 11);
            types[2] = new BlockType(2, "dark_grass", true, 35, 35, 35, 35, 7, 13);
            types[3] = new BlockType(3, "bedrock", true, 0, 0, 0, 0, 0, 0);
            types[4] = new BlockType(4, "dirt", true, 7, 7, 7, 7, 7, 7);
            types[5] = new BlockType(5, "stone", true, 29, 29, 29, 29, 29, 29);
            types[6] = new BlockType(6, "sand", true, 27, 27, 27, 27, 27, 27);
            types[7] = new BlockType(7, "bricks", true, 3, 3, 3, 3, 3, 3);
            types[8] = new BlockType(8, "wood", true, 19, 19, 19, 19, 20, 20);
            types[9] = new BlockType(9, "planks", true, 21, 21, 21, 21, 21, 21);
            types[10] = new BlockType(10, "cobblestone", true, 4, 4, 4, 4, 4, 4);
            types[11] = new BlockType(11, "black_terracotta", true, 1, 1, 1, 1, 1, 1);
            types[12] = new BlockType(12, "blue_wool", true, 2, 2, 2, 2, 2, 2);
            types[13] = new BlockType(13, "cyan_wool", true, 5, 5, 5, 5, 5, 5);
            types[14] = new BlockType(14, "diamond", true, 6, 6, 6, 6, 6, 6);
            types[15] = new BlockType(15, "end_stone", true, 8, 8, 8, 8, 8, 8);
            types[16] = new BlockType(16, "gold", true, 9, 9, 9, 9, 9, 9);
            types[17] = new BlockType(17, "gravel", true, 12, 12, 12, 12, 12, 12);
            types[18] = new BlockType(18, "green_wool", true, 13, 13, 13, 13, 13, 13);
            types[19] = new BlockType(19, "ice", true, 14, 14, 14, 14, 14, 14);
            types[20] = new BlockType(20, "lime_wool", true, 15, 15, 15, 15, 15, 15);
            types[21] = new BlockType(21, "magma", true, 16, 16, 16, 16, 16, 16);
            types[22] = new BlockType(22, "mossy_stone_bricks", true, 17, 17, 17, 17, 17, 17);
            types[23] = new BlockType(23, "nether_bricks", true, 18, 18, 18, 18, 18, 18);
            types[24] = new BlockType(24, "polished_andesite", true, 22, 22, 22, 22, 22, 22);
            types[25] = new BlockType(25, "purple_wool", true, 23, 23, 23, 23, 23, 23);
            types[26] = new BlockType(26, "purpur", true, 24, 24, 24, 24, 24, 24);
            types[27] = new BlockType(27, "quartz", true, 25, 25, 25, 25, 25, 25);
            types[28] = new BlockType(28, "red_wool", true, 26, 26, 26, 26, 26, 26);
            types[29] = new BlockType(29, "snow", true, 28, 28, 28, 28, 28, 28);
            types[30] = new BlockType(30, "stone_bricks", true, 30, 30, 30, 30, 30, 30);
            types[31] = new ImageBlockType(31);
            types[32] = new VideoBlockType(32);
            types[33] = new LinkBlockType(33);
            types[34] = new TdObjectBlockType(34);
            types[35] = new MarkerBlockType(35);
            // types[36] = new LightBlockType(36);
            types[37] = new NftBlockType(37);
        }

        public List<string> GetNonMetaBlockTypes()
        {
            return types.Values
                .Where(blockType => !(blockType is MetaBlockType))
                .Select(x => x.name).ToList();
        }

        public List<BlockType> GetBlockTypes()
        {
            return types.Values.ToList();
        }

        public IEnumerator GetMetaBlocksForChunk(Vector3Int coordinate, Action<Dictionary<Vector3Int, MetaBlock>> done)
        {
            Dictionary<Vector3Int, MetaBlock> blocks;
            metaBlocks.TryGetValue(coordinate, out blocks);
            done.Invoke(blocks);
            yield break;
        }

        public IEnumerator FillChunk(Vector3Int coordinate, uint[,,] voxels)
        {
            InitiateChunk(coordinate, voxels);

            Dictionary<Vector3Int, uint> chunkChanges;
            if (changes.TryGetValue(coordinate, out chunkChanges))
            {
                foreach (var change in chunkChanges)
                {
                    var voxel = change.Key;
                    voxels[voxel.x, voxel.y, voxel.z] = change.Value;
                }
            }

            yield break;
        }


        private void InitiateChunk(Vector3Int position, uint[,,] voxels)
        {
            var stone = GetBlockType("end_stone").id;
            uint body;
            uint top;

            if (position.y == 0)
            {
                InitGroundLevel(position, voxels, stone);
                return;
            }

            if (position.y < 0)
            {
                top = body = stone;
            }
            else
                body = top = GetBlockType("air").id;

            for (int x = 0; x < voxels.GetLength(0); x++)
            {
                for (int z = 0; z < voxels.GetLength(2); z++)
                {
                    FillAtXY(top, body, x, z, voxels);
                }
            }
        }

        private void InitGroundLevel(Vector3Int position, uint[,,] voxels, uint stone)
        {
            uint body, top;
            var grass = GetBlockType("grass").id;
            var darkGrass = GetBlockType("dark_grass").id;
            var dirt = GetBlockType("dirt").id;

            var lands = landRegistry.GetLandsForChunk(new Vector2Int(position.x, position.z));
            var wallet = Settings.WalletId();

            for (var x = 0; x < Chunk.CHUNK_WIDTH; ++x)
            {
                for (var z = 0; z < Chunk.CHUNK_WIDTH; ++z)
                {
                    top = body = stone;
                    if (lands != null)
                    {
                        var pos = new Vector3Int(x + position.x * Chunk.CHUNK_WIDTH, 0,
                            z + position.z * Chunk.CHUNK_WIDTH);
                        var land = lands.FirstOrDefault(land => land.Contains(pos));
                        if (land != null)
                        {
                            body = dirt;
                            top = land.owner.Equals(wallet) ? darkGrass : grass;
                        }
                    }

                    FillAtXY(top, body, x, z, voxels);
                }
            }
        }

        private void FillAtXY(uint top, uint body, int x, int z, uint[,,] voxels)
        {
            int maxy = voxels.GetLength(1) - 1;

            voxels[x, maxy, z] = top;
            for (int y = 0; y < maxy; y++)
                voxels[x, y, z] = body;
        }

        public BlockType GetBlockType(uint id)
        {
            return ColorBlocks.IsColorTypeId(id, out var blockType) ? blockType : types[id];
        }

        public BlockType GetBlockType(string name, bool excludeMetaBlocks = false, bool excludeBaseBlocks = false)
        {
            if (ColorBlocks.IsColorBlockType(name, out var blockType))
                return blockType;

            foreach (var entry in from entry in types
                     where !excludeMetaBlocks || !(entry.Value is MetaBlockType)
                     where !excludeBaseBlocks || entry.Value is MetaBlockType
                     where entry.Value.name.Equals(name)
                     select entry)
                return entry.Value;

            Debug.LogError("Invalid block type: " + name);
            return null;
        }

        public MetaBlock GetMetaAt(VoxelPosition vp)
        {
            Dictionary<Vector3Int, MetaBlock> chunk;
            if (metaBlocks.TryGetValue(vp.chunk, out chunk))
            {
                MetaBlock block;
                if (chunk.TryGetValue(vp.local, out block))
                {
                    return block;
                }
            }

            return null;
        }
        
        
        public IEnumerator IsSolid(VoxelPosition voxelPosition, Action<bool> consumer)
        {
            consumer.Invoke(IsSolidIfLoaded(voxelPosition));
            yield break;
        }
        
        public bool IsSolidIfLoaded(VoxelPosition vp)
        {
            Dictionary<Vector3Int, uint> chunkChanges;
            if (changes.TryGetValue(vp.chunk, out chunkChanges))
            {
                uint type;
                if (chunkChanges.TryGetValue(vp.local, out type))
                {
                    return GetBlockType(type).isSolid;
                }
            }

            return vp.chunk.y <= 0;
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
                var pos = LandDetails.PraseKey(entry.Key) + landStart;
                if (land.Contains(pos))
                {
                    var type = GetBlockType(change.name);
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
                var pos = LandDetails.PraseKey(entry.Key) + landStart;
                var position = new VoxelPosition(pos);
                if (land.Contains(pos))
                {
                    var type = (MetaBlockType) GetBlockType(meta.type);
                    if (type == null) continue;
                    try
                    {
                        var block = type.New(land, meta.properties);
                        Dictionary<Vector3Int, MetaBlock> chunk;
                        if (!metaBlocks.TryGetValue(position.chunk, out chunk))
                            metaBlocks[position.chunk] = chunk = new Dictionary<Vector3Int, MetaBlock>();
                        chunk[position.local] = block;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Exception occured while parsing meta props. " + ex);
                    }
                }
            }
        }

        private IEnumerator LoadDetails(Loading loading, Action<Land, LandDetails> consumer, Action onFailed)
        {
            loading.UpdateText("Loading Lands\n0/0");

            var failed = false;
            yield return landRegistry.ReloadLands(() =>
            {
                failed = true;
                onFailed();
            });
            if (failed) yield break;


            var playerLands = landRegistry.GetLandsForOwner(Settings.WalletId()); 
            var landsCount = playerLands.Count;
            var enums = new IEnumerator[landsCount];

            var index = 0;
            foreach (var land in playerLands)
            {
                if (!string.IsNullOrWhiteSpace(land.ipfsKey))
                    enums[index] = IpfsClient.INSATANCE.GetLandDetails(land, details => consumer(land, details));
                index++;
            }

            for (int i = 0; i < landsCount; i++)
                if (enums[i] != null)
                {
                    loading.UpdateText(string.Format("Loading Lands\n{0}/{1}", i, enums.Length));
                    yield return enums[i];
                }
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
                change.name = GetBlockType(type).name;
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

            metas[pos.local] = type.New(land, "");

            if (type is MarkerBlockType)
                markerBlocks.Add(pos.ToWorld(), metas[pos.local]);

            changedLands.Add(land);
            blockPlaced.Invoke(new Tuple<Vector3Int, string>(pos.ToWorld(), type.name));

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
            blockPlaced.Invoke(new Tuple<Vector3Int, string>(pos.ToWorld(), type.name));
        }

        public void AddChange(VoxelPosition pos, Land land)
        {
            AddChange(pos, types[0], land);
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

        public Land GetLandByPosition(Vector3 position)
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
    }
}