using UnityEngine;

/// <summary>
/// Desactiva todos los TerrainCollider de la escena al inicio.
/// Evita que la lanza registre colisiones falsas con el terreno.
/// </summary>
public class DisableTerrainColliders : MonoBehaviour
{
    void Awake()
    {
        TerrainCollider[] terrainColliders = FindObjectsByType<TerrainCollider>(FindObjectsSortMode.None);

        foreach (TerrainCollider tc in terrainColliders)
            tc.enabled = false;

        Debug.Log($"[DisableTerrainColliders] {terrainColliders.Length} TerrainCollider(s) desactivados.");
    }
}
