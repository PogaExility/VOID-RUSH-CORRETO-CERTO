using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerAttack : MonoBehaviour
{
    private CombatController combatController;
    private InventoryManager inventoryManager;
    private PlayerStats playerStats;

    // Lógica de Combate Melee
    private int meleeComboCount = 0;
    private float lastAttackTime = 0f;
    private const float COMBO_RESET_TIME = 1.0f; // Tempo para resetar o combo

    // Lógica do Buster
    private Coroutine busterChargeCoroutine;
    private float currentChargeTime = 0f;
    private ItemSO chargingBuster;

    // Referências do player (se necessário para animação/IK)
    public Transform headPivot;
    public Transform rightArmPivot;
    private bool isAiming = false;
    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
        // Opcional: Ativar/desativar os sprites dos braços/cabeça aqui
    }

    void Awake()
    {
        combatController = GetComponent<CombatController>();
        inventoryManager = GetComponent<InventoryManager>();
        playerStats = GetComponent<PlayerStats>();
    }

    // Chamado pelo CombatController no MOUSE DOWN (ou botão de ataque)
    public void PerformAttack(ItemSO weapon, Vector3 aimDirection)
    {
        if (weapon.attackRate > Time.time - lastAttackTime) return; // Cooldown de ataque

        if (weapon.weaponType == WeaponType.Melee)
        {
            ExecuteMeleeCombo(weapon);
        }
        else if (weapon.weaponType == WeaponType.Firearm)
        {
            ExecuteFirearmShot(weapon, aimDirection);
        }

        lastAttackTime = Time.time;
    }

    // --- LÓGICA DE MELEE COMBO (REQUISITO: 3+ ANIMÇÕES) ---
    private void ExecuteMeleeCombo(ItemSO weapon)
    {
        if (Time.time > lastAttackTime + COMBO_RESET_TIME)
        {
            meleeComboCount = 0; // Reseta se demorou demais
        }

        // Garante que o contador não exceda o número de animações
        int animationIndex = meleeComboCount % weapon.comboAnimations.Length;

        // TODO: Chamar a função de animação com base no animationIndex
        Debug.Log($"Ataque Melee Combo {animationIndex + 1} de {weapon.comboAnimations.Length}");

        // TODO: Instanciar slashEffectPrefab no ponto de impacto

        meleeComboCount++;
    }

    // --- LÓGICA DE FIREARM (ARMA DE FOGO) ---
    private void ExecuteFirearmShot(ItemSO weapon, Vector3 aimDirection)
    {
        // TODO: Lógica de munição real deve ser integrada aqui, usando o InventoryManager

        // Placeholder: Dispara o projétil
        if (weapon.bulletPrefab != null)
        {
            // Instancia o projétil na ponta da arma (use a direção de mira)
            Instantiate(weapon.bulletPrefab, transform.position, Quaternion.identity);
            Debug.Log($"Firearm disparado. Dano: {weapon.damage}");
        }
    }

    // --- LÓGICA DO BUSTER (CARREGAMENTO) ---
    public void StartBusterCharge(ItemSO weapon)
    {
        if (busterChargeCoroutine != null) return;
        currentChargeTime = 0f;
        chargingBuster = weapon;
        busterChargeCoroutine = StartCoroutine(BusterChargeRoutine(weapon));
        Debug.Log("Buster: Começando a carregar...");
    }

    private IEnumerator BusterChargeRoutine(ItemSO weapon)
    {
        // TODO: Drenar energia do PlayerStats
        while (Input.GetButton("Fire1") && currentChargeTime < weapon.chargeTime)
        {
            currentChargeTime += Time.deltaTime;
            // Drenar energyCostPerChargeSecond do playerStats a cada frame
            Debug.Log($"Buster Carregando: {currentChargeTime:F2}s");
            yield return null;
        }
    }

    public void ReleaseBusterCharge(ItemSO weapon, Vector3 aimDirection)
    {
        if (busterChargeCoroutine == null) return;
        StopCoroutine(busterChargeCoroutine);
        busterChargeCoroutine = null;

        if (currentChargeTime >= weapon.chargeTime)
        {
            // Tiro Carregado
            // TODO: Instanciar chargedShotPrefab
            Debug.Log($"Buster: TIRO CARREGADO LIBERADO! Dano: {weapon.chargedShotDamage}");
        }
        else
        {
            // Tiro Padrão
            // TODO: Instanciar busterShotPrefab
            Debug.Log($"Buster: Tiro padrão liberado. Dano: {weapon.damage}");
        }
        currentChargeTime = 0f;
    }

    // --- LÓGICA DE MIRA E PIVOTAMENTO (A SER INTEGRADA NO PRÓXIMO PASSO) ---
    // A lógica de IK para braços e cabeça será implementada na função LateUpdate
    // ou no componente de animação, dependendo da sua estrutura de IK.

    public void Reload(ItemSO firearmWeapon)
    {
        // Lógica de Recarga
        Debug.Log("Recarregando " + firearmWeapon.itemName);
    }
    void LateUpdate()
    {
        if (isAiming)
        {
            // Pega a direção de mira do CombatController
            Vector3 aimDir = combatController.aimDirection;

            // Calcula o ângulo em graus
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

            // Aplica a rotação aos pivots
            if (headPivot != null) headPivot.rotation = Quaternion.Euler(0, 0, angle);
            if (rightArmPivot != null) rightArmPivot.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
