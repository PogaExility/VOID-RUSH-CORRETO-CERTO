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
        // Garante que o colisor seja um gatilho para a detecção automática.
        GetComponent<Collider2D>().isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (activationParticles != null)
        {
            activationParticles.gameObject.SetActive(true);
            activationParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Deactivate(); // Garante que comece no estado inativo
    }

    // --- NOVA FUNÇÃO PARA DETECÇÃO AUTOMÁTICA ---
    // Esta função é chamada automaticamente pela Unity quando um objeto entra no gatilho.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se quem entrou foi o jogador.
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jogador ativou o checkpoint automaticamente.");

            // Chama a função interna para se registrar no RespawnManager.
            RegisterCheckpoint();
        }
    }

    // A função de interação manual foi renomeada para ser mais clara.
    // Ela ainda existe caso você queira ter checkpoints manuais e automáticos.
    private void RegisterCheckpoint()
    {
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.SetNewCheckpoint(this);
        }
    }

    // --- A FUNÇÃO Interact() PODE SER REMOVIDA OU MANTIDA ---
    // Eu vou mantê-la aqui comentada. Se você não tem mais nenhum sistema
    // de interação que a chame, você pode apagá-la para limpar o código.
    /*
    public void Interact()
    {
        RegisterCheckpoint();
    }
    */

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
        // Desativa o colisor para que o OnTriggerEnter2D não seja chamado repetidamente.
        GetComponent<Collider2D>().enabled = false;
    }

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
        // Reativa o colisor para que um checkpoint antigo possa ser reativado
        // caso o jogador volte no nível.
        GetComponent<Collider2D>().enabled = true;
    }
}