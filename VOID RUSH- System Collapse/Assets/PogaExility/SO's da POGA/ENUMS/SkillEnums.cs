// Este arquivo agora � a �nica fonte da verdade para os tipos de skill.

public enum SkillClass { Movimento, Buff, Dano }
public enum DashType { Normal, Aereo }

/// <summary>
/// Define todas as l�gicas de movimento poss�veis que uma SkillSO pode representar.
/// Cada item aqui corresponde a um "case" no SkillRelease.
/// </summary>
public enum MovementSkillType
{
    None,         // Nenhuma a��o
    SuperJump,    // Pulo normal ou a�reo
    Dash,         // Dash normal no ch�o ou a�reo
    WallJump,     // Pulo para longe da parede
    WallDash,     // Dash horizontal a partir da parede
    WallSlide,    // A��o de se "agarrar" e come�ar a deslizar
    WallDashJump, // O lan�amento diagonal a partir da parede
    Stealth       // Para uso futuro
}