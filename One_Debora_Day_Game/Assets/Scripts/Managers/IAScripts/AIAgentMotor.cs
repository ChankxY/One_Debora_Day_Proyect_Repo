using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AIAgentMotor : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 2.0f;
    public float accel = 5.0f;
    public float stopDistance = 0.35f;
    public float turnSpeedDeg = 320f;

    private Rigidbody rb;
    private Vector3 desiredVel;
    private Vector3 velocity;
    private Vector3? overrideTarget; // objetivo temporal

    void Awake() => rb = GetComponent<Rigidbody>();

    public void SetTarget(Vector3 worldPoint) => overrideTarget = worldPoint;
    public void ClearTarget() => overrideTarget = null;

    void FixedUpdate()
    {
        
rb.linearDamping = 3f;            // suaviza frenado
rb.angularDamping = 8f;     // suaviza giros bruscos

        if (overrideTarget.HasValue)
        {
            Vector3 to = overrideTarget.Value - transform.position;
            to.y = 0f;

            if (to.magnitude > stopDistance)
            {
                desiredVel = to.normalized * speed;
                velocity = Vector3.MoveTowards(velocity, desiredVel, accel * Time.fixedDeltaTime);

                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

                // Girar hacia la dirección de avance
                if (velocity.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);
                    rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, turnSpeedDeg * Time.fixedDeltaTime));
                }
            }
            else
            {
                // Freno suave al llegar
                velocity = Vector3.MoveTowards(velocity, Vector3.zero, accel * Time.fixedDeltaTime);
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Sin target → freno pasivo
            velocity = Vector3.MoveTowards(velocity, Vector3.zero, accel * Time.fixedDeltaTime);
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
        }
    }
}