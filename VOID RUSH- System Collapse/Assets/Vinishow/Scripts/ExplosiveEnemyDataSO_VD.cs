using UnityEngine;

[CreateAssetMenu(fileName = "NovoInimigoExplosivoData", menuName = "IA_VD/Configura��o de Inimigo Explosivo")]
public class ExplosiveEnemyDataSO_VD : ScriptableObject
{
    [Header("Comportamento de Patrulha")]
    [Tooltip("A velocidade do inimigo quando est� patrulhando.")]
    public float patrolSpeed = 1.5f;

    [Header("Comportamento de Combate")]
    [Tooltip("O raio de detec��o. Quando o jogador entra neste c�rculo, o inimigo entra em alerta.")]
    public float alertRadius = 5f;

    [Tooltip("O raio onde a contagem regressiva da explos�o come�a.")]
    public float explosionRadius = 3f;

    [Tooltip("O tempo em segundos que o inimigo fica em alerta antes de explodir.")]
    public float fuseTime = 1.5f;

    [Header("Atributos da Explos�o")]
    [Tooltip("A quantidade de dano que a explos�o causa.")]
    public float explosionDamage = 50f;

    [Tooltip("A for�a de repuls�o (knockback) que a explos�o aplica.")]
    public float explosionKnockback = 15f;

    [Tooltip("Arraste aqui o prefab do efeito visual da explos�o.")]
    public GameObject explosionPrefab;
}