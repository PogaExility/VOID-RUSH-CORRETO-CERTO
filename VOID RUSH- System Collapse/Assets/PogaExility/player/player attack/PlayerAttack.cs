using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject attackPrefab; // Prefab do ataque (projetil de curto alcance)
    public Transform attackPoint;   // Ponto de origem do ataque
    public float attackDuration = 0.3f; // Tempo que o ataque fica ativo

    private GameObject currentAttack;

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) // Botão de ataque padrão (mouse esquerdo)
        {
            Attack();
        }
    }

    void Attack()
    {
        if (currentAttack == null)
        {
            currentAttack = Instantiate(attackPrefab, attackPoint.position, attackPoint.rotation);
            Destroy(currentAttack, attackDuration);
        }
    }
}
