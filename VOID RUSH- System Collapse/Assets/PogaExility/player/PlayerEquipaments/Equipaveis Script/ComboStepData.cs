// NOME DO ARQUIVO: ComboStepData.cs

using UnityEngine;

/// <summary>
/// Esta é uma classe de dados, não um componente.
/// Ela serve como um "pacote" para agrupar todas as propriedades de um único golpe de combo.
/// O [System.Serializable] permite que a Unity a exiba no Inspector dentro de outras classes (como o ItemSO).
/// </summary>
[System.Serializable]
public class ComboStepData
{
    [Header("Animações")]
    [Tooltip("Animação do corpo do jogador para este golpe.")]
    public AnimationClip comboBodyAnimation;

    [Tooltip("Animação que o prefab do efeito de corte deve tocar.")]
    public AnimationClip slashAnimation;

    [Header("Efeitos e Dano")]
    [Tooltip("O prefab do efeito de corte a ser instanciado para este golpe.")]
    public GameObject slashEffectPrefab;

    [Tooltip("O dano base deste golpe específico.")]
    public float damage;

    [Tooltip("A 'força' de repulsão que este golpe aplica.")]
    public float knockbackPower;

    [Header("Física e Timing")]
    [Tooltip("A distância que o jogador avança (lunge) durante este golpe.")]
    public float lungeDistance;

    [Tooltip("A janela de tempo (em segundos), após o início deste golpe, dentro da qual o próximo ataque pode ser iniciado para continuar o combo.")]
    public float comboWindow = 0.5f;
}