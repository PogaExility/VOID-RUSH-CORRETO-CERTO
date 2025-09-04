using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("REFER�NCIAS")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject slotViewPrefab;
    [SerializeField] private GameObject itemViewPrefab;
    [SerializeField] private Transform backpackPanel;

    [HideInInspector] public Canvas mainCanvas;

    private List<GameObject> activeItemViews = new List<GameObject>();
    private bool isGridCreated = false; // Flag de seguran�a

    void Awake()
    {
        Instance = this;
        mainCanvas = GetComponentInParent<Canvas>();

        // Garante que a refer�ncia ao manager existe desde o in�cio
        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
            Debug.LogError("FATAL: InventoryManager n�o encontrado!", this);
    }

    void Start()
    {
        CreateGrid();
        // A inscri��o ao evento s� acontece DEPOIS que a grade est� pronta.
        inventoryManager.OnInventoryChanged += Redraw;
        Redraw(); // Primeiro desenho, agora � seguro.
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged -= Redraw;
    }

    private void CreateGrid()
    {
        // Limpa qualquer coisa que estivesse l� antes, para evitar duplicatas
        foreach (Transform child in backpackPanel)
        {
            Destroy(child.gameObject);
        }

        // Cria os slots de fundo
        for (int i = 0; i < inventoryManager.GetBackpackSize(); i++)
        {
            var slotGO = Instantiate(slotViewPrefab, backpackPanel);
            slotGO.GetComponent<SlotView>().Initialize(i);
        }

        isGridCreated = true; // SINAL VERDE: A grade est� pronta.
    }

    private void Redraw()
    {
        // CONDI��O DE SEGURAN�A: S� redesenha se a grade j� foi criada.
        if (!isGridCreated) return;

        foreach (var view in activeItemViews) Destroy(view);
        activeItemViews.Clear();

        // Agora, este loop � seguro.
        for (int i = 0; i < inventoryManager.GetBackpackSize(); i++)
        {
            // Pega o transform do slot. Esta linha n�o vai mais quebrar.
            Transform slotTransform = backpackPanel.GetChild(i);
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

    public void RequestRedraw() => Redraw();
}