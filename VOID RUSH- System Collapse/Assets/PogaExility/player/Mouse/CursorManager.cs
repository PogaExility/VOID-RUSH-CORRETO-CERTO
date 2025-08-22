using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Textura do cursor (Texture Type = Default ou Cursor)")]
    public Texture2D inventoryCursor;

    [Header("Hotspot (px) a partir do canto SUPERIOR ESQUERDO da textura")]
    public Vector2Int hotspot = Vector2Int.zero;

    [Header("Modo do cursor")]
    public CursorMode cursorMode = CursorMode.Auto;

    public void SetInventoryCursor()
    {
        Cursor.visible = true;

        if (inventoryCursor == null)
        {
            Debug.LogWarning("[CursorManager] inventoryCursor não atribuído.");
            Cursor.SetCursor(null, Vector2.zero, cursorMode);
            return;
        }

        Cursor.SetCursor(inventoryCursor, new Vector2(hotspot.x, hotspot.y), cursorMode);
    }

    public void SetDefaultCursor()
    {
        Cursor.visible = true;
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
}
