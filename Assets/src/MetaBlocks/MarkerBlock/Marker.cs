using src.Utils;
using UnityEngine;

namespace src.MetaBlocks.MarkerBlock
{
    [System.Serializable]
    public class Marker
    {
        public string name;
        public SerializableVector3 position;

        public Marker(string name, Vector3 position)
        {
            this.name = name;
            this.position = SerializableVector3.From(position);
        }
    }
}