using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Piramix : MonoBehaviour
{
    [SerializeField] float duracao = 0.25f;
    [SerializeField] float limiarArraste = 12f;

    // eixos
    static readonly Vector3[] Eixos =
    {
        new Vector3( 1,  1,  1).normalized,
        new Vector3( 1, -1, -1).normalized,
        new Vector3(-1,  1, -1).normalized,
        new Vector3(-1, -1,  1).normalized
    };
    static readonly float S = 1f / (2f * Mathf.Sqrt(2f));

    readonly List<Transform> pecas = new List<Transform>();
    bool girando;

    // estado do arraste
    Transform pecaClicada;
    Vector3 pontoClicado;
    Vector2 inicioArraste;
    int faceClicada = -1;

    void Awake()
    {
        foreach (Transform peca in transform)
        {
            pecas.Add(peca);
            if (peca.GetComponent<Collider>() == null)
                peca.gameObject.AddComponent<MeshCollider>().sharedMesh = peca.GetComponent<MeshFilter>().sharedMesh;
        }
    }

    // 0 = ponta, 1 = meio, 2 = base
    static int CamadaDe(Vector3 posLocal, int eixo)
    {
        float d = Vector3.Dot(posLocal, Eixos[eixo]);
        if (d > 0.9f) return 0;
        if (d > 0.3f) return 1;
        return 2;
    }

    void Update()
    {
        var ponteiro = Pointer.current;
        if (ponteiro == null || girando) return;

        if (CameraOrbita.AltPressionado)
        {
            pecaClicada = null;
            return;
        }

        if (ponteiro.press.wasPressedThisFrame)
            Clicar(ponteiro.position.ReadValue());
        else if (ponteiro.press.isPressed && pecaClicada != null)
        {
            Vector2 arraste = ponteiro.position.ReadValue() - inicioArraste;
            if (arraste.magnitude >= limiarArraste)
            {
                EscolherGiro(arraste);
                pecaClicada = null;
            }
        }
        else if (ponteiro.press.wasReleasedThisFrame)
            pecaClicada = null;
    }

    void Clicar(Vector2 posTela)
    {
        pecaClicada = null;
        var cam = Camera.main;
        Physics.SyncTransforms();
        if (cam == null || !Physics.Raycast(cam.ScreenPointToRay(posTela), out var hit)) return;

        Transform t = hit.transform;
        while (t != null && t.parent != transform) t = t.parent;
        if (t == null) return;

        Vector3 normal = transform.InverseTransformDirection(hit.normal);
        faceClicada = -1;
        for (int m = 0; m < 4; m++)
            if (Vector3.Dot(normal, -Eixos[m]) > 0.99f) faceClicada = m;
        if (faceClicada < 0) return;

        pecaClicada = t;
        pontoClicado = transform.InverseTransformPoint(hit.point);
        inicioArraste = posTela;
    }

    void EscolherGiro(Vector2 arraste)
    {
        var cam = Camera.main;
        Vector2 dir = arraste.normalized;
        Vector2 telaPonto = cam.WorldToScreenPoint(transform.TransformPoint(pontoClicado));
        float melhor = float.NegativeInfinity;
        int eixo = -1, sentido = 0;

        for (int m = 0; m < 4; m++)
        {
            if (m == faceClicada) continue;
            foreach (int sg in new[] { 1, -1 })
            {
                Vector3 girado = Quaternion.AngleAxis(sg * 5f, Eixos[m]) * pontoClicado;
                Vector2 tela = cam.WorldToScreenPoint(transform.TransformPoint(girado));
                Vector2 mov = tela - telaPonto;
                if (mov.sqrMagnitude < 1e-6f) continue;
                float score = Vector2.Dot(mov.normalized, dir);
                if (score > melhor) { melhor = score; eixo = m; sentido = sg; }
            }
        }
        if (eixo < 0) return;
        StartCoroutine(Girar(eixo, CamadaDe(pecaClicada.localPosition, eixo), sentido, duracao));
    }

    IEnumerator Girar(int eixo, int camada, int sentido, float tempo)
    {
        girando = true;
        var camadaPecas = new List<(Transform t, Vector3 pos, Quaternion rot)>();
        foreach (var p in pecas)
            if (CamadaDe(p.localPosition, eixo) == camada)
                camadaPecas.Add((p, p.localPosition, p.localRotation));

        float angulo = 120f * sentido;
        for (float t = 0; t < tempo; t += Time.deltaTime)
        {
            var q = Quaternion.AngleAxis(angulo * Mathf.SmoothStep(0, 1, t / tempo), Eixos[eixo]);
            foreach (var (p, pos, rot) in camadaPecas)
            {
                p.localPosition = q * pos;
                p.localRotation = q * rot;
            }
            yield return null;
        }
        foreach (var (p, pos, rot) in camadaPecas)
            Aplicar(p, Quaternion.AngleAxis(angulo, Eixos[eixo]), pos, rot);
        girando = false;
    }

    static void Aplicar(Transform p, Quaternion q, Vector3 pos, Quaternion rot)
    {
        Vector3 nova = q * pos / S;
        p.localPosition = new Vector3(Mathf.Round(nova.x), Mathf.Round(nova.y), Mathf.Round(nova.z)) * S;

        Quaternion r = q * rot;
        Vector3 frente = Arredondar(r * Vector3.forward);
        Vector3 cima = Arredondar(r * Vector3.up);
        p.localRotation = Quaternion.LookRotation(frente, cima);
    }

    static Vector3 Arredondar(Vector3 v) => new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
}
