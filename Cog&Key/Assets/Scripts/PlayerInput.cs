using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerInput
{
    public enum Action
    {
        Jump,
        Right,
        Left
    }

    // should be called once per frame
    public void Update() { 

    }

    public bool IsPressed(Action action) {
        //    bool keyPressed = Keyboard.current != null && GetKeyboardKey(Keyboard.current).isPressed;
        //    bool buttonPressed = Gamepad.current != null && GetGamePadButton(Gamepad.current).isPressed;
        //return keyPressed || buttonPressed;
        return false;
    }

    public bool JustPressed(Action action) {
        //bool keyPressed = Keyboard.current != null && GetKeyboardKey(Keyboard.current).wasPressedThisFrame;
        //bool buttonPressed = Gamepad.current != null && GetGamePadButton(Gamepad.current).wasPressedThisFrame;

        //bool keyReleased = Keyboard.current == null || !GetKeyboardKey(Keyboard.current).isPressed;
        //bool buttonReleased = Gamepad.current == null || !GetGamePadButton(Gamepad.current).isPressed;
        //return (keyPressed && buttonReleased) || (buttonPressed && keyReleased); // don't trigger if pressed when the other button is currently held
        // if the player somehow presses both on the same frame the input will be missed
        return false;
    }

    private ButtonControl[] GetButtons(Action action) {
        if(Gamepad.current == null) {
            return null;
        }

        switch(action)
        {
            case Action.Jump:
                return new ButtonControl[] { Gamepad.current.aButton, Gamepad.current.dpad.up };

            case Action.Right:
                return new ButtonControl[] { Gamepad.current.leftStick.right, Gamepad.current.dpad.right };

            case Action.Left:
                return new ButtonControl[] { Gamepad.current.leftStick.left, Gamepad.current.dpad.left };
        }

        return null;
    }

    private KeyControl[] GetKeys(Action action) {
        if(Keyboard.current == null) {
            return null;
        }

        switch(action)
        {
            case Action.Jump:
                return new KeyControl[] { Keyboard.current.spaceKey, Keyboard.current.rightArrowKey, Keyboard.current.wKey };

            case Action.Right:
                return new KeyControl[] { Keyboard.current.dKey, Keyboard.current.rightArrowKey };

            case Action.Left:
                return new KeyControl[] { Keyboard.current.aKey, Keyboard.current.leftArrowKey };
        }

        return null;
    }
}
