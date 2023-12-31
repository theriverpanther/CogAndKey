using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

// helper class for input. Key bindings are set in ConstructKeyBindings()
public class PlayerInput
{
    public enum Action
    {
        Jump = 0,
        Right,
        Left,
        Up,
        Down,
        ThrowUp,
        ThrowDown,
        ThrowLeft,
        ThrowRight,
        FastKey,
        LockKey,
        ReverseKey,
        Pause
    }

    private readonly int NUM_ACTIONS;

    // contain a spot for each Action, index matches enum int value
    private bool[] pressedLastFrame;
    private bool[] pressedThisFrame;
    private bool mouseClicked;

    // used to detect when a controller is plugged in or unplugged
    private Gamepad currentGP;
    private Keyboard currentKB;
    private Mouse currentMouse;

    private string controllerName;

    public string ControllerName { get { return controllerName; } }

    private Dictionary<Action, List<ButtonControl>> keyBindings;
    private float jumpBuffer;

    public bool JumpBuffered { get { return jumpBuffer > 0; } }

    public PlayerInput() {
        NUM_ACTIONS = Enum.GetNames(typeof(Action)).Length;
        pressedLastFrame = new bool[NUM_ACTIONS];
        pressedThisFrame = new bool[NUM_ACTIONS];

        keyBindings = new Dictionary<Action, List<ButtonControl>>();
        for(int i = 0; i < NUM_ACTIONS; i++) {
            keyBindings[(Action)i] = new List<ButtonControl>();
        }
        ConstructKeyBindings();
    }

    // should be called once per frame
    public void Update() {
        if(Gamepad.current != currentGP || Keyboard.current != currentKB || Mouse.current != currentMouse) {
            ConstructKeyBindings();
        }

        pressedThisFrame.CopyTo(pressedLastFrame, 0);

        for(int i = 0; i < NUM_ACTIONS; i++) {
            pressedThisFrame[i] = false;
            foreach(ButtonControl button in keyBindings[(Action)i]) {
                if(button.isPressed) {
                    pressedThisFrame[i] = true;
                    break;
                }
            }
        }

        mouseClicked = false;
        if(currentMouse != null && currentMouse.leftButton.isPressed) {
            mouseClicked = true;
        }

        // manage jump buffer
        if(JustPressed(Action.Jump)) {
            jumpBuffer = 0.07f;
        }
        else if(jumpBuffer > 0) {
            jumpBuffer -= Time.deltaTime;
        }
    }

    public bool IsPressed(Action action) {
        return pressedThisFrame[(int)action];
    }

    public bool JustPressed(Action action) {
        return pressedThisFrame[(int)action] && !pressedLastFrame[(int)action];
    }

    // called at initialization and whenever a controller is plugged in or unplugged. Constructs the keyBindings dictionary
    private void ConstructKeyBindings() {
        currentGP = Gamepad.current;
        currentKB = Keyboard.current;
        currentMouse = Mouse.current;

        controllerName = Input.GetJoystickNames().Length > 0 ? Input.GetJoystickNames()[0] : null;

        for(int i = 0; i < NUM_ACTIONS; i++) {
            keyBindings[(Action)i].Clear();
        }

        // add gamepad bindings
        if(currentGP != null) {
            keyBindings[Action.Jump].AddRange(new List<ButtonControl>() { currentGP.aButton });
            keyBindings[Action.Right].AddRange(new List<ButtonControl>() { currentGP.leftStick.right, currentGP.dpad.right });
            keyBindings[Action.Left].AddRange(new List<ButtonControl>() { currentGP.leftStick.left, currentGP.dpad.left });
            keyBindings[Action.Up].AddRange(new List<ButtonControl>() { currentGP.leftStick.up, currentGP.dpad.up });
            keyBindings[Action.Down].AddRange(new List<ButtonControl>() { currentGP.leftStick.down, currentGP.dpad.down });
            keyBindings[Action.ThrowUp].AddRange(new List<ButtonControl>() { currentGP.rightStick.up });
            keyBindings[Action.ThrowDown].AddRange(new List<ButtonControl>() { currentGP.rightStick.down });
            keyBindings[Action.ThrowLeft].AddRange(new List<ButtonControl>() { currentGP.rightStick.left });
            keyBindings[Action.ThrowRight].AddRange(new List<ButtonControl>() { currentGP.rightStick.right });
            keyBindings[Action.FastKey].AddRange(new List<ButtonControl>() { currentGP.xButton });
            keyBindings[Action.LockKey].AddRange(new List<ButtonControl>() { currentGP.yButton });
            keyBindings[Action.ReverseKey].AddRange(new List<ButtonControl>() { currentGP.bButton });
            keyBindings[Action.Pause].AddRange(new List<ButtonControl>() { currentGP.startButton });
        }

        // add keyboard bindings
        if(currentKB != null) {
            keyBindings[Action.Jump].AddRange(new List<ButtonControl>() { currentKB.spaceKey });
            keyBindings[Action.Right].AddRange(new List<ButtonControl>() { currentKB.dKey });
            keyBindings[Action.Left].AddRange(new List<ButtonControl>() {  currentKB.aKey });
            keyBindings[Action.Up].AddRange(new List<ButtonControl>() { currentKB.wKey });
            keyBindings[Action.Down].AddRange(new List<ButtonControl>() { currentKB.sKey });
            keyBindings[Action.ThrowUp].AddRange(new List<ButtonControl>() { currentKB.upArrowKey });
            keyBindings[Action.ThrowDown].AddRange(new List<ButtonControl>() { currentKB.downArrowKey });
            keyBindings[Action.ThrowLeft].AddRange(new List<ButtonControl>() { currentKB.leftArrowKey });
            keyBindings[Action.ThrowRight].AddRange(new List<ButtonControl>() { currentKB.rightArrowKey });
            keyBindings[Action.FastKey].AddRange(new List<ButtonControl>() { currentKB.digit1Key, currentKB.numpad1Key, currentKB.zKey });
            keyBindings[Action.LockKey].AddRange(new List<ButtonControl>() { currentKB.digit2Key, currentKB.numpad2Key, currentKB.xKey });
            keyBindings[Action.ReverseKey].AddRange(new List<ButtonControl>() { currentKB.digit3Key, currentKB.numpad3Key, currentKB.cKey });
            keyBindings[Action.Pause].AddRange(new List<ButtonControl>() { currentKB.escapeKey });
        }

        // add mouse input
        if(currentMouse != null) {
            //keyBindings[Action.FastKey].AddRange(new List<ButtonControl>() { currentMouse.leftButton });
        }
    }

    public bool MouseClicked() {
        return mouseClicked;
    }

    // should not be called if there is no mouse
    public Vector3 GetMouseWorldPosition() {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
