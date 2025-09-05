using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUDView : MonoBehaviour
{
    [Header("Referências da UI")]
    public Image weaponIcon;
    public TextMeshProUGUI ammoText;
    public Slider cooldownSlider; // Exemplo

    // >> A REFERÊNCIA CORRETA <<
    [Header("Referências do Jogo")]
    [SerializeField] private WeaponHandler weaponHandler;

    void Start()
    {
        // Garante que a referência existe
        if (weaponHandler == null)
            weaponHandler = FindFirstObjectByType<WeaponHandler>();
        if (weaponHandler == null)
        {
            Debug.LogError("WeaponHandler não encontrado na cena!", this);
            enabled = false;
            return;
        }

        // Se inscreve para ouvir quando a arma ativa muda
        weaponHandler.OnActiveWeaponChanged += UpdateView;
        UpdateView(weaponHandler.currentWeaponIndex); // Desenho inicial
    }

    private void OnDestroy()
    {
        if (weaponHandler != null)
            weaponHandler.OnActiveWeaponChanged -= UpdateView;
    }

    // A função que redesenha a hotbar
    void UpdateView(int activeIndex)
    {
        var activeWeaponSlot = weaponHandler.GetActiveWeaponSlot(); // Pede ao WeaponHandler

        if (activeWeaponSlot != null && activeWeaponSlot.item != null)
        {
            weaponIcon.enabled = true;
            weaponIcon.sprite = activeWeaponSlot.item.itemIcon;

            var weapon = activeWeaponSlot.item;
            ammoText.enabled = false;
            cooldownSlider.gameObject.SetActive(false);

            switch (weapon.weaponType)
            {
                case WeaponType.Ranger:
                    ammoText.enabled = true;
                    // Lógica futura virá do WeaponHandler
                    ammoText.text = $"MUNIÇÃO: -- / {weapon.magazineSize}";
                    break;
                    // ... outros casos
            }
        }
        else
        {
            weaponIcon.enabled = false;
            ammoText.enabled = false;
            cooldownSlider.gameObject.SetActive(false);
        }
    }
}