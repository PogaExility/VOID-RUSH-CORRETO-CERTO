using UnityEngine;
using System.Collections;

public class PlayerAttack : MonoBehaviour
{
    [Header("Referências de Componentes")]
    private CombatController combatController;
    private InventoryManager inventoryManager;
    private PlayerStats playerStats;
    [Header("Pivots de Mira (IK)")]
    public GameObject headPivotObject;
    public GameObject rightArmPivotObject;

    private bool isAiming = false;
    private int meleeComboCount = 0;
    private float lastAttackTime = 0f;
    private const float COMBO_RESET_TIME = 1.0f;
    private Coroutine busterChargeCoroutine;
    private float currentChargeTime = 0f;
    [Header("Sprites do Player")]
    public SpriteRenderer fullBodySprite; // Para o sprite do corpo completo
    public SpriteRenderer torsoOnlySprite; // Para o sprite do corpo "cotoco"
    void Awake()
    {
        // Use GetComponentInParent para mais flexibilidade na hierarquia
        combatController = GetComponentInParent<CombatController>();

        if (headPivotObject != null)
        {
            //headPivotTransform = headPivotObject.transform;
        }
        else
        {
            Debug.LogError("Referência 'headPivotObject' não está definida no PlayerAttack!", this);
        }

        if (rightArmPivotObject != null)
        {
           // rightArmPivotTransform = rightArmPivotObject.transform;
        }
        else
        {
            Debug.LogError("Referência 'rightArmPivotObject' não está definida no PlayerAttack!", this);
        }

        // Garante que eles comecem desativados
        SetAiming(false);
    }


    void LateUpdate()
    {
        // A lógica de rotação só deve acontecer se estiver mirando E as referências existirem
        if (isAiming && combatController != null)
        {
            Vector3 aimDir = combatController.aimDirection;
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

           // if (headPivotTransform != null) headPivotTransform.rotation = Quaternion.Euler(0, 0, angle);
            //if (rightArmPivotTransform != null) rightArmPivotTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    public void SetAiming(bool aiming)
    {
        isAiming = aiming;

        // --- LÓGICA CORRIGIDA E SEGURA ---
        // A checagem '!= null' garante que o código não quebre se você esquecer de arrastar a referência.
        if (headPivotObject != null)
        {
            headPivotObject.SetActive(aiming);
        }

        if (rightArmPivotObject != null)
        {
            rightArmPivotObject.SetActive(aiming);
        }
    }

    // --- FUNÇÕES DE ATAQUE ---

    public void PerformAttack(ItemSO weapon, Vector3 aimDirection)
    {
        if (Time.time < lastAttackTime + weapon.attackRate) return;

        if (weapon.weaponType == WeaponType.Melee)
        {
            ExecuteMeleeCombo(weapon);
        }
        else if (weapon.weaponType == WeaponType.Ranger)
        {
            ExecuteFirearmShot(weapon, aimDirection);
        }

        lastAttackTime = Time.time;
    }

    private void ExecuteMeleeCombo(ItemSO weapon)
    {
        if (Time.time > lastAttackTime + COMBO_RESET_TIME)
        {
            meleeComboCount = 0;
        }

        int animationIndex = meleeComboCount % weapon.comboAnimations.Length;
        Debug.Log($"Ataque Melee Combo {animationIndex + 1}");
        // TODO: Chamar animação

        meleeComboCount++;
    }

    private void ExecuteFirearmShot(ItemSO weapon, Vector3 aimDirection)
    {
        Debug.Log("Atirou com Firearm!");
        // TODO: Lógica de munição e instanciar projétil
    }

    public void StartBusterCharge(ItemSO weapon)
    {
        if (busterChargeCoroutine != null) return;
        currentChargeTime = 0f;
        busterChargeCoroutine = StartCoroutine(BusterChargeRoutine(weapon));
    }

    private IEnumerator BusterChargeRoutine(ItemSO weapon)
    {
        Debug.Log("Carregando Buster...");
        while (Input.GetButton("Fire1") && currentChargeTime < weapon.chargeTime)
        {
            currentChargeTime += Time.deltaTime;
            // TODO: Drenar energia
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
            Debug.Log("Tiro Carregado do Buster!");
        }
        else
        {
            Debug.Log("Tiro Padrão do Buster!");
        }
        currentChargeTime = 0f;
        lastAttackTime = Time.time;
    }
}