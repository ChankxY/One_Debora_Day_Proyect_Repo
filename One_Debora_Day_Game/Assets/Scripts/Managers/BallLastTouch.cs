using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BallLastTouch : MonoBehaviour
{
    public TeamSide lastTouch = TeamSide.Local;

    public void SetLastTouch(TeamSide team)
    {
        lastTouch = team;
    }
}