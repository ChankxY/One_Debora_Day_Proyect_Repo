using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FootballKicker : MonoBehaviour
{
    public KickZone kickZone;
    public float passForce = 7f;
    public float shootForce = 12f;
    public float upward = 0.10f;
    public float cooldown = 0.35f;

    private float t;
    private Transform aimRef;

    void Awake() => aimRef = transform;
    void Update() { if (t > 0) t -= Time.deltaTime; }

    public void OnPass()
    {
        Rigidbody ball = GetBallInRange();
        if (!ball) return;
        Kick(ball, passForce);
    }

    public void OnShoot()
    {
        Rigidbody ball = GetBallInRange();
        if (!ball) return;
        Kick(ball, shootForce);
    }

    private Rigidbody GetBallInRange()
    {
        if (t > 0 || kickZone == null) return null;
        return kickZone.currentBall; // Sólo si la pelota está dentro del trigger
    }

    private void Kick(Rigidbody ball, float f)
    {
        Vector3 dir = aimRef.forward + Vector3.up * upward;
        ball.AddForce(dir.normalized * f, ForceMode.VelocityChange);
        t = cooldown;
    }
}
