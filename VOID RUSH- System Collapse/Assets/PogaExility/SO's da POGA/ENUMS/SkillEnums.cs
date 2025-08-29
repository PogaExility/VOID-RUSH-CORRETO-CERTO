// Salve como "SkillEnums.cs"

public enum SkillClass { Movimento, Buff, Dano }
public enum DashType { Normal, Aereo }

/// <summary>
/// Define todas as lógicas de movimento possíveis que uma SkillSO pode representar.
/// Cada item aqui corresponde a um "case" no SkillRelease.
/// </summary>
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