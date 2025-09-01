using UnityEngine;

public enum HUDState { InGame, InventoryOpen }

public class HUDController : MonoBehaviour
{
    public EffectsPanelController effectsPanel;
    public RotBarController rotBar;
    public InventoryPanelController inventoryPanel;
    public TooltipController tooltip;
    public Canvas mainCanvas;

    public bool pauseOnInventory = false;
    private HUDState currentState;

    void Start()
    {
        inventoryPanel.gameObject.SetActive(false);
        effectsPanel.gameObject.SetActive(false);
        currentState = HUDState.InGame;
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventoryPanel();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (currentState == HUDState.InGame)
            {
                ToggleInventoryPanel();
            }
            inventoryPanel.OpenBackpack();
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            effectsPanel.Toggle();
        }

        // Placeholders de eventos
        if (Input.GetKeyDown(KeyCode.Alpha1)) rotBar.OnSkillPressed?.Invoke(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) rotBar.OnSkillPressed?.Invoke(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) rotBar.OnSkillPressed?.Invoke(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) rotBar.OnSkillPressed?.Invoke(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) rotBar.OnSkillPressed?.Invoke(5);

        if (Input.GetKeyDown(KeyCode.Q)) rotBar.OnQuickPressed?.Invoke(1);
        if (Input.GetKeyDown(KeyCode.E)) rotBar.OnQuickPressed?.Invoke(2);
        if (Input.GetKeyDown(KeyCode.R)) rotBar.OnQuickPressed?.Invoke(3);
    }

    private void ToggleInventoryPanel()
    {
        bool isOpen = !inventoryPanel.gameObject.activeSelf;
        inventoryPanel.gameObject.SetActive(isOpen);
        currentState = isOpen ? HUDState.InventoryOpen : HUDState.InGame;

        if (pauseOnInventory)
        {
            Time.timeScale = isOpen ? 0f : 1f;
        }
    }
}