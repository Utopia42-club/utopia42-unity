using System;
using src.Model;
using src.Service;
using src.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src
{
    public class SelectableBlock
    {
        public Vector3 position { get; private set; }
        private Land land;
        public readonly Transform highlight;

        private readonly byte blockTypeId;
        private readonly byte metaBlockTypeId;
        private readonly object metaProperties;
        private readonly bool metaAttached;

        private const float SelectedBlocksHighlightAlpha = 0.3f;

        private SelectableBlock(Vector3 pos, byte blockTypeId, Transform highlight, byte metaBlockTypeId,
            object metaProperties, Land land)
        {
            metaAttached = true;
            position = pos;
            this.blockTypeId = blockTypeId;
            this.metaBlockTypeId = metaBlockTypeId;
            this.metaProperties = metaProperties;
            this.highlight = highlight;
            this.land = land;
        }

        private SelectableBlock(Vector3 pos, byte blockTypeId, Transform highlight, Land land)
        {
            metaAttached = false;
            position = pos;
            this.blockTypeId = blockTypeId;
            this.highlight = highlight;
            this.land = land;
        }

        public static SelectableBlock Create(Vector3 position, World world, Transform highlight, Land land)
        {
            if (world == null) return null;
            var vp = new VoxelPosition(position);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk == null) return null;

            var blockTypeId = chunk.GetBlock(vp.local).id;
            var blockHighlight = Object.Instantiate(highlight, position, Quaternion.identity);
            var material = blockHighlight.GetComponentInChildren<MeshRenderer>().material;
            Color color = material.color;
            color.a = Mathf.Clamp(SelectedBlocksHighlightAlpha, 0, 1);
            material.color = color;
            blockHighlight.gameObject.SetActive(true);

            var meta = chunk.GetMetaAt(vp);
            if (meta != null)
            {
                return new SelectableBlock(position, blockTypeId, blockHighlight, meta.type.id,
                    ((ICloneable) meta.GetProps()).Clone(), land);
            }

            return new SelectableBlock(position, blockTypeId, blockHighlight, land);
        }

        public void PutInPosition(World world, Vector3 pos, Land land)
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

        public void ConfirmMove(World world)
        {
            if (position.Equals(highlight.position)) return;
            Remove(world);
            if (Player.INSTANCE.CanEdit(Vectors.FloorToInt(highlight.position), out var land))
                PutInPosition(world, highlight.position, land);
        }

        private void Remove(World world)
        {
            var vp = new VoxelPosition(position);
            var chunk = world.GetChunkIfInited(vp.chunk);
            if (chunk != null)
            {
                chunk.DeleteVoxel(vp, land);
                if (chunk.GetMetaAt(vp) != null)
                    chunk.DeleteMeta(vp);
            }
        }

        public void RotateAroundY(Vector3 center)
        {
            RotateAround(center, Vector3.up);
        }

        public void RotateAroundZ(Vector3 center)
        {
            RotateAround(center, Vector3.forward);
        }

        private void RotateAround(Vector3 center, Vector3 axis)
        {
            var vector3 = Quaternion.AngleAxis(90, axis) * (highlight.position + 0.5f * Vector3.one - center);
            highlight.position = center + vector3 - 0.5f * Vector3.one;
        }

        public void Move(Vector3Int delta)
        {
            highlight.position += delta;
        }
    }
}