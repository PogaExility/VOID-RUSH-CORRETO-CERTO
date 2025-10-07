using UnityEngine;

// O enum foi atualizado para ter estados de ataque mais específicos.
public enum AIState
{
    Patrolling,
    Chasing,
    MeleeAttacking,
    RangedAttacking,
    Stunned,
    Dead
}

[CreateAssetMenu(fileName = "NovoInimigoData", menuName = "IA_VD/Configuração de Inimigo")]
public class EnemyDataSO_VD : ScriptableObject
{
    [Header("Comportamento da IA")]
    [Tooltip("O estado em que o inimigo começará quando for instanciado.")]
    public AIState initialState = AIState.Patrolling;

    // --- VARIÁVEL ADICIONADA ---
    [Tooltip("A distância máxima que o inimigo perseguirá o jogador antes de desistir e voltar a patrulhar.")]
    public float chaseRange = 12f;

    [Header("Status de Combate")]
    [Tooltip("A quantidade máxima de vida que o inimigo possui.")]
    public float maxHealth = 100f;

    [Tooltip("A capacidade do inimigo de resistir a repulsão. Subtraído da 'Força' de um ataque recebido.")]
    public float knockbackResistance = 2f;

    [Header("Movimentação")]
    [Tooltip("A velocidade padrão do inimigo quando está patrulhando.")]
    public float patrolSpeed = 2f;

    [Tooltip("A velocidade do inimigo quando está perseguindo o jogador.")]
    public float chaseSpeed = 4f;

    [Header("Ataque Físico (Melee)")]
    [Tooltip("A distância a partir da qual o inimigo pode iniciar um ataque físico.")]
    public float meleeAttackRange = 1.5f;
    [Tooltip("O tempo de espera entre ataques físicos.")]
    public float meleeAttackCooldown = 2f;
    [Tooltip("O prefab para o ataque físico (ex: área de corte).")]
    public GameObject meleeAttackPrefab;
    [Tooltip("O dano do ataque físico.")]
    public float meleeAttackDamage = 15f;
    [Tooltip("A força de knockback do ataque físico.")]
    public float meleeAttackKnockbackPower = 8f;

    [Header("Ataque à Distância (Ranged)")]
    [Tooltip("A distância MÁXIMA para o ataque à distância. O ataque acontecerá entre o alcance melee e este.")]
    public float rangedAttackRange = 8f;
    [Tooltip("O tempo de espera entre ataques à distância.")]
    public float rangedAttackCooldown = 3f;
    [Tooltip("O prefab para o ataque à distância (o projétil).")]
    public GameObject rangedAttackPrefab;
    [Tooltip("O dano do projétil.")]
    public float rangedAttackDamage = 10f;
    [Tooltip("A força de knockback do projétil.")]
    public float rangedAttackKnockbackPower = 5f;
    [Tooltip("A velocidade com que o projétil viaja.")]
    public float projectileSpeed = 10f;
}