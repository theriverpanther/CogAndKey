using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyPickupScript : MonoBehaviour
{
    public KeyState providedKey;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        if(player != null)
        {
            switch(providedKey)
            {
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

            Destroy(gameObject);
        }
    }
}
