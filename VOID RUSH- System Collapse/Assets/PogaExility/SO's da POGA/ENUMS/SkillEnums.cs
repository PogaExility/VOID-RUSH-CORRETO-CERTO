public enum SkillClass { Movimento, Buff, Dano }
public enum DashType { Normal, Aereo }

// <<< ESTA É A ESTRUTURA CORRETA E COMPLETA >>>
public enum MovementSkillType
{
    None,
    Dash,
    SuperJump,
    WallJump,     // Ação separada
    WallDash,     // Ação separada
    WallDashJump, // A AÇÃO COMBINADA
    Stealth
}