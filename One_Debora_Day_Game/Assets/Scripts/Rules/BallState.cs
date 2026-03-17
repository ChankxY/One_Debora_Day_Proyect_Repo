using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BallState : MonoBehaviour
{
    public static event Action<Team> OnLastTouchChanged;
    public Team LastTouchTeam { get; private set; } = Team.Local; // default

    public void SetLastTouch(Team team)
    {
        if (LastTouchTeam == team) return;
        LastTouchTeam = team;
        OnLastTouchChanged?.Invoke(team);
    }
}