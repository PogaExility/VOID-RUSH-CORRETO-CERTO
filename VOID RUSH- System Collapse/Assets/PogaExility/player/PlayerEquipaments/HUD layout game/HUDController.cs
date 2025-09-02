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

        if (Input.GetKeyDown(KeyCode.Alpha1)) rotBar.TriggerSkill(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) rotBar.TriggerSkill(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) rotBar.TriggerSkill(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) rotBar.TriggerSkill(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) rotBar.TriggerSkill(5);

        if (Input.GetKeyDown(KeyCode.Q)) rotBar.TriggerQuickUse(1);
        if (Input.GetKeyDown(KeyCode.E)) rotBar.TriggerQuickUse(2);
        if (Input.GetKeyDown(KeyCode.R)) rotBar.TriggerQuickUse(3);
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