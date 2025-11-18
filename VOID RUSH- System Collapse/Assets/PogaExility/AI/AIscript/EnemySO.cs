using UnityEngine;

/// <summary>
/// Define os status base e o escalonamento por nível para um tipo de inimigo.
/// </summary>
[CreateAssetMenu(fileName = "NovoEnemySO", menuName = "Inimigo/Criar Novo Inimigo SO")]
public class EnemySO : ScriptableObject
{
    [Header("Informações Básicas")]
    [Tooltip("Nome do inimigo para fácil identificação.")]
    public string nomeInimigo;

    [Header("Atributos Base (Nível 1)")]
    [Tooltip("Vida Mínima(X) e Máxima(Y) que o inimigo pode ter no nível 1.")]
    public Vector2 vidaBase = new Vector2(10, 15);

    [Tooltip("Dano Mínimo(X) e Máximo(Y) que o inimigo pode causar no nível 1.")]
    public Vector2 danoBase = new Vector2(1, 3);

    [Tooltip("Velocidade de movimento do inimigo no nível 1.")]
    public float velocidadeMovimentoBase = 3f;

    [Tooltip("Escala (tamanho) inicial do inimigo. O padrão é 1.")]
    public float escalaBase = 1f;


    [Header("Fatores de Crescimento por Nível")]
    [Tooltip("Quanto a vida Mínima(X) e Máxima(Y) aumentam a cada nível acima do 1.")]
    public Vector2 aumentoVidaPorNivel = new Vector2(2, 2);

    [Tooltip("Quanto o dano Mínimo(X) e Máximo(Y) aumenta a cada nível acima do 1.")]
    public Vector2 aumentoDanoPorNivel = new Vector2(1, 1);

    [Tooltip("Quanto a velocidade de movimento aumenta a cada nível.")]
    public float aumentoVelocidadePorNivel = 0.2f;

    [Tooltip("Aumento percentual do tamanho a cada nível. 0.05 = 5% maior por nível.")]
    [Range(0f, 1f)]
    public float aumentoEscalaPercentualPorNivel = 0.05f;


    [Header("Parâmetros de Detecção de Alvo")]
    [Tooltip("A distância máxima que o inimigo pode detectar um alvo.")]
    public float raioVisao = 8f;

    [Tooltip("O ângulo do cone de visão do inimigo (em graus). 90 = visão bem aberta.")]
    [Range(1f, 360f)]
    public float anguloVisao = 90f;

    [Tooltip("O número de raios disparados dentro do cone de visão. Mais raios = mais precisão.")]
    [Range(2, 50)]
    public int quantidadeRaiosVisao = 10;

    [Tooltip("A camada em que o(s) alvo(s) da IA se encontram (ex: Player).")]
    public LayerMask camadaAlvo;

    [Tooltip("As camadas que bloqueiam a visão da IA (ex: Chão, Paredes).")]
    public LayerMask camadaObstaculos;
}