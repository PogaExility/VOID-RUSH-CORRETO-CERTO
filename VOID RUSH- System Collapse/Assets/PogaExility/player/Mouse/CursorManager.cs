using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Sprites do Cursor")]
    [Tooltip("O sprite para o cursor quando o invent�rio est� aberto.")]
    public Texture2D inventoryCursor;

    [Tooltip("O ponto do sprite que deve alinhar com a ponta do mouse (geralmente o canto superior esquerdo).")]
    public Vector2 cursorHotspot = Vector2.zero;

    // Fun��o p�blica para definir o cursor para o modo "Invent�rio"
    public void SetInventoryCursor()
    {
        // Esconde o cursor padr�o do sistema operacional
        Cursor.visible = true;

        // Define o sprite e o ponto de clique do novo cursor
        Cursor.SetCursor(inventoryCursor, cursorHotspot, CursorMode.Auto);
    }

    // Fun��o p�blica para definir o cursor para o modo "Gameplay" (padr�o)
    public void SetDefaultCursor()
    {
        // Mostra o cursor padr�o do sistema operacional
        Cursor.visible = true;

        // Remove qualquer cursor customizado
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}