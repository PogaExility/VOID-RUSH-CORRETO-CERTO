using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AIPlatformerMotor), typeof(AINavigationSystem))]
public class AIController_Stalker : MonoBehaviour
{
    public Transform target;
    public float repathRate = 0.5f;
    public float nextWaypointDistance = 0.5f;

    private AIPlatformerMotor _motor;
    private AINavigationSystem _nav;

    private List<Vector3> _path;
    private int _pathIndex;
    private float _timer;

    void Start()
    {
        _motor = GetComponent<AIPlatformerMotor>();
        _nav = GetComponent<AINavigationSystem>();
    }

    void Update()
    {
        // 1. Calcular Rota (Waypoints)
        _timer += Time.deltaTime;
        if (_timer > repathRate && target != null)
        {
            _timer = 0;
            if (WaypointManager.Instance != null)
                _path = WaypointManager.Instance.GetPath(transform.position, target.position);

            if (_path != null && _path.Count > 0) _pathIndex = 0;
        }

        // 2. LÓGICA REATIVA (PRIORIDADE MÁXIMA)
        // Isso garante que ele use o sistema de duto que criamos, independente do waypoint

        // A. Duto Detectado? Entra.
        if (_nav.IsVentOpening())
        {
            _motor.StartCrouch();
            // Empurra na direção que está olhando
            float dir = transform.localScale.x;
            _motor.MoveTowards(transform.position.x + dir);
            return;
        }

        // B. Dentro do Duto? Desliga escalada se entrou.
        if (_motor.IsClimbing && _motor.IsCrouching && !_nav.CanStandUpSafely())
        {
            _motor.StopClimb();
        }

        // C. Segurança de Teto (Se tem teto, não levanta)
        if (_motor.IsCrouching && !_nav.CanStandUpSafely())
        {
            // Se estiver em manobra híbrida (Climb+Crouch), continua empurrando
            if (_motor.IsClimbing)
            {
                float dir = transform.localScale.x;
                _motor.MoveTowards(transform.position.x + dir);
            }
            return; // Trava o resto da lógica
        }

        // 3. EXECUÇÃO DO CAMINHO
        if (_path == null || _pathIndex >= _path.Count)
        {
            _motor.StopMoving();
            // Se parou e não tem teto, levanta
            if (_motor.IsCrouching && _nav.CanStandUpSafely()) _motor.StopCrouch();
            return;
        }

        Vector3 dest = _path[_pathIndex];
        float dist = Vector2.Distance(transform.position, dest);

        // Chegou no ponto?
        if (dist < nextWaypointDistance)
        {
            _pathIndex++;
            return;
        }

        // --- INFERÊNCIA DE AÇÃO (Baseado na Posição do Waypoint + Sensores) ---

        // 1. Waypoint está ALTO? (Escalar ou Pular)
        if (dest.y > transform.position.y + 1.0f)
        {
            // Se tem parede, escala
            if (_nav.HasClimbableWall() && !_motor.IsCrouching)
            {
                _motor.StartClimb();
            }
            // Se não tem parede e está no chão, pula
            else if (_motor.IsGrounded && !_motor.IsClimbing)
            {
                _motor.Jump();
            }
        }
        else
        {
            // Se o destino não é alto, para de escalar
            if (_motor.IsClimbing) _motor.StopClimb();
        }

        // 2. Teto baixo no caminho? (Sensor de Chão)
        if (_nav.ShouldStartCrouching())
        {
            _motor.StartCrouch();
        }
        else if (_nav.CanStandUpSafely() && !_motor.IsClimbing)
        {
            _motor.StopCrouch();
        }

        // 3. Move X
        _motor.MoveTowards(dest.x);
    }

    void OnDrawGizmos()
    {
        if (_path != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < _path.Count - 1; i++)
                Gizmos.DrawLine(_path[i], _path[i + 1]);
        }
    }
}