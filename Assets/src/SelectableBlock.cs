using System;
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
        private Transform tdHighlight;

        private readonly byte blockTypeId;
        private readonly byte metaBlockTypeId;
        private readonly object metaProperties;
        private readonly bool metaAttached;

        private const float SelectedBlocksHighlightAlpha = 0.3f;

        private SelectableBlock(Vector3Int pos, byte blockTypeId, Transform highlight, Transform tdHighlight,
            byte metaBlockTypeId,
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

        private SelectableBlock(Vector3Int pos, byte blockTypeId, Transform highlight, Land land)
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
            var material = blockHighlight.GetComponentInChildren<MeshRenderer>().material;
            Color color = material.color;
            color.a = Mathf.Clamp(SelectedBlocksHighlightAlpha, 0, 1);
            material.color = color;
            blockHighlight.gameObject.SetActive(showHighlight);

            var meta = chunk.GetMetaAt(vp);
            if (meta != null)
            {
                if (meta.blockObject is TdObjectBlockObject)
                {
                    return new SelectableBlock(position, blockTypeId, blockHighlight,
                        CreateObjectHighlightBox(((TdObjectBlockObject) meta.blockObject).TdObjectBoxCollider,
                            tdHighlightModel, showHighlight), meta.type.id, ((ICloneable) meta.GetProps()).Clone(),
                        land);
                }

                return new SelectableBlock(position, blockTypeId, blockHighlight, null, meta.type.id,
                    ((ICloneable) meta.GetProps()).Clone(), land);
            }

            return new SelectableBlock(position, blockTypeId, blockHighlight, land);
        }

        public void PutInPosition(World world, Vector3Int pos, Land land)
        {
            var vp = new VoxelPosition(pos);
            var chunk = world.GetChunkIfInited(vp.chunk);
            chunk.PutVoxel(vp, VoxelService.INSTANCE.GetBlockType(blockTypeId), land);
            if (metaAttached)
            {
                chunk.PutMeta(vp, VoxelService.INSTANCE.GetBlockType(metaBlockTypeId), land);
                chunk.GetMetaAt(vp).SetProps(metaProperties, land);
            }
        }

        public void Remove(World world)
        {
            var vp = new VoxelPosition(Position);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk != null)
            {
                chunk.DeleteVoxel(vp, land);
                if (chunk.GetMetaAt(vp) != null)
                    chunk.DeleteMeta(vp);
            }
        }

        public void RotateAround(Vector3 center, Vector3 axis) // TODO: Do the the truncate floor here if necessary
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

        public void DestroyHighlights()
        {
            Object.DestroyImmediate(highlight.gameObject);
            if (tdHighlight != null)
            {
                Object.DestroyImmediate(tdHighlight.gameObject);
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
            tdHighlight = CreateObjectHighlightBox(((TdObjectBlockObject) meta.blockObject).TdObjectBoxCollider,
                tdObjectHighlightBox);
        }

        public Vector3Int HighlightPosition => Vectors.FloorToInt(highlight.position);
    }
}