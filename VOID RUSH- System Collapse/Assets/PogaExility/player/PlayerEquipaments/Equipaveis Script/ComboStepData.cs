// NOME DO ARQUIVO: ComboStepData.cs

using UnityEngine;

/// <summary>
/// Esta � uma classe de dados, n�o um componente.
/// Ela serve como um "pacote" para agrupar todas as propriedades de um �nico golpe de combo.
/// O [System.Serializable] permite que a Unity a exiba no Inspector dentro de outras classes (como o ItemSO).
/// </summary>
[System.Serializable]
public class ComboStepData
{
    [Header("Anima��es")]
    [Tooltip("Anima��o do corpo do jogador para este golpe.")]
    public AnimationClip comboBodyAnimation;

    [Tooltip("Anima��o que o prefab do efeito de corte deve tocar.")]
    public AnimationClip slashAnimation;

    [Header("Efeitos e Dano")]
    [Tooltip("O prefab do efeito de corte a ser instanciado para este golpe.")]
    public GameObject slashEffectPrefab;

    [Tooltip("O dano base deste golpe espec�fico.")]
    public float damage;

    [Tooltip("A 'for�a' de repuls�o que este golpe aplica.")]
    public float knockbackPower;

    [Header("F�sica e Timing")]
    [Tooltip("A dist�ncia que o jogador avan�a (lunge) durante este golpe.")]
    public float lungeDistance;

    [Tooltip("A janela de tempo (em segundos), ap�s o in�cio deste golpe, dentro da qual o pr�ximo ataque pode ser iniciado para continuar o combo.")]
    public float comboWindow = 0.5f;
}