using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public static readonly int CHUNK_LENGTH = 10;

    private readonly byte[,,] voxels = new byte[CHUNK_LENGTH, CHUNK_LENGTH, CHUNK_LENGTH];
    private World world;
    public GameObject chunkObject;


    public readonly Vector3Int coordinate;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;


    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public Chunk(Vector3Int coordinate, World world)
    {
        this.coordinate = coordinate;
        this.world = world;

        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = this.position;
        chunkObject.name = "Chunck " + coordinate;

        world.service.FillChunk(this.coordinate, this.voxels);
        CreateMeshData();
        CreateMesh();
    }

    void CreateMeshData()
    {
        for (int y = 0; y < voxels.GetLength(1); y++)
            for (int x = 0; x < voxels.GetLength(0); x++)
                for (int z = 0; z < voxels.GetLength(2); z++)
                    if (world.service.GetBlockType(voxels[x, y, z]).isSolid)
                        addVisibleFaces(new Vector3Int(x, y, z));
    }

    // Inputs: x,y,z local to this chunk
    private bool IsVoxelInChunk(int x, int y, int z)
    {
        return x >= 0 && x < CHUNK_LENGTH &&
                y >= 0 && y < CHUNK_LENGTH &&
                z >= 0 && z < CHUNK_LENGTH;
    }

    public BlockType GetBlock(Vector3Int localPos)
    {
        if (!IsVoxelInChunk(localPos.x, localPos.y, localPos.z))
            throw new System.ArgumentException("Invalid local position: " + localPos);

        return world.service.GetBlockType(voxels[localPos.x, localPos.y, localPos.z]);
    }

    private bool IsPositionSolid(Vector3Int localPos)
    {
        if (!IsVoxelInChunk(localPos.x, localPos.y, localPos.z))
            return world.IsSolidAt(ToGlobal(localPos));

        return GetBlock(localPos).isSolid;
    }

    void addVisibleFaces(Vector3Int pos)
    {
        Vector3Int[] verts = Voxels.GenereateVertices(pos);

        // vertsToIdx maps index of vertex in verts to its index in this.vertices
        // index of vertex in this.vertices is vertsToIdx[localIdx]
        int[] vertsToIdx = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };
        byte blockID = voxels[pos.x, pos.y, pos.z];
        if (blockID == 0)
            return;
        foreach (Voxels.Face face in Voxels.Face.FACES)
        {
            if (!IsPositionSolid(pos + face.direction))
            {

                foreach (int vertIdx in face.triangles)
                {
                    if (vertsToIdx[vertIdx] == -1)
                    {
                        vertsToIdx[vertIdx] = vertices.Count;
                        vertices.Add(verts[vertIdx]);
                    }
                    triangles.Add(vertsToIdx[vertIdx]);
                }
                //AddTexture(world.blocktypes[blockID].GetTextureID(p));
            }
        }
    }

    void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(int textureID)
    {
        //float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        //float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        //x *= VoxelData.NormalizedBlockTextureSize;
        //y *= VoxelData.NormalizedBlockTextureSize;

        //y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 0));
        //uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        //uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        //uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

    private Vector3Int ToGlobal(Vector3Int localPoint)
    {
        return localPoint + new Vector3Int((int)position.x, (int)position.y, (int)position.z);
    }

    public bool isActive
    {
        get { return chunkObject.activeSelf; }
        set { chunkObject.SetActive(value); }
    }

    public Vector3Int position
    {
        get
        {
            return coordinate * CHUNK_LENGTH;
        }
    }
}
