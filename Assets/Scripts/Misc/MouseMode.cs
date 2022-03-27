using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MouseMode
{
    public static void Play()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
