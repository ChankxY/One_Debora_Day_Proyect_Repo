using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchClock : MonoBehaviour
{
    public float durationSec = 180f;
    public TextMeshProUGUI timeText;

    private float t;
    private bool finished;

    void OnEnable()
    {
        ResetClock();
    }

    public void ResetClock()
    {
        t = durationSec;
        finished = false;
        UpdateUI();
    }

    void Update()
    {
        if (finished) return;

        t -= Time.deltaTime;
        if (t < 0) t = 0;

        UpdateUI();

        if (t <= 0f)
        {
            finished = true;

            if (MatchManager.Instance != null)
                MatchManager.Instance.EndMatch();
        }
    }

    private void UpdateUI()
    {
        if (!timeText) return;

        int m = (int)(t / 60f);
        int s = (int)(t % 60f);
        timeText.text = $"{m:00}:{s:00}";
    }
}