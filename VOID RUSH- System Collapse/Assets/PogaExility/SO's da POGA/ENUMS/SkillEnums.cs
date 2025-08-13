// ARQUIVO: SkillEnums.cs

public enum SkillClass
{
    Movimento,
    Buff,
    Dano
}

public enum MovementSkillType
{
    None,
    Dash,
    SuperJump,
    Stealth
}

// <<< NOVO ENUM PARA OS TIPOS DE DASH >>>
public enum DashType
{
    Normal, // Dash terrestre que pode carregar momento
    Aereo   // Dash aéreo omnidirecional

}
public enum WeaponType
{
    Melee,
    Firearm,
    Buster
}