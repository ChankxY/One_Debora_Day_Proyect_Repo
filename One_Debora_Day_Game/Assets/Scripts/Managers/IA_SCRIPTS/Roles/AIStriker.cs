using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStriker : BaseAIAgent
{
    public float shootDistance = 6f;
    public float shootForce = 10f;
    public float chaseDistance = 3f;

    void Update()
    {
        if (ball == null) return;

        float dist = Vector3.Distance(transform.position, ball.position);

        if (dist > chaseDistance)
        {
            MoveTowards(ball.position);
        }
        else
        {
            // Avanzar hacia arco rival
            MoveTowards(opponentGoal.position);

            if (Vector3.Distance(transform.position, opponentGoal.position) < shootDistance)
            {
                Vector3 dir = opponentGoal.position - transform.position;
                KickBall(dir, shootForce);
            }
        }
    }
}
