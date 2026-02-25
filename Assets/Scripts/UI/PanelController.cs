using UnityEngine;

public class PanelController : MonoBehaviour
{
    // Panel asociado a este botón
    public GameObject panel;

    // Lista de todos los otros paneles que deben cerrarse
    public GameObject[] otrosPaneles;

    // Audio
    public AudioSource audioSource;      // El componente que reproducirá el sonido
    public AudioClip[] sonidos;          // Array con los 2 sonidos

    // Este método se asigna al botón que abre/cierra el panel
    public void TogglePanel()
    {
        if (panel != null)
        {
            // Cerrar todos los otros paneles
            foreach (GameObject p in otrosPaneles)
            {
                if (p != null && p.activeSelf)
                {
                    p.SetActive(false);
                }
            }

            // Abrir/cerrar el panel actual
            panel.SetActive(!panel.activeSelf);

            // Reproducir sonido
            PlayRandomSound();
        }
    }

    // Este método se asigna al botón dentro del panel para cerrarlo
    public void ClosePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            // Reproducir sonido
            PlayRandomSound();
        }
    }

    // Método auxiliar para reproducir un sonido aleatorio
    private void PlayRandomSound()
    {
        if (audioSource != null && sonidos != null && sonidos.Length > 0)
        {
            int index = Random.Range(0, sonidos.Length);
            audioSource.PlayOneShot(sonidos[index]);
        }
    }
}
