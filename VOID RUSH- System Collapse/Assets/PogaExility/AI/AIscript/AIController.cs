using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// O "Cérebro" central da Inteligência Artificial.
/// Este componente funciona como um maestro, orquestrando os outros módulos (Motor, Percepção, Scanner, Vida)
/// para tomar decisões e executar comportamentos complexos com base nos dados do EnemySO.
/// Ele utiliza uma máquina de estados finitos (FSM) interna para gerenciar seu comportamento.
/// </summary>
[RequireComponent(typeof(AIMovement))]
[RequireComponent(typeof(AIPerception))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(AIEnvironmentScanner))]
public class AIController : MonoBehaviour
{
    // =================================================================================================
    // CONFIGURAÇÃO PÚBLICA
    // =================================================================================================

    #region Configuração

    [Header("▶ Perfil da IA")]
    [Tooltip("Arraste aqui o ScriptableObject com todos os dados de configuração para esta IA.")]
    public EnemySO enemyData;

    #endregion

    // =================================================================================================
    // MÁQUINA DE ESTADOS
    // =================================================================================================

    #region Máquina de Estados

    /// <summary>
    /// Os diferentes estados de comportamento que a IA pode assumir.
    /// Cada estado dita um conjunto de ações e decisões específicas.
    /// </summary>
    private enum State
    {
        // Estados de Navegação e Baixo Alerta
        // --------------------------------------------------
        Patrolling,     // Movendo-se por um caminho, procurando por alvos e analisando o terreno.
        Analyzing,      // Parado, analisando ativamente um obstáculo ou som com os olhos.
        Jumping,        // Executando a ação de pular sobre um obstáculo.
        Falling,        // Estado de transição enquanto está no ar.

        // Estados de Combate / Alerta Alto
        // --------------------------------------------------
        Chasing,        // Perseguindo o jogador ativamente para diminuir a distância.
        Searching,      // Procurando o jogador na última posição conhecida após perdê-lo de vista.
        Attacking,      // Executando uma manobra de ataque (melee, ranged, ou especial).
        Repositioning,  // Afastando-se do jogador para manter uma distância segura (comportamento de 'kiting').

        // Estado Final
        // --------------------------------------------------
        Dead            // IA foi derrotada.
    }

    /// <summary>
    /// O estado de comportamento atual da máquina de estados.
    /// </summary>
    private State currentState;

    #endregion

    // =================================================================================================
    // MÓDULOS (DEPENDÊNCIAS)
    // =================================================================================================

    #region Módulos

    // Módulos internos que compõem as capacidades da IA. Cada módulo tem uma responsabilidade única.
    private AIMovement motor;               // O "Corpo", responsável pela física e movimento.
    private AIPerception perception;          // Os "Sentidos", responsáveis pela visão, audição e consciência.
    private AIEnvironmentScanner scanner;   // O "Radar", responsável por analisar o terreno.
    private HealthSystem health;              // A "Vitalidade", responsável pela vida e dano.

    #endregion

    // =================================================================================================
    // VARIÁVEIS DE ESTADO INTERNO
    // =================================================================================================

    #region Variáveis Internas

    // Referências e dados de estado que a IA usa para tomar decisões.
    private Transform playerTarget;
    private Vector2 lastKnownPlayerPosition;
    private float stateTimer;
    private bool canAttack = true;
    private bool isExecutingAction = false; // Trava para ações complexas como analisar.
    private bool isTakingKnockback = false;

    #endregion

    // =================================================================================================
    // CICLO DE VIDA E INICIALIZAÇÃO
    // =================================================================================================

    #region Unity Lifecycle & Inicialização

    /// <summary>
    /// Awake é chamado quando a instância do script é carregada. Ideal para pegar referências.
    /// </summary>
    private void Awake()
    {
        motor = GetComponent<AIMovement>();
        perception = GetComponent<AIPerception>();
        scanner = GetComponent<AIEnvironmentScanner>();
        health = GetComponent<HealthSystem>();
    }

