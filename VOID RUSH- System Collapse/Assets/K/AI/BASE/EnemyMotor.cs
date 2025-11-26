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
        IsFacingRight = transform.localScale.x > 0;
    }

    public void MoveTo(Vector3 target, bool isChasing)
    {
        if (_isFrozen || _isInKnockback) return;

        float speed = isChasing ? _brain.stats.chaseSpeed : _brain.stats.patrolSpeed;
        float dirX = Mathf.Sign(target.x - transform.position.x);

        // Verifica buraco ou parede antes de andar
        if (!isChasing && (IsObstacleAhead() || !IsGroundAhead()))
        {
            Stop();
            return;
        }

        _rb.linearVelocity = new Vector2(dirX * speed, _rb.linearVelocity.y);

        if (dirX > 0 && !IsFacingRight) Flip();
        else if (dirX < 0 && IsFacingRight) Flip();
    }

    public void Stop()
    {
        if (!_isInKnockback) _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
    }

    public void Freeze(bool state) => _isFrozen = state;

    public void ApplyKnockback(Vector2 dir, float force)
    {
        StartCoroutine(KnockbackRoutine(dir, force));
    }

    IEnumerator KnockbackRoutine(Vector2 dir, float force)
    {
        _isInKnockback = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(dir * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.2f);
        _isInKnockback = false;
    }

    public void FacePoint(Vector3 point)
    {
        float dir = Mathf.Sign(point.x - transform.position.x);
        if (dir > 0 && !IsFacingRight) Flip();
        else if (dir < 0 && IsFacingRight) Flip();
    }

    void Flip()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    // CORREÇÃO: Renomeei de IsWallAhead para IsObstacleAhead para o Controller encontrar
    public bool IsObstacleAhead() => Physics2D.Raycast(wallCheck.position, transform.right * (IsFacingRight ? 1 : -1), 0.5f, _brain.stats.obstacleLayer);

    public bool IsGroundAhead() => Physics2D.Raycast(groundCheck.position, Vector2.down, 1f, _brain.stats.obstacleLayer);
}