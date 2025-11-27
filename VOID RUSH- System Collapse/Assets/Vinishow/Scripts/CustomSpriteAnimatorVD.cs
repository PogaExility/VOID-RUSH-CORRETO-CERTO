using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CustomSpriteAnimatorVD : MonoBehaviour
{
    [Header("Configuração das Animações")]
    [SerializeField] private List<SpriteAnimationVD> animationStates = new List<SpriteAnimationVD>();

    // Componentes e controle interno
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, SpriteAnimationVD> animationDict;
    private SpriteAnimationVD currentAnimation;
    private float timer;
    private int currentFrameIndex;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        animationDict = new Dictionary<string, SpriteAnimationVD>();
        foreach (var animState in animationStates)
        {
            // Adiciona um log para cada animação carregada
            Debug.Log("<color=cyan>Animator carregou a animação:</color> " + animState.stateName, this);
            animationDict[animState.stateName] = animState;
        }
    }

    void Update()
    {
        if (currentAnimation == null || currentAnimation.frames.Count == 0)
        {
            return;
        }

        // --- PROTEÇÃO ADICIONADA ---
        // Se FPS for 0 ou negativo, a animação não progride.
        if (currentAnimation.framesPerSecond <= 0)
        {
            return;
        }

        float frameDuration = 1f / currentAnimation.framesPerSecond;
        timer += Time.deltaTime;

        if (timer >= frameDuration)
        {
            // --- DEBUG ADICIONADO ---
            Debug.Log("Trocando frame para a animação: " + currentAnimation.stateName);

            timer -= frameDuration;
            currentFrameIndex++;

            if (currentFrameIndex >= currentAnimation.frames.Count)
            {
                if (currentAnimation.loop)
                {
                    currentFrameIndex = 0;
                }
                else
                {
                    currentFrameIndex = currentAnimation.frames.Count - 1;
                }
            }

            // Garante que o índice não saia do limite da lista antes de usá-lo
            if (currentFrameIndex < currentAnimation.frames.Count)
            {
                spriteRenderer.sprite = currentAnimation.frames[currentFrameIndex];
            }
        }
    }

    public void Play(string stateName)
    {
        // --- DEBUG ADICIONADO ---
        Debug.Log("<color=yellow>Recebido comando para tocar:</color> " + stateName, this);

        if (currentAnimation != null && currentAnimation.stateName == stateName)
        {
            return;
        }

        if (animationDict.TryGetValue(stateName, out SpriteAnimationVD newAnimation))
        {
            // --- DEBUG ADICIONADO ---
            Debug.Log("<color=green>Animação encontrada! Trocando para:</color> " + stateName, this);

            currentAnimation = newAnimation;
            currentFrameIndex = 0;
            timer = 0;

            if (currentAnimation.frames.Count > 0)
            {
                spriteRenderer.sprite = currentAnimation.frames[0];
            }
        }
        else
        {
            Debug.LogWarning("Animação '" + stateName + "' não foi encontrada no dicionário!", this);
            currentAnimation = null;
        }
    }

    public SpriteAnimationVD GetAnimationByName(string name)
    {
        if (animationDict.TryGetValue(name, out SpriteAnimationVD anim))
        {
            return anim;
        }
        return null;
    }
}