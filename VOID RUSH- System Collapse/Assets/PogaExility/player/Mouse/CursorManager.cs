using UnityEngine;
using UnityEngine.UI; // Adicionamos a biblioteca da UI
using TMPro;
public class CursorManager : MonoBehaviour
{
    // --- Texturas do Cursor (Funcionalidade antiga, mantida) ---
    [Header("Textura do cursor (Texture Type = Default ou Cursor)")]
    public Texture2D inventoryCursor;
    public Texture2D aimCursor;
    public CursorMode cursorMode = CursorMode.Auto;

    // --- NOVO: Ícone para Arrastar Itens ---
    [Header("UI para Arrastar Itens")]
    [SerializeField] private Image heldItemIcon; // Arraste o GameObject "HeldItemIcon" aqui
    [SerializeField] private TextMeshProUGUI heldItemCountText;
    private bool isHoldingItem = false;

    private void Start()
    {
        // Garante que o ícone fantasma comece desativado
        if (heldItemIcon != null)
        {
            heldItemIcon.gameObject.SetActive(false);
        }
        SetDefaultCursor();
    }

    private void Update()
    {
        // Se estivermos segurando um item, a imagem da UI deve seguir o mouse
        if (isHoldingItem && heldItemIcon != null)
        {
            heldItemIcon.transform.position = Input.mousePosition;
        }
    }

    // --- MÉTODOS PÚBLICOS PARA MUDAR O CURSOR ---

    public void SetInventoryCursor()
    {
        // Se já estivermos segurando um item, não mude o cursor padrão
        if (isHoldingItem) return;

        Cursor.visible = true;
        Cursor.SetCursor(inventoryCursor, Vector2.zero, cursorMode);
    }

    public void SetDefaultCursor()
    {
        if (isHoldingItem) return;

        Cursor.visible = true;
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
    public void SetAimCursor()
    {
        if (isHoldingItem) return;

        Cursor.visible = true;
        Vector2 hotspot = new Vector2(aimCursor.width / 2, aimCursor.height / 2);
        Cursor.SetCursor(aimCursor, hotspot, cursorMode);
    }

    // --- NOVAS FUNÇÕES PARA O INVENTÁRIO CHAMAR ---

    /// <summary>
    /// Mostra um ícone de item seguindo o mouse e esconde o cursor do sistema.
    /// </summary>
    public void ShowHeldItem(Sprite itemSprite, int count)
    {
        if (heldItemIcon == null) return;

        isHoldingItem = true;
        heldItemIcon.sprite = itemSprite;
        heldItemIcon.gameObject.SetActive(true);

        bool showCount = count > 1;
        heldItemCountText.enabled = showCount;
        if (showCount)
        {
            heldItemCountText.text = count.ToString();
        }

        Cursor.visible = false;
    }
    public void HideHeldItem()
    {
        if (heldItemIcon == null) return;

        isHoldingItem = false;
        heldItemIcon.gameObject.SetActive(false);
        SetInventoryCursor();
    }
}