    /// <summary>
    /// Start é chamado antes do primeiro frame. Ideal para configurar dependências.
    /// </summary>
    private void Start()
    {
        if (enemyData == null)
        {
            Debug.LogError($"CRÍTICO: Inimigo '{gameObject.name}' não possui um EnemySO! IA desativada.", this);
            this.enabled = false;
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
        else
        {
            Debug.LogError($"CRÍTICO: Inimigo '{gameObject.name}' não encontrou o jogador. IA desativada.", this);
            this.enabled = false;
            return;
        }

        // Injeta as dependências nos outros módulos.
        motor.Initialize(enemyData.obstacleLayer, enemyData.ladderLayer);
        scanner.Initialize(enemyData.obstacleLayer);
        perception.Initialize(this, playerTarget);
        health.Initialize(this);

        ChangeState(State.Patrolling);
    }

    #endregion

    // =================================================================================================
    // LÓGICA PRINCIPAL DA MÁQUINA DE ESTADOS
    // =================================================================================================

    #region Máquina de Estados (Update Principal)

    /// <summary>
    /// O loop principal da IA, chamado a cada frame.
    /// </summary>
    private void Update()
    {
        if (currentState == State.Dead || isTakingKnockback || isExecutingAction) return;
        RunStateMachine();
    }

    private void RunStateMachine()
    {
        State nextState = GetNextState();
        if (nextState != currentState) ChangeState(nextState);
        ExecuteCurrentStateLogic();
    }

    private State GetNextState()
    {
        // Se a IA está no ar, o único estado possível é 'Falling' até tocar o chão.
        if (!motor.IsGrounded() && currentState != State.Jumping && currentState != State.Falling)
        {
            return State.Falling;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        // --- HIERARQUIA DE DECISÃO ---

        // REGRA 1 (SAÍDA DO COMBATE): Se a IA está em combate e o jogador fugiu para muito longe.
        if (perception.IsAwareOfPlayer && distanceToPlayer > enemyData.engagementRange)
        {
            return State.Patrolling; // Desiste.
        }

        // REGRA 2 (PÂNICO): Se o jogador está no espaço pessoal.
        if (distanceToPlayer <= enemyData.personalSpaceRadius)
        {
            return enemyData.aiType == AIType.Ranged ? State.Repositioning : State.Attacking;
        }

        // REGRA 3 (AQUISIÇÃO/MANUTENÇÃO DE ALVO): Se a IA pode ver o jogador.
        if (perception.CheckVision())
        {
            // Lógica de combate baseada no tipo de IA
            switch (enemyData.aiType)
            {
                case AIType.Melee:
                    if (distanceToPlayer <= enemyData.attackRange && canAttack) return State.Attacking;
                    return State.Chasing;
                case AIType.Ranged:
                    if (distanceToPlayer < enemyData.idealAttackDistance) return State.Repositioning;
                    if (distanceToPlayer <= enemyData.attackRange && canAttack) return State.Attacking;
                    return State.Chasing;
                case AIType.Kamikaze:
                    return State.Attacking;
            }
        }

        // REGRA 4 (MEMÓRIA): Se a IA ESTAVA ciente do jogador, mas acabou de perdê-lo de vista.
        if (perception.IsAwareOfPlayer)
        {
            return State.Searching;
        }

        // --- LÓGICA DE SAÍDA DO 'SEARCHING' CORRIGIDA ---
        // REGRA 5 (PACIÊNCIA): Se a IA ESTÁ procurando e o tempo de busca acabou.
        if (currentState == State.Searching && stateTimer <= 0)
        {
            return State.Patrolling;
        }
        // --- FIM DA CORREÇÃO ---

        // REGRA PADRÃO: Se nenhuma das regras acima se aplicar, continue no estado atual.
        return currentState;
    }
    private void ChangeState(State newState)
    {
        if (currentState == newState) return;

        StopAllCoroutines();
        canAttack = true;
        isExecutingAction = false;

        Debug.Log($"[AI State] {gameObject.name}: {currentState} -> {newState}");
        currentState = newState;

        switch (currentState)
        {
            case State.Analyzing:
                StartCoroutine(AnalyzeObstacleCoroutine());
                break;
            case State.Jumping:
                motor.Jump();
                break;
            case State.Attacking:
                if (canAttack) StartCoroutine(AttackCoroutine());
                break;
        }
    }
    private void ExecuteCurrentStateLogic()
    {
        switch (currentState)
        {
            case State.Patrolling:
                // Durante a patrulha, a IA está constantemente escaneando o caminho à frente.
                var navQuery = scanner.AnalyzePathAhead();
                switch (navQuery.ObstacleType)
                {
                    case PathObstacleType.JumpableObstacle:
                        if (navQuery.ObstacleHeight <= enemyData.maxJumpableHeight)
                        {
                            ChangeState(State.Jumping);
                        }
                        else
                        {
                            // O obstáculo é muito alto para pular, trate como uma parede.
                            ChangeState(State.Analyzing);
                        }
                        break;
                    case PathObstacleType.Wall:
                    case PathObstacleType.Ledge:
                        ChangeState(State.Analyzing);
                        break;
                    case PathObstacleType.None:
                        motor.Move(enemyData.patrolSpeed);
                        break;
                }
                break;

            case State.Falling:
                // Se aterrissou, volta a patrulhar.
                if (motor.IsGrounded())
                {
                    ChangeState(State.Patrolling);
                }
                break;

            case State.Analyzing:
                // A corrotina de análise tem o controle.
                break;

            case State.Jumping:
                // Uma vez que o pulo começou, a IA entra em 'Falling' para esperar a aterrissagem.
                // Isso evita que ela tente pular de novo no ar.
                if (motor.Velocity.y < 0) // Começou a cair
                {
                    ChangeState(State.Falling);
                }
                break;

            case State.Chasing:
                FaceTarget(playerTarget.position);
                motor.Move(enemyData.chaseSpeed);
                break;
            case State.Searching:
                stateTimer -= Time.deltaTime; // O timer é decrementado aqui
                FaceTarget(lastKnownPlayerPosition);
                if (Vector2.Distance(transform.position, lastKnownPlayerPosition) > 1f) motor.Move(enemyData.chaseSpeed);
                else motor.Stop();
                break;
            case State.Repositioning:
                if (playerTarget == null) { motor.Stop(); break; }
                FaceTarget(playerTarget.position);
                motor.Move(-enemyData.chaseSpeed);
                break;
        }
    }

    #endregion

    #region Corrotinas de Comportamento

    /// <summary>
    /// Corrotina que gerencia a análise de um obstáculo, incluindo a animação dos olhos.
    /// </summary>
    private IEnumerator AnalyzeObstacleCoroutine()
    {
        isExecutingAction = true;
        motor.Stop();

        // Comanda os "olhos" para executarem uma animação de escaneamento.
        // A corrotina de olhar para cima/baixo seria implementada no AIPerception.
        // perception.ExecuteScanGaze(); 

        yield return new WaitForSeconds(enemyData.patrolPauseDuration); // Tempo de "pensamento"

        // Reavalia o obstáculo após a pausa para tomar a decisão final.
        var finalQuery = scanner.AnalyzePathAhead();

        if (finalQuery.ObstacleType == PathObstacleType.JumpableObstacle && finalQuery.ObstacleHeight <= enemyData.maxJumpableHeight)
        {
            ChangeState(State.Jumping);
        }
        else // Se for uma parede, beirada, ou obstáculo alto demais
        {
            motor.Flip();
            yield return motor.MoveForDuration(1, enemyData.patrolSpeed, 0.1f); // Dá um passo na nova direção.
            ChangeState(State.Patrolling);
        }

        isExecutingAction = false;
    }

    /// <summary>
    /// Corrotina que executa a sequência de ataque.
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        isExecutingAction = true;
        canAttack = false;
        motor.Stop();

        switch (enemyData.aiType)
        {
            case AIType.Melee:
                yield return new WaitForSeconds(0.5f);
                Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, enemyData.attackRange, enemyData.playerLayer);
                foreach (var target in targets)
                {
                    if (target.TryGetComponent<PlayerStats>(out var pStats))
                    {
                        Vector2 knockbackDir = (target.transform.position - transform.position).normalized;
                        pStats.TakeDamage(enemyData.attackDamage, knockbackDir, enemyData.attackKnockbackPower);
                    }
                }
                break;
            case AIType.Ranged:
                yield return new WaitForSeconds(0.5f);
                if (enemyData.projectilePrefab != null)
                {
                    Vector2 fireDir = (playerTarget.position - transform.position).normalized;
                    Instantiate(enemyData.projectilePrefab, perception.Eyes.position, Quaternion.FromToRotation(Vector3.right, fireDir));
                }
                break;
        }

        isExecutingAction = false;
        yield return new WaitForSeconds(enemyData.attackCooldown);
        canAttack = true;
    }

