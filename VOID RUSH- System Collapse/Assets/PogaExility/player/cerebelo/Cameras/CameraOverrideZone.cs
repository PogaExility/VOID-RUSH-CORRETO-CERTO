using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class CameraOverrideZone : MonoBehaviour
{
    [Header("Configurações da Zona")]
    [SerializeField] private float targetOrthographicSize = 5f;
    [SerializeField] private float transitionDuration = 0.5f;

    private Collider2D zoneCollider;
    private Coroutine activeTransitionCoroutine;

    void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();
        if (!zoneCollider.isTrigger) Debug.LogWarning("CameraOverrideZone requer Is Trigger.", gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CinemachineCamera virtualCamera = FindAnyObjectByType<CinemachineCamera>();
            CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();

            if (virtualCamera == null || confiner == null) return;

            if (activeTransitionCoroutine != null) StopCoroutine(activeTransitionCoroutine);

            // Override temporário
            confiner.BoundingShape2D = zoneCollider;
            confiner.InvalidateBoundingShapeCache();

            activeTransitionCoroutine = StartCoroutine(TransitionZoom(virtualCamera, targetOrthographicSize, transitionDuration));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Procura a sala "mãe" onde o player está agora
            RoomBoundary parentRoom = FindParentRoomFor(other.transform);

            if (parentRoom != null)
            {
                // Chama ActivateRoom diretamente para restaurar o estado correto
                parentRoom.ActivateRoom(other);
            }
        }
    }

    private RoomBoundary FindParentRoomFor(Transform playerTransform)
    {
        RoomBoundary[] allRooms = FindObjectsByType<RoomBoundary>(FindObjectsSortMode.None);
        foreach (var room in allRooms)
        {
            Collider2D roomCollider = room.GetComponent<Collider2D>();
            // Verifica qual sala contém o player
            if (roomCollider.bounds.Contains(playerTransform.position))
            {
                return room;
            }
        }
        return null;
    }

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