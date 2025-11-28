using UnityEngine;

[RequireComponent(typeof(EnemyBrain))]
public class EnemyAnimationLink : MonoBehaviour
{
    private EnemyBrain _brain;
    private AudioSource _audioSource;
    public Animator animator;

    void Start()
    {
        _brain = GetComponent<EnemyBrain>();
        _audioSource = GetComponent<AudioSource>(); // Pega o mesmo AudioSource do Health
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null || _brain.motor == null) return;

        float speed = _brain.motor.GetComponent<Rigidbody2D>().linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);
    }

    // --- FUNÇÕES CHAMADAS PELO ANIMATION EVENT ---

    // 1. Adicione um Evento na Animação de WALK com o nome: PlayFootstep
    public void PlayFootstep()
    {
        if (_brain.stats.footstepSounds.Length > 0 && _audioSource)
        {
            // Escolhe um som aleatório da lista para não ficar repetitivo
            AudioClip clip = _brain.stats.footstepSounds[Random.Range(0, _brain.stats.footstepSounds.Length)];

            _audioSource.pitch = Random.Range(0.8f, 1.2f); // Muda levemente o tom
            _audioSource.PlayOneShot(clip, 0.5f); // Volume 0.5 para não ficar muito alto
        }
    }

    // 2. Adicione um Evento na Animação de IDLE com o nome: PlayIdleSound (Opcional)
    public void PlayIdleSound()
    {
        if (_brain.stats.idleSound && _audioSource)
        {
            // Só toca se não estiver tocando outra coisa importante (opcional)
            if (!_audioSource.isPlaying)
                _audioSource.PlayOneShot(_brain.stats.idleSound, 0.3f);
        }
    }

    // --- COMANDOS DE ANIMAÇÃO ---

    public void TriggerAttackAnim()
    {
        if (animator) animator.SetTrigger("Attack");
    }

    public void SetKamikazePrepare(bool state)
    {
        if (animator) animator.SetBool("PrepareExplosion", state);
    }

    public void TriggerFinalPhase()
    {
        if (animator) animator.SetTrigger("FinalPhase");
    }
}