using System;
using System.Collections.Generic;
using src.MetaBlocks;
using src.MetaBlocks.TdObjectBlock;
using src.Model;
using src.Service;
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

        private Transform tdHighlight; // Highlight for box collider
        private MeshRenderer colliderRenderer; // Highlight for mesh collider

        private readonly uint blockTypeId;
        private readonly uint metaBlockTypeId;
        private readonly object metaProperties;
        private readonly bool metaAttached;

        private const float SelectedBlocksHighlightAlpha = 0.3f;

        private SelectableBlock(Vector3Int pos, uint blockTypeId, Transform highlight, Transform tdHighlight,
            uint metaBlockTypeId,
            object metaProperties, Land land)
        {
            metaAttached = true;
            Position = pos;
            this.blockTypeId = blockTypeId;
            this.metaBlockTypeId = metaBlockTypeId;
            this.metaProperties = metaProperties;
            this.highlight = highlight;
            this.tdHighlight = tdHighlight;
            this.land = land;
        }

        private SelectableBlock(Vector3Int pos, uint blockTypeId, Transform highlight, MeshRenderer colliderRenderer,
            uint metaBlockTypeId,
            object metaProperties, Land land)
        {
            metaAttached = true;
            Position = pos;
            this.blockTypeId = blockTypeId;
            this.metaBlockTypeId = metaBlockTypeId;
            this.metaProperties = metaProperties;
            this.highlight = highlight;
            this.colliderRenderer = colliderRenderer;
            this.land = land;
        }

        private SelectableBlock(Vector3Int pos, uint blockTypeId, Transform highlight, uint metaBlockTypeId,
            object metaProperties, Land land)
        {
            metaAttached = true;
            Position = pos;
            this.blockTypeId = blockTypeId;
            this.metaBlockTypeId = metaBlockTypeId;
            this.metaProperties = metaProperties;
            this.highlight = highlight;
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

        public static SelectableBlock Create(Vector3Int position, World world, Transform highlightModel,
            Transform tdHighlightModel, Land land, bool showHighlight = true)
        {
            if (world == null) return null;
            var vp = new VoxelPosition(position);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk == null) return null;

            var blockType = chunk.GetBlock(vp.local);
            if (!blockType.isSolid) return null;
            var blockTypeId = blockType.id;

            var blockHighlight = Object.Instantiate(highlightModel, position, Quaternion.identity);
            var material = blockHighlight.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            Color color = material.color;
            color.a = Mathf.Clamp(SelectedBlocksHighlightAlpha, 0, 1);
            material.color = color;
            blockHighlight.gameObject.SetActive(showHighlight);

            var meta = chunk.GetMetaAt(vp);
            if (meta == null) return new SelectableBlock(position, blockTypeId, blockHighlight, land);

            if (!(meta.blockObject is TdObjectBlockObject blockObject))
                return new SelectableBlock(position, blockTypeId, blockHighlight, meta.type.id,
                    ((ICloneable) meta.GetProps())?.Clone(), land);

            var collider = blockObject.TdObjectCollider;
            if (collider is BoxCollider boxCollider)
            {
                return new SelectableBlock(position, blockTypeId, blockHighlight,
                    CreateObjectHighlightBox(boxCollider, tdHighlightModel, showHighlight),
                    meta.type.id, ((ICloneable) meta.GetProps())?.Clone(), land);
            }

            if (showHighlight)
                blockObject.ColliderRendererFoSelection.enabled = true;
            return new SelectableBlock(position, blockTypeId, blockHighlight, blockObject.ColliderRendererFoSelection,
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
            if (tdHighlight != null)
                tdHighlight.position += HighlightPosition - oldPos;
        }

        public void Move(Vector3Int delta)
        {
            highlight.position += delta;
            if (tdHighlight != null)
                tdHighlight.position += delta;
        }

        public bool IsMoved()
        {
            return !Position.Equals(HighlightPosition);
        }

        public void RemoveHighlights()
        {
            Object.DestroyImmediate(highlight.gameObject);
            if (tdHighlight != null)
            {
                Object.DestroyImmediate(tdHighlight.gameObject);
            }

            if (colliderRenderer != null)
            {
                colliderRenderer.enabled = false;
            }
        }

        private static Transform CreateObjectHighlightBox(BoxCollider boxCollider, Transform model, bool active = true)
        {
            if (boxCollider == null) return null;
            var highlightBox = Object.Instantiate(model, default, Quaternion.identity);

            var colliderTransform = boxCollider.transform;
            highlightBox.transform.rotation = colliderTransform.rotation;

            var size = boxCollider.size;
            var minPos = boxCollider.center - size / 2;

            var gameObjectTransform = boxCollider.gameObject.transform;
            size.Scale(gameObjectTransform.localScale);
            size.Scale(gameObjectTransform.parent.localScale);

            highlightBox.localScale = size;
            highlightBox.position = colliderTransform.TransformPoint(minPos);
            highlightBox.gameObject.SetActive(active);

            return highlightBox;
        }

        public void ReCreateTdObjectHighlight(World world, Transform tdObjectHighlightBox)
        {
            if (!metaAttached || tdHighlight != null || world == null) return;
            var vp = new VoxelPosition(Position);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk == null) return;
            var meta = chunk.GetMetaAt(vp);

            if (((TdObjectBlockObject) meta.blockObject).TdObjectCollider is BoxCollider boxCollider)
            {
                tdHighlight = CreateObjectHighlightBox(boxCollider, tdObjectHighlightBox);
                colliderRenderer = null;
            }
            else
            {
                tdHighlight = null;
                colliderRenderer = ((TdObjectBlockObject) meta.blockObject).ColliderRendererFoSelection;
            }
        }

        public Vector3Int HighlightPosition => Vectors.FloorToInt(highlight.position);
    }
}