using UnityEngine;

/// <summary>
/// Centralizes input blocking. Using CursorLockMode.None (not Locked) so ESC cannot release it.
/// Call Block() to disable input; Unblock() to restore it.
/// All OnMouseDown handlers must check IsBlocked at the top.
/// </summary>
public static class InputBlocker
{
    public static bool IsBlocked { get; private set; }

    public static void Block()
    {
        IsBlocked = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public static void Unblock()
    {
        IsBlocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
