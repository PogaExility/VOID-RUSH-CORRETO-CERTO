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

    // ===== INÍCIO DA ALTERAÇÃO 1: NOVOS CAMPOS GERAIS =====
    [Tooltip("O tempo em segundos entre ataques básicos (cadência de tiro).")]
    public float attackRate = 0.5f;
    [Tooltip("Marque se esta arma deve usar o sistema de mira com o mouse.")]
    public bool useAimMode = false;
    // ===== FIM DA ALTERAÇÃO 1 =====

    // --- Campos Específicos do Tipo de Arma ---

    [Header("Exclusivo: Corpo a Corpo (Melee)")]
    [Tooltip("As animações para o combo de 3 ataques. Deixe vazio se for um ataque único.")]
    public AnimationClip[] comboAnimations;
    // ===== INÍCIO DA ALTERAÇÃO 2: CAMPO DO PROJÉTIL DE CORTE =====
    [Tooltip("O prefab do projétil de corte que será instanciado.")]
    public GameObject slashEffectPrefab;
    // ===== FIM DA ALTERAÇÃO 2 =====

    [Header("Exclusivo: Arma de Fogo (Firearm)")]
    [Tooltip("Quantas balas cabem no pente.")]
    public int magazineSize;
    [Tooltip("Tempo em segundos para recarregar a arma.")]
    public float reloadTime;
    // ===== INÍCIO DA ALTERAÇÃO 3: CAMPO DO PROJÉTIL DA BALA =====
    [Tooltip("O prefab da bala que será disparada.")]
    public GameObject bulletPrefab;
    // ===== FIM DA ALTERAÇÃO 3 =====

    [Header("Exclusivo: Buster")]
    [Tooltip("Tempo em segundos para atingir a carga máxima.")]
    public float chargeTime;
    [Tooltip("Custo de energia por segundo enquanto o tiro está sendo carregado.")]
    public float energyCostPerChargeSecond;
    [Tooltip("Custo de energia para um tiro rápido (sem carregar).")]
    public float baseEnergyCost;
    // ===== INÍCIO DA ALTERAÇÃO 4: CAMPOS DOS PROJÉTEIS DO BUSTER =====
    [Tooltip("O prefab do tiro básico do Buster.")]
    public GameObject busterShotPrefab;
    [Tooltip("O prefab do tiro carregado do Buster.")]
    public GameObject chargedShotPrefab;
    [Tooltip("O dano específico do tiro carregado (se for diferente do dano base).")]
    public float chargedShotDamage;
    // ===== FIM DA ALTERAÇÃO 4 =====
}