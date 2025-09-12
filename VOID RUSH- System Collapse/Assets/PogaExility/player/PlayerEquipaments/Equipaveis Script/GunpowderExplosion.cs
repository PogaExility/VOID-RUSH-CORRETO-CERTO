// GunpowderExplosion.cs - VERS�O COMPLETA E CORRIGIDA
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(ProjectileAnimatorController))]
public class GunpowderExplosion : MonoBehaviour
{
    // Vari�veis para guardar os dados recebidos da arma
    private float damage;
    private float radius;
    [SerializeField] private LayerMask enemyLayer;

    void Start()
    {
        // A primeira coisa que ele faz � pedir ao maestro para tocar a anima��o "polvora".
        GetComponent<ProjectileAnimatorController>().PlayAnimation("polvora");
    }

    /// <summary>
    /// ESTA � A FUN��O QUE ESTAVA FALTANDO. 
    /// A RangedWeapon chama esta fun��o para passar os dados de dano e raio.
    /// </summary>
    public void Initialize(float damageAmount, float explosionRadius)
    {
        this.damage = damageAmount;
        this.radius = explosionRadius;
    }

    /// <summary>
    /// Esta fun��o causa o dano. Ela ser� chamada pelo Animation Event.
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
    /// Esta fun��o destr�i o objeto. Ela ser� chamada pelo Animation Event no final.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }

}