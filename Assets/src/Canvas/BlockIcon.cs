using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockIcon 
{
    public readonly GameObject gameObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public BlockIcon(byte blockId, Material material)
    {
        gameObject = new GameObject();
        gameObject.transform.Rotate(new Vector3(-15f, 50f, -15f));
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        SetType(blockId);
    }

    void CreateMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }


    void SetType(byte blockId)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        Vector3Int[] verts = new Vector3Int[] {
            Voxels.Vertices[0],
            Voxels.Vertices[1],
            Voxels.Vertices[2],
            Voxels.Vertices[3],
            Voxels.Vertices[4],
            Voxels.Vertices[5],
            Voxels.Vertices[6],
            Voxels.Vertices[7]
        };

        var type = VoxelService.INSTANCE.GetBlockType(blockId);
        if (!type.isSolid) return;

        foreach (Voxels.Face face in Voxels.Face.FACES)
        {
            int idx = vertices.Count;

            vertices.Add(verts[face.verts[0]]);
            vertices.Add(verts[face.verts[1]]);
            vertices.Add(verts[face.verts[2]]);
            vertices.Add(verts[face.verts[3]]);

            AddTexture(type.GetTextureID(face), uvs);

            triangles.Add(idx);
            triangles.Add(idx + 1);
            triangles.Add(idx + 2);
            triangles.Add(idx + 2);
            triangles.Add(idx + 1);
            triangles.Add(idx + 3);
        }
        CreateMesh(vertices, triangles, uvs);
    }


    void AddTexture(int textureID, List<Vector2> uvs)
    {
        float y = textureID / Voxels.TextureAtlasSizeInBlocks;
        float x = textureID - (y * Voxels.TextureAtlasSizeInBlocks);

        x *= Voxels.NormalizedBlockTextureSize;
        y *= Voxels.NormalizedBlockTextureSize;

        y = 1f - y - Voxels.NormalizedBlockTextureSize;
        float eps = 0.01f;
        uvs.Add(new Vector2(x + eps, y + eps));
        uvs.Add(new Vector2(x + eps, y + Voxels.NormalizedBlockTextureSize - eps));
        uvs.Add(new Vector2(x + Voxels.NormalizedBlockTextureSize - eps, y + eps));
        uvs.Add(new Vector2(x + Voxels.NormalizedBlockTextureSize - eps, y + Voxels.NormalizedBlockTextureSize - eps));
    }

}
