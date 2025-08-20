using UnityEngine;

// O menuName aqui cria um sub-menu "Arma" dentro do menu "Itens".
[CreateAssetMenu(fileName = "NovaArma", menuName = "NEXUS/Itens/Arma", order = 1)]
public class WeaponSO : ItemSO
{
    [Header("Configura��o de Combate")]
    [Tooltip("Define o tipo de arma e quais campos aparecer�o abaixo.")]
    public WeaponType weaponType;

    [Tooltip("O dano base do ataque principal desta arma.")]
    public float damage;

    // ===== IN�CIO DA ALTERA��O 1: NOVOS CAMPOS GERAIS =====
    [Tooltip("O tempo em segundos entre ataques b�sicos (cad�ncia de tiro).")]
    public float attackRate = 0.5f;
    [Tooltip("Marque se esta arma deve usar o sistema de mira com o mouse.")]
    public bool useAimMode = false;
    // ===== FIM DA ALTERA��O 1 =====

    // --- Campos Espec�ficos do Tipo de Arma ---

    [Header("Exclusivo: Corpo a Corpo (Melee)")]
    [Tooltip("As anima��es para o combo de 3 ataques. Deixe vazio se for um ataque �nico.")]
    public AnimationClip[] comboAnimations;
    // ===== IN�CIO DA ALTERA��O 2: CAMPO DO PROJ�TIL DE CORTE =====
    [Tooltip("O prefab do proj�til de corte que ser� instanciado.")]
    public GameObject slashEffectPrefab;
    // ===== FIM DA ALTERA��O 2 =====

    [Header("Exclusivo: Arma de Fogo (Firearm)")]
    [Tooltip("Quantas balas cabem no pente.")]
    public int magazineSize;
    [Tooltip("Tempo em segundos para recarregar a arma.")]
    public float reloadTime;
    // ===== IN�CIO DA ALTERA��O 3: CAMPO DO PROJ�TIL DA BALA =====
    [Tooltip("O prefab da bala que ser� disparada.")]
    public GameObject bulletPrefab;
    // ===== FIM DA ALTERA��O 3 =====

    [Header("Exclusivo: Buster")]
    [Tooltip("Tempo em segundos para atingir a carga m�xima.")]
    public float chargeTime;
    [Tooltip("Custo de energia por segundo enquanto o tiro est� sendo carregado.")]
    public float energyCostPerChargeSecond;
    [Tooltip("Custo de energia para um tiro r�pido (sem carregar).")]
    public float baseEnergyCost;
    // ===== IN�CIO DA ALTERA��O 4: CAMPOS DOS PROJ�TEIS DO BUSTER =====
    [Tooltip("O prefab do tiro b�sico do Buster.")]
    public GameObject busterShotPrefab;
    [Tooltip("O prefab do tiro carregado do Buster.")]
    public GameObject chargedShotPrefab;
    [Tooltip("O dano espec�fico do tiro carregado (se for diferente do dano base).")]
    public float chargedShotDamage;
    // ===== FIM DA ALTERA��O 4 =====
}