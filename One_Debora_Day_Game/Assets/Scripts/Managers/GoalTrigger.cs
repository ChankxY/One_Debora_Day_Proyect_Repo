using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class GoalTrigger : MonoBehaviour
{
    [Tooltip("Si la pelota entra aquí, ¿suma gol al local? (true) o al visitante? (false)")]
    public bool goalForLocal = false;

    [Tooltip("Tag que debe tener la pelota")]
    public string ballTag = "Ball";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(ballTag)) return;

        if (MatchManager.Instance == null)
        {
            Debug.LogError("[GoalTrigger] No hay MatchManager en la escena.");
            return;
        }

        if (goalForLocal) MatchManager.Instance.AddGoalLocal();
        else MatchManager.Instance.AddGoalVisit();
    }
}
