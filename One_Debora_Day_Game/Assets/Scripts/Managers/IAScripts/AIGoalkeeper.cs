
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIAgentMotor))]
public class AIGoalkeeper : MonoBehaviour
{
    public AISharedRefs shared;

    [Header("Portería")]
    public Transform goalLineCenter;     // centro propio (defiende)
    public float halfGoalWidth = 2.2f;   // mitad del ancho entre postes

    [Header("Ajustes de posicionamiento")]
    public float lineDepth = 0.15f;       // cuán "dentro" de la portería (Z)
    public float trackLerp = 6f;        // qué tan rápido se alinea con la pelota (X)

    [Header("Intercepción / Rechazo")]
    public float interceptRadius = 2.2f; // si la pelota entra aquí → achique
    public float clearForce = 7.5f;
    public float clearUpward = 0.08f;
    public float controlRadius = 1.1f;
    public float maxHeightDelta = 0.6f;

    private AIAgentMotor motor;

    void Awake() => motor = GetComponent<AIAgentMotor>();

    void FixedUpdate()
    {
        if (!shared || !shared.ball || !goalLineCenter) return;

        Vector3 ballPos = shared.ball.position;

        // 1) Posición preferida en la línea: clamp X entre postes, Z un poco adelante/dentro
        Vector3 desired = goalLineCenter.position;
        desired.x = Mathf.Clamp(ballPos.x, goalLineCenter.position.x - halfGoalWidth,
                                           goalLineCenter.position.x + halfGoalWidth);
        desired.z += shared.thisTeamIsLocal ? lineDepth : -lineDepth;

        // 2) Si la pelota está cerca → ir hacia ella (achique)
        float dist = Vector3.Distance(transform.position, ballPos);
        if (dist <= interceptRadius)
        {
            // Achique hacia la pelota (pero no demasiado encima)
            Vector3 achique = Vector3.Lerp(transform.position, ballPos, 0.6f);
            achique.y = transform.position.y;
            motor.SetTarget(achique);

            // Si está a alcance → despeje hacia adelante (campo rival)
            Vector3 forwardField = (shared.MyAttackingGoal.position - transform.position).normalized;
            Vector3 targetClear = transform.position + forwardField * 5f;

            KickHelper.TryKickToward(shared.ball, transform, targetClear, clearForce, clearUpward,
                                     controlRadius, maxHeightDelta);
        }
        else
        {
            // Alinear sobre la línea con suavidad
            Vector3 smooth = Vector3.Lerp(transform.position, desired, trackLerp * Time.fixedDeltaTime);
            smooth.y = transform.position.y;
            motor.SetTarget(smooth);
        }
    }
}