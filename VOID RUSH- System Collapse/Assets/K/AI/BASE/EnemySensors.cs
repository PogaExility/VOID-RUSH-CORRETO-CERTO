using UnityEngine;

public class EnemySensors : MonoBehaviour
{
    private EnemyBrain _brain;

    // Variável para lembrar se o Kamikaze já viu o player
    private bool _hasLockedOn = false;

    void Start() => _brain = GetComponent<EnemyBrain>();

    public Collider2D ScanForPlayer()
    {
        // Pega todos os possíveis alvos dentro do raio máximo de visão
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, _brain.stats.visionRange, _brain.stats.targetLayer);

        foreach (var target in potentialTargets)
        {
            // Verifica se esse alvo atende aos critérios de visão (Cone, Área ou Lock do Kamikaze)
            if (CanSeeTarget(target.transform))
            {
                return target; // Retorna o primeiro alvo válido encontrado
            }
        }

        return null;
    }

    public bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;

        // --- REGRA 1: KAMIKAZE INFINITO ---
        // Se for Kamikaze e já tiver travado a mira antes, ignora paredes, ângulo e distância.
        if (_brain.stats.isExploder && _hasLockedOn)
        {
            return true;
        }

        float dist = Vector2.Distance(_brain.eyes.position, target.position);

        // Se estiver fora do alcance máximo global, não vê
        if (dist > _brain.stats.visionRange) return false;

        // --- REGRA 2: VISÃO EM ÁREA (PROXIMIDADE) ---
        // Se o player entrar nessa pequena área, detecta automaticamente (ignora ângulo e paredes)
        if (dist <= _brain.stats.proximityDetectionRange)
        {
            MarkTargetAsSeen(); // Marca que viu (importante pro Kamikaze)
            return true;
        }

        // --- REGRA 3: VISÃO EM CONE (PADRÃO) ---
        Vector2 dirToTarget = (target.position - _brain.eyes.position).normalized;
        Vector2 facingDir = _brain.motor.IsFacingRight ? Vector2.right : Vector2.left;

        // 3.1 Checa o Ângulo
        if (Vector2.Angle(facingDir, dirToTarget) > _brain.stats.visionAngle / 2f) return false;

        // 3.2 Checa Paredes (Raycast)
        Vector2 startPos = _brain.eyes.position + (Vector3)(dirToTarget * 0.2f); // Pequeno offset para não bater no próprio colisor
        if (Physics2D.Raycast(startPos, dirToTarget, dist, _brain.stats.obstacleLayer)) return false;

        // Se passou por tudo (ângulo ok, sem parede), então viu!
        MarkTargetAsSeen();
        return true;
    }

    // Método auxiliar para ativar o "Lock" do Kamikaze
    private void MarkTargetAsSeen()
    {
        if (_brain.stats.isExploder)
        {
            _hasLockedOn = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        if (_brain == null || _brain.stats == null || _brain.eyes == null || !_brain.showGizmos) return;

        // --- DESENHO DA ÁREA DE VISÃO CONE (AMARELO) ---
        Gizmos.color = new Color(1, 1, 0, 0.1f); // Amarelo transparente
        Gizmos.DrawSphere(_brain.eyes.position, _brain.stats.visionRange);

        Gizmos.color = Color.yellow;
        Vector3 forward = _brain.motor.IsFacingRight ? Vector3.right : Vector3.left;
        Vector3 leftRay = Quaternion.AngleAxis(-_brain.stats.visionAngle / 2, Vector3.forward) * forward;
        Vector3 rightRay = Quaternion.AngleAxis(_brain.stats.visionAngle / 2, Vector3.forward) * forward;

        Gizmos.DrawRay(_brain.eyes.position, leftRay * _brain.stats.visionRange);
        Gizmos.DrawRay(_brain.eyes.position, rightRay * _brain.stats.visionRange);

        // --- DESENHO DA ÁREA DE PROXIMIDADE (VERMELHO) ---
        // Essa é a área onde ele detecta automaticamente
        Gizmos.color = new Color(1, 0, 0, 0.3f); // Vermelho transparente
        Gizmos.DrawSphere(_brain.eyes.position, _brain.stats.proximityDetectionRange);
    }
}