using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbita : MonoBehaviour
{
    [SerializeField] Transform alvo;
    [SerializeField] float sensibilidade = 0.3f;
    [SerializeField] float passoZoom = 0.1f;
    [SerializeField] float distanciaMin = 3.5f;
    [SerializeField] float distanciaMax = 20f;
    [SerializeField] float pitchMin = -80f;
    [SerializeField] float pitchMax = 80f;

    float distancia, yaw, pitch;
    Vector2 ultimaPos;

    public static bool AltPressionado => Keyboard.current != null && Keyboard.current.altKey.isPressed;

    void Start()
    {
        Vector3 offset = transform.position - alvo.position;
        distancia = Mathf.Clamp(offset.magnitude, distanciaMin, distanciaMax);
        Vector3 angulos = Quaternion.LookRotation(-offset).eulerAngles;
        yaw = angulos.y;
        pitch = Mathf.Clamp(angulos.x > 180f ? angulos.x - 360f : angulos.x, pitchMin, pitchMax);
        Posicionar();
    }

    void LateUpdate()
    {
        var ponteiro = Pointer.current;
        if (ponteiro != null)
        {
            Vector2 pos = ponteiro.position.ReadValue();
            if (AltPressionado && ponteiro.press.isPressed && !ponteiro.press.wasPressedThisFrame)
            {
                Vector2 delta = pos - ultimaPos;
                yaw += delta.x * sensibilidade;
                pitch = Mathf.Clamp(pitch - delta.y * sensibilidade, pitchMin, pitchMax);
            }
            ultimaPos = pos;
        }

        var mouse = Mouse.current;
        if (mouse != null)
        {
            float scroll = Mathf.Clamp(mouse.scroll.ReadValue().y, -3f, 3f);
            if (scroll != 0f)
                distancia = Mathf.Clamp(distancia * Mathf.Pow(1f - passoZoom, scroll), distanciaMin, distanciaMax);
        }

        Posicionar();
    }

    void Posicionar()
    {
        transform.position = alvo.position + Quaternion.Euler(pitch, yaw, 0f) * new Vector3(0f, 0f, -distancia);
        transform.LookAt(alvo.position);
    }
}
