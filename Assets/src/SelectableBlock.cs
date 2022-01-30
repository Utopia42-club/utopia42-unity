using System;
using src.Model;
using src.Service;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src
{
    public class SelectableBlock
    {
        public readonly Vector3 position;
        public readonly Transform highlight;

        private readonly byte blockTypeId;
        private readonly byte metaBlockTypeId;
        private readonly object metaProperties;
        private readonly bool metaAttached;

        private const float SelectedBlocksHighlightAlpha = 0.3f;

        private SelectableBlock(Vector3 position, byte blockTypeId, Transform highlight, byte metaBlockTypeId,
            object metaProperties)
        {
            metaAttached = true;
            this.position = position;
            this.blockTypeId = blockTypeId;
            this.metaBlockTypeId = metaBlockTypeId;
            this.metaProperties = metaProperties;
            this.highlight = highlight;
        }

        private SelectableBlock(Vector3 position, byte blockTypeId, Transform highlight)
        {
            metaAttached = false;
            this.position = position;
            this.blockTypeId = blockTypeId;
            this.highlight = highlight;
        }

        public static SelectableBlock Create(Vector3 position, World world, Transform highlight)
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

            var meta = chunk.GetMetaAt(vp);
            if (meta != null)
            {
                return new SelectableBlock(position, blockTypeId, blockHighlight, meta.type.id,
                    ((ICloneable) meta.GetProps()).Clone());
            }

            return new SelectableBlock(position, blockTypeId, blockHighlight);
        }

        public void PutInNewPosition(World world, Vector3 newPosition, Land land)
        {
            var vp = new VoxelPosition(newPosition);
            var chunk = world.GetChunkIfInited(vp.chunk);
            chunk.PutVoxel(vp, VoxelService.INSTANCE.GetBlockType(blockTypeId), land);
            if (metaAttached)
            {
                chunk.PutMeta(vp, VoxelService.INSTANCE.GetBlockType(metaBlockTypeId), land);
                chunk.GetMetaAt(vp).SetProps(metaProperties, land);
            }
        }
    }
}