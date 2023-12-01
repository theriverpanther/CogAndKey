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

    private State currentState;
    private Vector3 velocity;
    private KeyWindable insertTarget;
    private Rigidbody2D physicsBody;
    private GameObject player;

    private KeyShowcaser uiKeys;
    private KeyUI keyUI;

    [SerializeField] private KeyState type;
    public KeyState Type { get { return type; } }

    void Awake()
    {
        currentState = State.Pickup;
        uiKeys = GameObject.Find("OverlayMain")?.GetComponent<KeyShowcaser>();
        physicsBody = GetComponent<Rigidbody2D>();
        keyUI = GameObject.Find("KeyBG")?.GetComponent<KeyUI>();
        player = GameObject.Find("Player");
    }

    void Update()
    {
        if(currentState == State.Attacking) {
            Vector3 startVel = velocity;
            velocity += Time.deltaTime * ACCEL * -velocity.normalized;

            if(Vector2.Dot(startVel, velocity) < 0) {
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
            if(Mathf.Abs(player.transform.position.y - transform.position.y) > 12f || Mathf.Abs(player.transform.position.x - transform.position.x) > 12f) {
                Detach();
            }
        }
    }

    public void SetState(State keyState) {
        currentState = keyState;
        transform.SetParent(null);

        switch(currentState)
        {
            case State.Pickup:
                break;
            case State.PlayerHeld:
                gameObject.SetActive(false);
                break;
            case State.Attacking:
                gameObject.SetActive(true);
                transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
                break;
            case State.Attached:
                break;
        }

        keyUI?.KeyUpdate(currentState, Type);
    }

    // gives the player possession of a key pickup, turning it into an ability
    public void Equip() {
        SetState(State.PlayerHeld);
        PlayerScript player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        player.SetSelectedKey(Type);

        switch(Type) {
            case KeyState.Fast:
                player.FastKey = this;
                break;

            case KeyState.Lock:
                player.LockKey = this;
                break;

            case KeyState.Reverse:
                player.ReverseKey = this;
                break;
        }

        if(keyUI != null) {
            keyUI.UpdateKeyUI(Type);
        }

    }

    // called when the player shoots the key out to try and insert it
    public void Attack(Vector2 direction, Vector2 playerSpeed) {
        if(currentState != State.PlayerHeld) {
            return;
        }

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
        if(currentState == State.Pickup && collision.gameObject.tag == "Player") {
            Equip();
            return;
        }

        KeyWindable windable = collision.gameObject.GetComponent<KeyWindable>();
        if(currentState == State.Attacking && windable != null) {
            insertTarget = windable;
            insertTarget.InsertKey(this);
            SetState(State.Attached);
            transform.SetParent(collision.gameObject.transform);
            return;
        }

        if(currentState == State.Attacking && collision.gameObject.tag == "Wall") {
            SetState(State.Returning);
        }
    }
}
