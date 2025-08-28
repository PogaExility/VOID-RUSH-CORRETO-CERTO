using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Efeitos Visuais (Opcional)")]
    [Tooltip("Um efeito para tocar quando o checkpoint � ativado.")]
    public GameObject activationEffect;
    [Tooltip("A cor ou sprite para indicar que o checkpoint est� ativo.")]
    public Sprite activeSprite;

    private bool isActivated = false;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se � o jogador e se o checkpoint ainda n�o foi ativado
        if (!isActivated && other.CompareTag("Player"))
        {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;

        // Avisa o RespawnManager sobre a nova posi��o e a cena atual
        if (RespawnManager.Instance != null)
        {
            RespawnManager.Instance.SetNewCheckpoint(transform.position, gameObject.scene.name);
        }

        // --- Feedback Visual ---
        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }
        if (spriteRenderer != null && activeSprite != null)
        {
            spriteRenderer.sprite = activeSprite;
        }

        // Opcional: Desativar este checkpoint para n�o poder ser ativado novamente
        // gameObject.SetActive(false); // ou apenas o collider
        GetComponent<Collider2D>().enabled = false;
    }
}