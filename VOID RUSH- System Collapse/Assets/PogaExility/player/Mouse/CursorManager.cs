using UnityEngine;
using UnityEngine.UI; // Necessário para manipular a UI

public class CursorManager : MonoBehaviour
{
    [Header("Cursor de Inventário (Padrão)")]
    public Texture2D inventoryCursor;
    public CursorMode cursorMode = CursorMode.Auto;

    [Header("Mira (Cursor de Software)")]
    [Tooltip("Arraste aqui o objeto 'Image' do Canvas que será sua mira.")]
    public RectTransform crosshairUI;

    [Tooltip("Velocidade de rotação ao atirar.")]
    public float rotationSpeed = 500f;

    [Tooltip("Tamanho da mira (1 = normal, 2 = dobro).")]
    public float crosshairScale = 2f;

    private bool isAiming = false;

    void Start()
    {
        // Garante que a mira comece desativada e com o tamanho correto
        if (crosshairUI != null)
        {
            crosshairUI.gameObject.SetActive(false);
            crosshairUI.localScale = new Vector3(crosshairScale, crosshairScale, 1f);
        }

        SetDefaultCursor();
    }

    void Update()
    {
        // Lógica exclusiva da mira
        if (isAiming && crosshairUI != null)
        {
            // 1. Faz a imagem da mira seguir a posição do mouse na tela
            crosshairUI.position = Input.mousePosition;

            // 2. Se estiver atirando (Botão esquerdo), gira a imagem
            if (Input.GetButton("Fire1"))
            {
                crosshairUI.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            }
            else
            {
                // MUDANÇA AQUI: Reseta a rotação para o padrão (0,0,0) quando solta o botão.
                // Isso faz ela "voltar ao normal" imediatamente.
                crosshairUI.rotation = Quaternion.identity;
            }
        }
    }

    // --- MÉTODOS DE TROCA ---

    public void SetDefaultCursor()
    {
        isAiming = false;
        if (crosshairUI != null) crosshairUI.gameObject.SetActive(false);

        Cursor.visible = true; // Mostra a setinha do Windows
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    public void SetInventoryCursor()
    {
        isAiming = false;
        if (crosshairUI != null) crosshairUI.gameObject.SetActive(false);

        Cursor.visible = true; // Mostra a setinha do Windows
        Cursor.SetCursor(inventoryCursor, Vector2.zero, cursorMode);
    }

    public void SetAimCursor()
    {
        isAiming = true;

        Cursor.visible = false; // ESCONDE a setinha do Windows

        if (crosshairUI != null)
        {
            crosshairUI.gameObject.SetActive(true); // Mostra nossa imagem de mira
            // Garante o tamanho configurado
            crosshairUI.localScale = new Vector3(crosshairScale, crosshairScale, 1f);
        }
    }
}