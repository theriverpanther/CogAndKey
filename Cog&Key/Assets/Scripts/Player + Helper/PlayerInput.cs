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
        SelectFast,
        SelectLock,
        SelectReverse,
        Recall,
        Pause
    }

    private readonly int NUM_ACTIONS;

    // contain a spot for each Action, index matches enum int value
    private bool[] pressedLastFrame;
    private bool[] pressedThisFrame;

    // used to detect when a controller is plugged in or unplugged
    private Gamepad currentGP;
    private Keyboard currentKB;
    private Mouse currentMouse;

    private string controllerName;

    public string ControllerName { get { return controllerName; } }

    private Dictionary<Action, List<ButtonControl>> keyBindings;
    private Dictionary<Action, Vector2> throwDirectionToVector = new Dictionary<Action, Vector2>() {
        { Action.ThrowUp, Vector2.up },
        { Action.ThrowDown, Vector2.down },
        { Action.ThrowLeft, Vector2.left },
        { Action.ThrowRight , Vector2.right}
    };
    private Dictionary<KeyState, Action> keyToSelector = new Dictionary<KeyState, Action>() {
        { KeyState.Fast, Action.SelectFast },
        { KeyState.Lock, Action.SelectLock },
        { KeyState.Reverse, Action.SelectReverse }
    };
    private float jumpBuffer;

    public bool JumpBuffered { get { return jumpBuffer > 0; } }
    public KeyState SelectedKey { get; set; }

    public PlayerScript Player { get; set; }

    private bool locked;
    public bool Locked { set {
        locked = value;
        if(locked) { 
            ClearKeybindings();
        } else { 
            ConstructKeyBindings();
        }    
    } }

    private static PlayerInput instance;
    public static PlayerInput Instance { get {  
        if(instance == null) {
            instance = new PlayerInput();
        }
        return instance;
    } }

    private PlayerInput() {
        NUM_ACTIONS = Enum.GetNames(typeof(Action)).Length;
        pressedLastFrame = new bool[NUM_ACTIONS];
        pressedThisFrame = new bool[NUM_ACTIONS];
        SelectedKey = KeyState.None;

        keyBindings = new Dictionary<Action, List<ButtonControl>>();
        for(int i = 0; i < NUM_ACTIONS; i++) {
            keyBindings[(Action)i] = new List<ButtonControl>();
        }
        ConstructKeyBindings();
    }

    // must be called once per frame
    public void Update() {
        if(!locked && (Gamepad.current != currentGP || Keyboard.current != currentKB || Mouse.current != currentMouse)) {
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

        foreach(KeyState keyType in KeyScript.keyToInput.Keys) {
            if((JustPressed(KeyScript.keyToInput[keyType]) || JustPressed(keyToSelector[keyType])) && Player.EquippedKeys[keyType]) {
                SelectedKey = keyType;
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

    // if the player is input a throw this frame, returns the direction the player is attempting to throw the key this frame
    public Vector2? GetThrowDirection(KeyState key) {
        if(JustPressed(KeyScript.keyToInput[key])) {
            // when pressing the controller button, use the left stick for the direction and default to the player's facing direction
            Vector2 result = Player.gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
            if(IsPressed(Action.Up)) {
                result = Vector2.up;
            }
            else if(IsPressed(Action.Down)) {
                result = Vector2.down;
            }
            return result;
        }

        if(key != SelectedKey) {
            return null;
        }

        if(MouseJustClicked()) {
            // throw in the direction the mouse is relative to the player's position
            Vector3 mouseDir = GetMouseWorldPosition() - Player.transform.position;
            if(Mathf.Abs(mouseDir.x) > Mathf.Abs(mouseDir.y)) {
                mouseDir.y = 0;
            } else {
                mouseDir.x = 0;
            }
            return mouseDir.normalized;
        }

        Action[] throwDirections = new Action[4] { Action.ThrowUp, Action.ThrowDown, Action.ThrowLeft, Action.ThrowRight };
        foreach(Action throwDirection in throwDirections) {
            if(JustPressed(throwDirection)) {
                return throwDirectionToVector[throwDirection];
            }
        }

        return null;
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

        ClearKeybindings();

        // add gamepad bindings
        if (currentGP != null) {
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
            keyBindings[Action.SelectFast].AddRange(new List<ButtonControl>() { currentKB.digit1Key, currentKB.numpad1Key, currentKB.zKey });
            keyBindings[Action.SelectLock].AddRange(new List<ButtonControl>() { currentKB.digit2Key, currentKB.numpad2Key, currentKB.xKey });
            keyBindings[Action.SelectReverse].AddRange(new List<ButtonControl>() { currentKB.digit3Key, currentKB.numpad3Key, currentKB.cKey });
            keyBindings[Action.Pause].AddRange(new List<ButtonControl>() { currentKB.escapeKey });
        }

        // add mouse input
        if(currentMouse != null) {
            //keyBindings[Action.FastKey].AddRange(new List<ButtonControl>() { currentMouse.leftButton });
            keyBindings[Action.Recall].AddRange(new List<ButtonControl>() { currentMouse.rightButton });
        }
    }

    private void ClearKeybindings() {
        for(int i = 0; i < NUM_ACTIONS; i++) {
            keyBindings[(Action)i].Clear();
        }
    }

    public bool MouseJustClicked() {
        return !locked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    // should not be called if there is no mouse
    public Vector3 GetMouseWorldPosition() {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
