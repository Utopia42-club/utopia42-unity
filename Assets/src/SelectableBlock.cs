using System;
using System.Collections.Generic;
using src.MetaBlocks;
using src.Model;
using src.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src
{
    public class SelectableBlock
    {
        public Vector3Int Position { get; }
        private readonly Land land;
        private readonly Transform highlight;
        private Transform metaHighlight; // Highlight for box collider
        private readonly uint blockTypeId;
        private readonly uint metaBlockTypeId;
        private readonly object metaProperties;
        private readonly bool metaAttached;
        private const float SelectedBlocksHighlightAlpha = 0.3f;

        private SelectableBlock(Vector3Int pos, uint blockTypeId, Transform highlight, Transform metaHighlight,
            uint metaBlockTypeId,
            object metaProperties, Land land)
        {
            metaAttached = true;
            Position = pos;
            this.blockTypeId = blockTypeId;
            this.metaBlockTypeId = metaBlockTypeId;
            this.metaProperties = metaProperties;
            this.highlight = highlight;
            this.metaHighlight = metaHighlight;
            this.land = land;
        }

        private SelectableBlock(Vector3Int pos, uint blockTypeId, Transform highlight, Land land)
        {
            metaAttached = false;
            Position = pos;
            this.blockTypeId = blockTypeId;
            this.highlight = highlight;
            this.land = land;
        }

        public static SelectableBlock Create(Vector3Int position, Land land, bool showHighlight = true)
        {
            var world = World.INSTANCE;
            if (world == null) return null;
            var vp = new VoxelPosition(position);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk == null) return null;

            var blockType = chunk.GetBlock(vp.local);
            if (!blockType.isSolid) return null;
            var blockTypeId = blockType.id;

            var blockHighlight = Object.Instantiate(Player.INSTANCE.HighlightBlock, position, Quaternion.identity);
            var material = blockHighlight.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            var color = material.color;
            color.a = Mathf.Clamp(SelectedBlocksHighlightAlpha, 0, 1);
            material.color = color;
            blockHighlight.gameObject.SetActive(showHighlight);

            var meta = chunk.GetMetaAt(vp);
            if (meta == null) return new SelectableBlock(position, blockTypeId, blockHighlight, land);

            return new SelectableBlock(position, blockTypeId, blockHighlight,
                meta.blockObject.CreateSelectHighlight(showHighlight),
                meta.type.id, ((ICloneable) meta.GetProps())?.Clone(), land);
        }

        public static void PutInPositions(World world,
            Dictionary<Vector3Int, Tuple<SelectableBlock, Land>> selectableBlocks)
        {
            var blocks = new Dictionary<VoxelPosition, Tuple<BlockType, Land>>();
            var metas = new Dictionary<VoxelPosition, Tuple<SelectableBlock, Land>>();
            foreach (var pos in selectableBlocks.Keys)
            {
                var selectableBlock = selectableBlocks[pos].Item1;
                var land = selectableBlocks[pos].Item2;
                var vp = new VoxelPosition(pos);
                blocks.Add(vp,
                    new Tuple<BlockType, Land>(Blocks.GetBlockType(selectableBlock.blockTypeId), land));
                if (selectableBlock.metaAttached)
                    metas.Add(vp, new Tuple<SelectableBlock, Land>(selectableBlock, land));
            }

            world.PutBlocks(blocks);
            foreach (var vp in metas.Keys)
            {
                var (selectableBlock, land) = metas[vp];
                world.PutMetaWithProps(vp,
                    (MetaBlockType) Blocks.GetBlockType(selectableBlock.metaBlockTypeId),
                    selectableBlock.metaProperties, land);
            }
        }

        public static void Remove(World world,
            List<SelectableBlock> selectableBlocks)
        {
            var blocks = new Dictionary<VoxelPosition, Land>();
            foreach (var selectableBlock in selectableBlocks)
            {
                var vp = new VoxelPosition(selectableBlock.Position);
                blocks.Add(vp, selectableBlock.land);

                var chunk = world.GetChunkIfInited(vp.chunk);
                if (chunk.GetMetaAt(vp) != null)
                    chunk.DeleteMeta(vp);
            }

            world.DeleteBlocks(blocks);
        }

        public void RotateAround(Vector3 center, Vector3 axis) 
        {
            var vector3 = Quaternion.AngleAxis(90, axis) * (highlight.position + 0.5f * Vector3.one - center);
            var oldPos = HighlightPosition;
            highlight.position = Vectors.TruncateFloor(center + vector3 - 0.5f * Vector3.one);
            if (metaHighlight != null)
                metaHighlight.position += HighlightPosition - oldPos;
        }

        public void Move(Vector3Int delta)
        {
            highlight.position += delta;
            if (metaHighlight != null)
                metaHighlight.position += delta;
        }

        public bool IsMoved()
        {
            return !Position.Equals(HighlightPosition);
        }

        public void RemoveHighlights()
        {
            Object.DestroyImmediate(highlight.gameObject);
            if (metaHighlight != null)
            {
                Object.DestroyImmediate(metaHighlight.gameObject);
            }
        }

        public void ReCreateTdObjectHighlight()
        {
            var world = World.INSTANCE;
            if (!metaAttached || metaHighlight != null || world == null) return;
            var vp = new VoxelPosition(Position);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk == null) return;
            var meta = chunk.GetMetaAt(vp);

            if (metaHighlight != null)
                Object.DestroyImmediate(metaHighlight);

            metaHighlight = meta.blockObject.CreateSelectHighlight();
        }

        public Vector3Int HighlightPosition => Vectors.FloorToInt(highlight.position);
    }
}