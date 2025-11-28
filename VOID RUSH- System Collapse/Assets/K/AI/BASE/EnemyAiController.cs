using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyBrain))]
public class EnemyAIController : MonoBehaviour
{
    private EnemyBrain _brain;

    [Header("Patrulha")]
    public List<EnemyPatrolPoint> patrolPoints;
    private int _currentPatrolIndex;
    private float _waitTimer;
    private bool _isWaitingAtPoint;

    // Variáveis para o Wander (Andar aleatório sem pontos)
    private float _wanderTimer;

    private enum State { Patrolling, Analyzing, Chasing, Attacking, Searching, InvestigatingNoise }
    private State _currentState;
    private float _searchTimer;
    private Coroutine _scanCoroutine;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        // Se tiver pontos ou puder vagar, começa patrulhando
        _currentState = (_brain.stats.canWander || (patrolPoints != null && patrolPoints.Count > 0)) ? State.Patrolling : State.Analyzing;
        _wanderTimer = Random.Range(3f, 6f);
    }

    public void OnSuspiciousActivityDetected(Vector3 position)
    {
        if (_currentState == State.Chasing || _currentState == State.Attacking) return;
        if (Vector2.Distance(transform.position, position) <= _brain.stats.hearingRange)
        {
            _brain.LastKnownPosition = position;
            ChangeState(State.InvestigatingNoise);
        }
    }

    void Update()
    {
        CheckPerception();
        HandleEyeRotation();

        switch (_currentState)
        {
            case State.Patrolling: HandlePatrol(); break;
            case State.Analyzing: break;
            case State.Chasing: HandleChase(); break;
            case State.Attacking: HandleAttackState(); break;
            case State.Searching: HandleSearch(); break;
            case State.InvestigatingNoise: HandleNoiseInvestigation(); break;
        }
    }

    void HandleEyeRotation()
    {
        if (_brain.eyes == null) return;

        if ((_currentState == State.Chasing || _currentState == State.Attacking) && _brain.CurrentTarget != null)
        {
            Vector3 direction = _brain.CurrentTarget.position - _brain.eyes.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (!_brain.motor.IsFacingRight) angle = 180f - angle;
            _brain.eyes.localRotation = Quaternion.Euler(0, 0, angle);
        }
        else if (_currentState != State.Analyzing)
        {
            _brain.eyes.localRotation = Quaternion.Slerp(_brain.eyes.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }
    }

    void ChangeState(State newState)
    {
        if (_currentState == newState) return;
        if (_currentState == State.Analyzing && _scanCoroutine != null)
        {
            StopCoroutine(_scanCoroutine);
            _scanCoroutine = null;
            _brain.ResetEyeRotation();
        }
        _currentState = newState;
    }

    void CheckPerception()
    {
        Collider2D targetCol = _brain.sensors.ScanForPlayer();
        if (targetCol != null && _brain.sensors.CanSeeTarget(targetCol.transform))
        {
            _brain.OnPlayerDetected(targetCol.transform);
            if (_currentState != State.Attacking) ChangeState(State.Chasing);
        }
    }

    void HandlePatrol()
    {
        // 1. Sem pontos? Vagar aleatoriamente
        if (patrolPoints == null || patrolPoints.Count == 0)
        {
            if (_brain.stats.canWander) HandleRandomWander();
            else if (_scanCoroutine == null) _scanCoroutine = StartCoroutine(AnalyzeSurroundingsRoutine());
            return;
        }

        // 2. Com Pontos
        EnemyPatrolPoint targetPoint = patrolPoints[_currentPatrolIndex];

        // Distância tolerante para não travar (Vector2 ignora altura Z errada)
        if (Vector2.Distance(transform.position, targetPoint.transform.position) < 0.5f)
        {
            if (!_isWaitingAtPoint)
            {
                _isWaitingAtPoint = true;
                _waitTimer = targetPoint.waitTime;
                _brain.motor.Stop();

                // Snap de posição e virar para o lado correto
                transform.position = new Vector3(targetPoint.transform.position.x, transform.position.y, transform.position.z);
                Vector3 faceDir = targetPoint.transform.position + (targetPoint.faceRight ? Vector3.right : Vector3.left);
                _brain.motor.FacePoint(faceDir);
            }

            if (_isWaitingAtPoint)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0)
                {
                    _isWaitingAtPoint = false;
                    _currentPatrolIndex = (_currentPatrolIndex + 1) % patrolPoints.Count;
                }
            }
        }
        else
        {
            _brain.motor.MoveTo(targetPoint.transform.position, false);
            _isWaitingAtPoint = false;
        }
    }

    void HandleRandomWander()
    {
        if (_isWaitingAtPoint)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0)
            {
                _isWaitingAtPoint = false;
                // Vira para o outro lado
                float randomX = transform.position.x + (_brain.motor.IsFacingRight ? -5f : 5f);
                _brain.motor.FacePoint(new Vector3(randomX, transform.position.y, 0));
                _wanderTimer = Random.Range(2f, 5f);
            }
            return;
        }

        if (_brain.motor.IsObstacleAhead() || !_brain.motor.IsGroundAhead())
        {
            _brain.motor.Stop();
            _isWaitingAtPoint = true;
            _waitTimer = _brain.stats.patrolWaitTime;
            return;
        }

        Vector3 moveDir = _brain.motor.IsFacingRight ? Vector3.right : Vector3.left;
        _brain.motor.MoveTo(transform.position + moveDir, false);

        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0)
        {
            _brain.motor.Stop();
            _isWaitingAtPoint = true;
            _waitTimer = _brain.stats.patrolWaitTime;
        }
    }

    void HandleChase()
    {
        if (_brain.CurrentTarget == null) { ChangeState(State.Patrolling); return; }

        float dist = Vector2.Distance(transform.position, _brain.CurrentTarget.position);

        if (!_brain.sensors.CanSeeTarget(_brain.CurrentTarget))
        {
            _brain.LastKnownPosition = _brain.CurrentTarget.position;
            _searchTimer = _brain.stats.memoryDuration;
            ChangeState(State.Searching);
            return;
        }

        // CÁLCULO DE PARADA SEGURO (Evita empurrão infinito)
        float stopDist;
        if (_brain.stats.isRanged) stopDist = _brain.stats.attackRange;
        else stopDist = Mathf.Max(_brain.stats.attackRange - _brain.stats.stopDistancePadding, 0.8f);

        if (dist <= stopDist)
        {
            _brain.motor.Stop();
            ChangeState(State.Attacking);
        }
        else
        {
            _brain.motor.MoveTo(_brain.CurrentTarget.position, true);
        }
    }

    void HandleAttackState()
    {
        if (_brain.CurrentTarget == null) { ChangeState(State.Patrolling); return; }
        _brain.motor.FacePoint(_brain.CurrentTarget.position);

        if (_brain.stats.isExploder) { _brain.combat.TryAttack(_brain.CurrentTarget); return; }

        float dist = Vector2.Distance(transform.position, _brain.CurrentTarget.position);
        if (dist > _brain.stats.attackRange + 0.5f) ChangeState(State.Chasing);
        else _brain.combat.TryAttack(_brain.CurrentTarget);
    }

    void HandleSearch()
    {
        _brain.motor.MoveTo(_brain.LastKnownPosition, true);
        _searchTimer -= Time.deltaTime;
        if (Vector2.Distance(transform.position, _brain.LastKnownPosition) < 0.5f || _searchTimer <= 0)
        {
            if (_scanCoroutine == null) _scanCoroutine = StartCoroutine(AnalyzeSurroundingsRoutine());
        }
    }

    void HandleNoiseInvestigation()
    {
        _brain.motor.MoveTo(_brain.LastKnownPosition, true);
        if (Vector2.Distance(transform.position, _brain.LastKnownPosition) < 1.0f)
        {
            if (_scanCoroutine == null) _scanCoroutine = StartCoroutine(AnalyzeSurroundingsRoutine());
        }
    }

    IEnumerator AnalyzeSurroundingsRoutine()
    {
        ChangeState(State.Analyzing);
        _brain.motor.Stop();
        Transform eyes = _brain.eyes;
        if (eyes != null)
        {
            Quaternion startRot = Quaternion.identity;
            eyes.localRotation = startRot;
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
        else yield return new WaitForSeconds(1.5f);
        _scanCoroutine = null;
        ChangeState((_brain.stats.canWander || (patrolPoints != null && patrolPoints.Count > 0)) ? State.Patrolling : State.Analyzing);
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return; // Esconde Gizmos no Play

        if (_brain != null && !_brain.showGizmos) return;
        if (_currentState == State.Chasing && _brain.CurrentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _brain.CurrentTarget.position);
        }
    }
}