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
        FastKey,
        LockKey,
        ReverseKey
    }

    private readonly int NUM_ACTIONS;

    // contain a spot for each Action, index matches enum int value
    private bool[] pressedLastFrame;
    private bool[] pressedThisFrame;

    // used to detect when a controller is plugged in or unplugged
    private Gamepad currentGP;
    private Keyboard currentKB;

    private Dictionary<Action, List<ButtonControl>> keyBindings;
    private float jumpBuffer;

    public bool JumpBuffered { get { return jumpBuffer > 0; } }

    public PlayerInput() {
        NUM_ACTIONS = Enum.GetNames(typeof(Action)).Length;
        pressedLastFrame = new bool[NUM_ACTIONS];
        pressedThisFrame = new bool[NUM_ACTIONS];
        ConstructKeyBindings();
    }

    // should be called once per frame
    public void Update() {
        if(Gamepad.current != currentGP || Keyboard.current != currentKB) {
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

        keyBindings = new Dictionary<Action, List<ButtonControl>>();
        for(int i = 0; i < NUM_ACTIONS; i++) {
            keyBindings[(Action)i] = new List<ButtonControl>();
        }

        // add gamepad bindings
        if(currentGP != null) {
            keyBindings[Action.Jump].AddRange(new List<ButtonControl>() { currentGP.aButton, currentGP.dpad.up });
            keyBindings[Action.Right].AddRange(new List<ButtonControl>() { currentGP.leftStick.right, currentGP.dpad.right });
            keyBindings[Action.Left].AddRange(new List<ButtonControl>() { currentGP.leftStick.left, currentGP.dpad.left });
            keyBindings[Action.FastKey].AddRange(new List<ButtonControl>() { currentGP.xButton });
            keyBindings[Action.LockKey].AddRange(new List<ButtonControl>() { currentGP.yButton });
            keyBindings[Action.ReverseKey].AddRange(new List<ButtonControl>() { currentGP.bButton });
        }

        // add keyboard bindings
        if(currentKB != null) {
            keyBindings[Action.Jump].AddRange(new List<ButtonControl>() { currentKB.upArrowKey, currentKB.wKey, currentKB.spaceKey });
            keyBindings[Action.Right].AddRange(new List<ButtonControl>() { currentKB.rightArrowKey, currentKB.dKey });
            keyBindings[Action.Left].AddRange(new List<ButtonControl>() { currentKB.leftArrowKey, currentKB.aKey });
            keyBindings[Action.FastKey].AddRange(new List<ButtonControl>() { currentKB.digit1Key, currentKB.numpad1Key });
            keyBindings[Action.LockKey].AddRange(new List<ButtonControl>() { currentKB.digit2Key, currentKB.numpad2Key });
            keyBindings[Action.ReverseKey].AddRange(new List<ButtonControl>() { currentKB.digit3Key, currentKB.numpad3Key });
        }
    }
}
