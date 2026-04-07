using UnityEngine;

public class CamaraCircular : MonoBehaviour
{
    public Transform centro;
    public float radio = 5f;
    public float velocidad = 50f;
    public float altura = 20f;     // Altura de la cámara

    private float angulo = 0f;

    void Update()
    {
        if (centro == null) return;

        // Incrementar ángulo
        angulo += velocidad * Time.deltaTime;

        float rad = angulo * Mathf.Deg2Rad;

        // Posición en círculo
        float x = Mathf.Cos(rad) * radio;
        float z = Mathf.Sin(rad) * radio;

        // Aplicar altura
        Vector3 posicion = centro.position + new Vector3(x, altura, z);
        transform.position = posicion;

        // Mirar al centro (esto ya hace que mire hacia abajo)
        transform.LookAt(centro.position);
    }
}