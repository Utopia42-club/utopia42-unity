using System;
using src.Model;
using UnityEngine;

namespace src.MetaBlocks.MarkerBlock
{
    [Serializable]
    public class Marker
    {
        public string name;
        public SerializableVector3 position;

        public Marker(string name, Vector3 position)
        {
            this.name = name;
            this.position = new SerializableVector3(position);
        }
    }
}