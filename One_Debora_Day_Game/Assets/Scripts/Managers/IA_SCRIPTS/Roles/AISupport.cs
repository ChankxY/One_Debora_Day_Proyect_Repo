using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISupport : BaseAIAgent
{
    public Transform player;
    public float engageDistance = 3f;
    public float passForce = 6f;

    void Update()
    {
        if (ball == null || player == null) return;

        Vector3 midpoint = (ball.position + ownGoal.position) * 0.5f;
        MoveTowards(midpoint);

        if (Vector3.Distance(transform.position, ball.position) < engageDistance)
        {
            Vector3 passDir = player.position - transform.position;
            KickBall(passDir, passForce);
        }
    }
}
