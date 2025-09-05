using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUDView : MonoBehaviour
{
    [Header("Refer�ncias da UI")]
    public Image weaponIcon;
    public TextMeshProUGUI ammoText;
    public Slider cooldownSlider; // Exemplo

    // >> A REFER�NCIA CORRETA <<
    [Header("Refer�ncias do Jogo")]
    [SerializeField] private WeaponHandler weaponHandler;

    void Start()
    {
        // Garante que a refer�ncia existe
        if (weaponHandler == null)
            weaponHandler = FindFirstObjectByType<WeaponHandler>();
        if (weaponHandler == null)
        {
            Debug.LogError("WeaponHandler n�o encontrado na cena!", this);
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

    // A fun��o que redesenha a hotbar
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
                    // L�gica futura vir� do WeaponHandler
                    ammoText.text = $"MUNI��O: -- / {weapon.magazineSize}";
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