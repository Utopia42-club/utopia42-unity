using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public static readonly int CHUNK_WIDTH = 16;
    public static readonly int CHUNK_HEIGHT = 32;

    private Dictionary<Vector3Int, MetaBlock> metaBlocks;
    private readonly byte[,,] voxels = new byte[CHUNK_WIDTH, CHUNK_HEIGHT, CHUNK_WIDTH];
    private World world;
    public GameObject chunkObject;

    public readonly Vector3Int position;
    public readonly Vector3Int coordinate;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    private bool inited = false;
    private bool active = true;

    public Chunk(Vector3Int coordinate, World world)
    {
        this.coordinate = coordinate;
        this.world = world;
        this.position = new Vector3Int(coordinate.x * CHUNK_WIDTH, coordinate.y * CHUNK_HEIGHT, coordinate.z * CHUNK_WIDTH);
    }

    public bool IsInited()
    {
        return inited;
    }

    public void Init()
    {
        if (inited) return;
        inited = true;
        chunkObject = new GameObject();
        chunkObject.SetActive(active);
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = position;
        chunkObject.name = "Chunck " + coordinate;

        VoxelService.INSTANCE.FillChunk(coordinate, voxels);
        metaBlocks = VoxelService.INSTANCE.GetMetaBlocks(coordinate);
        DrawVoxels();
        DrawMetaBlocks();
        //var block = new ImageBlockTpe(50).New("{front:{height:5, width:5, url:\"https://www.wpbeginner.com/wp-content/uploads/2020/03/ultimate-small-business-resource-180x180.png\"}}");
        //block.RenderAt(chunkObject.transform, position+new Vector3Int(0, 32, 0));
    }

    private void DrawMetaBlocks()
    {
        if (metaBlocks != null)
        {
            foreach (var entry in metaBlocks)
                entry.Value.RenderAt(chunkObject.transform, entry.Key, this);
        }
    }

    private void DrawVoxels()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        CreateMeshData(vertices, triangles, uvs);
        CreateMesh(vertices, triangles, uvs);
    }

    void CreateMeshData(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        for (int y = 0; y < voxels.GetLength(1); y++)
            for (int x = 0; x < voxels.GetLength(0); x++)
                for (int z = 0; z < voxels.GetLength(2); z++)
                    if (VoxelService.INSTANCE.GetBlockType(voxels[x, y, z]).isSolid)
                        AddVisibleFaces(new Vector3Int(x, y, z), vertices, triangles, uvs);
    }

    // Inputs: x,y,z local to this chunk
    private bool IsVoxelInChunk(int x, int y, int z)
    {
        return x >= 0 && x < voxels.GetLength(0) &&
                y >= 0 && y < voxels.GetLength(1) &&
                z >= 0 && z < voxels.GetLength(2);
    }

    public BlockType GetBlock(Vector3Int localPos)
    {
        if (!IsVoxelInChunk(localPos.x, localPos.y, localPos.z))
            throw new System.ArgumentException("Invalid local position: " + localPos);

        return VoxelService.INSTANCE.GetBlockType(voxels[localPos.x, localPos.y, localPos.z]);
    }

    public MetaBlock GetMetaAt(VoxelPosition vp)
    {
        if (metaBlocks == null) return null;

        MetaBlock block;
        if (metaBlocks.TryGetValue(vp.local, out block))
            return block;

        return null;
    }

    private bool IsPositionSolid(Vector3Int localPos)
    {
        if (!IsVoxelInChunk(localPos.x, localPos.y, localPos.z))
            return world.IsSolidAt(ToGlobal(localPos));

        return GetBlock(localPos).isSolid;
    }

    void AddVisibleFaces(Vector3Int pos, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        Vector3Int[] verts = new Vector3Int[] {
            Voxels.Vertices[0] + pos,
            Voxels.Vertices[1] + pos,
            Voxels.Vertices[2] + pos,
            Voxels.Vertices[3] + pos,
            Voxels.Vertices[4] + pos,
            Voxels.Vertices[5] + pos,
            Voxels.Vertices[6] + pos,
            Voxels.Vertices[7] + pos
        };

        byte blockId = voxels[pos.x, pos.y, pos.z];
        var type = VoxelService.INSTANCE.GetBlockType(blockId);
        if (!type.isSolid) return;

        foreach (Voxels.Face face in Voxels.Face.FACES)
        {
            if (!IsPositionSolid(pos + face.direction))
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
        }
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

    void AddTexture(int textureID, List<Vector2> uvs)
    {
        float y = textureID / Voxels.TextureAtlasSizeInBlocks;
        float x = textureID - (y * Voxels.TextureAtlasSizeInBlocks);

        x *= Voxels.NormalizedBlockTextureSize;
        y *= Voxels.NormalizedBlockTextureSize;

        y = 1f - y - Voxels.NormalizedBlockTextureSize;
        float eps = 0.005f;
        uvs.Add(new Vector2(x + eps, y + eps));
        uvs.Add(new Vector2(x + eps, y + Voxels.NormalizedBlockTextureSize - eps));
        uvs.Add(new Vector2(x + Voxels.NormalizedBlockTextureSize - eps, y + eps));
        uvs.Add(new Vector2(x + Voxels.NormalizedBlockTextureSize - eps, y + Voxels.NormalizedBlockTextureSize - eps));
    }

    public void DeleteVoxel(VoxelPosition pos, Land land)
    {
        voxels[pos.local.x, pos.local.y, pos.local.z] = 0;//FIXME
        VoxelService.INSTANCE.AddChange(pos, 0, land);
        OnChanged(pos);
    }

    public void PutVoxel(VoxelPosition pos, BlockType type, Land land)
    {
        if (type.isSolid)
        {
            voxels[pos.local.x, pos.local.y, pos.local.z] = type.id;
            VoxelService.INSTANCE.AddChange(pos, type.id, land);
            OnChanged(pos);
        }
    }
    public void PutMeta(VoxelPosition pos, BlockType type, Land land)
    {
        metaBlocks = VoxelService.INSTANCE.AddMetaBlock(pos, type.id, land);
        metaBlocks[pos.local].RenderAt(chunkObject.transform, pos.local, this);
    }

    public void DeleteMeta(VoxelPosition pos)
    {
        var block = metaBlocks[pos.local];
        metaBlocks.Remove(pos.local);
        block.Destroy();
    }

    private void OnChanged(VoxelPosition pos)
    {
        DrawVoxels();
        if (pos.local.x == voxels.GetLength(0) - 1)
            world.GetChunkIfInited(pos.chunk + Vector3Int.right).DrawVoxels();
        if (pos.local.y == voxels.GetLength(1) - 1)
            world.GetChunkIfInited(pos.chunk + Vector3Int.up).DrawVoxels();
        if (pos.local.z == voxels.GetLength(2) - 1)
            world.GetChunkIfInited(pos.chunk + Vector3Int.forward).DrawVoxels();

        if (pos.local.x == 0)
            world.GetChunkIfInited(pos.chunk + Vector3Int.left).DrawVoxels();
        if (pos.local.y == 0)
            world.GetChunkIfInited(pos.chunk + Vector3Int.down).DrawVoxels();
        if (pos.local.z == 0)
            world.GetChunkIfInited(pos.chunk + Vector3Int.back).DrawVoxels();
    }

    private Vector3Int ToGlobal(Vector3Int localPoint)
    {
        return localPoint + new Vector3Int((int)position.x, (int)position.y, (int)position.z);
    }

    public bool IsActive()
    {
        return active;
    }

    public void SetActive(bool active)
    {
        if (chunkObject != null) chunkObject.SetActive(active);
        this.active = active;
    }

    public override bool Equals(object obj)
    {
        if (obj == this) return true;
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;

        return this.coordinate.Equals(((Chunk)obj).coordinate);
    }

    public override int GetHashCode()
    {
        return this.coordinate.GetHashCode();
    }
}
