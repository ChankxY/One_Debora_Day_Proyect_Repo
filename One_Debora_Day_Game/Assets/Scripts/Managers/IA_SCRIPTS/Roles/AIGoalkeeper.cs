using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGoalkeeper : BaseAIAgent
{
    public float goalWidth = 3f;
    public float clearDistance = 2f;
    public float clearForce = 8f;

    void Update()
    {
        if (ball == null) return;

        Vector3 ballPos = ball.position;

        // Seguir el balón en X, quedarse en el arco
        Vector3 target = ownGoal.position;
        target.x = Mathf.Clamp(
            ballPos.x,
            ownGoal.position.x - goalWidth,
            ownGoal.position.x + goalWidth
        );

        MoveTowards(target);

        // Despejar
        if (Vector3.Distance(transform.position, ballPos) < clearDistance)
        {
            Vector3 dir = opponentGoal.position - transform.position;
            KickBall(dir, clearForce);
        }
    }
}
