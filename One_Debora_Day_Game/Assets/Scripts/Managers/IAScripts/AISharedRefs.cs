using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISharedRefs : MonoBehaviour
{
    [Header("Referencias")]
    public Rigidbody ball;
    public Transform localGoalCenter;   // centro arco Local
    public Transform visitGoalCenter;   // centro arco Visit

    // Útil para saber hacia dónde atacar
    public bool thisTeamIsLocal = true;

    public Transform MyAttackingGoal => thisTeamIsLocal ? visitGoalCenter : localGoalCenter;
    public Transform MyDefendingGoal => thisTeamIsLocal ? localGoalCenter : visitGoalCenter;
}