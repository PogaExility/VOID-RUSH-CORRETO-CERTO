using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // <<-- IMPORTANTE
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotViewPrefab;
    [SerializeField] private GameObject itemViewPrefab;
    [SerializeField] private Transform backpackPanel;

    // A referência que é NULA em tempo de execução
    [SerializeField] private InventoryManager inventoryManager;
    private List<GameObject> activeItemViews = new List<GameObject>();

    void Awake()
    {
        // Esta linha falha em garantir a referência ANTES do Start
      
    }

    void Start()
    {
        CreateGrid();
        inventoryManager.OnInventoryChanged += Redraw;
        Redraw();
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged -= Redraw;
    }

    private void CreateGrid()
    {
        // LINHA 54 (APROXIMADA): AQUI QUEBRA, PORQUE 'inventoryManager' É NULO
        for (int i = 0; i < inventoryManager.gridWidth * inventoryManager.gridHeight; i++)
        {
            var slotGO = Instantiate(slotViewPrefab, backpackPanel);
            slotGO.GetComponent<SlotView>().Initialize(i);
        }
    }

    // Em InventoryUI.cs, SUBSTITUA a função Redraw
    private void Redraw()
    {
        foreach (var view in activeItemViews) Destroy(view);
        activeItemViews.Clear();

        // >> A CORREÇÃO ESTÁ AQUI <<
        // Em vez de 'backpackPanel.childCount', usamos o tamanho REAL do inventário.
        int inventorySize = inventoryManager.GetBackpackSize();

        for (int i = 0; i < inventorySize; i++)
        {
            // Pega o transform do slot, que deve corresponder ao índice
            var slotTransform = backpackPanel.GetChild(i);
            var data = inventoryManager.GetBackpackSlot(i);

            if (data != null && data.item != null)
            {
                var itemGO = Instantiate(itemViewPrefab, slotTransform);
                var itemView = itemGO.GetComponent<ItemView>();
                itemView.Render(i, data.item, data.count);
                activeItemViews.Add(itemGO);
            }
        }
    }
}