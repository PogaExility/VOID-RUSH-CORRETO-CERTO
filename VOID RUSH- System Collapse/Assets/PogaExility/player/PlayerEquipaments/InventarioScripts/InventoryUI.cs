using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

[DisallowMultipleComponent]

public class InventoryUI : MonoBehaviour

{

    [Header("Refer�ncias (arraste do Inspector)")]

    [SerializeField] private InventoryManager inventoryManager;

    [SerializeField] private GameObject slotUIPrefab;

    [SerializeField] private Transform backpackPanel; // O painel com o GridLayoutGroup

    // Mapeamento interno para acesso r�pido

    private List<InventorySlotUI> slotUIInstances = new List<InventorySlotUI>();

    private void Start()

    {

        // Garante que a refer�ncia ao manager existe

        if (inventoryManager == null)

        {

            inventoryManager = FindFirstObjectByType<InventoryManager>();

            if (inventoryManager == null)

            {

                Debug.LogError("InventoryManager n�o encontrado na cena! A UI n�o pode funcionar.", this);

                enabled = false;

                return;

            }

        }

        CreateGridSlots();

        SubscribeToEvents();

        // Desenho inicial de todos os slots

        RedrawAll();

    }

    private void OnDestroy()

    {

        UnsubscribeFromEvents();

    }

    private void SubscribeToEvents()

    {

        inventoryManager.OnBackpackSlotChanged += OnSlotChanged;

        inventoryManager.OnInventoryRefreshed += OnInventoryRefreshed;

    }

    private void UnsubscribeFromEvents()

    {

        if (inventoryManager != null)

        {

            inventoryManager.OnBackpackSlotChanged -= OnSlotChanged;

            inventoryManager.OnInventoryRefreshed -= OnInventoryRefreshed;

        }

    }

    /// <summary>

    /// Cria a grade de slots visuais uma �nica vez.

    /// </summary>

    private void CreateGridSlots()

    {

        // Limpa qualquer slot antigo que possa existir (�til para testes no editor)

        foreach (Transform child in backpackPanel)

        {

            Destroy(child.gameObject);

        }

        slotUIInstances.Clear();

        // Configura o GridLayout para corresponder ao manager

        var gridLayout = backpackPanel.GetComponent<GridLayoutGroup>();

        if (gridLayout != null)

        {

            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

            gridLayout.constraintCount = inventoryManager.gridWidth;

        }

        // Instancia um slot visual para cada slot de dados no manager

        int totalSlots = inventoryManager.GetBackpackSize();

        for (int i = 0; i < totalSlots; i++)

        {

            GameObject newSlotGO = Instantiate(slotUIPrefab, backpackPanel);

            newSlotGO.name = $"Slot_{i}";

            InventorySlotUI slotUI = newSlotGO.GetComponent<InventorySlotUI>();

            slotUI.Initialize(inventoryManager, i); // Dando identidade ao slot

            slotUIInstances.Add(slotUI);

        }

    }

    // --- Handlers de Eventos ---

    /// <summary> Chamado pelo InventoryManager quando um �NICO slot muda. </summary>

    private void OnSlotChanged(int index)

    {

        if (index >= 0 && index < slotUIInstances.Count)

        {

            slotUIInstances[index].Refresh();

        }

    }

    /// <summary> Chamado quando o invent�rio inteiro precisa ser redesenhado. </summary>

    private void OnInventoryRefreshed()

    {

        // Se a grade mudou de tamanho, recria. Sen�o, apenas redesenha.

        if (slotUIInstances.Count != inventoryManager.GetBackpackSize())

        {

            CreateGridSlots();

        }

        RedrawAll();

    }

    /// <summary> For�a a atualiza��o visual de todos os slots. </summary>

    private void RedrawAll()

    {

        foreach (var slotUI in slotUIInstances)

        {

            slotUI.Refresh();

        }

    }

}