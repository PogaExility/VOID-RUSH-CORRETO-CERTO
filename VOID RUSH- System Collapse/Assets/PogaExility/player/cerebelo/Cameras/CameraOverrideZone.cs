using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class CameraOverrideZone : MonoBehaviour
{
    [Header("Configurações da Zona")]
    [Tooltip("O tamanho (zoom) que a câmera terá ao entrar nesta zona.")]
    [SerializeField] private float targetOrthographicSize = 5f;

    [Tooltip("A duração em segundos que a transição de zoom levará.")]
    [SerializeField] private float transitionDuration = 0.5f;

    private Collider2D zoneCollider;

    // Referência estática para a coroutine de transição.
    // Usaremos a mesma ideia do RoomBoundary para evitar conflitos.
    // NOTA: Para isso funcionar, a variável em RoomBoundary.cs também precisa ser pública.
    // Vamos fazer uma pequena alteração lá depois. Por enquanto, esta é a estrutura.
    // Para simplificar por agora, vamos usar uma coroutine local.
    private Coroutine activeTransitionCoroutine;


    void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
        if (!zoneCollider.isTrigger)
        {
            Debug.LogWarning($"O Collider2D no objeto '{gameObject.name}' precisa estar marcado como 'Is Trigger' para o script CameraOverrideZone funcionar.", gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Jogador entrou na Zona de Override '{gameObject.name}'.");

            // Pega a câmera e o confiner
            CinemachineCamera virtualCamera = FindAnyObjectByType<CinemachineCamera>();
            CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();

            if (virtualCamera == null || confiner == null) return;

            // Para qualquer coroutine de câmera que esteja rodando e para a câmera.
            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);
            // Idealmente, pararíamos a coroutine do RoomBoundary também.

            // Define o colisor DESTA ZONA como o novo limite
            confiner.BoundingShape2D = zoneCollider;
            confiner.InvalidateBoundingShapeCache();

            // Inicia a transição para o zoom DESTA ZONA
            activeTransitionCoroutine = StartCoroutine(TransitionZoom(virtualCamera, targetOrthographicSize, transitionDuration));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Jogador saiu da Zona de Override '{gameObject.name}'. Restaurando câmera da sala principal.");

            // Encontra a sala principal em que o jogador está agora
            RoomBoundary parentRoom = FindParentRoomFor(other.transform);

            if (parentRoom != null)
            {
                // Dispara manualmente um evento OnTriggerEnter2D "falso" no script da sala principal
                // para que ele reaplique suas próprias configurações.
                // Esta é uma maneira inteligente de reutilizar toda a lógica que já escrevemos lá.
                parentRoom.SendMessage("OnTriggerEnter2D", other, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    // Função auxiliar para encontrar a qual RoomBoundary o jogador pertence
    private RoomBoundary FindParentRoomFor(Transform playerTransform)
    {
        RoomBoundary[] allRooms = FindObjectsByType<RoomBoundary>(FindObjectsSortMode.None);
        foreach (var room in allRooms)
        {
            Collider2D roomCollider = room.GetComponent<Collider2D>();
            if (roomCollider.bounds.Contains(playerTransform.position))
            {
                return room; // Encontrou a sala!
            }
        }
        return null; // Não encontrou nenhuma sala (caso raro)
    }

    // Coroutine para a transição de zoom (idêntica à do RoomBoundary)
    private IEnumerator TransitionZoom(CinemachineCamera cam, float targetSize, float duration)
    {
        float startSize = cam.Lens.OrthographicSize;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            cam.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cam.Lens.OrthographicSize = targetSize;
    }
}