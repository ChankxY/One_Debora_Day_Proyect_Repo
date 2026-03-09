using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Escenas")]
    [SerializeField] private string campusSceneName = "Campus";
    [SerializeField] private string rewardsSceneName = "Rewards";

    [Header("Marcador")]
    [SerializeField] private int localScore = 0;
    [SerializeField] private int visitScore = 0;

    [Header("Pelota")]
    public Transform ballTransform;
    public Transform ballSpawnPoint;
    private Rigidbody ballRb;

    [Header("Tiempo")]
    public MatchClock matchClock;

    [Header("Jugadores (3 Local y 3 Visit)")]
    public PlayerResettable[] localPlayers = new PlayerResettable[3];
    public PlayerResettable[] visitPlayers = new PlayerResettable[3];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (ballTransform) ballRb = ballTransform.GetComponent<Rigidbody>();
        UpdateScoreUI();
    }

    public void AddGoalLocal()
    {
        localScore++;
        UpdateScoreUI();
    }

    public void AddGoalVisit()
    {
        visitScore++;
        UpdateScoreUI();
    }

    public void EndMatch()
    {
        Debug.Log($"[MatchManager] Fin: {localScore} — {visitScore}");

        if (visitScore > localScore)
        {
            // (Opcional) Recompensa visitante aquí si lo usas
            // PlayerPrefs.SetInt("reward_shirt", 1); PlayerPrefs.Save();

            SceneManager.LoadScene(rewardsSceneName);
        }
        else if (localScore > visitScore)
        {
            SceneManager.LoadScene(campusSceneName);
        }
        else
        {
            // Empate -> reiniciar partido
            RestartMatch();
        }
    }

    private void RestartMatch()
    {
        // 1) Marcador 0-0
        localScore = 0;
        visitScore = 0;
        UpdateScoreUI();

        // 2) Reset reloj
        if (matchClock) matchClock.ResetClock();

        // 3) Reset Ball a spawn
        if (ballTransform && ballSpawnPoint)
        {
            ballTransform.position = ballSpawnPoint.position;
            ballTransform.rotation = ballSpawnPoint.rotation;

            if (ballRb)
            {
                ballRb.linearVelocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;
                ballRb.Sleep();
            }
        }

        // 4) Reset jugadores Local (3)
        if (localPlayers != null)
        {
            for (int i = 0; i < localPlayers.Length; i++)
            {
                if (localPlayers[i] != null)
                    localPlayers[i].ResetToSpawn();
            }
        }

        // 5) Reset jugadores Visit (3)
        if (visitPlayers != null)
        {
            for (int i = 0; i < visitPlayers.Length; i++)
            {
                if (visitPlayers[i] != null)
                    visitPlayers[i].ResetToSpawn();
            }
        }

        // 6) (Opcional) Pausa breve antes de habilitar input, si lo necesitas
        // StartCoroutine(EnableGameplayIn(0.25f));
    }

    private void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = $"{localScore} — {visitScore}";
    }
}