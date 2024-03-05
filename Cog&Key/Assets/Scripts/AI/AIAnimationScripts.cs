using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAnimationScripts : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void KillPlayOnAnimationEnd()
    {
        PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();
        player.Die();
        player.playerAnimation.SetBool("Dead", false);
        PlayerInput.Instance.Locked = false;
    }
}
