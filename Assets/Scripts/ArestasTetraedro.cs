using UnityEngine;

[RequireComponent(typeof(TetraedroRegular))]
public class ArestasTetraedro : MonoBehaviour
{
    [SerializeField] float aresta = 1f;
    [SerializeField] float espessura = 0.02f;
    [SerializeField] Color cor = Color.black;

    void Start()
    {
        float s = aresta / (2f * Mathf.Sqrt(2f));
        var v = new[]
        {
            s * new Vector3( 1,  1,  1),
            s * new Vector3( 1, -1, -1),
            s * new Vector3(-1,  1, -1),
            s * new Vector3(-1, -1,  1)
        };

        int[,] pares = { {0,1},{0,2},{0,3},{1,2},{1,3},{2,3} };
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = cor;

        for (int i = 0; i < 6; i++)
        {
            var go = new GameObject("Aresta");
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, v[pares[i, 0]]);
            lr.SetPosition(1, v[pares[i, 1]]);
            lr.startWidth = lr.endWidth = espessura;
            lr.material = mat;
            lr.useWorldSpace = false;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }
}