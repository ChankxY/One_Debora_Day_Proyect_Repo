using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIAgentMotor))]
public class AISupportTeammate : MonoBehaviour
{
    public AISharedRefs shared;

    [Header("Apoyo")]
    public Transform humanPlayer;         // a quién apoyar/pasar
    public float supportOffsetSide = 1.2f; // apertura lateral
    public float supportOffsetBack = 0.7f; // un poco por detrás de la línea balón-arco
    public float repositionRadius = 5.5f;    // distancia a la que busca apoyo

    [Header("Decisiones de posesión")]
    public float controlRadius = 1.1f;
    public float maxHeightDelta = 0.6f;
    public float shootForce = 7.0f;
    public float passForce = 5.0f;
    public float upward = 0.07f;
    public float shootDistance = 6.0f;     // si está a menos de esto del arco → intenta tirar
    public float faceAngleMax = 45f;      // tolerancia de ángulo al arco para tiro

    [Header("Conducción básica")]
    public float dribbleStep = 1.5f;      // avance corto

    private AIAgentMotor motor;
    private float decisionCd;

    void Awake() => motor = GetComponent<AIAgentMotor>();

    void FixedUpdate()
    {
        if (!shared || !shared.ball || !shared.MyAttackingGoal) return;

        Vector3 ballPos = shared.ball.position;
        Vector3 goalPos = shared.MyAttackingGoal.position;

        // 1) ¿Tengo opción de patear ahora?
        bool closeToBall = IsCloseToBall(transform, shared.ball, controlRadius, maxHeightDelta);

        if (closeToBall && decisionCd <= 0f)
        {
            // ¿Tiro al arco?
            float distToGoal = Vector3.Distance(transform.position, goalPos);
            bool goodAngle = AngleTo(transform, goalPos) <= faceAngleMax;

            if (distToGoal <= shootDistance && goodAngle)
            {
                // Tiro
                if (KickHelper.TryKickToward(shared.ball, transform, goalPos, shootForce, upward,
                                             controlRadius, maxHeightDelta))
                {
                    decisionCd = 0.3f; // pequeña cadencia
                    return;
                }
            }

            // ¿Pase al humano?
            if (humanPlayer != null)
            {
                Vector3 passTarget = humanPlayer.position;
                if (KickHelper.TryKickToward(shared.ball, transform, passTarget, passForce, upward,
                                             controlRadius, maxHeightDelta))
                {
                    decisionCd = 0.3f;
                    return;
                }
            }

            // Si no pudo tirar ni pasar → conduce
            Vector3 step = Vector3.MoveTowards(transform.position, goalPos, dribbleStep);
            step.y = transform.position.y;
            motor.SetTarget(step);
            decisionCd = 0.15f;
            return;
        }

        // 2) Sin posesión → posición de apoyo (abrirse)
        Vector3 support = ComputeSupportPoint(ballPos, goalPos);
        motor.SetTarget(support);

        // 3) Enfriamiento de decisión
        if (decisionCd > 0f) decisionCd -= Time.fixedDeltaTime;
    }

    private bool IsCloseToBall(Transform who, Rigidbody ball, float radius, float maxY)
    {
        Vector3 d = ball.position - who.position;
        if (Mathf.Abs(d.y) > maxY) return false;
        d.y = 0f;
        return d.sqrMagnitude <= radius * radius;
    }

    private float AngleTo(Transform who, Vector3 worldPoint)
    {
        Vector3 dir = (worldPoint - who.position); dir.y = 0f;
        Vector3 fwd = who.forward; fwd.y = 0f;
        return Vector3.Angle(fwd, dir);
    }

    // Punto de apoyo simple: lateral respecto a la línea balón→arco y un poco por detrás
    private Vector3 ComputeSupportPoint(Vector3 ballPos, Vector3 goalPos)
    {
        Vector3 line = (goalPos - ballPos); line.y = 0f;
        Vector3 right = Vector3.Cross(Vector3.up, line.normalized); // lateral

        // Alterna lado según “qué tan a la derecha” estoy, para no superponerme
        float side = Vector3.Dot((transform.position - ballPos).normalized, right) > 0f ? 1f : -1f;

        Vector3 target = ballPos + (-line.normalized * supportOffsetBack) + (right * side * supportOffsetSide);

        // No alejarse demasiado
        Vector3 toTarget = target - ballPos; toTarget.y = 0f;
        if (toTarget.magnitude > repositionRadius)
            target = ballPos + toTarget.normalized * repositionRadius;

        target.y = transform.position.y;
        return target;
    }
}