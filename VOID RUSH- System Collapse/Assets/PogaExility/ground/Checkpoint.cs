using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Efeitos Visuais")]
    [Tooltip("O sprite ou cor quando o checkpoint est� ATIVO.")]
    public Sprite activeSprite;
    [Tooltip("O sprite ou cor quando o checkpoint est� INATIVO (mas pode ser ativado).")]
    public Sprite inactiveSprite;
    [Tooltip("O sistema de part�culas que toca ao ativar.")]
    public ParticleSystem activationParticles;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // --- CORRE��O IMPORTANTE ---
        // Garantimos que o GameObject das part�culas esteja ATIVO,
        // mas paramos a EMISS�O de part�culas.
        if (activationParticles != null)
        {
            // Garante que o objeto filho esteja ativo na hierarquia
            activationParticles.gameObject.SetActive(true);

            // Para a emiss�o e limpa quaisquer part�culas que possam ter sobrado
            activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // Come�a no estado inativo
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

        // Agora, como o GameObject est� ativo, esta linha vai funcionar perfeitamente.
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

        // Quando desativado, tamb�m paramos a emiss�o das part�culas.
        if (activationParticles != null)
        {
            activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        GetComponent<Collider2D>().enabled = true;
    }
}