using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAIAgent : MonoBehaviour
{
    [Header("Common References")]
    public Rigidbody ball;
    public Transform ownGoal;
    public Transform opponentGoal;

    [Header("Movement")]
    public float speed = 3f;
    public float rotationSpeed = 360f;

    protected void MoveTowards(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            rot,
            rotationSpeed * Time.deltaTime
        );
    }

    protected void LookAt(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        transform.rotation = Quaternion.LookRotation(dir);
    }

    protected void KickBall(Vector3 direction, float force)
    {
        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;
        ball.AddForce(direction.normalized * force, ForceMode.Impulse);
    }
}