using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Efeitos Visuais")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;
    public ParticleSystem activationParticles;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (activationParticles != null)
        {
            activationParticles.gameObject.SetActive(true);
            activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Deactivate(); // Garante que comece no estado inativo
    }

    public void Interact()
    {
        // A única responsabilidade é se anunciar para o manager.
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.SetNewCheckpoint(this); // Passa a si mesmo como referência
        }
    }

    // Chamado pelo RespawnManager para LIGAR este checkpoint
    public void Activate()
    {
        if (spriteRenderer != null && activeSprite != null)
        {
            spriteRenderer.sprite = activeSprite;
        }
        if (activationParticles != null)
        {
            activationParticles.Play();
        }
        GetComponent<Collider2D>().enabled = false; // Desativa para não interagir de novo
    }

    // Chamado pelo RespawnManager para DESLIGAR este checkpoint
    public void Deactivate()
    {
        if (spriteRenderer != null && inactiveSprite != null)
        {
            spriteRenderer.sprite = inactiveSprite;
        }
        if (activationParticles != null)
        {
            activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        GetComponent<Collider2D>().enabled = true; // Reativa para poder ser usado de novo
    }
}