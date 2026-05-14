using UnityEngine;
using UnityEditor;

public class PhysicalJoustSetup : EditorWindow
{
    [MenuItem("Tools/Fields of Glory/Setup Physical Jousting")]
    public static void SetupJousting()
    {
        JoustManager manager = FindObjectOfType<JoustManager>();
        if (manager == null)
        {
            Debug.LogError("No se encontró un JoustManager en la escena. Abre la escena de la justa primero.");
            return;
        }

        Camera cam = manager.mainCamera;
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("No se encontró la cámara.");
                return;
            }
        }

        // Crear Pivote de Lanza
        GameObject lancePivot = GameObject.Find("LancePivot");
        if (lancePivot == null)
        {
            lancePivot = new GameObject("LancePivot");
            lancePivot.transform.SetParent(cam.transform);
            lancePivot.transform.localPosition = new Vector3(0.5f, -0.4f, 0.8f); // Abajo a la derecha
            lancePivot.transform.localRotation = Quaternion.identity;
            
            PhysicalLanceController lanceController = lancePivot.AddComponent<PhysicalLanceController>();
            lanceController.lancePivot = lancePivot.transform;
            Debug.Log("✅ LancePivot creado.");
        }

        // Crear Pivote de Escudo
        GameObject shieldPivot = GameObject.Find("ShieldPivot");
        if (shieldPivot == null)
        {
            shieldPivot = new GameObject("ShieldPivot");
            shieldPivot.transform.SetParent(cam.transform);
            shieldPivot.transform.localPosition = new Vector3(-0.5f, -0.4f, 0.8f); // Abajo a la izquierda
            shieldPivot.transform.localRotation = Quaternion.identity;
            
            PhysicalShieldController shieldController = shieldPivot.AddComponent<PhysicalShieldController>();
            shieldController.shieldPivot = shieldPivot.transform;
            Debug.Log("✅ ShieldPivot creado.");
        }

        // Desactivar sistemas antiguos por seguridad
        if (manager.attackPart != null) manager.attackPart.gameObject.SetActive(false);
        if (manager.defensePart != null) manager.defensePart.gameObject.SetActive(false);
        if (manager.horsePart != null) manager.horsePart.gameObject.SetActive(false);

        Selection.activeGameObject = cam.gameObject;
        Debug.Log("🎉 ¡Configuración Física inicial completada! Ahora solo tienes que arrastrar tus modelos 3D de la lanza y escudo dentro de LancePivot y ShieldPivot.");
    }
}
