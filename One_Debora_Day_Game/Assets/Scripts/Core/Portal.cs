
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Portal : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena que se cargará.")]
    public string sceneName;

    private bool inside;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            inside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            inside = false;
    }

    // Ejecutado automáticamente cuando presionas Gameplay/Submit
    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (inside && !string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
