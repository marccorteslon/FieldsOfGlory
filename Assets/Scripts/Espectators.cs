using UnityEngine;

public class Espectators : MonoBehaviour
{
    [Header("Movimiento")]
    public float altura = 2f;
    public float velocidad = 2f;

    [Header("Fase")]
    public bool faseInvertida;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;

        // 50% de probabilidad de true o false
        faseInvertida = Random.value > 0.5f;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * velocidad);

        if (faseInvertida)
            offset *= -1f;

        transform.position = posicionInicial + Vector3.up * offset * altura;
    }
}