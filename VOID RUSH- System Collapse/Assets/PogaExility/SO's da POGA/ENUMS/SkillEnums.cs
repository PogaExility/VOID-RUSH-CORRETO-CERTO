// Este arquivo agora é a única fonte da verdade para os tipos de skill.

public enum SkillClass { Movimento, Buff, Dano }
public enum DashType { Normal, Aereo }

/// <summary>
/// Define todas as lógicas de movimento possíveis que uma SkillSO pode representar.
/// Cada item aqui corresponde a um "case" no SkillRelease.
/// </summary>
public enum MovementSkillType
{
    None,         // Nenhuma ação
    SuperJump,    // Pulo normal ou aéreo
    Dash,         // Dash normal no chão ou aéreo
    WallJump,     // Pulo para longe da parede
    WallDash,     // Dash horizontal a partir da parede
    WallSlide,    // Ação de se "agarrar" e começar a deslizar
    WallDashJump, // O lançamento diagonal a partir da parede
    Stealth       // Para uso futuro
}