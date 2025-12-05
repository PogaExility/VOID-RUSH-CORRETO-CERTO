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

    [Header("Combate")] 
    public AudioClip damageSound;

    [Header("Configuração de Volume (Multiplicadores)")]
    [Tooltip("Aumente ou diminua o volume específico de cada som aqui.")]
    [Range(0f, 3f)] public float footstepVolume = 1f;
    [Range(0f, 3f)] public float jumpVolume = 1f;
    [Range(0f, 3f)] public float landVolume = 1f;
    [Range(0f, 3f)] public float dashVolume = 1f;
    [Range(0f, 3f)] public float damageVolume = 1f;

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
        // Verifica o volume global E multiplica pelo volume específico dos passos
        if (AudioManager.Instance != null)
        {
            audioSource.volume = AudioManager.Instance.GetFinalSFXVolume() * footstepVolume;
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

    public void PlayDamageSound()
    {
        if (damageSound != null)
        {
            // PlayOneShot aceita um segundo parâmetro de escala de volume (0 a 1+)
            audioSource.PlayOneShot(damageSound, damageVolume);
        }
    }

    public void PlayJumpSound()
    {
        if (AudioManager.Instance != null && jumpSound != null)
        {
            // Envia o jumpVolume como multiplicador
            AudioManager.Instance.PlaySoundEffect(jumpSound, transform.position, jumpVolume);
        }
    }

    public void PlayLandSound()
    {
        if (AudioManager.Instance != null && landSound != null)
        {
            AudioManager.Instance.PlaySoundEffect(landSound, transform.position, landVolume);
        }
    }

    public void PlayDashSound()
    {
        if (AudioManager.Instance != null && dashSound != null)
        {
            AudioManager.Instance.PlaySoundEffect(dashSound, transform.position, dashVolume);
        }
    }
}