/// <summary>
/// Define a categoria geral de uma habilidade, usado principalmente para
/// organizar o Inspector através do SkillSOEditor.
/// </summary>
public enum SkillClass
{
    Movimento,
    Buff,
    Dano
}

/// <summary>
/// Define as variantes de uma habilidade de Dash, permitindo, por exemplo,
/// que um Dash só possa ser usado no chão (Normal) ou no ar (Aereo).
/// </summary>
public enum DashType
{
    Normal,
    Aereo
}

/// <summary>
/// Define todas as ações de movimento distintas que um SkillSO pode executar.
/// Cada item aqui corresponde a uma lógica ("case") específica dentro do SkillRelease,
/// garantindo que cada habilidade seja tratada de forma independente.
/// </summary>
public enum MovementSkillType
{
    None,         // Nenhuma ação, usado como padrão.
    SuperJump,    // O pulo padrão do jogador, tanto no chão quanto no ar.
    Dash,         // O dash padrão, horizontal.
    DashJump,     // Ação de lançamento para frente e para cima a partir do chão.
    WallSlide,    // A ação de se "agarrar" e começar a deslizar na parede.
    WallJump,     // O pulo para longe da parede.
    WallDash,     // O dash horizontal a partir de uma parede.
    WallDashJump, // O lançamento diagonal a partir de uma parede.
    Stealth       // Reservado para uso futuro.
}

/// <summary>
/// Define todos os estados de física ou de ação em que o jogador pode estar.
/// Usado pelo SkillSO para definir as condições de ativação de uma habilidade.
/// O SkillRelease vai checar estes estados no AdvancedPlayerMovement2D.
/// </summary>
public enum PlayerState
{
    None,
    IsGrounded,     // Está no chão?
    IsTouchingWall, // Está colidindo com uma parede?
    IsWallSliding,  // Está no estado de deslize na parede?
    IsDashing,      // Está no meio de um dash?
    IsJumping,      // A velocidade Y é positiva?
    IsInAir,
    IsInParabola,   // Está no arco de um lançamento (como o WallDashJump)?
    IsWallJumping   // Está no breve estado de pulo de parede?

}

/// <summary>
/// Define como um grupo de condições de PlayerState deve ser avaliado.
/// Permite a criação de lógicas "E" (AllOf) e "OU" (AnyOf).
/// </summary>
public enum ConditionLogic
{
    AllOf, // TODAS as condições no grupo devem ser verdadeiras.
    AnyOf  // PELO MENOS UMA das condições no grupo deve ser verdadeira.
}