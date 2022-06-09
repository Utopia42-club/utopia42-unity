using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using src.Canvas;
using src.MetaBlocks;
using src.MetaBlocks.MarkerBlock;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace src
{
    public class UtopiaApi : MonoBehaviour
    {
        public UnityEvent<object> CurrentLand()
        {
            return FindObjectOfType<Owner>().currentLandChanged;
        }

        public UnityEvent<object> BlockPlaced()
        {
            return WorldService.INSTANCE.blockPlaced;
        }

        public string GetBlockTypeAt(string request)
        {
            var pos = JsonConvert.DeserializeObject<SerializableVector3Int>(request).ToVector3();
            var vp = new VoxelPosition(pos);
            return WorldService.INSTANCE.GetBlockTypeIfLoaded(vp)?.name;
        }

        public List<Marker> GetMarkers()
        {
            return WorldService.INSTANCE.GetMarkers();
        }

        public List<Land> GetPlayerLands(string walletId)
        {
            return WorldService.INSTANCE.GetPlayerLands();
        }

        public Land GetCurrentLand()
        {
            return WorldService.INSTANCE.GetLandForPosition(Player.INSTANCE.GetPosition());
        }

        public SerializableVector3 GetPlayerPosition()
        {
            var pos = Player.INSTANCE.GetPosition();
            return new SerializableVector3(pos);
        }

        public List<string> GetBlockTypes()
        {
            return Blocks.GetNonMetaBlockTypes();
        }

        public Dictionary<Vector3Int, bool> PlaceBlocks(string request)
        {
            return PlaceBlocksWithOffset(request, Vector3Int.zero);
        }

        public Dictionary<Vector3, bool> PlaceMetaBlocks(string request)
        {
            return PlaceMetaBlocksWithOffset(request, Vector3Int.zero);
        }

        public void PreviewBlocks(string request) // TODO [detach metablock]: add support for metablocks later
        {
            PreviewBlocksWithOffset(request, Vector3Int.zero);
        }

        public Dictionary<Vector3Int, bool> PlaceBlocksWithOffset(string request, Vector3Int offset)
        {
            if (!Player.INSTANCE.PluginWriteAllowed(out var msg))
            {
                Debug.LogWarning(msg);
                return null;
            }

            var reqs = JsonConvert.DeserializeObject<List<PlaceBlockRequest>>(request);

            var blocks = new Dictionary<VoxelPosition, BlockType>();
            var placed = new Dictionary<Vector3Int, bool>();

            foreach (var req in reqs)
            {
                var pos = new VoxelPosition(req.position.ToVector3() + offset);
                var globalPos = pos.ToWorld();
                if (placed.ContainsKey(globalPos))
                    continue;

                placed.Add(globalPos, false);
                var type = Blocks.GetBlockType(req.name);
                if (type != null) blocks.Add(pos, type);
            }

            var baseBlocksResult = PutBlocks(blocks);

            foreach (var pos in placed.Keys.ToArray())
            {
                if (baseBlocksResult.TryGetValue(pos, out var baseResult) && baseResult)
                    placed[pos] = true;
            }

            return placed;
        }

        public Dictionary<Vector3, bool> PlaceMetaBlocksWithOffset(string request, Vector3Int offset)
        {
            if (!Player.INSTANCE.PluginWriteAllowed(out var msg))
            {
                Debug.LogWarning(msg);
                return null;
            }

            var reqs = JsonConvert.DeserializeObject<List<PlaceMetaBlockRequest>>(request);

            var metaBlocks = new Dictionary<MetaPosition, Tuple<MetaBlockType, object>>();

            var processed = new HashSet<MetaPosition>();

            foreach (var req in reqs)
            {
                var pos = new MetaPosition(req.position.ToVector3() + offset);
                if (processed.Contains(pos))
                    continue;

                processed.Add(pos);
                var metaType = Blocks.GetBlockType(req.name);

                if (metaType is not MetaBlockType metaBlockType) continue;

                var props = metaBlockType.DeserializeProps(req.properties);
                metaBlocks.Add(pos, new Tuple<MetaBlockType, object>(metaBlockType, props));
            }

            var metaBlocksResult = PutMetaBlocks(metaBlocks);

            var placed = new Dictionary<Vector3, bool>();
            foreach (var pos in processed)
            {
                if (metaBlocksResult.TryGetValue(pos, out var metaResult) && metaResult)
                    placed[pos.ToWorld()] = true;
                else
                    placed[pos.ToWorld()] = false;
            }

            return placed;
        }

        public void PreviewBlocksWithOffset(string request, Vector3Int offset)
        {
            if (!Player.INSTANCE.PluginWriteAllowed(out var msg))
            {
                Debug.LogWarning(msg);
                return;
            }

            var reqs = JsonConvert.DeserializeObject<List<PlaceBlockRequest>>(request);
            var highlights = new Dictionary<VoxelPosition, uint>();

            foreach (var req in reqs)
            {
                var vp = new VoxelPosition(req.position.ToVector3() + offset);
                if (highlights.ContainsKey(vp))
                    continue;

                var type = Blocks.GetBlockType(req.name);
                if (type == null) continue;
                highlights.Add(vp, type.id);
            }

            BlockSelectionController.INSTANCE.AddPreviewHighlights(highlights);
        }

        public void SelectBlocks(string request)
        {
            if (!Player.INSTANCE.PluginWriteAllowed(out var msg))
            {
                Debug.LogWarning(msg);
                return;
            }

            var positions = JsonConvert.DeserializeObject<List<SerializableVector3>>(request);
            if (positions == null || positions.Count == 0) return;
            var blockPositions = new HashSet<VoxelPosition>();
            var metaBlockPositions = new HashSet<MetaPosition>();
            foreach (var pos in positions)
            {
                blockPositions.Add(new VoxelPosition(pos));
                metaBlockPositions.Add(new MetaPosition(pos));
            }

            BlockSelectionController.INSTANCE.AddHighlights(blockPositions.ToList());
            foreach (var position in metaBlockPositions)
            {
                BlockSelectionController.INSTANCE.AddHighlight(position);
            }
        }

        public static Dictionary<Vector3Int, bool> PutBlocks(Dictionary<VoxelPosition, BlockType> blocks)
        {
            var result = new Dictionary<Vector3Int, bool>();
            var toBePut = new Dictionary<VoxelPosition, Tuple<BlockType, Land>>();
            foreach (var vp in blocks.Keys)
            {
                var type = blocks[vp];
                var blockPos = vp.ToWorld();
                if (!(type is MetaBlockType) && Player.INSTANCE.CanEdit(blockPos, out var ownerLand))
                {
                    toBePut.Add(vp, new Tuple<BlockType, Land>(type, ownerLand));
                    result.Add(blockPos, true);
                    continue;
                }

                result.Add(blockPos, false);
            }

            World.INSTANCE.PutBlocks(toBePut);
            return result;
        }

        private static Dictionary<MetaPosition, bool> PutMetaBlocks(
            Dictionary<MetaPosition, Tuple<MetaBlockType, object>> metaBlocks)
        {
            if (!Player.INSTANCE.PluginWriteAllowed(out var msg))
            {
                Debug.LogWarning(msg);
                return null;
            }

            var result = new Dictionary<MetaPosition, bool>();
            foreach (var mp in metaBlocks.Keys)
            {
                var (type, props) = metaBlocks[mp];
                result.Add(mp, World.INSTANCE.PutMetaWithProps(mp, type, props));
            }

            return result;
        }

        public static UtopiaApi INSTANCE => GameObject.Find("UtopiaApi").GetComponent<UtopiaApi>();


        [Serializable]
        private class PlaceBlockRequest
        {
            public string name;
            public SerializableVector3 position;
        }

        [Serializable]
        private class PlaceMetaBlockRequest
        {
            public string name;
            public string properties;
            public SerializableVector3 position;
        }
    }
}