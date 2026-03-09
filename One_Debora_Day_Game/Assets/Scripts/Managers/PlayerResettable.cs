using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResettable : MonoBehaviour
{
    [Tooltip("SpawnPoint que define la posición/rotación inicial de ESTE jugador")]
    public Transform spawnPoint;

    private Rigidbody rb;
    private CharacterController cc;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    
public void ResetToSpawn()
{
    // 1) Apagar movimiento
    var mover = GetComponent<PlayerMovement>(); // o tu script real
    if (mover != null) mover.movementEnabled = false;

    // 2) Apagar CharacterController
    bool ccEnabled = false;
    if (cc != null) { ccEnabled = cc.enabled; cc.enabled = false; }

    // 3) Teletransportar
    transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

    if (rb != null)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
    }

    // 4) Volver a encender CharacterController
    if (cc != null) cc.enabled = ccEnabled;

    // 5) Reactivar movimiento un frame después (importante)
    StartCoroutine(ReenableMovementNextFrame());
}

private IEnumerator ReenableMovementNextFrame()
{
    yield return null; // espera un frame
    var mover = GetComponent<PlayerMovement>();
    if (mover != null) mover.movementEnabled = true;
}

}