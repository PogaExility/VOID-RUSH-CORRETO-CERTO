/// <summary>
/// Define a categoria geral de uma habilidade, usado principalmente para
/// organizar o Inspector atrav�s do SkillSOEditor.
/// </summary>
public enum SkillClass
{
    Movimento,
    Buff,
    Dano
}

/// <summary>
/// Define as variantes de uma habilidade de Dash, permitindo, por exemplo,
/// que um Dash s� possa ser usado no ch�o (Normal) ou no ar (Aereo).
/// </summary>
public enum DashType
{
    Normal,
    Aereo
}

/// <summary>
/// Define todas as a��es de movimento distintas que um SkillSO pode executar.
/// Cada item aqui corresponde a uma l�gica ("case") espec�fica dentro do SkillRelease,
/// garantindo que cada habilidade seja tratada de forma independente.
/// </summary>
public enum MovementSkillType
{
    None,         // Nenhuma a��o, usado como padr�o.
    SuperJump,    // O pulo padr�o do jogador, tanto no ch�o quanto no ar.
    Dash,         // O dash padr�o, horizontal.
    DashJump,     // A��o de lan�amento para frente e para cima a partir do ch�o.
    WallSlide,    // A a��o de se "agarrar" e come�ar a deslizar na parede.
    WallJump,     // O pulo para longe da parede.
    WallDash,     // O dash horizontal a partir de uma parede.
    WallDashJump, // O lan�amento diagonal a partir de uma parede.
    Stealth       // Reservado para uso futuro.
}

/// <summary>
/// Define todos os estados de f�sica ou de a��o em que o jogador pode estar.
/// Usado pelo SkillSO para definir as condi��es de ativa��o de uma habilidade.
/// O SkillRelease vai checar estes estados no AdvancedPlayerMovement2D.
/// </summary>
public enum PlayerState
{
    None,
    IsGrounded,     // Est� no ch�o?
    IsTouchingWall, // Est� colidindo com uma parede?
    IsWallSliding,  // Est� no estado de deslize na parede?
    IsDashing,      // Est� no meio de um dash?
    IsJumping,      // A velocidade Y � positiva?
    IsInAir,
    IsInParabola,   // Est� no arco de um lan�amento (como o WallDashJump)?
    IsWallJumping   // Est� no breve estado de pulo de parede?

}

/// <summary>
/// Define como um grupo de condi��es de PlayerState deve ser avaliado.
/// Permite a cria��o de l�gicas "E" (AllOf) e "OU" (AnyOf).
/// </summary>
public enum ConditionLogic
{
    AllOf, // TODAS as condi��es no grupo devem ser verdadeiras.
    AnyOf  // PELO MENOS UMA das condi��es no grupo deve ser verdadeira.
}