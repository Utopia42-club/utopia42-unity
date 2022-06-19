using System;
using Source.MetaBlocks;
using Source.Model;
using Source.Utils;
using UnityEngine;

namespace Source
{
    public class MetaFocusable : Focusable
    {
        public MetaBlockObject MetaBlockObject { protected set; get; }

        public void Initialize(MetaBlockObject metaBlockObject)
        {
            if (initialized) return;
            MetaBlockObject = metaBlockObject;
            initialized = true;
        }
        public override void UnFocus()
        {
            if (!initialized) return;
            MetaBlockObject.UnFocus();
        }

        public override void Focus(Vector3? point = null)
        {
            if (!initialized) return;

            if (World.INSTANCE.SelectionActive)
            {
                MetaBlockObject.ShowFocusHighlight();
                return;
            }
            MetaBlockObject.Focus();
        }
        
        public override Vector3? GetBlockPosition()
        {
            return new MetaPosition(MetaBlockObject.transform.position).ToWorld();
        }

        public override bool IsSelected()
        {
            return World.INSTANCE.IsSelected(new MetaPosition(MetaBlockObject.transform.position));
        }

        public MetaBlock GetBlock()
        {
            return MetaBlockObject.Block;
        }
        
    }
}