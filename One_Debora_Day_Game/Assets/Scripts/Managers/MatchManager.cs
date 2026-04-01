using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    [Header("Ball")]
    public Rigidbody ball;

    [Header("Spawns")]
    public Transform spawnCenter;
    public Transform spawnGoalKickLocal;
    public Transform spawnGoalKickVisit;
    public Transform spawnCornerLocal;
    public Transform spawnCornerVisit;

    [Header("Field")]
    public Collider fieldCollider;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    [Header("Countdown UI (opcional)")]
    public TextMeshProUGUI countdownText;

    [Header("Scenes")]
    [SerializeField] private string campusSceneName = "Campus";
    [SerializeField] private string rewardsSceneName = "Rewards";

    // No declaramos TeamSide aquí para evitar el conflicto de tipos (tu proyecto ya lo tiene). [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
    public enum PlayerTeam { Local, Visit }
    [Header("Quién es el Player")]
    [SerializeField] private PlayerTeam playerTeam = PlayerTeam.Local;

    [Header("Players (Asignar en Inspector)")]
    [SerializeField] private Transform[] localPlayers;
    [SerializeField] private Transform[] visitPlayers;

    private Vector3[] localInitPos;
    private Quaternion[] localInitRot;
    private Vector3[] visitInitPos;
    private Quaternion[] visitInitRot;

    // =================== Freeze & Countdown ===================
    [Header("Freeze & Countdown")]
    [SerializeField] private int countdownSeconds = 3;

    [Tooltip("Arrastra aquí los scripts de IA y Player que quieras pausar durante el conteo.")]
    [SerializeField] private MonoBehaviour[] freezeBehaviours;

    [Tooltip("Opcional: RBs de jugadores/IA. Se ponen kinematic en freeze.")]
    [SerializeField] private Rigidbody[] freezeRigidbodies;

    [Tooltip("Opcional: CharacterControllers a desactivar durante freeze.")]
    [SerializeField] private CharacterController[] freezeCharacterControllers;

    private bool matchEnded = false;
    private bool isFrozen = false;
    private float cachedTimeScale = 1f;

    private bool[] cachedBehaviourEnabled;
    private bool[] cachedRbKinematic;
    private bool[] cachedCcEnabled;
    private bool cachedBallKinematic;

    // =================== Marcador ===================
    private int localScore;
    private int visitScore;

    // =================== Tiempo partido ===================
    [Header("Match Time (segundos)")]
    [SerializeField] private float matchDuration = 180f;
    private float matchTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        CacheInitialPlayerTransforms();

        localScore = 0;
        visitScore = 0;
        UpdateScoreUI();

        matchTimer = matchDuration;
        matchEnded = false;

        // Reset inicial
        ResetPlayersToInitial();
        ResetBallHard();

        // Conteo de inicio con freeze real
        StartCoroutine(CountdownSequence(reason: "INICIO"));
    }

    void Update()
    {
        // Si congelado o terminado, no bajar reloj
        if (matchEnded || isFrozen) return;

        matchTimer -= Time.deltaTime;

        if (matchTimer <= 0f)
        {
            matchTimer = 0f;
            UpdateTimeUI();
            matchEnded = true;
            EndMatch();
            return;
        }

        UpdateTimeUI();
    }

    // =================== GOLES ===================
    public void GoalLocal()
    {
        if (matchEnded) return;

        localScore++;
        UpdateScoreUI();
        if (GameAudioManager.Instance != null) GameAudioManager.Instance.PlayGoalSound();

        StartCoroutine(AfterGoalSequence());
    }

    public void GoalVisit()
    {
        if (matchEnded) return;

        visitScore++;
        UpdateScoreUI();
        if (GameAudioManager.Instance != null) GameAudioManager.Instance.PlayGoalSound();

        StartCoroutine(AfterGoalSequence());
    }

    private IEnumerator AfterGoalSequence()
    {
        FreezeGameplay(true);

        // Mantengo tu patrón: esperar 1 frame antes de resetear. [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
        yield return null;

        ResetPlayersToInitial();
        ResetBallHard();

        yield return CountdownSequence(reason: "GOL");
    }

    // =================== OUT ===================
    public void OnBallOut(OutType type, Vector3 exitPoint)
    {
        if (matchEnded) return;
        StartCoroutine(AfterOutSequence(type, exitPoint));
        if (GameAudioManager.Instance != null) GameAudioManager.Instance.PlayWhistleShort();
    }

    private IEnumerator AfterOutSequence(OutType type, Vector3 exitPoint)
    {
        FreezeGameplay(true);
        yield return null; // mismo patrón que usabas. [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)

        BallLastTouch lastTouch = ball != null ? ball.GetComponent<BallLastTouch>() : null;

        switch (type)
        {
            case OutType.SidelineLeft:
                ResetBallToPosition(BuildSidelinePosition(true, exitPoint));
                break;

            case OutType.SidelineRight:
                ResetBallToPosition(BuildSidelinePosition(false, exitPoint));
                break;

            case OutType.GoalLineLocal:
                if (lastTouch != null && lastTouch.lastTouch == TeamSide.Visit)
                    ResetBallTo(spawnCornerLocal);
                else
                    ResetBallTo(spawnGoalKickLocal);
                break;

            case OutType.GoalLineVisit:
                if (lastTouch != null && lastTouch.lastTouch == TeamSide.Local)
                    ResetBallTo(spawnCornerVisit);
                else
                    ResetBallTo(spawnGoalKickVisit);
                break;
        }

        // Si quieres también resetear jugadores tras OUT, descomenta:
        // ResetPlayersToInitial();

        yield return CountdownSequence(reason: "OUT");
    }

    // =================== FIN DEL PARTIDO ===================
    public void EndMatch()
    {
        // Empate: reinicia (con conteo)
        if (localScore == visitScore)
        {
            StartCoroutine(RestartMatchSequence());
            return;
        }

        bool playerWon =
            (playerTeam == PlayerTeam.Local && localScore > visitScore) ||
            (playerTeam == PlayerTeam.Visit && visitScore > localScore);

        SceneManager.LoadScene(playerWon ? rewardsSceneName : campusSceneName);
    }

    private IEnumerator RestartMatchSequence()
    {
        FreezeGameplay(true);

        localScore = 0;
        visitScore = 0;
        UpdateScoreUI();

        matchTimer = matchDuration;
        matchEnded = false;
        UpdateTimeUI();

        yield return null;

        ResetPlayersToInitial();
        ResetBallHard();

        yield return CountdownSequence(reason: "EMPATE");
    }

    // =================== COUNTDOWN + FREEZE REAL ===================
    private IEnumerator CountdownSequence(string reason)
    {
        // Freeze REAL: timescale 0 + deshabilitar scripts + RB kinematic
        FreezeGameplay(true);

        var cd = (countdownText != null) ? countdownText : timeText;
        if (cd != null) cd.gameObject.SetActive(true);

        for (int t = countdownSeconds; t >= 1; t--)
        {
            if (cd != null) cd.text = t.ToString();
            yield return new WaitForSecondsRealtime(1f);
        }

        if (cd != null) cd.text = "GO!";
        yield return new WaitForSecondsRealtime(0.35f);

        // Volver a mostrar reloj normal
        UpdateTimeUI();

        FreezeGameplay(false);
    }

    private void FreezeGameplay(bool freeze)
    {
        if (freeze == isFrozen) return; // evita doble trabajo
        isFrozen = freeze;

        // 1) Freeze global del tiempo/física
        if (freeze)
        {
            cachedTimeScale = Time.timeScale;
            Time.timeScale = 0f; // <- aquí sí se congela TODO el deltaTime/física
        }
        else
        {
            Time.timeScale = cachedTimeScale <= 0f ? 1f : cachedTimeScale;
        }

        // 2) Behaviours (IA/Player)
        if (freezeBehaviours != null)
        {
            if (cachedBehaviourEnabled == null || cachedBehaviourEnabled.Length != freezeBehaviours.Length)
                cachedBehaviourEnabled = new bool[freezeBehaviours.Length];

            for (int i = 0; i < freezeBehaviours.Length; i++)
            {
                var b = freezeBehaviours[i];
                if (b == null) continue;

                if (freeze)
                {
                    cachedBehaviourEnabled[i] = b.enabled;
                    b.enabled = false;
                }
                else
                {
                    b.enabled = cachedBehaviourEnabled[i];
                }
            }
        }

        // 3) CharacterControllers (si existen)
        if (freezeCharacterControllers != null)
        {
            if (cachedCcEnabled == null || cachedCcEnabled.Length != freezeCharacterControllers.Length)
                cachedCcEnabled = new bool[freezeCharacterControllers.Length];

            for (int i = 0; i < freezeCharacterControllers.Length; i++)
            {
                var cc = freezeCharacterControllers[i];
                if (cc == null) continue;

                if (freeze)
                {
                    cachedCcEnabled[i] = cc.enabled;
                    cc.enabled = false;
                }
                else
                {
                    cc.enabled = cachedCcEnabled[i];
                }
            }
        }

        // 4) Rigidbody de jugadores/IA (si existen)
        if (freezeRigidbodies != null)
        {
            if (cachedRbKinematic == null || cachedRbKinematic.Length != freezeRigidbodies.Length)
                cachedRbKinematic = new bool[freezeRigidbodies.Length];

            for (int i = 0; i < freezeRigidbodies.Length; i++)
            {
                var rb = freezeRigidbodies[i];
                if (rb == null) continue;

                if (freeze)
                {
                    cachedRbKinematic[i] = rb.isKinematic;
                    rb.linearVelocity = Vector3.zero;   // Unity 6 [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
                else
                {
                    rb.isKinematic = cachedRbKinematic[i];
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }

        // 5) Pelota
        if (ball != null)
        {
            if (freeze)
            {
                cachedBallKinematic = ball.isKinematic;
                ball.linearVelocity = Vector3.zero;    // Unity 6 [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
                ball.angularVelocity = Vector3.zero;
                ball.isKinematic = true;
            }
            else
            {
                ball.isKinematic = cachedBallKinematic;
                ball.linearVelocity = Vector3.zero;
                ball.angularVelocity = Vector3.zero;
            }
        }
    }

    // =================== POSICIONES INICIALES ===================
    private void CacheInitialPlayerTransforms()
    {
        if (localPlayers != null && localPlayers.Length > 0)
        {
            localInitPos = new Vector3[localPlayers.Length];
            localInitRot = new Quaternion[localPlayers.Length];
            for (int i = 0; i < localPlayers.Length; i++)
            {
                if (localPlayers[i] == null) continue;
                localInitPos[i] = localPlayers[i].position;
                localInitRot[i] = localPlayers[i].rotation;
            }
        }

        if (visitPlayers != null && visitPlayers.Length > 0)
        {
            visitInitPos = new Vector3[visitPlayers.Length];
            visitInitRot = new Quaternion[visitPlayers.Length];
            for (int i = 0; i < visitPlayers.Length; i++)
            {
                if (visitPlayers[i] == null) continue;
                visitInitPos[i] = visitPlayers[i].position;
                visitInitRot[i] = visitPlayers[i].rotation;
            }
        }
    }

    private void ResetPlayersToInitial()
    {
        if (localPlayers != null && localInitPos != null)
        {
            for (int i = 0; i < localPlayers.Length; i++)
            {
                var t = localPlayers[i];
                if (t == null) continue;
                TeleportTransformSafely(t, localInitPos[i], localInitRot[i]);
            }
        }

        if (visitPlayers != null && visitInitPos != null)
        {
            for (int i = 0; i < visitPlayers.Length; i++)
            {
                var t = visitPlayers[i];
                if (t == null) continue;
                TeleportTransformSafely(t, visitInitPos[i], visitInitRot[i]);
            }
        }
    }

    private void TeleportTransformSafely(Transform t, Vector3 pos, Quaternion rot)
    {
        var cc = t.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        var rb = t.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;  // Unity 6 [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        t.SetPositionAndRotation(pos, rot);
        Physics.SyncTransforms();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (cc != null) cc.enabled = true;
    }

    // =================== RESET BALÓN ===================
    private void ResetBallTo(Transform spawn)
    {
        if (spawn == null || ball == null) return;

        ball.linearVelocity = Vector3.zero;   // Unity 6 [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
        ball.angularVelocity = Vector3.zero;

        ball.transform.position = spawn.position;
        ball.transform.rotation = Quaternion.identity;
        Physics.SyncTransforms();
    }

    private void ResetBallToPosition(Vector3 position)
    {
        if (ball == null) return;

        ball.linearVelocity = Vector3.zero;   // Unity 6 [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
        ball.angularVelocity = Vector3.zero;

        ball.transform.position = position;
        ball.transform.rotation = Quaternion.identity;
        Physics.SyncTransforms();
    }

    private void ResetBallHard()
    {
        if (ball == null || spawnCenter == null) return;

        ball.linearVelocity = Vector3.zero;   // Unity 6 [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
        ball.angularVelocity = Vector3.zero;
        ball.isKinematic = true;

        ball.transform.position = spawnCenter.position;
        ball.transform.rotation = Quaternion.identity;
        Physics.SyncTransforms();

        ball.isKinematic = false;
        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
    }

    // =================== POSICIÓN DE BANDA ===================
    private Vector3 BuildSidelinePosition(bool left, Vector3 exitPoint)
    {
        Bounds fieldBounds = fieldCollider.bounds;
        float safeOffset = 1.5f;

        Vector3 pos = Vector3.zero;

        pos.x = left ? fieldBounds.min.x + safeOffset : fieldBounds.max.x - safeOffset;
        pos.z = Mathf.Clamp(exitPoint.z, fieldBounds.min.z + 1f, fieldBounds.max.z - 1f);
        pos.y = ball != null ? ball.transform.position.y : 0f;

        return pos;
    }

    // =================== UI ===================
    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = localScore + " - " + visitScore;
    }

    private void UpdateTimeUI()
    {
        if (timeText == null) return;
        int minutes = Mathf.FloorToInt(matchTimer / 60f);
        int seconds = Mathf.FloorToInt(matchTimer % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }
}

/*
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;


public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance;

    // =================== CONFIG BÁSICA ===================
    [Header("Ball")]
    public Rigidbody ball;

    [Header("Spawns")]
    public Transform spawnCenter;
    public Transform spawnGoalKickLocal;
    public Transform spawnGoalKickVisit;
    public Transform spawnCornerLocal;
    public Transform spawnCornerVisit;

    [Header("Field")]
    public Collider fieldCollider;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    [Header("Scenes")]
    [SerializeField] private string campusSceneName = "Campus";
    [SerializeField] private string rewardsSceneName = "Rewards";

    // Evitamos conflicto con tu TeamSide global (usado por BallLastTouch).
    public enum PlayerTeam { Local, Visit }

    [Header("Quién es el Player")]
    [SerializeField] private PlayerTeam playerTeam = PlayerTeam.Local;

    // =================== JUGADORES A RESETEAR ===================
    [Header("Players (Asignar en Inspector)")]
    [Tooltip("Jugadores del equipo Local que deben volver a su posición inicial")]
    [SerializeField] private Transform[] localPlayers;

    [Tooltip("Jugadores del equipo Visitante que deben volver a su posición inicial")]
    [SerializeField] private Transform[] visitPlayers;

    // Posiciones iniciales guardadas
    private Vector3[] localInitPos;
    private Quaternion[] localInitRot;
    private Vector3[] visitInitPos;
    private Quaternion[] visitInitRot;

    // =================== MARCADOR ===================
    private int localScore;
    private int visitScore;

    // =================== TIEMPO ===================
    [Header("Match Time (segundos)")]
    [SerializeField] private float matchDuration = 180f;
    private float matchTimer;
    private bool matchEnded = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Guardar posiciones iniciales de jugadores UNA VEZ
        CacheInitialPlayerTransforms();

        // UI inicial
        UpdateScoreUI();

        // Balón al centro
        ResetBallTo(spawnCenter);

        // Tiempo
        matchTimer = matchDuration;
        matchEnded = false;
        UpdateTimeUI();
    }

    void Update()
    {
        if (matchEnded) return;

        matchTimer -= Time.deltaTime;

        if (matchTimer <= 0f)
        {
            matchTimer = 0f;
            matchEnded = true;
            EndMatch();
        }

        UpdateTimeUI();
    }

    // =================== GUARDAR POSICIONES INICIALES ===================
    private void CacheInitialPlayerTransforms()
    {
        // Local
        if (localPlayers != null && localPlayers.Length > 0)
        {
            localInitPos = new Vector3[localPlayers.Length];
            localInitRot = new Quaternion[localPlayers.Length];

            for (int i = 0; i < localPlayers.Length; i++)
            {
                if (localPlayers[i] == null) continue;
                localInitPos[i] = localPlayers[i].position;
                localInitRot[i] = localPlayers[i].rotation;
            }
        }

        // Visit
        if (visitPlayers != null && visitPlayers.Length > 0)
        {
            visitInitPos = new Vector3[visitPlayers.Length];
            visitInitRot = new Quaternion[visitPlayers.Length];

            for (int i = 0; i < visitPlayers.Length; i++)
            {
                if (visitPlayers[i] == null) continue;
                visitInitPos[i] = visitPlayers[i].position;
                visitInitRot[i] = visitPlayers[i].rotation;
            }
        }
    }

    // =================== GOLES ===================
    public void GoalLocal()
    {
        if (matchEnded) return;

        localScore++;
        UpdateScoreUI();
        StartCoroutine(ResetAfterGoal()); // Reset tras gol [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
        if (GameAudioManager.Instance != null) GameAudioManager.Instance.PlayGoalSound();
    }

    public void GoalVisit()
    {
        if (matchEnded) return;

        visitScore++;
        UpdateScoreUI();
        StartCoroutine(ResetAfterGoal()); // Reset tras gol [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
        if (GameAudioManager.Instance != null) GameAudioManager.Instance.PlayGoalSound();
    }

    private IEnumerator ResetAfterGoal()
    {
        // Espera 1 frame para que todo se actualice (igual que tu patrón actual) [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
        yield return null;

        // ✅ Reset jugadores a posiciones iniciales
        ResetPlayersToInitial();

        // ✅ Reset balón al centro
        ResetBallHard();
    }

    // =================== OUT ===================
    public void OnBallOut(OutType type, Vector3 exitPoint)
    {
        StartCoroutine(ResetAfterOut(type, exitPoint)); // Patrón actual [1](https://internoredpedu-my.sharepoint.com/personal/yonnathan_chacon798_educacionbogota_edu_co/Documents/Archivos%20de%20Microsoft%C2%A0Copilot%20Chat/MatchManager.cs)
        if (GameAudioManager.Instance != null) GameAudioManager.Instance.PlayWhistleShort();
    }

    private IEnumerator ResetAfterOut(OutType type, Vector3 exitPoint)
    {
        yield return null;

        var lastTouch = ball != null ? ball.GetComponent<BallLastTouch>() : null;

        switch (type)
        {
            case OutType.SidelineLeft:
                ResetBallToPosition(BuildSidelinePosition(true, exitPoint));
                break;

            case OutType.SidelineRight:
                ResetBallToPosition(BuildSidelinePosition(false, exitPoint));
                break;

            case OutType.GoalLineLocal:
                if (lastTouch != null && lastTouch.lastTouch == TeamSide.Visit)
                    ResetBallTo(spawnCornerLocal);
                else
                    ResetBallTo(spawnGoalKickLocal);
                break;

            case OutType.GoalLineVisit:
                if (lastTouch != null && lastTouch.lastTouch == TeamSide.Local)
                    ResetBallTo(spawnCornerVisit);
                else
                    ResetBallTo(spawnGoalKickVisit);
                break;
        }
    }

    // =================== FIN DEL PARTIDO ===================
    public void EndMatch()
    {
        Debug.Log($"[MatchManager] Fin del partido: {localScore} - {visitScore}");

        // Empate → reinicio completo
        if (localScore == visitScore)
        {
            RestartMatch();
            return;
        }

        bool playerWon =
            (playerTeam == PlayerTeam.Local && localScore > visitScore) ||
            (playerTeam == PlayerTeam.Visit && visitScore > localScore);

        if (playerWon) SceneManager.LoadScene(rewardsSceneName);
        else SceneManager.LoadScene(campusSceneName);
    }

    private void RestartMatch()
    {
        Debug.Log("[MatchManager] EMPATE → Reinicio del partido");

        // 1) Marcador
        localScore = 0;
        visitScore = 0;
        UpdateScoreUI();

        // 2) Tiempo
        matchTimer = matchDuration;
        matchEnded = false;
        UpdateTimeUI();

        // 3) Reset jugadores
        ResetPlayersToInitial();

        // 4) Reset balón
        ResetBallHard();
    }

    // =================== RESET JUGADORES ===================
    private void ResetPlayersToInitial()
    {
        // Local
        if (localPlayers != null && localInitPos != null)
        {
            for (int i = 0; i < localPlayers.Length; i++)
            {
                var t = localPlayers[i];
                if (t == null) continue;

                TeleportTransformSafely(t, localInitPos[i], localInitRot[i]);
            }
        }

        // Visit
        if (visitPlayers != null && visitInitPos != null)
        {
            for (int i = 0; i < visitPlayers.Length; i++)
            {
                var t = visitPlayers[i];
                if (t == null) continue;

                TeleportTransformSafely(t, visitInitPos[i], visitInitRot[i]);
            }
        }
    }

    // Teletransporte seguro (evita bugs si hay CharacterController/Rigidbody)
    private void TeleportTransformSafely(Transform t, Vector3 pos, Quaternion rot)
    {
        // Si tiene CharacterController, desactivar/activar para moverlo sin "snap back"
        var cc = t.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Si tiene Rigidbody, parar física antes
        var rb = t.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Aplicar transform
        t.SetPositionAndRotation(pos, rot);
        Physics.SyncTransforms();

        // Reactivar Rigidbody
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Reactivar CharacterController
        if (cc != null) cc.enabled = true;
    }

    // =================== RESET BALÓN ===================
    private void ResetBallTo(Transform spawn)
    {
        if (spawn == null || ball == null) return;

        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;

        ball.transform.position = spawn.position;
        ball.transform.rotation = Quaternion.identity;
        Physics.SyncTransforms();
    }

    private void ResetBallToPosition(Vector3 position)
    {
        if (ball == null) return;

        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;

        ball.transform.position = position;
        ball.transform.rotation = Quaternion.identity;
        Physics.SyncTransforms();
    }

    private void ResetBallHard()
    {
        if (ball == null || spawnCenter == null) return;

        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
        ball.isKinematic = true;

        ball.transform.position = spawnCenter.position;
        ball.transform.rotation = Quaternion.identity;
        Physics.SyncTransforms();

        ball.isKinematic = false;
        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
    }

    // =================== POSICIÓN DE BANDA (TU LÓGICA) ===================
    private Vector3 BuildSidelinePosition(bool left, Vector3 exitPoint)
    {
        Bounds fieldBounds = fieldCollider.bounds;
        float safeOffset = 1.5f;

        Vector3 pos = Vector3.zero;

        pos.x = left
            ? fieldBounds.min.x + safeOffset
            : fieldBounds.max.x - safeOffset;

        pos.z = Mathf.Clamp(exitPoint.z, fieldBounds.min.z + 1f, fieldBounds.max.z - 1f);

        pos.y = ball != null ? ball.transform.position.y : 0f;

        return pos;
    }

    // =================== UI ===================
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = localScore + " - " + visitScore;
    }

    private void UpdateTimeUI()
    {
        if (timeText == null) return;

        int minutes = Mathf.FloorToInt(matchTimer / 60f);
        int seconds = Mathf.FloorToInt(matchTimer % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }
}
*/