// ----------------------------------------------------------------
// CRIADO EM: 2026-05
// FEITO POR: Debs Carvalho
// DESCRIÇÃO: Controla o cursor do mouse.
// ----------------------------------------------------------------

using UnityEngine;

public class UICursorChange : MonoBehaviour
{
    public Texture2D cursorTexture;

    public Vector2 hotspot = Vector2.zero;
    
    public Texture2D dragCursorTexture;
    public Vector2 dragHotspot = Vector2.zero;
    private bool _isDragging;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    public void OnHoverCursor()
    {
        if (_isDragging) return;
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
    }

    public void OnBeginDragCursor()
    {
        _isDragging = true;

        var tex = dragCursorTexture != null ? dragCursorTexture : cursorTexture;
        var hot = dragCursorTexture != null ? dragHotspot : hotspot;

        if (tex != null)
            Cursor.SetCursor(tex, hot, CursorMode.Auto);
    }

    public void OnEndDragCursor()
    {
        _isDragging = false;

        // Ao soltar, volta ao padrão (ou pode chamar OnHoverCursor se quiser manter hover)
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

    public void OnExitCursor()
    {
        if (_isDragging) return;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}