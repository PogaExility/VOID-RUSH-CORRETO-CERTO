using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

// Simulação de dados de efeito
public class EffectData
{
    public string Name;
    public Sprite Icon;
    public float Duration;
}

public class EffectsPanelController : MonoBehaviour
{
    public RectTransform panelRoot;
    public Button effectCounterButton; // Botão-ícone de poção
    public Text effectCounterText;
    public Transform effectEntryContainer; // O pai com GridLayoutGroup
    public GameObject effectEntryPrefab; // O prefab da entrada do efeito

    public float slideDuration = 0.3f;
    private Vector2 closedPosition;
    private Vector2 openPosition;
    private bool isOpen = false;
    private List<EffectData> activeEffects = new List<EffectData>();

    void Awake()
    {
        closedPosition = panelRoot.anchoredPosition;
        openPosition = new Vector2(closedPosition.x, closedPosition.y - ((RectTransform)panelRoot.parent).rect.height);
        panelRoot.anchoredPosition = closedPosition; // Garante que começa fechado
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(SlidePanel(isOpen));
    }

    private IEnumerator SlidePanel(bool open)
    {
        Vector2 startPos = panelRoot.anchoredPosition;
        Vector2 endPos = open ? openPosition : closedPosition;
        float time = 0f;

        while (time < slideDuration)
        {
            panelRoot.anchoredPosition = Vector2.Lerp(startPos, endPos, time / slideDuration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        panelRoot.anchoredPosition = endPos;
    }

    public void AddEffect(EffectData newEffect)
    {
        activeEffects.Add(newEffect);
        RedrawEffects();
    }

    private void RedrawEffects()
    {
        // Limpa as entradas antigas
        foreach (Transform child in effectEntryContainer)
        {
            Destroy(child.gameObject);
        }

        // Cria as novas entradas
        foreach (var effect in activeEffects)
        {
            var entryGO = Instantiate(effectEntryPrefab, effectEntryContainer);
            // TODO: Criar um script EffectEntryView para popular os dados
            // entryGO.GetComponent<EffectEntryView>().Setup(effect);
        }

        // Atualiza o contador
        effectCounterText.text = activeEffects.Count.ToString();
        effectCounterButton.gameObject.SetActive(activeEffects.Count > 0);
    }
}