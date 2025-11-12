using UnityEngine;

// O Enum de tipos de IA que provou ser uma boa ideia.
public enum AIType
{
    Melee,      // Persegue agressivamente para atacar de perto.
    Ranged,     // Mantém distância para atacar de longe.
    Kamikaze    // Persegue para explodir ao chegar perto.
    // O tipo 'Mixed' será uma variação configurada no Controller, não um tipo base.
}

[CreateAssetMenu(fileName = "NovoEnemyProfile", menuName = "IA/Enemy Profile", order = 1)]
public class EnemySO : ScriptableObject
{
    [Header("▶ Identidade da IA")]
    [Tooltip("Define o arquétipo de comportamento principal desta IA.")]
    public AIType aiType = AIType.Melee;

    [Header("▶ Atributos Principais")]
    [Tooltip("A quantidade máxima de vida.")]
    public float maxHealth = 100f;
    [Tooltip("Resistência a ser empurrado por ataques. Subtraído da força do knockback recebido.")]
    public float knockbackResistance = 2f;

    [Header("▶ Movimento")]
    [Tooltip("Velocidade ao patrulhar.")]
    public float patrolSpeed = 2f;
    [Tooltip("Velocidade ao perseguir o jogador.")]
    public float chaseSpeed = 5f;

    [Header("▶ Percepção (Sentidos)")]
    [Tooltip("A distância máxima que a IA pode ver.")]
    public float visionRange = 15f;
    [Tooltip("A largura do cone de visão em graus.")]
    [Range(1, 360)] public float visionAngle = 90f;
    [Tooltip("Quais layers são consideradas o jogador.")]
    public LayerMask playerLayer;
    [Tooltip("Quais layers bloqueiam a linha de visão (ex: Chao, Paredes).")]
    public LayerMask visionBlockers;

    [Header("▶ Cognição (Memória)")]
    [Tooltip("Quanto tempo (em segundos) a IA continua procurando na última posição conhecida do jogador após perdê-lo de vista.")]
    public float memoryDuration = 5f;
    [Tooltip("Quanto tempo (em segundos) a IA para para 'analisar' um obstáculo na patrulha antes de virar.")]
    public float patrolPauseDuration = 1f;

    [Header("▶ Combate: Geral")]
    [Tooltip("A distância na qual a IA pode iniciar um ataque.")]
    public float attackRange = 1.5f;
    [Tooltip("O dano base dos ataques.")]
    public float attackDamage = 15f;
    [Tooltip("A força do empurrão que a IA aplica no jogador.")]
    public float attackKnockbackPower = 5f;
    [Tooltip("O tempo de espera (em segundos) entre um ataque e outro.")]
    public float attackCooldown = 2f;

    [Header("▶ Combate: Ranged")]
    [Tooltip("A distância 'confortável' que a IA Ranged tenta manter do jogador.")]
    public float idealAttackDistance = 8f;
    [Tooltip("O prefab do projétil que será disparado.")]
    public GameObject projectilePrefab;
    [Tooltip("A velocidade do projétil.")]
    public float projectileSpeed = 10f;

    [Header("▶ Combate: Kamikaze")]
    [Tooltip("O tempo em segundos desde que a IA vê o jogador até a explosão.")]
    public float fuseTime = 3f;
    [Tooltip("O raio da explosão. Usará o 'attackDamage' como dano.")]
    public float explosionRadius = 3f;
}