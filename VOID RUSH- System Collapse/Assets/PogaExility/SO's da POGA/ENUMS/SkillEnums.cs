// ARQUIVO: SkillEnums.cs
// FUN��O: Centraliza todas as defini��es de enum para o sistema de Skills.

public enum SkillClass
{
    Movimento,
    Buff,
    Dano
}

public enum MovementSkillType
{
    None, // Valor padr�o para evitar erros
    Dash,
    SuperJump, // Nome mais espec�fico que "Jump"
    Stealth
}

// Futuramente, podemos adicionar mais enums aqui:
// public enum BuffType { ... }
// public enum DamageType { ... }