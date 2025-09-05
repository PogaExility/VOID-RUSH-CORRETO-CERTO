using UnityEngine;
public class CursorManager : MonoBehaviour
{
    public Texture2D inventoryCursor;
    public Texture2D aimCursor;
    public CursorMode cursorMode = CursorMode.Auto;

    public void SetDefaultCursor() => Cursor.SetCursor(null, Vector2.zero, cursorMode);
    public void SetInventoryCursor() => Cursor.SetCursor(inventoryCursor, Vector2.zero, cursorMode);
}