using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class GoalTrigger : MonoBehaviour
{
    public string team; // "Local" o "Visit"
    public TextMeshProUGUI scoreText;

    private static int local = 0, visit = 0;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;

        if (team == "Local") local++;
        else visit++;

        if (scoreText)scoreText.text = $"{local} - {visit}";
    }
}
