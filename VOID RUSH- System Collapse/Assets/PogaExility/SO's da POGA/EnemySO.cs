using UnityEngine;

/// <summary>
/// Scriptable Object que serve como um perfil ou "ficha de personagem" para um tipo de inimigo.
/// Contém todos os dados configuráveis, desde stats de combate até comportamento e percepção.
/// </summary>

// O Enum de tipos de IA que define o comportamento base.
public enum AIType
{
    Melee,
    Ranged,
    Kamikaze
}

[CreateAssetMenu(fileName = "NovoEnemyProfile", menuName = "IA/Enemy Profile", order = 1)]
public class EnemySO : ScriptableObject
{
    // =================================================================================================
    // IDENTIDADE E ATRIBUTOS
    // =================================================================================================

    [Header("▶ Identidade da IA")]
    [Tooltip("Define o arquétipo de comportamento principal desta IA.")]
    public AIType aiType = AIType.Melee;

    [Header("▶ Atributos Principais")]
    [Tooltip("A quantidade máxima de vida.")]
    public float maxHealth = 100f;

    [Tooltip("Valor de defesa que reduz o dano recebido de ataques.")]
    public float defense = 0f;

    [Tooltip("Resistência a ser empurrado por ataques. Subtraído da força do knockback recebido.")]
    public float knockbackResistance = 2f;

    // =================================================================================================
    // MOVIMENTO E NAVEGAÇÃO
    // =================================================================================================
    [Header("▶ Movimento")]
    [Tooltip("Velocidade ao patrulhar.")]
    public float patrolSpeed = 2f;
    [Tooltip("Velocidade ao perseguir o jogador.")]
    public float chaseSpeed = 5f;
    [Tooltip("A altura máxima de um obstáculo que a IA tentará pular.")]
    public float maxJumpableHeight = 1.2f;
    [Tooltip("A força máxima de um pulo (com 100% de força).")]
    public float maxJumpForce = 15f;
    [Tooltip("Quão rápido a IA atinge a velocidade máxima.")]
    public float acceleration = 50f;
    [Tooltip("Quão rápido a IA para quando não recebe comando de movimento.")]
    public float deceleration = 60f;

    // =================================================================================================
    // PERCEPÇÃO E COGNIÇÃO
    // =================================================================================================

    [Header("▶ Percepção (Sentidos)")]
    [Tooltip("A distância máxima para a DETECÇÃO INICIAL do jogador através do cone de visão.")]
    public float visionRange = 15f;
    [Tooltip("Qual layer é considerada uma escada escalável.")]
    public LayerMask ladderLayer;

    [Tooltip("O raio da 'zona de combate'. Uma vez que o jogador é visto, a IA continuará a persegui-lo até que ele saia deste círculo.")]
    public float engagementRange = 20f;

    [Tooltip("A largura do cone de visão em graus.")]
    [Range(1, 360)] public float visionAngle = 90f;

    [Tooltip("Quais layers são consideradas o jogador.")]
    public LayerMask playerLayer;

    [Tooltip("Define o que a IA considera um obstáculo sólido para a VISÃO e para a NAVEGAÇÃO (patrulha).")]
    public LayerMask obstacleLayer;

    [Header("▶ Cognição (Memória)")]
    [Tooltip("Quanto tempo (em segundos) a IA continua procurando na última posição conhecida do jogador.")]
    public float memoryDuration = 5f;

    [Tooltip("Quanto tempo (em segundos) a IA para para 'analisar' um obstáculo na patrulha antes de virar.")]
    public float patrolPauseDuration = 1f;

    // =================================================================================================
    // COMBATE
    // =================================================================================================

    [Header("▶ Combate: Geral")]
    [Tooltip("A distância na qual a IA pode iniciar um ataque.")]
    public float attackRange = 1.5f;

    [Tooltip("O raio do 'espaço pessoal'. Se o jogador entrar nesta área, a IA entra em modo de pânico (ataque melee ou recuo máximo).")]
    public float personalSpaceRadius = 1f;

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

    // =================================================================================================
    // EFEITOS E RECOMPENSAS
    // =================================================================================================

    [Header("▶ Efeitos e Recompensas (Opcional)")]
    [Tooltip("Efeito visual a ser instanciado na morte.")]
    public GameObject deathVFX;

    [Tooltip("Som a ser tocado ao atacar.")]
    public AudioClip attackSFX;

    [Tooltip("Som a ser tocado ao morrer.")]
    public AudioClip deathSFX;

    [Tooltip("O item/coletável que pode ser dropado na morte.")]
    public GameObject lootDropPrefab;

    [Tooltip("A chance (de 0 a 1) de dropar o loot.")]
    [Range(0f, 1f)] public float lootDropChance = 0.1f;
}