using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    [Header("Build Indexes (File > Build Settings...)")]
    [SerializeField] private int mainMenuIndex = 0;
    [SerializeField] private int campusIndex = 1;
    [SerializeField] private int creditsIndex = 2;

    // Botón "Salir" (volver)
    public void ExitOrBack()
    {
        // Por si vienes de gameplay donde congelaste el tiempo
        Time.timeScale = 1f;

        int current = SceneManager.GetActiveScene().buildIndex;

        // Si estás en Credits -> MainMenu
        if (current == creditsIndex)
        {
            SafeLoad(mainMenuIndex);
        }
        else
        {
            // En cualquier otro caso -> Campus
            SafeLoad(campusIndex);
        }
    }

    // Opcional: mantener también un Quit real (si lo necesitas en MainMenu)
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
        Debug.Log("QuitGame() llamado (solo funciona en Build).");
    }

    private void SafeLoad(int index)
    {
        // Validación para evitar errores por índice mal puesto
        if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"[SceneNavigator] Índice inválido: {index}. Revisa Build Settings.");
            return;
        }
        SceneManager.LoadScene(index);
    }
}
