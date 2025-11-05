// File: AudioManager.cs
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("AudioSource para música de fundo.")]
    [SerializeField] private AudioSource musicSource;
    [Tooltip("Prefab de um AudioSource para efeitos sonoros. Será instanciado e destruído.")]
    [SerializeField] private AudioSource sfxSourcePrefab; // Um prefab simples com um AudioSource

    [Header("Volume Settings (0 to 1)")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    private const int MAX_SFX_SOURCES = 10; // Limite para evitar sobrecarga

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Para persistir entre cenas
            UpdateMusicVolume();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Limpa AudioSources de SFX que terminaram de tocar
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            if (activeSfxSources[i] == null || !activeSfxSources[i].isPlaying)
            {
                if (activeSfxSources[i] != null) Destroy(activeSfxSources[i].gameObject);
                activeSfxSources.RemoveAt(i);
            }
        }
    }

    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource == null || musicClip == null)
        {
            Debug.LogWarning("MusicSource ou musicClip não atribuído no AudioManager.");
            return;
        }
        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.Play();
        UpdateMusicVolume();
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume();
    }

    private void UpdateMusicVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }

    public void PlaySoundEffect(AudioClip sfxClip, Vector3? position = null, float volumeMultiplier = 1f)
    {
        if (sfxSourcePrefab == null || sfxClip == null || activeSfxSources.Count >= MAX_SFX_SOURCES)
        {
            if (sfxSourcePrefab == null) Debug.LogWarning("SFX Source Prefab não atribuído no AudioManager.");
            if (sfxClip == null) Debug.LogWarning("SFX Clip nulo passado para PlaySoundEffect.");
            return;
        }

        AudioSource sourceInstance = Instantiate(sfxSourcePrefab);
        if (position.HasValue)
        {
            sourceInstance.transform.position = position.Value;
            sourceInstance.spatialBlend = 1.0f; // Som 3D
        }
        else
        {
            sourceInstance.spatialBlend = 0.0f; // Som 2D
        }

        sourceInstance.clip = sfxClip;
        sourceInstance.volume = sfxVolume * masterVolume * volumeMultiplier;
        sourceInstance.Play();
        activeSfxSources.Add(sourceInstance);
        // O Update limpará esta instância quando terminar
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        // Volumes de SFX já tocando não serão alterados, apenas os novos.
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateMusicVolume(); // Atualiza o volume da música que está tocando
        // SFX em reprodução não são afetados dinamicamente aqui, mas novos SFX usarão o novo masterVolume.
    }
}