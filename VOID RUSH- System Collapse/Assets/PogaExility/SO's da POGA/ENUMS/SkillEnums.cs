
public enum SkillClass
{
    Movimento,
    Buff,
    Combate
}
public enum DashType
{
    Normal,
    Aereo
}

public enum CombatSkillType
{
    None,
    MeleeAttack,
    FirearmAttack,
    BusterAttack,
    Block,          
    Parry
}
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
    CanJumpFromGround,
    IsTouchingWall, // Está colidindo com uma parede?
    IsWallSliding,  // Está no estado de deslize na parede?
    IsDashing,      // Está no meio de um dash?
    IsJumping,      // A velocidade Y é positiva?
    IsInAir,
    IsInParabola,   // Está no arco de um lançamento (como o WallDashJump)?
    IsWallJumping,   // Está no breve estado de pulo de parede?
    IsLanding,
    IsBlocking,
    IsParrying,

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