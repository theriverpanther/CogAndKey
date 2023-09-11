using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// stores keybindings and uses them to check if an action is used
public class InputAction
{
    public delegate KeyControl KeyboardKeyGetter(Keyboard keys);
    public delegate ButtonControl GamepadButtonGetter(Gamepad controller);

    private KeyboardKeyGetter GetKeyboardKey;
    private GamepadButtonGetter GetGamePadButton;

    // new InputAction((keyboard) => keyboard.[key], (gamepad) => gamepad.[button]);
    public InputAction(KeyboardKeyGetter key, GamepadButtonGetter button)
    {
        GetKeyboardKey = key;
        GetGamePadButton = button;
    }

    // return true if the button is currently held down
    public bool IsPressed()
    {
        bool keyPressed = Keyboard.current != null && GetKeyboardKey(Keyboard.current).isPressed;
        bool buttonPressed = Gamepad.current != null && GetGamePadButton(Gamepad.current).isPressed;
        return keyPressed || buttonPressed;
    }

    // returns true on the first frame this action is pressed
    public bool JustPressed()
    {
        bool keyPressed = Keyboard.current != null && GetKeyboardKey(Keyboard.current).wasPressedThisFrame;
        bool buttonPressed = Gamepad.current != null && GetGamePadButton(Gamepad.current).wasPressedThisFrame;

        bool keyReleased = Keyboard.current == null || !GetKeyboardKey(Keyboard.current).isPressed;
        bool buttonReleased = Gamepad.current == null || !GetGamePadButton(Gamepad.current).isPressed;
        return (keyPressed && buttonReleased) || (buttonPressed && keyReleased);
        // don't trigger if pressed when the other button is currently held
        
        // if the player somehow presses both on the same frame the input will be missed
    }
}
