using System.Collections;
using UnityEngine;

public class PlatformDropVD : MonoBehaviour
{
    [Header("Configura��o")]
    [Tooltip("A camada (Layer) em que as plataformas 'one-way' se encontram.")]
    [SerializeField] private LayerMask platformLayer;

    [Tooltip("Por quanto tempo (em segundos) a colis�o com as plataformas ficar� desativada ao descer.")]
    [SerializeField] private float dropDuration = 0.25f;

    // Refer�ncias e controle
    private CapsuleCollider2D playerCollider; // Assumindo que seu jogador usa um CapsuleCollider2D
    private float verticalInput;

    void Awake()
    {
        playerCollider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        // L� o input vertical
        verticalInput = Input.GetAxisRaw("Vertical");

        // Se o jogador apertar "para baixo" E o bot�o de pulo...
        if (verticalInput < -0.5f && Input.GetButtonDown("Jump"))
        {
            // Inicia a rotina para descer da plataforma
            StartCoroutine(DisableCollisionCoroutine());
        }
    }

    private IEnumerator DisableCollisionCoroutine()
    {
        // Pega o n�mero da camada da plataforma a partir do LayerMask
        int platformLayerIndex = LayerMask.NameToLayer("Plataforma");

        // Se a camada "Plataforma" existir...
        if (platformLayerIndex != -1)
        {
            // Desativa a colis�o entre a camada do jogador e a camada da plataforma
            Physics2D.IgnoreLayerCollision(gameObject.layer, platformLayerIndex, true);

            // Espera pelo tempo definido
            yield return new WaitForSeconds(dropDuration);

            // Reativa a colis�o
            Physics2D.IgnoreLayerCollision(gameObject.layer, platformLayerIndex, false);
        }
        else
        {
            Debug.LogWarning("A camada 'Plataforma' n�o foi encontrada! Verifique as configura��es em 'Tags and Layers'.");
        }
    }
}