// ARQUIVO: SkillEnums.cs
// FUNÇÃO: Centraliza todas as definições de enum para o sistema de Skills.

public enum SkillClass
{
    Movimento,
    Buff,
    Dano
}

public enum MovementSkillType
{
    None, // Valor padrão para evitar erros
    Dash,
    SuperJump, // Nome mais específico que "Jump"
    Stealth
}

// Futuramente, podemos adicionar mais enums aqui:
// public enum BuffType { ... }
// public enum DamageType { ... }