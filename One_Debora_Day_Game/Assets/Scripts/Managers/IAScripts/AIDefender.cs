using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIAgentMotor))]
public class AIDefender : MonoBehaviour
{
    public AISharedRefs shared;
    public Transform humanPlayer;
    public float defendRadius = 4.0f;
    public float closeRadius = 2.0f;

    public float clearForce = 7.2f;
    public float upward = 0.06f;
    public float controlRadius = 1.1f;
    public float maxHeightDelta = 0.6f;

    private AIAgentMotor motor;

    void Awake() => motor = GetComponent<AIAgentMotor>();

    void FixedUpdate()
    {
        if (!shared || !shared.ball) return;

        Vector3 ballPos = shared.ball.position;
        float distToBall = Vector3.Distance(transform.position, ballPos);

        // 1) Si la pelota está cerca, recuperación
        if (distToBall <= closeRadius)
        {
            motor.SetTarget(ballPos);

            // si logra alcanzarla, despeje
            Vector3 dirClear = (shared.MyAttackingGoal.position - shared.ball.position).normalized;
            Vector3 targetClear = shared.ball.position + dirClear * 6f;

            KickHelper.TryKickToward(shared.ball, transform, targetClear,
                                     clearForce, upward,
                                     controlRadius, maxHeightDelta);
            return;
        }

        // 2) Marca al jugador humano si este tiene la pelota o está cerca
        if (humanPlayer != null)
        {
            float distToHuman = Vector3.Distance(humanPlayer.position, ballPos);

            if (distToHuman < defendRadius)
            {
                motor.SetTarget(humanPlayer.position);
                return;
            }
        }

        // 3) Posición base: defensa central
        Vector3 defensivePoint = shared.MyDefendingGoal.position + transform.right * 2f;
        defensivePoint.y = transform.position.y;

        motor.SetTarget(defensivePoint);
    }
}