using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MatchClock : MonoBehaviour
{
public float durationSec = 180f;
public TextMeshProUGUI timeText; 
public bool goToRewardsOnEnd = true;

float t;

    void OnEnable() => t = durationSec;

    void Update()
    {
        t -= Time.deltaTime;
        if(t < 0) t = 0;
        UpdateUI();
if (t <= 0f)
        {
            if (goToRewardsOnEnd)
            SceneManager.LoadScene("Rewards");
            enabled = false;
        }
    }
    void UpdateUI()
    {
        if (!timeText) return;
        int m = (int)(t /60f);
        int s = (int)(t % 60f);
        timeText.text = $"{m:00}:{s:00}";
    }
}
