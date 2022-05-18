using System;
using System.Collections.Generic;
using System.Linq;
using src.MetaBlocks;
using src.Model;
using src.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace src
{
    public class SelectedBlockProperties
    {
        public readonly uint blockTypeId;
        public readonly uint metaBlockTypeId;
        public readonly object metaProperties;
        public readonly bool metaAttached;

        public SelectedBlockProperties(uint blockTypeId, uint metaBlockTypeId,
            object metaProperties)
        {
            metaAttached = true;
            this.blockTypeId = blockTypeId;
            this.metaBlockTypeId = metaBlockTypeId;
            this.metaProperties = metaProperties;
        }

        public SelectedBlockProperties(uint blockTypeId)
        {
            metaAttached = false;
            this.blockTypeId = blockTypeId;
        }
    }
}