    /// <summary>
    /// Inicia a rotina de knockback.
    /// </summary>
    public void TriggerKnockback(Vector2 direction, float force)
    {
        StartCoroutine(KnockbackCoroutine(direction, force));
    }

    /// <summary>
    /// Corrotina que gerencia o estado de "stun" do knockback.
    /// </summary>
    private IEnumerator KnockbackCoroutine(Vector2 direction, float force)
    {
        isTakingKnockback = true;
        motor.ApplyKnockback(direction, force);
        yield return new WaitForSeconds(0.4f);
        isTakingKnockback = false;
    }

    /// <summary>
    /// Função de callback chamada pelo HealthSystem na morte.
    /// </summary>
    public void OnDeath()
    {
        ChangeState(State.Dead);
        motor.Stop();
        GetComponent<Collider2D>().enabled = false;
        if (enemyData.deathVFX != null) Instantiate(enemyData.deathVFX, transform.position, Quaternion.identity);
        Destroy(gameObject, 3f);
    }

    #endregion

    #region Funções de Suporte

    /// <summary>
    /// Comanda o motor para virar e encarar uma posição alvo.
    /// </summary>
    private void FaceTarget(Vector2 targetPosition)
    {
        if (isExecutingAction) return;
        if ((targetPosition.x > transform.position.x && !motor.IsFacingRight) ||
            (targetPosition.x < transform.position.x && motor.IsFacingRight))
        {
            motor.Flip();
        }
    }

    #endregion

    #region Gizmos para Debug

#if UNITY_EDITOR
    /// <summary>
    /// Desenha os Gizmos de percepção e estado no Editor.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        // Desenha os círculos de alcance a partir do corpo da IA.
        Handles.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.forward, enemyData.attackRange);

        Handles.color = new Color(0.8f, 0, 0.8f);
        Handles.DrawWireDisc(transform.position, Vector3.forward, enemyData.personalSpaceRadius);

        Handles.color = new Color(0.5f, 0, 0.5f, 0.5f);
        Handles.DrawWireDisc(transform.position, Vector3.forward, enemyData.engagementRange);

        if (enemyData.aiType == AIType.Ranged)
        {
            Handles.color = Color.cyan;
            Handles.DrawWireDisc(transform.position, Vector3.forward, enemyData.idealAttackDistance);
        }

        // O Gizmo do cone de visão agora é responsabilidade do AIPerception.
        // O Gizmo dos sensores de navegação agora é responsabilidade do AIEnvironmentScanner.
    }
#endif

    #endregion
}