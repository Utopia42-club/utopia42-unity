using System.Collections.Generic;
using Newtonsoft.Json;
using Source.MetaBlocks.TdObjectBlock;
using Source.Model;
using Source.Service.Migration.Models;
using UnityEngine;

namespace Source.Service.Migration
{
    internal class MetaDetachMigration : Migration
    {
        private const float Gap = 0.2f;

        public MetaDetachMigration()
            : base(new Version[] {new Version(0, 2, 0)}, new Version(0, 3, 0))
        {
        }

        public override LandDetails Migrate(Land land, LandDetails details)
        {
            var metaBlocks = new Dictionary<string, MetaBlockData>();

            MetaBlockData metaBlock;
            foreach (var posKey in details.metadata.Keys)
            {
                metaBlock = details.metadata[posKey];
                var coordinate = LandDetails.ParseKey(posKey);
                switch (metaBlock.type)
                {
                    case "image":
                        AddFacesProperties(coordinate,
                            JsonConvert.DeserializeObject<MediaBlockPropertiesLegacy>(metaBlock.properties),
                            metaBlocks, "image");
                        break;
                    case "nft":
                        AddFacesProperties(coordinate,
                            JsonConvert.DeserializeObject<NftBlockPropertiesLegacy>(metaBlock.properties),
                            metaBlocks, "nft");
                        break;
                    case "video":
                        AddFacesProperties(coordinate,
                            JsonConvert.DeserializeObject<VideoBlockPropertiesLegacy>(metaBlock.properties),
                            metaBlocks, "video");
                        break;
                    case "3d_object":
                        AddTdObjectProperties(coordinate, metaBlock, metaBlocks);
                        break;
                    case "link":
                        AddLinkProperties(coordinate, metaBlock, metaBlocks);
                        break;
                    default:
                        metaBlocks[posKey] = metaBlock;
                        break;
                }
            }

            details.v = GetTarget().ToString();
            details.metadata = metaBlocks;
            return details;
        }

        private static void AddLinkProperties(MetaLocalPosition coordinate, MetaBlockData metaBlock,
            Dictionary<string, MetaBlockData> metaBlocks)
        {
            var newPos = LandDetails.FormatKey(coordinate.position + new Vector3(0.5f, 0.5f, 0.5f));
            if (!metaBlocks.ContainsKey(newPos))
            {
                metaBlocks[newPos] = metaBlock;
            }
            else
            {
                Debug.LogWarning($"Unable to add link properties at position {newPos} (duplicate map key)");
            }
        }

        private static void AddTdObjectProperties(MetaLocalPosition coordinate, MetaBlockData metaBlock,
            Dictionary<string, MetaBlockData> metaBlocks)
        {
            var props = JsonConvert.DeserializeObject<TdObjectBlockPropertiesLegacy>(metaBlock.properties);
            var newPos = LandDetails.FormatKey(coordinate.position + props.offset.ToVector3());

            if (!metaBlocks.ContainsKey(newPos))
            {
                metaBlock.properties = JsonConvert.SerializeObject(props.toProperties());
                metaBlocks[newPos] = metaBlock;
            }
            else
            {
                Debug.LogWarning($"Unable to add 3d object properties at position {newPos} (duplicate map key)");
            }
        }

        private static void AddFacesProperties<T>(MetaLocalPosition startCoordinate, BaseImageBlockProperties<T> props,
            Dictionary<string, MetaBlockData> metaBlocks, string metaBlockType) where T : MetaBlockFaceProperties
        {
            T face;

            if ((face = props.back) != null)
            {
                AddFaceProperties(startCoordinate, metaBlocks, face, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, -Gap),
                    new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0.5f, 0f), metaBlockType);
            }

            if ((face = props.front) != null)
            {
                AddFaceProperties(startCoordinate, metaBlocks, face, new Vector3(0f, -180f, 0f),
                    new Vector3(0f, 0f, 1 + Gap), new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0.5f, 0f), metaBlockType);
            }

            if ((face = props.left) != null)
            {
                AddFaceProperties(startCoordinate, metaBlocks, face, new Vector3(0f, 90f, 0f),
                    new Vector3(-Gap, 0f, 0f), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0.5f, 0f), metaBlockType);
            }

            if ((face = props.right) != null)
            {
                AddFaceProperties(startCoordinate, metaBlocks, face, new Vector3(0f, -90f, 0f),
                    new Vector3(1 + Gap, 0f, 0f), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0.5f, 0f), metaBlockType);
            }

            if ((face = props.top) != null)
            {
                AddFaceProperties(startCoordinate, metaBlocks, face, new Vector3(90f, 0f, 0f),
                    new Vector3(0f, 1 + Gap, 0f), new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0f, 0.5f), metaBlockType);
            }

            if ((face = props.bottom) != null)
            {
                AddFaceProperties(startCoordinate, metaBlocks, face, new Vector3(-90f, 0f, 0f),
                    new Vector3(0f, -Gap, 0f), new Vector3(0.5f, 0f, 0f), new Vector3(0f, 0f, 0.5f), metaBlockType);
            }
        }

        private static void AddFaceProperties(MetaLocalPosition startCoordinate,
            Dictionary<string, MetaBlockData> metaBlocks, MetaBlockFaceProperties face,
            Vector3 rotation, Vector3 offset, Vector3 wScaledOffset, Vector3 hScaledOffset, string metaBlockType)
        {
            var properties = face.toProperties(new SerializableVector3(rotation));

            wScaledOffset = properties.width * wScaledOffset;
            hScaledOffset = properties.height * hScaledOffset;

            var metaBlock = new MetaBlockData
            {
                type = metaBlockType,
                properties = JsonConvert.SerializeObject(properties)
            };
            var key = LandDetails.FormatKey(startCoordinate.position + offset + wScaledOffset + hScaledOffset);
            if (metaBlocks.ContainsKey(key))
            {
                Debug.LogWarning($"Unable to add face properties at position {key} (duplicate map key)");
                return;
            }

            metaBlocks[key] = metaBlock;
        }
    }
}