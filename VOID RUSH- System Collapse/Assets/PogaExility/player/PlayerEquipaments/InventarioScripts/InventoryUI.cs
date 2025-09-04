using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("REFERÊNCIAS")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject slotViewPrefab;
    [SerializeField] private GameObject itemViewPrefab;
    [SerializeField] private Transform backpackPanel;

    [HideInInspector] public Canvas mainCanvas;

    private List<GameObject> activeItemViews = new List<GameObject>();
    private bool isGridCreated = false; // Flag de segurança

    void Awake()
    {
        Instance = this;
        mainCanvas = GetComponentInParent<Canvas>();

        // Garante que a referência ao manager existe desde o início
        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;
        if (inventoryManager == null)
            Debug.LogError("FATAL: InventoryManager não encontrado!", this);
    }

    void Start()
    {
        CreateGrid();
        // A inscrição ao evento só acontece DEPOIS que a grade está pronta.
        inventoryManager.OnInventoryChanged += Redraw;
        Redraw(); // Primeiro desenho, agora é seguro.
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged -= Redraw;
    }

    private void CreateGrid()
    {
        // Limpa qualquer coisa que estivesse lá antes, para evitar duplicatas
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

        isGridCreated = true; // SINAL VERDE: A grade está pronta.
    }

    private void Redraw()
    {
        // CONDIÇÃO DE SEGURANÇA: Só redesenha se a grade já foi criada.
        if (!isGridCreated) return;

        foreach (var view in activeItemViews) Destroy(view);
        activeItemViews.Clear();

        // Agora, este loop é seguro.
        for (int i = 0; i < inventoryManager.GetBackpackSize(); i++)
        {
            // Pega o transform do slot. Esta linha não vai mais quebrar.
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