using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D inventoryCursor;
    public Texture2D aimCursor;
    public CursorMode cursorMode = CursorMode.Auto;

    public void SetDefaultCursor() => Cursor.SetCursor(null, Vector2.zero, cursorMode);
    public void SetInventoryCursor() => Cursor.SetCursor(inventoryCursor, Vector2.zero, cursorMode);

    // ESTA É A FUNÇÃO IMPORTANTE
    public void SetAimCursor()
    {
        if (aimCursor != null)
        {
            // Centraliza o hotspot para a mira
            Vector2 hotspot = new Vector2(aimCursor.width / 2, aimCursor.height / 2);
            Cursor.SetCursor(aimCursor, hotspot, cursorMode);
        }
    }
}