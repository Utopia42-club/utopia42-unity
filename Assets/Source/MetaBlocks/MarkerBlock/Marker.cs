using System;
using Source.Model;
using UnityEngine;

namespace Source.MetaBlocks.MarkerBlock
{
    [Serializable]
    public class Marker
    {
        public string name;
        public SerializableVector3Int position;

        public Marker(string name, Vector3Int position)
        {
            this.name = name;
            this.position = new SerializableVector3Int(position);
        }
    }
}