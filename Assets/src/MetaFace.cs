using UnityEngine;

public class MetaFace : MonoBehaviour
{
    public MeshRenderer Initialize(Voxels.Face face, int width, int height)
    {
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        var meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer.material = new Material(Shader.Find("Standard"));

        if (face == Voxels.Face.FRONT || face == Voxels.Face.BACK)
            transform.localScale = new Vector3(width, height, 1);
        else if (face == Voxels.Face.LEFT || face == Voxels.Face.RIGHT)
            transform.localScale = new Vector3(1, height, width);
        else
            transform.localScale = new Vector3(width, 1, height);

        var vertices = new Vector3[4];
        for (int i = 0; i < 4; i++)
            vertices[i] = Voxels.Vertices[face.verts[i]];

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = new int[6] { 0, 1, 2, 2, 1, 3 };

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(0, 1)
        };
        mesh.uv = uv;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        meshFilter.mesh = mesh;

        return meshRenderer;
    }
}