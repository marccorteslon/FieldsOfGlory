using UnityEngine;

/// <summary>
/// Marcador de spawn del pueblo en las escenas World/TutorialWorld.
/// Cuando el jugador pulsa X para entrar al pueblo, el WalkingPlayer
/// se teletransporta a la posición de este objeto.
/// No tiene ninguna otra lógica.
/// </summary>
public class TownObjectSpawn : MonoBehaviour
{
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
        Gizmos.DrawSphere(transform.position, 0.4f);

        // Flecha de orientación
        Gizmos.color = new Color(0.1f, 1f, 0.4f, 0.9f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
#endif
}
