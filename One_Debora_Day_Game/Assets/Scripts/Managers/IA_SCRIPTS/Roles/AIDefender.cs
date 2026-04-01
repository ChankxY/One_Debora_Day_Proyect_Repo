using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIDefender : BaseAIAgent
{
    public float engageDistance = 2.5f;
    public float clearForce = 7f;

    void Update()
    {
        if (ball == null) return;

        MoveTowards(ball.position);

        if (Vector3.Distance(transform.position, ball.position) < engageDistance)
        {
            Vector3 dir = opponentGoal.position - transform.position;
            KickBall(dir, clearForce);
        }
    }
}
