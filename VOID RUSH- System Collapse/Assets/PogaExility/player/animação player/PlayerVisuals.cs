// PlayerVisuals.cs - Vers�o completa com sistema de partes e interruptor de ativa��o
using System.Collections;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("CONTROLE GERAL")]
    [Tooltip("Marque esta caixa para desativar a anima��o por c�digo e usar o Animator da Unity. Desmarque quando o sistema de partes estiver pronto.")]
    public bool useUnityAnimator = true; // O INTERRUPTOR PRINCIPAL

    [Header("Partes do Corpo Equipadas")]
    [Tooltip("O SO que define os sprites da cabe�a.")]
    public BodyPartSO headPart;
    [Tooltip("O SO que define os sprites do torso.")]
    public BodyPartSO torsoPart;
    [Tooltip("O SO que define os sprites do bra�o.")]
    public BodyPartSO armPart;
    [Tooltip("O SO que define os sprites da perna.")]
    public BodyPartSO legPart;

    [Header("Renderers das Partes")]
    [Tooltip("O Sprite Renderer do GameObject da cabe�a.")]
    public SpriteRenderer headRenderer;
    [Tooltip("O Sprite Renderer do GameObject do torso.")]
    public SpriteRenderer torsoRenderer;
    [Tooltip("O Sprite Renderer do GameObject do bra�o.")]
    public SpriteRenderer armRenderer;
    [Tooltip("O Sprite Renderer do GameObject da perna.")]
    public SpriteRenderer legRenderer;

    [Header("Configura��es da Anima��o")]
    [Tooltip("A velocidade da anima��o de andar (segundos por frame).")]
    [SerializeField] private float walkFrameRate = 0.1f;

    // --- Controle Interno ---
    private Coroutine walkAnimationCoroutine;
    private Coroutine runAnimationCoroutine; // Adicionado para futura anima��o de corrida

    /// <summary>
    /// O m�todo principal que o PlayerController chama para atualizar a apar�ncia.
    /// Ele s� funciona se o 'useUnityAnimator' estiver desmarcado.
    /// </summary>
    public void UpdateVisualState(string state, float yVelocity)
    {
        // Se o interruptor estiver ligado, o script n�o faz absolutamente NADA.
        if (useUnityAnimator)
        {
            return;
        }

        // Garante que temos um SO para cada parte antes de tentar animar.
        if (armPart == null || legPart == null) // Checagem b�sica
        {
            // Se n�o tivermos as partes, n�o faz nada para evitar erros.
            return;
        }

        // Para qualquer anima��o em andamento baseada em tempo (como a de andar/correr)
        StopAllTimedAnimations();

        // Decide qual anima��o/sprite mostrar com base no estado
        switch (state)
        {
            case "andando":
                walkAnimationCoroutine = StartCoroutine(PlayAnimationCycle(armPart.walkCycle, legPart.walkCycle, walkFrameRate));
                break;

            // Adicione aqui outros estados que usam ciclos de anima��o, como "correndo"
            // case "correndo":
            //     runAnimationCoroutine = StartCoroutine(PlayAnimationCycle(armPart.runCycle, legPart.runCycle, runFrameRate));
            //     break;

            case "pulando":
                SetStaticSprite(yVelocity > 0.1f ? armPart.jumpSprite : armPart.fallSprite,
                                yVelocity > 0.1f ? legPart.jumpSprite : legPart.fallSprite);
                break;

            case "derrapagem":
                SetStaticSprite(armPart.wallSlideSprite, legPart.wallSlideSprite);
                break;

            case "parado":
            default:
                SetStaticSprite(armPart.idleSprite, legPart.idleSprite);
                break;
        }
    }

    /// <summary>
    /// Para todas as coroutines de anima��o para evitar que elas rodem ao mesmo tempo.
    /// </summary>
    private void StopAllTimedAnimations()
    {
        if (walkAnimationCoroutine != null)
        {
            StopCoroutine(walkAnimationCoroutine);
            walkAnimationCoroutine = null;
        }
        if (runAnimationCoroutine != null)
        {
            StopCoroutine(runAnimationCoroutine);
            runAnimationCoroutine = null;
        }
    }

    /// <summary>
    /// Define um �nico sprite est�tico para as partes do corpo.
    /// </summary>
    private void SetStaticSprite(Sprite armSprite, Sprite legSprite)
    {
        if (armRenderer != null) armRenderer.sprite = armSprite;
        if (legRenderer != null) legRenderer.sprite = legSprite;
        // Adicione aqui o torso e a cabe�a se eles tamb�m tiverem sprites est�ticos para essa a��o
    }

    /// <summary>
    /// Uma coroutine que toca um ciclo de anima��o para m�ltiplas partes do corpo.
    /// </summary>
    private IEnumerator PlayAnimationCycle(Sprite[] armCycle, Sprite[] legCycle, float frameRate)
    {
        // Valida��o para evitar erros se os arrays estiverem vazios
        if (armCycle == null || armCycle.Length == 0) yield break;
        if (legCycle == null || legCycle.Length == 0) yield break;

        int currentFrame = 0;
        while (true)
        {
            // Garante que o �ndice n�o saia do alcance de cada array individualmente
            if (armRenderer != null) armRenderer.sprite = armCycle[currentFrame % armCycle.Length];
            if (legRenderer != null) legRenderer.sprite = legCycle[currentFrame % legCycle.Length];

            currentFrame++;
            yield return new WaitForSeconds(frameRate);
        }
    }
}