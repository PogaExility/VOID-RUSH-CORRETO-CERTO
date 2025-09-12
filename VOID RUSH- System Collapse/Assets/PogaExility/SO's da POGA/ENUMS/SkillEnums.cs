// SkillEnums.cs

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
    None,
    SuperJump,
    Dash,
    DashJump,
    WallSlide,
    WallJump,
    WallDash,
    WallDashJump,
    Stealth
}

/// <summary>
/// Define todos os estados de física ou de ação em que o jogador pode estar.
/// </summary>
public enum PlayerState
{
    None,
    IsGrounded,
    CanJumpFromGround,
    IsTouchingWall,
    IsWallSliding,
    IsDashing,
    IsJumping,
    IsInAir,
    IsInParabola,
    IsWallJumping,
    IsLanding,
    IsBlocking,
    IsParrying,

    // --- ADIÇÕES PARA CORRIGIR O ERRO E DAR MAIS CONTROLE ---
    IsWallDashing,  // Estado específico para o Dash a partir da parede.
    IsTakingDamage  // Estado para quando o jogador está em knockback.
}

/// <summary>
/// Define como um grupo de condições de PlayerState deve ser avaliado.
/// </summary>
public enum ConditionLogic
{
    AllOf,
    AnyOf
}