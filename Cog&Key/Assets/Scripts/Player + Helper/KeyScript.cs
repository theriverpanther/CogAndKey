using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// keys can be pickups in the world, an ability for the player to use, or attached to a level element
public class KeyScript : MonoBehaviour
{
    public enum State {
        Pickup,
        PlayerHeld,
        Attacking,
        Attached,
        Returning
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

    [SerializeField] private bool StartEquipped;
    [SerializeField] private KeyState type;
    public KeyState Type { get { return type; } }

    public bool Attached { get { return currentState == State.Attached; } }

    void Awake()
    {
        currentState = State.Pickup;
        uiKeys = GameObject.Find("OverlayMain")?.GetComponent<KeyShowcaser>();
        physicsBody = GetComponent<Rigidbody2D>();
        keyUI = GameObject.Find("KeyBG")?.GetComponent<KeyUI>();
        player = GameObject.Find("Player");
        visual = transform.GetChild(0).gameObject;
        keyAni = visual.GetComponent<Animator>();
        boxCollider = GetComponent<Collider2D>();
    }

    void Start() {
        if (StartEquipped) {
            Equip();
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
            Vector3 playerPos = player.transform.position;
            Vector3 towardsPlayer = (playerPos - transform.position).normalized;
            float newSpeed = velocity.magnitude + Time.deltaTime * 2 * ACCEL;

            velocity = newSpeed * towardsPlayer;
            transform.position += Time.deltaTime * velocity;

            if(Vector2.Distance(playerPos, transform.position) <= 0.5f) {
                // use distance check instead of collision trigger so that the key gets more to the center of the player
                SetState(State.PlayerHeld);
            }
        }
        else if(currentState == State.Attached) {
            if(PlayerInput.Instance.JustPressed(keyToInput[Type]) || PlayerInput.Instance.JustPressed(PlayerInput.Action.Recall)
                || Mathf.Abs(player.transform.position.y - transform.position.y) > 16f 
                || Mathf.Abs(player.transform.position.x - transform.position.x) > 16f
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
                transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
                break;
            case State.Attached:
                boxCollider.enabled = false; // disable collider because triggers are sent to the parent
                break;
        }

        keyUI?.KeyUpdate(currentState, Type);
    }

    // gives the player possession of a key pickup, turning it into an ability
    public void Equip() {
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
        if(insertTarget == null || currentState != State.Attached) {
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

        if(currentState == State.Pickup && collision.gameObject.tag == "Player") {
            keyAni.SetInteger("Status", 0);
            Equip();
            return;
        }

        KeyWindable windable = collision.gameObject.GetComponent<KeyWindable>();
        if(currentState == State.Attacking && windable != null) {
            insertTarget = windable;
            insertTarget.InsertKey(this);
            
            keyAni.SetInteger("Status", (int)windable.InsertedKeyType);

            SetState(State.Attached);
            transform.SetParent(collision.gameObject.transform);
            return;
        }

        if(currentState == State.Attacking && collision.gameObject.tag == "Wall") {
            keyAni.SetInteger("Status", 0);
            SetState(State.Returning);
        }
    }

    private void SetActive(bool active) {
        visual.SetActive(active);
        boxCollider.enabled = active;
    }
}
