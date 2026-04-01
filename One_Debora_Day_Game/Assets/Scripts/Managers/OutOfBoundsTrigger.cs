using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class OutOfBoundsTrigger : MonoBehaviour
{
    public OutType outType;
    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Ball")) return;

        triggered = true;

        Vector3 exitPoint = other.transform.position;
        MatchManager.Instance.OnBallOut(outType, exitPoint);
    }

    // ✅ ESTE MÉTODO ES LA CLAVE
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Ball")) return;

        // Cuando el balón sale COMPLETAMENTE del trigger,
        // el trigger queda listo para volver a detectar
        triggered = false;
    }
}

    
/*
public class OutOfBoundsTrigger : MonoBehaviour
{
    public OutType outType;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Ball")) return;

        triggered = true;

        Vector3 exitPoint = other.transform.position;

        // Desactivar este trigger
        GetComponent<Collider>().enabled = false;

        MatchManager.Instance.OnBallOut(outType, exitPoint);

        // Reactivarlo luego
        Invoke(nameof(ResetTrigger), 0.2f);
    }

    private void ResetTrigger()
    {
        triggered = false;
        GetComponent<Collider>().enabled = true;
    }
}
*/
