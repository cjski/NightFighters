using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ControlSchemeHandler
{
    public class ControlSchemeInformation
    {
        public Controller controller;
        public bool isInUse;

        public ControlSchemeInformation(Controller newController)
        {
            controller = newController;
            isInUse = false;
        }
    }

    public static ControlSchemeInformation[] controlSchemes =
    {
        new ControlSchemeInformation(new MouseController()),
        new ControlSchemeInformation(new KeyboardController(KeyCode.Z, KeyCode.X, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow)),
        new ControlSchemeInformation(new KeyboardController(KeyCode.K, KeyCode.L, KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.S)),
        new ControlSchemeInformation(new KeyboardController(KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad1, KeyCode.Keypad3, KeyCode.Keypad5, KeyCode.Keypad2))
    };
}
