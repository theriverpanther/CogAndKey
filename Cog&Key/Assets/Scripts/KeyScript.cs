using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// keys can be pickups in the world, an ability for the player to use, or attached to a level element
public class KeyScript : MonoBehaviour
{
    public enum State {
        Pickup,
        PlayerHeld,
        Attached
    }

    private State currentState;
    public KeyState Type;

    void Start()
    {
        currentState = State.Pickup;
    }

    void Update()
    {
        
    }

    public void SetState(State keyState) {
        currentState = keyState;

        switch(currentState)
        {
            case State.Pickup:
                break;
            case State.PlayerHeld:
                gameObject.SetActive(false);
                break;
            case State.Attached:
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
                player.FastKeyEquipped = true;
                break;

            case KeyState.Lock:
                player.LockKeyEquipped = true;
                break;

            case KeyState.Reverse:
                player.ReverseKeyEquipped = true;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        if(currentState == State.Pickup && player != null) {
            Equip();
        }
    }
}
