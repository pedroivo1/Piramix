using UnityEditor;
using UnityEngine;

public static class TetraedroMenu
{
    [MenuItem("GameObject/3D Object/Tetraedro Regular")]
    static void Criar()
    {
        var go = new GameObject("Tetraedro");
        Undo.RegisterCreatedObjectUndo(go, "Criar Tetraedro");

        var mf = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>().sharedMaterial = Preto();
        mf.sharedMesh = Salvar(Malha());

        Selection.activeGameObject = go;
    }

    static Mesh Malha()
    {
        float h = Mathf.Sqrt(6f) / 12f;
        float r = 1f / Mathf.Sqrt(3f);
        var m = new Mesh { name = "TetraedroPontaY" };
        m.vertices = new[]
        {
            new Vector3(0f, 3f * h, 0f),
            new Vector3(r, -h, 0f),
            new Vector3(-r / 2f, -h, r * Mathf.Sqrt(3f) / 2f),
            new Vector3(-r / 2f, -h, -r * Mathf.Sqrt(3f) / 2f)
        };
        m.triangles = new[] { 0, 2, 1, 0, 3, 2, 0, 1, 3, 1, 3, 2 };
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }

    static Mesh Salvar(Mesh mesh)
    {
        var path = AssetDatabase.GenerateUniqueAssetPath("Assets/TetraedroPontaY.asset");
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        return mesh;
    }

    static Material Preto()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Preto.mat");
        if (mat != null) return mat;
        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.black;
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
        AssetDatabase.CreateAsset(mat, "Assets/Materials/Preto.mat");
        return mat;
    }
}