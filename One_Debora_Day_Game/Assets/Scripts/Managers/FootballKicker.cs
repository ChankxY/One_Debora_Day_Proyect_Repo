using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FootballKicker : MonoBehaviour
{
    public Rigidbody ball;
    public float passForce = 7f;
    public float shootForce = 12f;
    public float upward = 0.10f;
    public float cooldown = 0.35f;

    private float t;
    private Transform aimRef;

    void Awake() { aimRef = transform; }
    void Update() { if (t > 0) t -= Time.deltaTime; }

    public void OnPass()
    {
        if (t > 0 || !ball) return;
        Kick(passForce);
    }

    public void OnShoot()
    {
        if (t > 0 || !ball) return;
        Kick(shootForce);
    }

    void Kick(float f)
    {
        // Dirección: suave elevación para “globo” mínimo
        Vector3 dir = aimRef.forward + Vector3.up * upward;
        ball.AddForce(dir.normalized * f, ForceMode.VelocityChange);
        t = cooldown;
    }
}
