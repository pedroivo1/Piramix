using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TetraedroRegular : MonoBehaviour
{
    [SerializeField] float aresta = 1f;

    void Awake()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf.sharedMesh == null)
            mf.sharedMesh = Criar(aresta);
    }

    [ContextMenu("Ponta no Y")]
    void PontaNoY()
    {
        transform.rotation = Quaternion.FromToRotation(new Vector3(1, 1, 1), Vector3.up);
    }

    public static Mesh Criar(float aresta)
    {
        float s = aresta / (2f * Mathf.Sqrt(2f));
        var verts = new[]
        {
            s * new Vector3( 1,  1,  1),
            s * new Vector3( 1, -1, -1),
            s * new Vector3(-1,  1, -1),
            s * new Vector3(-1, -1,  1)
        };
        var tris = new[]
        {
            0, 1, 2,
            0, 2, 3,
            0, 3, 1,
            1, 3, 2
        };
        var mesh = new Mesh { name = "TetraedroRegular" };
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}