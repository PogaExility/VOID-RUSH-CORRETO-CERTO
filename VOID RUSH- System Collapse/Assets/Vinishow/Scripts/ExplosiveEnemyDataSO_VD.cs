using UnityEngine;

[CreateAssetMenu(fileName = "NovoInimigoExplosivoData", menuName = "IA_VD/Configuração de Inimigo Explosivo")]
public class ExplosiveEnemyDataSO_VD : ScriptableObject
{
    [Header("Comportamento de Patrulha")]
    [Tooltip("A velocidade do inimigo quando está patrulhando.")]
    public float patrolSpeed = 1.5f;

    [Header("Comportamento de Combate")]
    [Tooltip("O raio de detecção. Quando o jogador entra neste círculo, o inimigo entra em alerta.")]
    public float alertRadius = 5f;

    [Tooltip("O raio onde a contagem regressiva da explosão começa.")]
    public float explosionRadius = 3f;

    [Tooltip("O tempo em segundos que o inimigo fica em alerta antes de explodir.")]
    public float fuseTime = 1.5f;

    [Header("Atributos da Explosão")]
    [Tooltip("A quantidade de dano que a explosão causa.")]
    public float explosionDamage = 50f;

    [Tooltip("A força de repulsão (knockback) que a explosão aplica.")]
    public float explosionKnockback = 15f;

    [Tooltip("Arraste aqui o prefab do efeito visual da explosão.")]
    public GameObject explosionPrefab;
}