using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// keys can be pickups in the world, an ability for the player to use, or attached to a level element
public class KeyScript : MonoBehaviour
{
    [SerializeField] private bool StartEquipped;
    [SerializeField] private KeyWindable StartAttachedTo;
    [SerializeField] private KeyState type;

    [SerializeField] private GameObject keyAcquiredUI;

    private Animator animator;

    public enum State {
        FloatPickup,
        PlayerHeld,
        Attacking,
        Attached,
        Returning,
        AttachPickup
    }

    private const float SPEED = 20f;
    private const float ACCEL = 40.0f;

    public static Dictionary<KeyState, PlayerInput.Action> keyToInput = new Dictionary<KeyState, PlayerInput.Action>() { 
        { KeyState.Fast, PlayerInput.Action.FastKey },
        { KeyState.Lock, PlayerInput.Action.LockKey },
        { KeyState.Reverse, PlayerInput.Action.ReverseKey }
    };

    private State currentState;
    private Vector3 velocity;
    private KeyWindable insertTarget;
    private Rigidbody2D physicsBody;
    private GameObject player;
    private Animator keyAni;

    private KeyShowcaser uiKeys;
    private KeyUI keyUI;
    private GameObject visual;
    private Collider2D boxCollider;
    private Vector3 returnStart;
    private float returnTime;
    private bool returnFaster;
    public KeyState Type { get { return type; } }

    public bool Attached { get { return currentState == State.Attached || currentState == State.AttachPickup; } }

    void Awake()
    {
        currentState = State.FloatPickup;
        uiKeys = GameObject.Find("OverlayMain")?.GetComponent<KeyShowcaser>();
        physicsBody = GetComponent<Rigidbody2D>();
        keyUI = GameObject.Find("KeyBG")?.GetComponent<KeyUI>();
        player = GameObject.Find("Player");
        visual = transform.GetChild(0).gameObject;
        keyAni = visual.GetComponent<Animator>();
        boxCollider = GetComponent<Collider2D>();
        animator = visual.GetComponent<Animator>();
    }

    void Start() {
        if(StartEquipped) {
            Equip();
        }
        else if(StartAttachedTo != null && currentState != State.PlayerHeld) { // the key might be equipped to the player before running this function
            SnapPoint? snap = StartAttachedTo.FindSnapPoint(this);
            transform.position = StartAttachedTo.transform.position + snap.Value.localPosition;
            AttachTo(StartAttachedTo);
            StartAttachedTo.Insertible = false;
            currentState = State.AttachPickup;
            boxCollider.enabled = true;
        }
    }

    void Update() {
        if(Time.timeScale == 0) {
            return;
        }

        if(currentState == State.PlayerHeld) {
            // check if the player is throwing this
            Vector2? throwDirection = PlayerInput.Instance.GetThrowDirection(Type);
            if(throwDirection.HasValue) {
                Attack(throwDirection.Value);
            }
        }
        else if(currentState == State.Attacking) {
            velocity += Time.deltaTime * ACCEL * -velocity.normalized;

            if(velocity.sqrMagnitude < 1f) {
                SetState(State.Returning);
            } else {
                transform.position += Time.deltaTime * velocity;
            }
        }
        else if(currentState == State.Returning) {
            returnTime += (returnFaster ? 6f : 3.0f) * Time.deltaTime;
            float scaledTime = returnTime * returnTime;
            transform.position = (1.0f - scaledTime) * returnStart + scaledTime * player.transform.position;

            if(returnTime >= 1f) {
                SetState(State.PlayerHeld);
                if(Gamepad.current != null) {
                    PlayerInput.Instance.Rumble(0.2f, 0.2f);
                }
            }
        }
        else if(currentState == State.Attached) {
            if(PlayerInput.Instance.JustPressed(keyToInput[Type]) || PlayerInput.Instance.IsRecallKey(type)
                || Mathf.Abs(player.transform.position.y - transform.position.y) > 30f 
                || Mathf.Abs(player.transform.position.x - transform.position.x) > 30f
            ) {
                Detach();
            }
        }
    }

    public void SetState(State keyState) {
        currentState = keyState;
        transform.SetParent(null);
        boxCollider.enabled = true;

        switch(currentState) {
            case State.PlayerHeld:
               
                SetActive(false);
                break;
            case State.Attacking:
                SetActive(true);
                transform.localPosition = player.transform.position;
                break;
            case State.Attached:
                boxCollider.enabled = false; // disable collider because triggers are sent to the parent
                break;
            case State.Returning:
                returnStart = transform.position;
                returnTime = 0.0f;
                returnFaster = Vector2.Distance(transform.position, player.transform.position) < 2.0f;
                break;
        }

        keyUI?.KeyUpdate(currentState, Type);
    }

    // gives the player possession of a key pickup, turning it into an ability
    public void Equip() {
        if(currentState == State.AttachPickup) {
            insertTarget.Insertible = true;
            Detach();
        }
        SetState(State.PlayerHeld);
        PlayerInput.Instance.SelectedKey = Type;
        player.GetComponent<PlayerScript>().EquippedKeys[Type] = true;
        if(keyUI != null) {
            keyUI.UpdateKeyUI(Type);
        }
    }

    // called when the player shoots the key out to try and insert it
    public void Attack(Vector2 direction) {
        if(currentState != State.PlayerHeld) {
            return;
        }

        Vector2 playerSpeed = player.GetComponent<Rigidbody2D>().velocity;
        if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
            playerSpeed.y = 0;
        } else {
            playerSpeed.x = 0;
        }

        direction = direction.normalized;
        SetState(State.Attacking);
        velocity = SPEED * direction + 0.5f * playerSpeed;

        // rotate the visual
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        if(direction.y >= 1) {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if(direction.y <= -1) {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if(direction.x >= 1) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if(direction.x <= -1) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public void Detach() {
        if(insertTarget == null) {
            return;
        }

        insertTarget.RemoveKey();
        insertTarget = null;
        SetState(State.Returning);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.isTrigger) {
            return;
        }

        if((currentState == State.FloatPickup || currentState == State.AttachPickup) && collision.gameObject.tag == "Player") {
            keyAni.SetInteger("Status", 0);
            keyAcquiredUI.GetComponent<WindKeySplash>().ShowInformation(Type);
            Equip();
            return;
        }

        KeyWindable windable = collision.gameObject.GetComponent<KeyWindable>();
        SnapPoint? snap = windable == null ? null : windable.FindSnapPoint(this);
        if(currentState == State.Attacking && windable != null && windable.Insertible && snap.HasValue) {
            // attach to an object
            transform.position = windable.transform.position + snap.Value.localPosition;
            AttachTo(windable);
            SoundManager.Instance.PlaySound("Lock", .3f); //Sound Manager Code
            if(Gamepad.current != null) {
                PlayerInput.Instance.Rumble(0.6f, 0.1f);
            }
            return;
        }

        if(currentState == State.Attacking && (collision.gameObject.tag == "Wall" || windable != null)) {
            // bounce off of walls
            keyAni.SetInteger("Status", 0);
            SetState(State.Returning);
        }
    }

    private void SetActive(bool active) {
        visual.SetActive(active);
        boxCollider.enabled = active;
    }

    private void AttachTo(KeyWindable windable) {
        insertTarget = windable;
        insertTarget.InsertKey(this);

        keyAni.SetInteger("Status", (int)windable.InsertedKeyType);

        SetState(State.Attached);
        transform.SetParent(windable.gameObject.transform);
    }
}
