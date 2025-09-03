using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class InventoryUI : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private CursorManager cursorManager;
    [SerializeField] private GameObject slotUIPrefab;
    [SerializeField] private Transform backpackPanel;

    private List<InventorySlotUI> slotUIInstances = new List<InventorySlotUI>();

    // Estado da Interação
    private InventorySlot heldItemSlot = new InventorySlot();
    private int originIndex = -1;
    private bool isDragging = false;

    void Start()
    {
        if (cursorManager == null) cursorManager = FindFirstObjectByType<CursorManager>();
        if (inventoryManager == null) { inventoryManager = FindFirstObjectByType<InventoryManager>(); if (inventoryManager == null) { Debug.LogError("InventoryManager não encontrado!", this); enabled = false; return; } }
        CreateGridSlots();
        SubscribeToEvents();
        RedrawAll();
    }
    void OnDestroy() { UnsubscribeFromEvents(); }
    private void SubscribeToEvents() { inventoryManager.OnBackpackSlotChanged += OnSlotChanged; inventoryManager.OnInventoryRefreshed += OnInventoryRefreshed; }
    private void UnsubscribeFromEvents() { if (inventoryManager != null) { inventoryManager.OnBackpackSlotChanged -= OnSlotChanged; inventoryManager.OnInventoryRefreshed -= OnInventoryRefreshed; } }

    private void CreateGridSlots()
    {
        foreach (Transform child in backpackPanel) { Destroy(child.gameObject); }
        slotUIInstances.Clear();
        var gridLayout = backpackPanel.GetComponent<GridLayoutGroup>();
        if (gridLayout != null) { gridLayout.constraintCount = inventoryManager.gridWidth; }
        for (int i = 0; i < inventoryManager.GetBackpackSize(); i++)
        {
            var slotGO = Instantiate(slotUIPrefab, backpackPanel);
            var slotUI = slotGO.GetComponent<InventorySlotUI>();
            slotUI.Initialize(inventoryManager, this, i);
            slotUIInstances.Add(slotUI);
        }
    }

    private void OnSlotChanged(int index) { if (index >= 0 && index < slotUIInstances.Count) { slotUIInstances[index].Refresh(); } }
    private void OnInventoryRefreshed() { if (slotUIInstances.Count != inventoryManager.GetBackpackSize()) { CreateGridSlots(); } RedrawAll(); }
    public void RedrawAll() { foreach (var slotUI in slotUIInstances) { slotUI.Refresh(); } }


    // --- LÓGICA CORRIGIDA ---

    public void OnSlotClicked(int index)
    {
        if (isDragging) return;

        var clickedSlot = inventoryManager.GetBackpackSlot(index);

        var tempItem = heldItemSlot.item;
        var tempCount = heldItemSlot.count;

        heldItemSlot.Set(clickedSlot.item, clickedSlot.count);
        originIndex = heldItemSlot.item != null ? index : -1;

        // CORREÇÃO: Pede para o manager atualizar o slot. Ele vai disparar o evento.
        inventoryManager.SetSlotContent(index, tempItem, tempCount);

        UpdateCursor();
    }

    public void OnSlotBeginDrag(int index)
    {
        if (heldItemSlot.item != null) return; // Se já segura no modo clique, não arrasta

        var draggedSlot = inventoryManager.GetBackpackSlot(index);
        if (draggedSlot == null || draggedSlot.item == null) return;

        isDragging = true;
        heldItemSlot.Set(draggedSlot.item, draggedSlot.count);
        originIndex = index;

        // CORREÇÃO: Pede para o manager esvaziar o slot original.
        inventoryManager.SetSlotContent(index, null, 0);
        UpdateCursor();
    }

    public void OnSlotDrop(int destinationIndex)
    {
        if (!isDragging || originIndex == -1) return;

        var destinationSlot = inventoryManager.GetBackpackSlot(destinationIndex);

        // CORREÇÃO: A troca acontece através do Manager.
        var tempItem = destinationSlot.item;
        var tempCount = destinationSlot.count;

        inventoryManager.SetSlotContent(destinationIndex, heldItemSlot.item, heldItemSlot.count);
        inventoryManager.SetSlotContent(originIndex, tempItem, tempCount);

        heldItemSlot.Clear();
        originIndex = -1;
    }

    public void OnSlotEndDrag()
    {
        if (!isDragging) return;

        if (originIndex != -1)
        {
            // CORREÇÃO: Devolve o item à origem através do manager.
            var originalSlot = inventoryManager.GetBackpackSlot(originIndex);
            inventoryManager.SetSlotContent(originIndex, heldItemSlot.item, heldItemSlot.count);

            heldItemSlot.Clear();
            originIndex = -1;
        }

        isDragging = false;
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        if (heldItemSlot.item != null)
            cursorManager.ShowHeldItem(heldItemSlot.item.itemIcon, heldItemSlot.count);
        else
            cursorManager.HideHeldItem();
    }

    public bool IsItemHeld() => heldItemSlot.item != null;
    public int GetOriginIndex() => originIndex;
}