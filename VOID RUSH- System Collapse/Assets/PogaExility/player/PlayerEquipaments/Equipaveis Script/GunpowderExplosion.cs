// GunpowderExplosion.cs - VERSÃO COMPLETA E CORRIGIDA
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(ProjectileAnimatorController))]
public class GunpowderExplosion : MonoBehaviour
{
    // Variáveis para guardar os dados recebidos da arma
    private float damage;
    private float radius;
    [SerializeField] private LayerMask enemyLayer;

    void Start()
    {
        // A primeira coisa que ele faz é pedir ao maestro para tocar a animação "polvora".
        GetComponent<ProjectileAnimatorController>().PlayAnimation("polvora");
    }

    /// <summary>
    /// ESTA É A FUNÇÃO QUE ESTAVA FALTANDO. 
    /// A RangedWeapon chama esta função para passar os dados de dano e raio.
    /// </summary>
    public void Initialize(float damageAmount, float explosionRadius)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
    }

    /// <summary>
    /// Esta função causa o dano. Ela será chamada pelo Animation Event.
    /// </summary>
    public void TriggerDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<AIController_Basic>(out AIController_Basic enemy))
            {
                Vector2 knockbackDirection = (hit.transform.position - transform.position).normalized;
                enemy.TakeDamage(this.damage, knockbackDirection);
            }
        }
    }

    /// <summary>
    /// Esta função destrói o objeto. Ela será chamada pelo Animation Event no final.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

}