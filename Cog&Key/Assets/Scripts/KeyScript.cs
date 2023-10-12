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

    private const float SPEED = 14f;
    private const float RANGE = 4f;

    private State currentState;
    private Vector2 attackDirection;
    private float distanceTravelled;
    private IKeyWindable insertTarget;
    private KeyShowcaser uiKeys;

    [SerializeField] private KeyState type;
    public KeyState Type { get { return type; } }

    void Awake()
    {
        currentState = State.Pickup;
        uiKeys = GameObject.Find("OverlayMain").GetComponent<KeyShowcaser>();
    }

    void Update()
    {
        if(currentState == State.Attacking) {
            float distance = SPEED * Time.deltaTime;
            transform.position += distance * (Vector3)attackDirection;
            distanceTravelled += distance;
            if(distanceTravelled >= RANGE) {
                SetState(State.Returning);
            }
        }
        else if(currentState == State.Returning) {
            Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            Vector3 towardsPlayer = (playerPos - transform.position).normalized;
            transform.position += SPEED * Time.deltaTime * towardsPlayer;
            if(Vector2.Distance(playerPos, transform.position) <= 0.5f) {
                SetState(State.PlayerHeld);
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
                uiKeys.MainKeyStatusUpdate(true, Type);
                uiKeys.SmallKeyStatusUpdate(true, Type);
                break;
            case State.Attacking:
                gameObject.SetActive(true);
                uiKeys.MainKeyStatusUpdate(false, Type);
                uiKeys.SmallKeyStatusUpdate(false, Type);
                transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
                distanceTravelled = 0;
                break;
            case State.Attached:
                uiKeys.MainKeyStatusUpdate(false, Type);
                uiKeys.SmallKeyStatusUpdate(false, Type);
                break;
        }
    }

    // gives the player possession of a key pickup, turning it into an ability
    public void Equip()
    {
        SetState(State.PlayerHeld);
        PlayerScript player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        switch(Type) {
            case KeyState.Fast:
                player.FastKey = this;
                uiKeys.SmallKeyStatusUpdate(true, Type);
                break;

            case KeyState.Lock:
                player.LockKey = this;
                uiKeys.SmallKeyStatusUpdate(true, Type);
                break;

            case KeyState.Reverse:
                player.ReverseKey = this;
                uiKeys.SmallKeyStatusUpdate(true, Type);
                break;
        }

    }

    // called when the player shoots the key out to try and insert it
    public void Attack(Vector2 direction) {
        if(currentState != State.PlayerHeld) {
            return;
        }

        SetState(State.Attacking);
        attackDirection = direction.normalized;

        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        if(attackDirection.y >= 1) {
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if(attackDirection.y <= -1) {
            transform.rotation = Quaternion.Euler(0, 0, -90);
        }
        else if(attackDirection.x >= 1) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if(attackDirection.x <= -1) {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public void Detach() {
        if(insertTarget == null || currentState != State.Attached) {
            return;
        }

        uiKeys.MainKeyStatusUpdate(false, Type);

        insertTarget.InsertKey(KeyState.Normal);
        insertTarget = null;
        SetState(State.Returning);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        if(currentState == State.Pickup && player != null) {
            Equip();
        }

        IKeyWindable windable = collision.gameObject.GetComponent<IKeyWindable>();
        if(currentState == State.Attacking && windable != null) {
            insertTarget = windable;
            insertTarget.InsertKey(type);
            SetState(State.Attached);
            transform.SetParent(collision.gameObject.transform);
        }
    }
}
