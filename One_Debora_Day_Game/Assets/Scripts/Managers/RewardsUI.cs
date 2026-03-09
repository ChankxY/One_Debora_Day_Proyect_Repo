using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class RewardsUI : MonoBehaviour
{
    [Header("UI (TextMeshPro)")]
    public TextMeshProUGUI titleText;     // Ej: “¡Recompensa obtenida!”
    public TextMeshProUGUI detailText;    // Lista de piezas obtenidas

    void Start()
    {
        // Lee ‘reward_shirt’ (1=obtenida, 0=no)
        bool hasShirt = PlayerPrefs.GetInt("reward_shirt", 0) == 1;

        if (titleText)
            titleText.text = hasShirt ? "¡Recompensa obtenida!" : "Sin recompensa";

        if (detailText)
        {
            if (hasShirt)
                detailText.text = "Has obtenido: SacoDAP";
            else
                detailText.text = "No obtuviste recompensas esta vez.";
        }
    }
}
