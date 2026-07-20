using UnityEngine;

/// <summary>
/// Centralizes input blocking. Using CursorLockMode.None (not Locked) so ESC cannot release it.
/// Call Block() to disable input; Unblock() to restore it.
/// Call OpenUI() when a UI overlay opens; CloseUI() when it closes (cursor stays visible).
/// All OnMouseDown handlers must check IsBlocked at the top.
/// </summary>
public static class InputBlocker
{
    private static bool _hardBlocked;
    private static int _uiOpenCount;

    public static bool IsBlocked => _hardBlocked || _uiOpenCount > 0;

    public static void Block()
    {
        _hardBlocked = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void Unblock()
    {
        _hardBlocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Use while a UI panel is open — blocks 3D OnMouseDown without hiding the cursor.
    public static void OpenUI() => _uiOpenCount++;

    public static void CloseUI()
    {
        if (_uiOpenCount > 0) _uiOpenCount--;
    }
}
