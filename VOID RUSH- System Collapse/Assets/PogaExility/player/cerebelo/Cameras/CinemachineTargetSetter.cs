using UnityEngine;
using Unity.Cinemachine; // Mantendo o namespace correto para a vers�o nova


// Garante que os componentes necess�rios estejam no mesmo GameObject.
// Atualizado para usar a nova classe 'CinemachineCamera'.
[RequireComponent(typeof(CinemachineCamera), typeof(CinemachineConfiner2D))]
public class CinemachineTargetSetter : MonoBehaviour
{
    // Define as tags que o script procurar� na cena.
    // Deixar como p�blico permite que voc� mude as tags pelo Inspector do Unity se precisar.
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string cameraBoundaryTag = "CameraBoundary";

    void Start()
    {
        // Pega as refer�ncias dos componentes que est�o neste mesmo GameObject.
        // Atualizado para usar a nova classe 'CinemachineCamera'.
        CinemachineCamera virtualCamera = GetComponent<CinemachineCamera>();
        CinemachineConfiner2D confiner = GetComponent<CinemachineConfiner2D>();

        // --- Configura��o do Alvo (Follow) ---

        // Procura pelo GameObject do jogador na cena usando a tag definida.
        GameObject playerTarget = GameObject.FindGameObjectWithTag(playerTag);

        // Verifica se o jogador foi encontrado.
        if (playerTarget != null)
        {
            // Se encontrou, atribui o Transform do jogador ao campo "Follow" da c�mera virtual.
            // Acessando a propriedade p�blica 'Follow' (sem o 'm_').
            virtualCamera.Follow = playerTarget.transform;
            Debug.Log($"Cinemachine: Alvo '{playerTarget.name}' definido como Follow.");
        }
        else
        {
            // Se n�o encontrou, exibe um erro claro no console para facilitar a depura��o.
            Debug.LogError($"Cinemachine ERROR: N�o foi poss�vel encontrar um GameObject com a tag '{playerTag}' na cena.");
        }

        // --- Configura��o dos Limites (Confiner) ---

        // Procura pelo GameObject que cont�m o colisor dos limites da c�mera.
        GameObject cameraBoundary = GameObject.FindGameObjectWithTag(cameraBoundaryTag);

        // Verifica se o objeto de limites foi encontrado.
        if (cameraBoundary != null)
        {
            // Tenta pegar o componente Collider2D (pode ser BoxCollider2D, PolygonCollider2D, etc.).
            Collider2D boundaryShape = cameraBoundary.GetComponent<Collider2D>();

            if (boundaryShape != null)
            {
                // Se encontrou o colisor, atribui ao campo "Bounding Shape 2D" do Confiner.
                // Acessando a propriedade p�blica 'BoundingShape2D' (sem o 'm_').
                confiner.BoundingShape2D = boundaryShape;

                // For�a o confiner a recalcular seus limites com base na nova forma.
                // Usando o nome de m�todo atualizado 'InvalidateBoundingShapeCache'.
                confiner.InvalidateBoundingShapeCache();

                Debug.Log($"Cinemachine: Limite '{cameraBoundary.name}' definido no Confiner.");
            }
            else
            {
                // Se o objeto foi encontrado mas n�o tem um Collider2D, avisa o erro.
                Debug.LogError($"Cinemachine ERROR: O GameObject '{cameraBoundary.name}' com a tag '{cameraBoundaryTag}' n�o possui um componente Collider2D.");
            }
        }
        else
        {
            // Se n�o encontrou o objeto de limites, exibe o erro.
            Debug.LogError($"Cinemachine ERROR: N�o foi poss�vel encontrar um GameObject com a tag '{cameraBoundaryTag}' na cena.");
        }
    }
}