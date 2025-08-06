// PlayerVisuals.cs - Versão completa com sistema de partes e interruptor de ativação
using System.Collections;
using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [Header("CONTROLE GERAL")]
    [Tooltip("Marque esta caixa para desativar a animação por código e usar o Animator da Unity. Desmarque quando o sistema de partes estiver pronto.")]
    public bool useUnityAnimator = true; // O INTERRUPTOR PRINCIPAL

    [Header("Partes do Corpo Equipadas")]
    [Tooltip("O SO que define os sprites da cabeça.")]
    public BodyPartSO headPart;
    [Tooltip("O SO que define os sprites do torso.")]
    public BodyPartSO torsoPart;
    [Tooltip("O SO que define os sprites do braço.")]
    public BodyPartSO armPart;
    [Tooltip("O SO que define os sprites da perna.")]
    public BodyPartSO legPart;

    [Header("Renderers das Partes")]
    [Tooltip("O Sprite Renderer do GameObject da cabeça.")]
    public SpriteRenderer headRenderer;
    [Tooltip("O Sprite Renderer do GameObject do torso.")]
    public SpriteRenderer torsoRenderer;
    [Tooltip("O Sprite Renderer do GameObject do braço.")]
    public SpriteRenderer armRenderer;
    [Tooltip("O Sprite Renderer do GameObject da perna.")]
    public SpriteRenderer legRenderer;

    [Header("Configurações da Animação")]
    [Tooltip("A velocidade da animação de andar (segundos por frame).")]
    [SerializeField] private float walkFrameRate = 0.1f;

    // --- Controle Interno ---
    private Coroutine walkAnimationCoroutine;
    private Coroutine runAnimationCoroutine; // Adicionado para futura animação de corrida

    /// <summary>
    /// O método principal que o PlayerController chama para atualizar a aparência.
    /// Ele só funciona se o 'useUnityAnimator' estiver desmarcado.
    /// </summary>
    public void UpdateVisualState(string state, float yVelocity)
    {
        // Se o interruptor estiver ligado, o script não faz absolutamente NADA.
        if (useUnityAnimator)
        {
            return;
        }

        // Garante que temos um SO para cada parte antes de tentar animar.
        if (armPart == null || legPart == null) // Checagem básica
        {
            // Se não tivermos as partes, não faz nada para evitar erros.
            return;
        }

        // Para qualquer animação em andamento baseada em tempo (como a de andar/correr)
        StopAllTimedAnimations();

        // Decide qual animação/sprite mostrar com base no estado
        switch (state)
        {
            case "andando":
                walkAnimationCoroutine = StartCoroutine(PlayAnimationCycle(armPart.walkCycle, legPart.walkCycle, walkFrameRate));
                break;

            // Adicione aqui outros estados que usam ciclos de animação, como "correndo"
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
    /// Para todas as coroutines de animação para evitar que elas rodem ao mesmo tempo.
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
    /// Define um único sprite estático para as partes do corpo.
    /// </summary>
    private void SetStaticSprite(Sprite armSprite, Sprite legSprite)
    {
        if (armRenderer != null) armRenderer.sprite = armSprite;
        if (legRenderer != null) legRenderer.sprite = legSprite;
        // Adicione aqui o torso e a cabeça se eles também tiverem sprites estáticos para essa ação
    }

    /// <summary>
    /// Uma coroutine que toca um ciclo de animação para múltiplas partes do corpo.
    /// </summary>
    private IEnumerator PlayAnimationCycle(Sprite[] armCycle, Sprite[] legCycle, float frameRate)
    {
        // Validação para evitar erros se os arrays estiverem vazios
        if (armCycle == null || armCycle.Length == 0) yield break;
        if (legCycle == null || legCycle.Length == 0) yield break;

        int currentFrame = 0;
        while (true)
        {
            // Garante que o índice não saia do alcance de cada array individualmente
            if (armRenderer != null) armRenderer.sprite = armCycle[currentFrame % armCycle.Length];
            if (legRenderer != null) legRenderer.sprite = legCycle[currentFrame % legCycle.Length];

            currentFrame++;
            yield return new WaitForSeconds(frameRate);
        }
    }
}