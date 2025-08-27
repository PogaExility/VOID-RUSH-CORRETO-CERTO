using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Efeitos Visuais")]
    [Tooltip("O sprite ou cor quando o checkpoint está ATIVO.")]
    public Sprite activeSprite;
    [Tooltip("O sprite ou cor quando o checkpoint está INATIVO (mas pode ser ativado).")]
    public Sprite inactiveSprite;
    [Tooltip("O sistema de partículas que toca ao ativar.")]
    public ParticleSystem activationParticles;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // --- CORREÇÃO IMPORTANTE ---
        // Garantimos que o GameObject das partículas esteja ATIVO,
        // mas paramos a EMISSÃO de partículas.
        if (activationParticles != null)
        {
            // Garante que o objeto filho esteja ativo na hierarquia
            activationParticles.gameObject.SetActive(true);

            // Para a emissão e limpa quaisquer partículas que possam ter sobrado
            activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Começa no estado inativo
        Deactivate();
    }

    public void Interact()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.SetNewCheckpoint(this);
        }
    }

    // Chamado pelo RespawnManager para LIGAR este checkpoint
    public void Activate()
    {
        if (spriteRenderer != null && activeSprite != null)
        {
            spriteRenderer.sprite = activeSprite;
        }

        // Agora, como o GameObject está ativo, esta linha vai funcionar perfeitamente.
        if (activationParticles != null)
        {
            activationParticles.Play();
        }

        GetComponent<Collider2D>().enabled = false;
    }

    // Chamado pelo RespawnManager para DESLIGAR este checkpoint
    public void Deactivate()
    {
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }

        // Quando desativado, também paramos a emissão das partículas.
        if (activationParticles != null)
        {
            activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        GetComponent<Collider2D>().enabled = true;
    }
}