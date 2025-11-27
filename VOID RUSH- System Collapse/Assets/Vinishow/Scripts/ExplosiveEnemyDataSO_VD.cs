using UnityEngine;

[CreateAssetMenu(fileName = "NovoInimigoExplosivoData", menuName = "IA_VD/Configuração de Inimigo Explosivo")]
public class ExplosiveEnemyDataSO_VD : ScriptableObject
{
    [Header("Comportamento de Patrulha")]
    public float patrolSpeed = 1.5f;

    [Header("Comportamento de Combate")]
    public float alertRadius = 5f;
    public float explosionRadius = 3f;
    public float fuseTime = 1.5f;

    [Header("Atributos da Explosão")]
    public float explosionDamage = 50f;
    public float explosionKnockback = 15f;
    public GameObject explosionPrefab;

    // --- SEÇÃO ADICIONADA ---
    [Header("Nomes dos Estados de Animação")]
    [Tooltip("O nome EXATO do estado 'Patrol' que você configurou no CustomSpriteAnimatorVD.")]
    public string patrolStateName = "Patrol";

    [Tooltip("O nome EXATO do estado 'Alert' que você configurou no CustomSpriteAnimatorVD.")]
    public string alertStateName = "Alert";

    [Tooltip("O nome EXATO do estado 'Explode' que você configurou no CustomSpriteAnimatorVD.")]
    public string explodeStateName = "Explode";
}