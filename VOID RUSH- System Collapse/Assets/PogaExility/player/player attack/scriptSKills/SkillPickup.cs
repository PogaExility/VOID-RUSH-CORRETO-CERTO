using UnityEngine;

public class SkillPickup : MonoBehaviour
{
    [Header("Configurações do Item")]
    [Tooltip("Arraste aqui o SkillSO que este chip vai desbloquear.")]
    public SkillSO skillToUnlock;

    [Header("Feedback")]
    [Tooltip("Som ao pegar o item (Opcional).")]
    public AudioClip pickupSound;
    [Tooltip("Partícula/Efeito visual ao pegar (Opcional).")]
    public GameObject pickupVFX;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se quem entrou na área foi o Player
        // Tenta pegar o componente PlayerController
        PlayerController player = collision.GetComponent<PlayerController>();

        // Se encontrou o script do Player
        if (player != null)
        {
            CollectItem(player);
        }
    }

    private void CollectItem(PlayerController player)
    {
        if (skillToUnlock != null)
        {
            // Manda o Player equipar a skill contida neste chip
            player.EquipSkill(skillToUnlock);

            // Feedback Sonoro
            if (pickupSound != null && AudioManager.Instance != null)
            {
                // Toca o som no local do objeto
                AudioManager.Instance.PlaySoundEffect(pickupSound, transform.position);
            }

            // Feedback Visual (instancia uma partícula e a destrói depois de 2s)
            if (pickupVFX != null)
            {
                GameObject vfx = Instantiate(pickupVFX, transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }
        }
        else
        {
            Debug.LogWarning($"O objeto '{gameObject.name}' é um SkillPickup mas não tem nenhum SkillSO atribuído!");
        }

        // Destrói o objeto do chip da cena
        Destroy(gameObject);
    }
}