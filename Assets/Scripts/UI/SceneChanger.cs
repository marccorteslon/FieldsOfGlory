using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Nombre de la escena a la que quieres cambiar
    [SerializeField] private string sceneName;

    // Método público que se puede asignar al botón
    public void ChangeScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("No se ha asignado ningún nombre de escena en SceneChanger.");
        }
    }
}