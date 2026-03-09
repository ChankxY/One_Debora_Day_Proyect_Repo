using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KickZone : MonoBehaviour
{
    [Tooltip("Tag que debe tener la pelota")]
    public string ballTag = "Ball";

    [HideInInspector] public Rigidbody currentBall; // será no-nulo si la pelota está dentro

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ballTag))
        {
            currentBall = other.attachedRigidbody;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(ballTag))
        {
            if (currentBall != null && other.attachedRigidbody == currentBall)
                currentBall = null;
        }
    }
}