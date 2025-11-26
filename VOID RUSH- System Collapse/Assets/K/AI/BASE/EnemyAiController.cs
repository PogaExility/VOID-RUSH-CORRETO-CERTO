using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyBrain))]
public class EnemyAIController : MonoBehaviour
{
    private EnemyBrain _brain;

    [Header("Patrulha")]
    public List<Transform> patrolPoints;
    private int _currentPatrolIndex;
    private float _waitTimer;

    private enum State { Patrolling, Analyzing, Chasing, Attacking, Searching, InvestigatingNoise }
    private State _currentState;
    private float _searchTimer;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        _currentState = _brain.stats.canWander ? State.Patrolling : State.Analyzing;
    }

    public void OnSuspiciousActivityDetected(Vector3 position)
    {
        if (_currentState == State.Chasing || _currentState == State.Attacking) return;

        float dist = Vector2.Distance(transform.position, position);
        if (dist <= _brain.stats.hearingRange)
        {
            _brain.LastKnownPosition = position;
            _currentState = State.InvestigatingNoise;
        }
    }

    void Update()
    {
        CheckPerception();

        switch (_currentState)
        {
            case State.Patrolling:
                HandlePatrol();
                break;
            case State.Analyzing:
                // Controlado pela corrotina
                break;
            case State.Chasing:
                HandleChase();
                break;
            case State.Attacking:
                HandleAttackState();
                break;
            case State.Searching:
                HandleSearch();
                break;
            case State.InvestigatingNoise:
                HandleNoiseInvestigation();
                break;
        }
    }

    // --- MÉTODOS DE COMPORTAMENTO (QUE ESTAVAM FALTANDO) ---

    void CheckPerception()
    {
        Collider2D targetCol = _brain.sensors.ScanForPlayer();

        if (targetCol != null && _brain.sensors.CanSeeTarget(targetCol.transform))
        {
            _brain.OnPlayerDetected(targetCol.transform);
            if (_currentState != State.Attacking) _currentState = State.Chasing;
        }
    }

    void HandlePatrol()
    {
        if (patrolPoints.Count == 0)
        {
            // Sem pontos, fica parado ou analisando
            if (!_brain.motor.IsObstacleAhead()) StartCoroutine(AnalyzeSurroundingsRoutine());
            return;
        }

        Transform targetPoint = patrolPoints[_currentPatrolIndex];
        _brain.motor.MoveTo(targetPoint.position, false);

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= _brain.stats.patrolWaitTime)
            {
                _waitTimer = 0;
                _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Count;
                // Opcional: Olhar em volta ao chegar no ponto
                StartCoroutine(AnalyzeSurroundingsRoutine());
            }
            else
            {
                _brain.motor.Stop();
            }
        }
    }

    void HandleChase()
    {
        if (_brain.CurrentTarget == null) { _currentState = State.Patrolling; return; }

        float dist = Vector2.Distance(transform.position, _brain.CurrentTarget.position);

        // Se perdeu de vista
        if (!_brain.sensors.CanSeeTarget(_brain.CurrentTarget))
        {
            _brain.LastKnownPosition = _brain.CurrentTarget.position;
            _searchTimer = _brain.stats.memoryDuration;
            _currentState = State.Searching;
            return;
        }

        if (dist <= _brain.stats.attackRange)
        {
            _brain.motor.Stop();
            _currentState = State.Attacking;
        }
        else
        {
            _brain.motor.MoveTo(_brain.CurrentTarget.position, true);
        }
    }

    void HandleAttackState()
    {
        if (_brain.CurrentTarget == null) { _currentState = State.Patrolling; return; }

        float dist = Vector2.Distance(transform.position, _brain.CurrentTarget.position);

        if (dist > _brain.stats.attackRange)
        {
            _currentState = State.Chasing;
        }
        else
        {
            _brain.combat.TryAttack(_brain.CurrentTarget);
        }
    }

    void HandleSearch()
    {
        _brain.motor.MoveTo(_brain.LastKnownPosition, true);
        _searchTimer -= Time.deltaTime;

        if (Vector2.Distance(transform.position, _brain.LastKnownPosition) < 0.5f || _searchTimer <= 0)
        {
            // Chegou onde viu por ultimo e nao achou ninguem -> Analisa
            StartCoroutine(AnalyzeSurroundingsRoutine());
        }
    }

    void HandleNoiseInvestigation()
    {
        _brain.motor.MoveTo(_brain.LastKnownPosition, true);

        if (Vector2.Distance(transform.position, _brain.LastKnownPosition) < 1.0f)
        {
            StartCoroutine(AnalyzeSurroundingsRoutine());
        }
    }

    IEnumerator AnalyzeSurroundingsRoutine()
    {
        _currentState = State.Analyzing;
        _brain.motor.Stop();

        Transform eyes = _brain.eyes;
        if (eyes != null)
        {
            Quaternion startRot = eyes.localRotation;
            float angle = _brain.stats.patrolScanAngle;
            float speed = _brain.stats.patrolScanSpeed;

            float t = 0;
            while (t < 1f) { eyes.localRotation = Quaternion.Lerp(startRot, Quaternion.Euler(0, 0, angle), t); t += Time.deltaTime * speed; yield return null; }
            t = 0;
            while (t < 1f) { eyes.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, angle), Quaternion.Euler(0, 0, -angle), t); t += Time.deltaTime * speed; yield return null; }
            t = 0;
            while (t < 1f) { eyes.localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, -angle), startRot, t); t += Time.deltaTime * speed; yield return null; }

            eyes.localRotation = startRot;
        }
        else
        {
            yield return new WaitForSeconds(1.5f); // Se nao tiver olhos configurados, só espera
        }

        _currentState = _brain.stats.canWander ? State.Patrolling : State.Analyzing;
    }
}