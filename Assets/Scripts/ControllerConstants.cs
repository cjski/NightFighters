using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ControllerConstants
{
    private struct ControllerSchemeInformation
    {
        Controller controller;
        bool isInUse;
    }

    public static Controller[] controlSchemes =
    {
        new MouseController(),
        new KeyboardController(KeyCode.Z, KeyCode.X, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow),
        new KeyboardController(KeyCode.K, KeyCode.L, KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S),
        new KeyboardController(KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad1, KeyCode.Keypad3, KeyCode.Keypad5, KeyCode.Keypad2)
    };
}
