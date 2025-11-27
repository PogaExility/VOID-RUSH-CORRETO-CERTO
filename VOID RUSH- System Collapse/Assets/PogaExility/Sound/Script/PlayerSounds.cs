using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerSounds : MonoBehaviour
{
    [Header("Passos")]
    [Tooltip("Coloque aqui UM som de passos (se for loop) ou deixe o código escolher.")]
    public List<AudioClip> footstepSounds;

    [Header("Ações de Movimento")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip dashSound;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        // --- SINCRONIA DE VOLUME ---
        // Verifica qual o volume global configurado no AudioManager e aplica neste AudioSource
        if (AudioManager.Instance != null)
        {
            audioSource.volume = AudioManager.Instance.GetFinalSFXVolume();
        }
    }

    public void UpdateWalkingSound(bool isWalking)
    {
        if (isWalking)
        {
            if (!audioSource.isPlaying)
            {
                // Escolhe um som e dá Play
                if (footstepSounds.Count > 0)
                {
                    // Se quiser variar o som toda vez que começa a andar:
                    audioSource.clip = footstepSounds[Random.Range(0, footstepSounds.Count)];
                    audioSource.Play();
                }
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    public AudioClip GetRandomFootstep()
    {
        if (footstepSounds == null || footstepSounds.Count == 0) return null;
        return footstepSounds[Random.Range(0, footstepSounds.Count)];
    }
}