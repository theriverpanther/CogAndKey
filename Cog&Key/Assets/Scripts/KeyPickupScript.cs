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
                    player.HasFastKey = true;
                    break;

                case KeyState.Lock:
                    player.HasLockKey = true;
                    break;

                case KeyState.Reverse:
                    player.HasReverseKey = true;
                    break;
            }

            Destroy(gameObject);
        }
    }
}
