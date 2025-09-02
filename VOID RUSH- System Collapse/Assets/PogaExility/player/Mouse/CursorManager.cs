using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("Textura do cursor (Texture Type = Default ou Cursor)")]
    public Texture2D inventoryCursor;
    public Texture2D aimCursor;

    [Header("Hotspots possíveis (px a partir do canto SUPERIOR ESQUERDO da textura)")]
    public Vector2Int hotspotBaixo = new Vector2Int(11, 5);
    public Vector2Int hotspotMeio = new Vector2Int(11, 4);
    public Vector2Int hotspotAlto = new Vector2Int(11, 3); // <<< MAIS ALTO

    public enum HotspotOption { Baixo, Meio, Alto }
    [Header("Hotspot ativo")]
    public HotspotOption hotspotAtivo = HotspotOption.Alto; // começa no mais alto

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

        Vector2Int hotspot = hotspotMeio;

        switch (hotspotAtivo)
        {
            case HotspotOption.Baixo: hotspot = hotspotBaixo; break;
            case HotspotOption.Meio: hotspot = hotspotMeio; break;
            case HotspotOption.Alto: hotspot = hotspotAlto; break;
        }

        Cursor.SetCursor(inventoryCursor, new Vector2(hotspot.x, hotspot.y), cursorMode);
    }

    public void SetDefaultCursor()
    {
        Cursor.visible = true;
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }
    public void SetAimCursor()
    {
        Cursor.visible = true; // Garante que o cursor esteja visível

        if (aimCursor != null)
        {
            // Centraliza o hotspot no meio da textura da mira
            Vector2 hotspot = new Vector2(aimCursor.width / 2, aimCursor.height / 2);
            Cursor.SetCursor(aimCursor, hotspot, cursorMode);
        }
        else
        {
            Debug.LogWarning("[CursorManager] aimCursor não atribuído.");
            // Como fallback, podemos usar o cursor padrão
            SetDefaultCursor();
        }
    }
}


