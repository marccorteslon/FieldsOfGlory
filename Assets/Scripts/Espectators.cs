using UnityEngine;

public class Espectators : MonoBehaviour
{
    [Header("Movimiento")]
    public float altura = 2f;          // Distancia máxima arriba/abajo
    public float velocidad = 2f;       // Velocidad del movimiento

    [Header("Fase")]
    public bool faseInvertida = false; // Si está activado, empieza al revés

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        // Calcula el movimiento usando seno
        float offset = Mathf.Sin(Time.time * velocidad);

        // Invierte la fase para que unos suban mientras otros bajan
        if (faseInvertida)
            offset *= -1f;

        // Aplica movimiento vertical
        transform.position = posicionInicial + Vector3.up * offset * altura;
    }
}