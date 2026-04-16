using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricCamera : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] Transform target;        // el jugador que la cámara seguirá

    [Header("Posición y ángulo")]
    [SerializeField] Vector3 offset = new Vector3(-8f, 10f, -8f); // distancia isométrica
    [SerializeField] float smoothSpeed = 5f;  // velocidad de interpolación (seguimiento suave)
    [SerializeField] bool lookAtTarget = true; // si la cámara debe mirar al jugador

    [Header("Rotación fija")]
    [SerializeField] Vector3 rotationEuler = new Vector3(35f, 45f, 0f); // ángulo isométrico clásico

    void Start()
    {
        if (target == null)
        {
            // busca el jugador automáticamente si no se asigna
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        // aplica rotación inicial
        transform.rotation = Quaternion.Euler(rotationEuler);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // posición deseada = posición del jugador + offset
        Vector3 desiredPosition = target.position + offset;

        // interpolación suave hacia la posición deseada
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // opcional: hacer que mire al jugador
        if (lookAtTarget)
        {
            transform.LookAt(target.position + Vector3.up * 1.5f); // mirar un poco encima del centro
        }
    }

    // herramienta visual en el editor
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(target.position, 0.3f);
    }
}