using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIAgentMotor))]
public class AIForward : MonoBehaviour
{
    public AISharedRefs shared;
    public float controlRadius = 1.0f;
    public float shootForce = 8.5f;
    public float passForce = 5f;
    public float upward = 0.06f;
    public float attackDistance = 6.0f;

    private AIAgentMotor motor;

    void Awake() => motor = GetComponent<AIAgentMotor>();

    void FixedUpdate()
    {
        if (!shared || !shared.ball) return;

        Vector3 ballPos = shared.ball.position;
        float dist = Vector3.Distance(transform.position, ballPos);

        // 1) Recuperación
        if (dist < 2f)
        {
            motor.SetTarget(ballPos);

            // decidir tiro
            Vector3 goal = shared.MyAttackingGoal.position;
            KickHelper.TryKickToward(shared.ball, transform, goal,
                                     shootForce, upward,
                                     controlRadius, 0.6f);
            return;
        }

        // 2) No tiene la pelota → buscar línea de ataque
        Vector3 attackPos = ballPos + (shared.MyAttackingGoal.position - ballPos).normalized * 3f;
        attackPos.y = transform.position.y;

        motor.SetTarget(attackPos);
    }
}