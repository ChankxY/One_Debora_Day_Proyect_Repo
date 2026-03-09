using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class RewardsUIP : MonoBehaviour
{
    public TextMeshProUGUI titleText;   // "¡Recompensa obtenida!" o similar
    public TextMeshProUGUI detailText;  // Lista de piezas

    void Start()
    {
        bool hasShirt = RewardManager.HasShirt();
        bool hasPants = RewardManager.HasPants();
        bool hasShoes = RewardManager.HasShoes();

        // Título
        if (titleText)
            titleText.text = (hasShirt || hasPants || hasShoes)
                ? "¡Recompensa obtenida!"
                : "Sin recompensa";

        // Detalle
        if (detailText)
        {
            var sb = new StringBuilder();
            if (hasShirt) sb.AppendLine("• Camiseta (Fútbol)");
            if (hasPants) sb.AppendLine("• Pantalón (Rítmico)");
            if (hasShoes) sb.AppendLine("• Zapatos (Bolos)");

            detailText.text = sb.Length > 0
                ? "Has obtenido:\n" + sb.ToString()
                : "No has obtenido nuevas recompensas.";
        }
    }
}