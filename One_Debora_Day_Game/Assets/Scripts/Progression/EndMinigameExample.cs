using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMinigameExample : MonoBehaviour
{
    public void FinishAndGoToRewards()
    {
        // Otorga recompensa según escena actual
        RewardManager.GrantForScene(SceneManager.GetActiveScene().name);

        // Ir a Rewards
        SceneManager.LoadScene("Rewards");
    }
}
