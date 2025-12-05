using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerSounds : MonoBehaviour
{
    // --- AUDIO SOURCE EXTRA PARA LOOP ---
    // Precisamos de um segundo canal de áudio para o WallSlide não cortar os passos/efeitos
    private AudioSource loopingAudioSource;
    private AudioSource sfxAudioSource; // O principal

    [Header("Passos (Loop)")]
    public List<AudioClip> footstepSounds;
    [Range(0f, 100f)] public float footstepVolume = 1f; // Alterado para 100

    [Header("Movimento Básico")]
    public AudioClip jumpSound;
    [Range(0f, 1000f)] public float jumpVolume = 1f; // Alterado para 100

    public AudioClip landSound;
    [Range(0f, 100f)] public float landVolume = 1f; // Alterado para 100

    public AudioClip dashSound;
    [Range(0f, 100f)] public float dashVolume = 1f; // Alterado para 100

    [Header("Combate & Dano")]
    public AudioClip damageSound;
    [Range(0f, 100f)] public float damageVolume = 1f; // Alterado para 100

    [Header("Habilidades de Parede")]
    public AudioClip wallJumpSound;
    [Range(0f, 1000f)] public float wallJumpVolume = 1f; // Alterado para 100

    public AudioClip wallDashSound;
    [Range(0f, 100f)] public float wallDashVolume = 1f; // Alterado para 100

    [Tooltip("Som contínuo de deslizar na parede.")]
    public AudioClip wallSlideSound;
    [Range(0f, 100f)] public float wallSlideVolume = 1f; // Alterado para 100

    [Header("Habilidades Combo (Múltiplos Sons)")]
    [Tooltip("Adicione vários sons aqui. O jogo escolherá um aleatório.")]
    public List<AudioClip> dashJumpSounds;
    [Range(0f, 100f)] public float dashJumpVolume = 1f; // Alterado para 100

    [Tooltip("Adicione vários sons aqui. O jogo escolherá um aleatório.")]
    public List<AudioClip> wallDashJumpSounds;
    [Range(0f, 100f)] public float wallDashJumpVolume = 1f; // Alterado para 100


    void Awake()
    {
        // Configura o AudioSource principal (Efeitos OneShot + Passos)
        sfxAudioSource = GetComponent<AudioSource>();
        sfxAudioSource.loop = false;
        sfxAudioSource.playOnAwake = false;

        // Cria um AudioSource extra via código para sons contínuos (Wall Slide)
        // para que deslizar na parede não impeça de ouvir outros sons.
        loopingAudioSource = gameObject.AddComponent<AudioSource>();
        loopingAudioSource.loop = true;
        loopingAudioSource.playOnAwake = false;
        loopingAudioSource.spatialBlend = sfxAudioSource.spatialBlend; // Copia config 2D/3D
    }

    void Update()
    {
        // --- SINCRONIA DE VOLUME GLOBAL ---
        if (AudioManager.Instance != null)
        {
            // O volume base muda conforme o AudioManager, mas aplicamos os multiplicadores locais
            float globalSfxVol = AudioManager.Instance.GetFinalSFXVolume();

            // Atualiza volumes dinamicamente (para funcionar no slider do inspector em tempo real)
            // Nota: Para OneShot, o volume é passado no momento do Play.
            // Para Loops (Passos/Slide), precisamos atualizar o .volume aqui.

            if (sfxAudioSource.isPlaying && sfxAudioSource.clip != null && footstepSounds.Contains(sfxAudioSource.clip))
            {
                sfxAudioSource.volume = globalSfxVol * footstepVolume;
            }

            if (loopingAudioSource.isPlaying)
            {
                loopingAudioSource.volume = globalSfxVol * wallSlideVolume;
            }
        }
    }

    // --- FUNÇÕES DE CONTROLE DE LOOP ---

    public void UpdateWalkingSound(bool isWalking)
    {
        // Se estiver deslizando na parede, a gente prioriza o som da parede no canal de loop,
        // mas aqui usamos o sfxAudioSource para passos.

        if (isWalking)
        {
            if (!sfxAudioSource.isPlaying)
            {
                if (footstepSounds.Count > 0)
                {
                    sfxAudioSource.clip = footstepSounds[Random.Range(0, footstepSounds.Count)];
                    sfxAudioSource.loop = true; // Passos são loop enquanto anda
                    sfxAudioSource.Play();
                }
            }
        }
        else
        {
            // Só para se o que estiver tocando for um som de passo
            if (sfxAudioSource.isPlaying && footstepSounds.Contains(sfxAudioSource.clip))
            {
                sfxAudioSource.Stop();
                sfxAudioSource.loop = false;
            }
        }
    }

    public void UpdateWallSlideSound(bool isSliding)
    {
        if (wallSlideSound == null) return;

        if (isSliding)
        {
            if (!loopingAudioSource.isPlaying)
            {
                loopingAudioSource.clip = wallSlideSound;
                loopingAudioSource.Play();
            }
        }
        else
        {
            if (loopingAudioSource.isPlaying && loopingAudioSource.clip == wallSlideSound)
            {
                loopingAudioSource.Stop();
            }
        }
    }

    // --- FUNÇÕES DE EFEITOS SONOROS (ONE SHOT) ---

    public void PlayDamageSound()
    {
        PlayClip(damageSound, damageVolume);
    }

    public void PlayJumpSound()
    {
        PlayClip(jumpSound, jumpVolume);
    }

    public void PlayLandSound()
    {
        PlayClip(landSound, landVolume);
    }

    public void PlayDashSound()
    {
        PlayClip(dashSound, dashVolume);
    }

    public void PlayWallJumpSound()
    {
        PlayClip(wallJumpSound, wallJumpVolume);
    }

    public void PlayWallDashSound()
    {
        PlayClip(wallDashSound, wallDashVolume);
    }

    // --- FUNÇÕES PARA OS COMBOS (LISTAS) ---

    public void PlayDashJumpSound()
    {
        PlayRandomClip(dashJumpSounds, dashJumpVolume);
    }

    public void PlayWallDashJumpSound()
    {
        PlayRandomClip(wallDashJumpSounds, wallDashJumpVolume);
    }

    // --- HELPERS PRIVADOS ---

    private void PlayClip(AudioClip clip, float volumeScale)
    {
        if (clip != null)
        {
            // PlayOneShot permite tocar sons uns cima dos outros no mesmo AudioSource
            // sem cortar o som de passos (se estiver rodando).
            sfxAudioSource.PlayOneShot(clip, volumeScale);
        }
    }

    private void PlayRandomClip(List<AudioClip> clips, float volumeScale)
    {
        if (clips != null && clips.Count > 0)
        {
            AudioClip randomClip = clips[Random.Range(0, clips.Count)];
            sfxAudioSource.PlayOneShot(randomClip, volumeScale);
        }
    }
}