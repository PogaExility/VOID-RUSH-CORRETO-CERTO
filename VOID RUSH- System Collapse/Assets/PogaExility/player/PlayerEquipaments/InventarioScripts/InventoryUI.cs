using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

[DisallowMultipleComponent]

public class InventoryUI : MonoBehaviour

{

    [Header("Referências (arraste do Inspector)")]

    [SerializeField] private InventoryManager inventoryManager;

    [SerializeField] private GameObject slotUIPrefab;

    [SerializeField] private Transform backpackPanel; // O painel com o GridLayoutGroup

    // Mapeamento interno para acesso rápido

    private List<InventorySlotUI> slotUIInstances = new List<InventorySlotUI>();

    private void Start()

    {

        // Garante que a referência ao manager existe

        if (inventoryManager == null)

        {

            inventoryManager = FindFirstObjectByType<InventoryManager>();

            if (inventoryManager == null)

            {

                Debug.LogError("InventoryManager não encontrado na cena! A UI não pode funcionar.", this);

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

    /// Cria a grade de slots visuais uma única vez.

    /// </summary>

    private void CreateGridSlots()

    {

        // Limpa qualquer slot antigo que possa existir (útil para testes no editor)

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

    /// <summary> Chamado pelo InventoryManager quando um ÚNICO slot muda. </summary>

    private void OnSlotChanged(int index)

    {

        if (index >= 0 && index < slotUIInstances.Count)

        {

            slotUIInstances[index].Refresh();

        }

    }

    /// <summary> Chamado quando o inventário inteiro precisa ser redesenhado. </summary>

    private void OnInventoryRefreshed()

    {

        // Se a grade mudou de tamanho, recria. Senão, apenas redesenha.

        if (slotUIInstances.Count != inventoryManager.GetBackpackSize())

        {

            CreateGridSlots();

        }

        RedrawAll();

    }

    /// <summary> Força a atualização visual de todos os slots. </summary>

    private void RedrawAll()

    {

        foreach (var slotUI in slotUIInstances)

        {

            slotUI.Refresh();

        }

    }

}