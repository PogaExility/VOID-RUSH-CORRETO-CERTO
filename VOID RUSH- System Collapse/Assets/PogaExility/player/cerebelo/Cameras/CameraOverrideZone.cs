using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class CameraOverrideZone : MonoBehaviour
{
    [Header("Configura��es da Zona")]
    [Tooltip("O tamanho (zoom) que a c�mera ter� ao entrar nesta zona.")]
    [SerializeField] private float targetOrthographicSize = 5f;

    [Tooltip("A dura��o em segundos que a transi��o de zoom levar�.")]
    [SerializeField] private float transitionDuration = 0.5f;

    private Collider2D zoneCollider;

    // Refer�ncia est�tica para a coroutine de transi��o.
    // Usaremos a mesma ideia do RoomBoundary para evitar conflitos.
    // NOTA: Para isso funcionar, a vari�vel em RoomBoundary.cs tamb�m precisa ser p�blica.
    // Vamos fazer uma pequena altera��o l� depois. Por enquanto, esta � a estrutura.
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

            // Pega a c�mera e o confiner
            CinemachineCamera virtualCamera = FindAnyObjectByType<CinemachineCamera>();
            CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();

            if (virtualCamera == null || confiner == null) return;

            // Para qualquer coroutine de c�mera que esteja rodando e para a c�mera.
            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);
            // Idealmente, parar�amos a coroutine do RoomBoundary tamb�m.

            // Define o colisor DESTA ZONA como o novo limite
            confiner.BoundingShape2D = zoneCollider;
            confiner.InvalidateBoundingShapeCache();

            // Inicia a transi��o para o zoom DESTA ZONA
            activeTransitionCoroutine = StartCoroutine(TransitionZoom(virtualCamera, targetOrthographicSize, transitionDuration));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Jogador saiu da Zona de Override '{gameObject.name}'. Restaurando c�mera da sala principal.");

            // Encontra a sala principal em que o jogador est� agora
            RoomBoundary parentRoom = FindParentRoomFor(other.transform);

            if (parentRoom != null)
            {
                // Dispara manualmente um evento OnTriggerEnter2D "falso" no script da sala principal
                // para que ele reaplique suas pr�prias configura��es.
                // Esta � uma maneira inteligente de reutilizar toda a l�gica que j� escrevemos l�.
                parentRoom.SendMessage("OnTriggerEnter2D", other, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    // Fun��o auxiliar para encontrar a qual RoomBoundary o jogador pertence
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
        return null; // N�o encontrou nenhuma sala (caso raro)
    }

    // Coroutine para a transi��o de zoom (id�ntica � do RoomBoundary)
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