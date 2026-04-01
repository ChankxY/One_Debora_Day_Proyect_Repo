using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallKick : MonoBehaviour
{
    [Header("Ball")]
    public Rigidbody ball;

    [Header("Ranges")]
    public float kickRange = 1.5f;

    [Header("Forces")]
    public float passForce = 4f;
    public float shootForce = 9f;
    public float upwardForce = 0.3f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            TryKick(passForce);

        if (Input.GetKeyDown(KeyCode.E))
            TryKick(shootForce);
    }

    void TryKick(float force)
    {
        if (ball == null) return;

        float dist = Vector3.Distance(transform.position, ball.position);
        if (dist > kickRange) return; // ❌ no patear desde lejos

        Vector3 dir = transform.forward + Vector3.up * upwardForce;

        ball.linearVelocity = Vector3.zero; // mejor control
        ball.angularVelocity = Vector3.zero;

        ball.AddForce(dir.normalized * force, ForceMode.Impulse);
    }
}