using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using K.Pathfinding;

[RequireComponent(typeof(AIPlatformerMotor), typeof(AINavigationSystem))]
public class AIController_Stalker : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;
    [SerializeField] private K.Pathfinding.Grid grid;

    [Header("AI Settings")]
    public float reactionTime = 0.2f;
    public float nextWaypointDistance = 0.5f;

    private AIPlatformerMotor _motor;
    private AINavigationSystem _nav;
    private List<Node> _currentPath;
    private int _pathIndex;

    void Start()
    {
        _motor = GetComponent<AIPlatformerMotor>();
        _nav = GetComponent<AINavigationSystem>();

        if (grid == null) grid = FindFirstObjectByType<K.Pathfinding.Grid>();
        StartCoroutine(ThinkRoutine());
    }

    IEnumerator ThinkRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(reactionTime);
        while (true)
        {
            if (target != null && grid != null)
            {
                List<Node> path = Pathfinding.FindPath(transform.position, target.position, grid);
                if (path != null && path.Count > 0)
                {
                    _currentPath = path;
                    if (_pathIndex >= _currentPath.Count) _pathIndex = 0;
                }
            }
            yield return wait;
        }
    }

    void FixedUpdate()
    {
        // Se não tem caminho, para.
        if (_currentPath == null || _pathIndex >= _currentPath.Count)
        {
            _motor.StopMoving();
            if (_motor.IsClimbing) _motor.StopClimb();
            return;
        }

        // --- RACIOCÍNIO ---
        Node currentNode = _currentPath[_pathIndex];
        Vector3 targetPoint = currentNode.worldPosition;

        switch (currentNode.action)
        {
            case ActionType.Walk:
            case ActionType.Crouch:
                if (_motor.IsClimbing) _motor.StopClimb();

                // Lógica de Agachar/Levantar
                if (currentNode.action == ActionType.Crouch || _nav.ShouldStartCrouching())
                    _motor.StartCrouch();
                else if (_nav.CanStandUpSafely())
                    _motor.StopCrouch();

                _motor.MoveTowards(targetPoint.x);
                break;

            case ActionType.Jump:
                if (_motor.IsClimbing) _motor.StopClimb();

                // CORREÇÃO AQUI: Substituído _motor.Crouch(false) por StopCrouch()
                if (_nav.CanStandUpSafely()) _motor.StopCrouch();

                _motor.MoveTowards(targetPoint.x);
                if (_motor.IsGrounded) _motor.Jump();
                break;

            case ActionType.Climb:
                // Só escala se o sensor confirmar parede
                if (_nav.HasClimbableWall())
                {
                    _motor.StartClimb();
                }
                else
                {
                    // Senão, anda até encostar
                    _motor.MoveTowards(targetPoint.x);
                }
                break;

            case ActionType.Fall:
                _motor.MoveTowards(targetPoint.x);
                break;
        }

        // --- EXCEÇÃO DE TÚNEL (VENT) ---
        if (_nav.IsVentOpening())
        {
            float dirToTarget = Mathf.Sign(targetPoint.x - transform.position.x);
            float facingDir = transform.localScale.x;

            if (Mathf.Approximately(dirToTarget, facingDir))
            {
                _motor.StartCrouch();
                _motor.MoveTowards(targetPoint.x);
            }
        }

        // --- AVANÇO ---
        float distX = Mathf.Abs(transform.position.x - targetPoint.x);
        float distY = Mathf.Abs(transform.position.y - targetPoint.y);

        if (distX < nextWaypointDistance && distY < 1.5f)
        {
            _pathIndex++;
        }

        Debug.DrawLine(transform.position, targetPoint, Color.green);
    }

    void OnDrawGizmos()
    {
        if (_currentPath != null)
        {
            for (int i = _pathIndex; i < _currentPath.Count - 1; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(_currentPath[i].worldPosition, _currentPath[i + 1].worldPosition);
            }
        }
    }
}