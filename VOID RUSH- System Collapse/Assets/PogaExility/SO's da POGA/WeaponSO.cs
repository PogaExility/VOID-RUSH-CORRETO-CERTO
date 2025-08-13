using UnityEngine;

// O menuName aqui cria um sub-menu "Arma" dentro do menu "Itens".
[CreateAssetMenu(fileName = "NovaArma", menuName = "NEXUS/Itens/Arma", order = 1)]
public class WeaponSO : ItemSO
{
    [Header("Configuração de Combate")]
    [Tooltip("Define o tipo de arma e quais campos aparecerão abaixo.")]
    public WeaponType weaponType;

    [Tooltip("O dano base do ataque principal desta arma.")]
    public float damage;

    // --- Campos Específicos do Tipo de Arma ---
    // Estes campos só aparecerão no Inspector se o WeaponType correspondente for selecionado.
    // A lógica para isso será feita no editor script (WeaponSOEditor.cs) que faremos a seguir.

    [Header("Exclusivo: Corpo a Corpo (Melee)")]
    [Tooltip("As animações para o combo de 3 ataques. Deixe vazio se for um ataque único.")]
    public AnimationClip[] comboAnimations;

    [Header("Exclusivo: Arma de Fogo (Firearm)")]
    [Tooltip("Quantas balas cabem no pente.")]
    public int magazineSize;
    [Tooltip("Tempo em segundos para recarregar a arma.")]
    public float reloadTime;

    [Header("Exclusivo: Buster")]
    [Tooltip("Tempo em segundos para atingir a carga máxima.")]
    public float chargeTime;
    [Tooltip("Custo de energia por segundo enquanto o tiro está sendo carregado.")]
    public float energyCostPerChargeSecond;
    [Tooltip("Custo de energia para um tiro rápido (sem carregar).")]
    public float baseEnergyCost;
}