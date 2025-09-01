using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TooltipController : MonoBehaviour
{
    public RectTransform root;
    public Text titleText;
    public Text bodyText;
    public Image iconImage;
    public float showDelay = 1.0f;

    private Coroutine showCoroutine;
    private Canvas mainCanvas;

    void Awake()
    {
        mainCanvas = GetComponentInParent<Canvas>();
        Hide();
    }

    void Update()
    {
        if (root.gameObject.activeSelf)
        {
            FollowMouse();
        }
    }

    private void FollowMouse()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvas.transform as RectTransform, Input.mousePosition, mainCanvas.worldCamera, out localPoint);

        // Clamp para não sair da tela
        Rect canvasRect = (mainCanvas.transform as RectTransform).rect;
        Vector2 pivot = root.pivot;
        float width = root.rect.width;
        float height = root.rect.height;

        localPoint.x = Mathf.Clamp(localPoint.x, canvasRect.xMin + (width * pivot.x), canvasRect.xMax - (width * (1 - pivot.x)));
        localPoint.y = Mathf.Clamp(localPoint.y, canvasRect.yMin + (height * pivot.y), canvasRect.yMax - (height * (1 - pivot.y)));

        root.anchoredPosition = localPoint;
    }

    public void RequestShow(ItemSO item)
    {
        if (showCoroutine != null) StopCoroutine(showCoroutine);
        showCoroutine = StartCoroutine(ShowAfterDelay(() => ShowItem(item)));
    }

    private IEnumerator ShowAfterDelay(System.Action showAction)
    {
        yield return new WaitForSecondsRealtime(showDelay);
        showAction();
    }

    public void ShowItem(ItemSO item)
    {
        if (item == null) return;

        titleText.text = item.itemName;
        iconImage.sprite = item.itemIcon;
        iconImage.gameObject.SetActive(item.itemIcon != null);

        // Constrói o corpo do tooltip
        string body = $"Tipo: {item.itemType}\n";
        if (item.stackable) body += $"Empilhável (Max: {item.maxStack})\n";

        if (item.itemType == ItemType.Weapon)
        {
            body += $"Dano: {item.damage}\n";
            body += $"Cadência: {item.attackRate}s\n";
        }
        else if (item.itemType == ItemType.Consumable)
        {
            body += $"Cura: {item.healthToRestore} HP\n";
        }
        bodyText.text = body;

        root.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(root); // Força o ajuste de tamanho
    }

    public void ShowEffect(EffectData effect) { /* TODO */ }
    public void ShowWeapon(ItemSO weapon, int ammoCur, int ammoMax, float cooldown) { /* TODO */ }
    public void ShowConsumable(ItemSO item, float duration) { /* TODO */ }

    public void Hide()
    {
        if (showCoroutine != null) StopCoroutine(showCoroutine);
        root.gameObject.SetActive(false);
    }
}
