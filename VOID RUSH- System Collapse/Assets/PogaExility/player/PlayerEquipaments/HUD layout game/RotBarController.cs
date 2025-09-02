using UnityEngine;
using System;
using UnityEngine.UI; // <-- ADICIONE ESTA LINHA AQUI

public class RotBarController : MonoBehaviour
{
    public SkillSlotView[] skillSlots; // Arraste os 5 slots aqui no Inspector
    public WeaponWidget weaponWidget;
    public QuickUseSlotView[] quickUseSlots; // Arraste os 3 slots aqui

    public event Action<int> OnSkillPressed;
    public event Action<int> OnQuickPressed;
    public void TriggerSkill(int skillIndex)
    {
        OnSkillPressed?.Invoke(skillIndex);
    }

    public void TriggerQuickUse(int quickIndex)
    {
        OnQuickPressed?.Invoke(quickIndex);
    }
    void Awake()
    {
        // Conectar os eventos dos placeholders de UI ao evento principal
        for (int i = 0; i < skillSlots.Length; i++)
        {
            int skillIndex = i + 1;
            // A linha abaixo usa o tipo 'Button', que agora será reconhecido
            skillSlots[i].GetComponent<Button>().onClick.AddListener(() => OnSkillPressed?.Invoke(skillIndex));
        }

        for (int i = 0; i < quickUseSlots.Length; i++)
        {
            int quickIndex = i + 1;
            // A linha abaixo usa o tipo 'Button', que agora será reconhecido
            quickUseSlots[i].GetComponent<Button>().onClick.AddListener(() => OnQuickPressed?.Invoke(quickIndex));
        }
    }

    public void UpdateWeaponWidget(ItemSO weapon)
    {
        weaponWidget.UpdateWeapon(weapon);
    }

    // TODO: Adicionar métodos para atualizar os ícones e cooldowns dos slots
}

// Estes scripts podem ser arquivos separados ou estar no mesmo arquivo do RotBarController
public class WeaponWidget : MonoBehaviour
{
    public Image weaponIcon;
    public Text ammoText;
    public Image cooldownBar;

    public void UpdateWeapon(ItemSO weapon)
    {
        if (weapon != null)
        {
            weaponIcon.sprite = weapon.itemIcon;
            weaponIcon.enabled = true;
            // TODO: Lógica para mostrar ammo/cooldown com base no weapon.weaponType
            ammoText.text = "10/10";
            cooldownBar.fillAmount = 0.5f;
        }
        else
        {
            weaponIcon.enabled = false;
            ammoText.text = "";
            cooldownBar.fillAmount = 0;
        }
    }
}

public class SkillSlotView : MonoBehaviour
{
    public Image icon;
    public Text numberText;
    public Image cooldownOverlay;
}

public class QuickUseSlotView : MonoBehaviour
{
    public Image icon;
}