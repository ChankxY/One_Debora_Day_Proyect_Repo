using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RewardManager
{
    // Llaves centralizadas para PlayerPrefs
    public const string KEY_SHIRT   = "reward_shirt";
    public const string KEY_PANTS   = "reward_pants";
    public const string KEY_SHOES   = "reward_shoes";

    /// <summary>
    /// Marca recompensa según el nombre de la escena.
    /// Minijuego_Futbol  → camiseta
    /// Minijuego_Ritmico → pantalon
    /// Minijuego_Bolos   → zapatos
    /// </summary>
    public static void GrantForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Minijuego_Futbol":
                SetOnce(KEY_SHIRT);
                break;

            case "Minijuego_Ritmico":
                SetOnce(KEY_PANTS);
                break;

            case "Minijuego_Bolos":
                SetOnce(KEY_SHOES);
                break;

            default:
                Debug.LogWarning($"[RewardManager] Escena '{sceneName}' no tiene recompensa asignada.");
                break;
        }
    }

    /// <summary>
    /// Marca una recompensa si aún no estaba marcada. (0 → 1)
    /// </summary>
    private static void SetOnce(string key)
    {
        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"[RewardManager] Recompensa '{key}' otorgada.");
        }
        else
        {
            Debug.Log($"[RewardManager] Recompensa '{key}' ya estaba obtenida.");
        }
    }

    // Métodos de lectura (por comodidad)
    public static bool HasShirt() => PlayerPrefs.GetInt(KEY_SHIRT, 0) == 1;
    public static bool HasPants() => PlayerPrefs.GetInt(KEY_PANTS, 0) == 1;
    public static bool HasShoes() => PlayerPrefs.GetInt(KEY_SHOES, 0) == 1;
}