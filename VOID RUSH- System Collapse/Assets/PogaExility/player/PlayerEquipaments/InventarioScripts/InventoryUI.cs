using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // <<-- IMPORTANTE
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotViewPrefab;
    [SerializeField] private GameObject itemViewPrefab;
    [SerializeField] private Transform backpackPanel;

    // A refer�ncia que � NULA em tempo de execu��o
    [SerializeField] private InventoryManager inventoryManager;
    private List<GameObject> activeItemViews = new List<GameObject>();

    void Awake()
    {
        // Esta linha falha em garantir a refer�ncia ANTES do Start
      
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
        // LINHA 54 (APROXIMADA): AQUI QUEBRA, PORQUE 'inventoryManager' � NULO
        for (int i = 0; i < inventoryManager.gridWidth * inventoryManager.gridHeight; i++)
        {
            var slotGO = Instantiate(slotViewPrefab, backpackPanel);
            slotGO.GetComponent<SlotView>().Initialize(i);
        }
    }

    // Em InventoryUI.cs, SUBSTITUA a fun��o Redraw
    private void Redraw()
    {
        foreach (var view in activeItemViews) Destroy(view);
        activeItemViews.Clear();

        // >> A CORRE��O EST� AQUI <<
        // Em vez de 'backpackPanel.childCount', usamos o tamanho REAL do invent�rio.
        int inventorySize = inventoryManager.GetBackpackSize();

        for (int i = 0; i < inventorySize; i++)
        {
            // Pega o transform do slot, que deve corresponder ao �ndice
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