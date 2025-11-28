using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMotor : MonoBehaviour
{
    private EnemyBrain _brain;
    private Rigidbody2D _rb;
    private bool _isFrozen = false;
    private bool _isInKnockback = false;

    public bool IsFacingRight { get; private set; } = true;

    [Header("Checks")]
    public Transform wallCheck;
    public Transform groundCheck;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        _rb = GetComponent<Rigidbody2D>();

        _rb.mass = 1f;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        LockPosition(true); // Começa travado

        // Define o lado inicial baseado na escala atual do objeto na cena
        IsFacingRight = transform.localScale.x > 0;
    }

    public void MoveTo(Vector3 target, bool isChasing)
    {
        if (_isFrozen || _isInKnockback) return;

        float distanceX = Mathf.Abs(target.x - transform.position.x);
        if (distanceX < 0.1f)
        {
            Stop();
            return;
        }

        LockPosition(false); // Destrava para andar

        float speed = isChasing ? _brain.stats.chaseSpeed : _brain.stats.patrolSpeed;
        float dirX = Mathf.Sign(target.x - transform.position.x);

        if (!isChasing && (IsObstacleAhead() || !IsGroundAhead()))
        {
            Stop();
            return;
        }

        _rb.linearVelocity = new Vector2(dirX * speed, _rb.linearVelocity.y);

        if (Mathf.Abs(dirX) > 0.1f)
        {
            if (dirX > 0 && !IsFacingRight) Flip();
            else if (dirX < 0 && IsFacingRight) Flip();
        }
    }

    public void Stop()
    {
        if (!_isInKnockback)
        {
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
            LockPosition(true); // Trava para não ser empurrado
        }
    }

    public void Freeze(bool state)
    {
        _isFrozen = state;
        if (state) Stop();
    }

    public void ApplyKnockback(Vector2 dir, float force)
    {
        StartCoroutine(KnockbackRoutine(dir, force));
    }

    IEnumerator KnockbackRoutine(Vector2 dir, float force)
    {
        _isInKnockback = true;
        LockPosition(false); // Solta para voar
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(dir * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);
        _isInKnockback = false;
    }

    private void LockPosition(bool isLocked)
    {
        if (isLocked) _rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        else _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void FacePoint(Vector3 point)
    {
        float dir = Mathf.Sign(point.x - transform.position.x);
        if (Mathf.Abs(point.x - transform.position.x) > 0.2f)
        {
            if (dir > 0 && !IsFacingRight) Flip();
            else if (dir < 0 && IsFacingRight) Flip();
        }
    }

    void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    // --- BOTÃO DE DEBUG PARA O EDITOR ---
    [ContextMenu("VIRAR INIMIGO AGORA")]
    public void DebugFlip()
    {
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
        IsFacingRight = s.x > 0;
    }
    // ------------------------------------

    public bool IsObstacleAhead() => Physics2D.Raycast(wallCheck.position, transform.right * (IsFacingRight ? 1 : -1), 0.5f, _brain.stats.obstacleLayer);
    public bool IsGroundAhead() => Physics2D.Raycast(groundCheck.position, Vector2.down, 1f, _brain.stats.obstacleLayer);

    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        EnemyBrain brainTemp = GetComponent<EnemyBrain>();
        if (brainTemp != null && !brainTemp.showGizmos) return;
        if (wallCheck) { Gizmos.color = Color.red; Gizmos.DrawLine(wallCheck.position, wallCheck.position + transform.right * (transform.localScale.x > 0 ? 1 : -1) * 0.5f); }
        if (groundCheck) { Gizmos.color = Color.green; Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 1f); }
    }
}