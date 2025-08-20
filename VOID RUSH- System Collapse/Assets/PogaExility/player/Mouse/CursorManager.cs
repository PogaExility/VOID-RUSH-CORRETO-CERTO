using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Sprites do Cursor")]
    [Tooltip("O sprite para o cursor quando o inventário está aberto.")]
    public Texture2D inventoryCursor;

    [Tooltip("O ponto do sprite que deve alinhar com a ponta do mouse (geralmente o canto superior esquerdo).")]
    public Vector2 cursorHotspot = Vector2.zero;

    // Função pública para definir o cursor para o modo "Inventário"
    public void SetInventoryCursor()
    {
        // Esconde o cursor padrão do sistema operacional
        Cursor.visible = true;

        // Define o sprite e o ponto de clique do novo cursor
        Cursor.SetCursor(inventoryCursor, cursorHotspot, CursorMode.Auto);
    }

    // Função pública para definir o cursor para o modo "Gameplay" (padrão)
    public void SetDefaultCursor()
    {
        // Mostra o cursor padrão do sistema operacional
        Cursor.visible = true;

        // Remove qualquer cursor customizado
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}