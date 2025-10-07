using UnityEngine;
using Unity.Cinemachine; // Mantendo o namespace correto para a versão nova


// Garante que os componentes necessários estejam no mesmo GameObject.
// Atualizado para usar a nova classe 'CinemachineCamera'.
[RequireComponent(typeof(CinemachineCamera), typeof(CinemachineConfiner2D))]
public class CinemachineTargetSetter : MonoBehaviour
{
    // Define as tags que o script procurará na cena.
    // Deixar como público permite que você mude as tags pelo Inspector do Unity se precisar.
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string cameraBoundaryTag = "CameraBoundary";

    void Start()
    {
        // Pega as referências dos componentes que estão neste mesmo GameObject.
        // Atualizado para usar a nova classe 'CinemachineCamera'.
        CinemachineCamera virtualCamera = GetComponent<CinemachineCamera>();
        CinemachineConfiner2D confiner = GetComponent<CinemachineConfiner2D>();

        // --- Configuração do Alvo (Follow) ---

        // Procura pelo GameObject do jogador na cena usando a tag definida.
        GameObject playerTarget = GameObject.FindGameObjectWithTag(playerTag);

        // Verifica se o jogador foi encontrado.
        if (playerTarget != null)
        {
            // Se encontrou, atribui o Transform do jogador ao campo "Follow" da câmera virtual.
            // Acessando a propriedade pública 'Follow' (sem o 'm_').
            virtualCamera.Follow = playerTarget.transform;
            Debug.Log($"Cinemachine: Alvo '{playerTarget.name}' definido como Follow.");
        }
        else
        {
            // Se não encontrou, exibe um erro claro no console para facilitar a depuração.
            Debug.LogError($"Cinemachine ERROR: Não foi possível encontrar um GameObject com a tag '{playerTag}' na cena.");
        }

        // --- Configuração dos Limites (Confiner) ---

        // Procura pelo GameObject que contém o colisor dos limites da câmera.
        GameObject cameraBoundary = GameObject.FindGameObjectWithTag(cameraBoundaryTag);

        // Verifica se o objeto de limites foi encontrado.
        if (cameraBoundary != null)
        {
            // Tenta pegar o componente Collider2D (pode ser BoxCollider2D, PolygonCollider2D, etc.).
            Collider2D boundaryShape = cameraBoundary.GetComponent<Collider2D>();

            if (boundaryShape != null)
            {
                // Se encontrou o colisor, atribui ao campo "Bounding Shape 2D" do Confiner.
                // Acessando a propriedade pública 'BoundingShape2D' (sem o 'm_').
                confiner.BoundingShape2D = boundaryShape;

                // Força o confiner a recalcular seus limites com base na nova forma.
                // Usando o nome de método atualizado 'InvalidateBoundingShapeCache'.
                confiner.InvalidateBoundingShapeCache();

                Debug.Log($"Cinemachine: Limite '{cameraBoundary.name}' definido no Confiner.");
            }
            else
            {
                // Se o objeto foi encontrado mas não tem um Collider2D, avisa o erro.
                Debug.LogError($"Cinemachine ERROR: O GameObject '{cameraBoundary.name}' com a tag '{cameraBoundaryTag}' não possui um componente Collider2D.");
            }
        }
        else
        {
            // Se não encontrou o objeto de limites, exibe o erro.
            Debug.LogError($"Cinemachine ERROR: Não foi possível encontrar um GameObject com a tag '{cameraBoundaryTag}' na cena.");
        }
    }
}