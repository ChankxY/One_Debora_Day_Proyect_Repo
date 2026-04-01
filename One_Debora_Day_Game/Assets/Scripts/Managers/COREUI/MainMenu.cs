using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Build Indexes (File > Build Settings...)")]
    [Tooltip("Índice de la escena Campus en Build Settings")]
    [SerializeField] private int campusSceneIndex = 1;

    [Tooltip("Índice de la escena Créditos en Build Settings")]
    [SerializeField] private int creditsSceneIndex = 2;

    [Header("Panels")]
    [Tooltip("Panel de opciones dentro del Canvas (inicia desactivado).")]
    [SerializeField] private GameObject optionsPanel;

    [Tooltip("Panel principal (botones del menú). Opcional: se oculta al abrir opciones.")]
    [SerializeField] private GameObject mainPanel;

    // -------------------- BOTONES --------------------

    // 1) JUGAR -> Campus (por índice)
    public void Play()
    {
        Time.timeScale = 1f;  // por si vienes de freeze en otra escena
        SceneManager.LoadScene(campusSceneIndex);
    }

    // 2) OPCIONES -> abre panel configuración (sin cambiar escena)
    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (mainPanel != null) mainPanel.SetActive(false);
    }

    // Botón dentro del panel de opciones para volver
    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
    }

    // 3) CRÉDITOS -> escena Créditos (por índice)
    public void Credits()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(creditsSceneIndex);
    }

    // 4) SALIR -> cerrar el juego
    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quit() llamado (solo funciona en Build).");
    }
}
