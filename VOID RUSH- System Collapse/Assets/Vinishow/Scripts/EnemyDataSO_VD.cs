using UnityEngine;

// O enum foi atualizado para ter estados de ataque mais espec�ficos.
public enum AIState
{
    Patrolling,
    Chasing,
    MeleeAttacking,
    RangedAttacking,
    Stunned,
    Dead
}

[CreateAssetMenu(fileName = "NovoInimigoData", menuName = "IA_VD/Configura��o de Inimigo")]
public class EnemyDataSO_VD : ScriptableObject
{
    [Header("Comportamento da IA")]
    [Tooltip("O estado em que o inimigo come�ar� quando for instanciado.")]
    public AIState initialState = AIState.Patrolling;

    // --- VARI�VEL ADICIONADA ---
    [Tooltip("A dist�ncia m�xima que o inimigo perseguir� o jogador antes de desistir e voltar a patrulhar.")]
    public float chaseRange = 12f;

    [Header("Status de Combate")]
    [Tooltip("A quantidade m�xima de vida que o inimigo possui.")]
    public float maxHealth = 100f;

    [Tooltip("A capacidade do inimigo de resistir a repuls�o. Subtra�do da 'For�a' de um ataque recebido.")]
    public float knockbackResistance = 2f;

    [Header("Movimenta��o")]
    [Tooltip("A velocidade padr�o do inimigo quando est� patrulhando.")]
    public float patrolSpeed = 2f;

    [Tooltip("A velocidade do inimigo quando est� perseguindo o jogador.")]
    public float chaseSpeed = 4f;

    [Header("Ataque F�sico (Melee)")]
    [Tooltip("A dist�ncia a partir da qual o inimigo pode iniciar um ataque f�sico.")]
    public float meleeAttackRange = 1.5f;
    [Tooltip("O tempo de espera entre ataques f�sicos.")]
    public float meleeAttackCooldown = 2f;
    [Tooltip("O prefab para o ataque f�sico (ex: �rea de corte).")]
    public GameObject meleeAttackPrefab;
    [Tooltip("O dano do ataque f�sico.")]
    public float meleeAttackDamage = 15f;
    [Tooltip("A for�a de knockback do ataque f�sico.")]
    public float meleeAttackKnockbackPower = 8f;

    [Header("Ataque � Dist�ncia (Ranged)")]
    [Tooltip("A dist�ncia M�XIMA para o ataque � dist�ncia. O ataque acontecer� entre o alcance melee e este.")]
    public float rangedAttackRange = 8f;
    [Tooltip("O tempo de espera entre ataques � dist�ncia.")]
    public float rangedAttackCooldown = 3f;
    [Tooltip("O prefab para o ataque � dist�ncia (o proj�til).")]
    public GameObject rangedAttackPrefab;
    [Tooltip("O dano do proj�til.")]
    public float rangedAttackDamage = 10f;
    [Tooltip("A for�a de knockback do proj�til.")]
    public float rangedAttackKnockbackPower = 5f;
    [Tooltip("A velocidade com que o proj�til viaja.")]
    public float projectileSpeed = 10f;
}