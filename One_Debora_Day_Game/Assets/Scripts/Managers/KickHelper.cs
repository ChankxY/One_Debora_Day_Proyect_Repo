using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KickHelper
{
    /// <summary>
    /// Intenta patear la pelota hacia un punto objetivo si está a distancia y altura válidas.
    /// </summary>
    public static bool TryKickToward(Rigidbody ball, Transform actor,
                                     Vector3 targetWorld, float force, float upward,
                                     float controlRadius = 1.2f, float maxHeightDelta = 0.6f)
    {
        if (!ball) return false;

        Vector3 toBall = ball.position - actor.position;
        Vector2 toBallXZ = new Vector2(toBall.x, toBall.z);
        if (toBallXZ.sqrMagnitude > controlRadius * controlRadius) return false;

        if (Mathf.Abs(ball.position.y - actor.position.y) > maxHeightDelta) return false;

        // Dirección aproximada hacia el objetivo, con ligera elevación
        Vector3 dir = (targetWorld - ball.position).normalized + Vector3.up * upward;
        ball.AddForce(dir.normalized * force, ForceMode.VelocityChange);
        return true;
    }

    /// <summary>
    /// Patea en la dirección "frente" del actor (útil como despeje).
    /// </summary>
    public static bool TryKickForward(Rigidbody ball, Transform actor, float force, float upward,
                                      float controlRadius = 1.2f, float maxHeightDelta = 0.6f)
    {
        if (!ball) return false;

        Vector3 toBall = ball.position - actor.position;
        Vector2 toBallXZ = new Vector2(toBall.x, toBall.z);
        if (toBallXZ.sqrMagnitude > controlRadius * controlRadius) return false;

        if (Mathf.Abs(ball.position.y - actor.position.y) > maxHeightDelta) return false;

        Vector3 dir = actor.forward + Vector3.up * upward;
        ball.AddForce(dir.normalized * force, ForceMode.VelocityChange);
        return true;
    }
}