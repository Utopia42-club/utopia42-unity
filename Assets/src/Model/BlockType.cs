using static src.Utils.Voxels;
using static src.Utils.Voxels.Face;

namespace src.Model
{
    public class BlockType
    {
        public readonly byte id;
        public readonly string name;
        public readonly bool isSolid;
        public readonly int[] textures = new int[FACES.Length];
        public bool IsWhite => id == 27 || id == 29; // for test only 

        public BlockType(byte id, string name, bool isSolid,
            int backTexture, int rightTexture,
            int frontTexture, int leftTexture,
            int bottomTexture, int topTexture)
        {
            this.id = id;
            this.name = name;
            this.isSolid = isSolid;
            textures[BACK.index] = backTexture;
            textures[RIGHT.index] = rightTexture;
            textures[FRONT.index] = frontTexture;
            textures[LEFT.index] = leftTexture;
            textures[BOTTOM.index] = bottomTexture;
            textures[TOP.index] = topTexture;
        }

        public int GetTextureID(Face face)
        {
            return textures[face.index];
        }

        public UnityEngine.Sprite GetIcon(bool failed = false)
        {
            return failed ? 
                UnityEngine.Resources.Load<UnityEngine.Sprite>("BlockIcons/failed") :
                UnityEngine.Resources.Load<UnityEngine.Sprite>("BlockIcons/" + name);
        }
    }
}