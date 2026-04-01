using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance;

    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip whistleShort;
    public AudioClip whistleLong;
    public AudioClip goalCrowd;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayWhistleShort()
    {
        sfxSource.PlayOneShot(whistleShort);
    }

    public void PlayWhistleLong()
    {
        sfxSource.PlayOneShot(whistleLong);
    }

    public void PlayGoalSound()
    {
        sfxSource.PlayOneShot(goalCrowd);
    }
}
