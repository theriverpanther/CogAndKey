using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialZone : MonoBehaviour
{
    // Start is called before the first frame update
    private int attempts;

    [SerializeField]
    int showAfterAttempts;

    [SerializeField]
    GameObject showThisZone;

    void Start()
    {
        showThisZone.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(attempts == showAfterAttempts - 1)
        {
            showThisZone.SetActive(true);
        }

        if(collision.gameObject.tag == "Player")
        {
            attempts++;
        }
    }
}
