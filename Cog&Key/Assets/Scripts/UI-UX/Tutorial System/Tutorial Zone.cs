using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialZone : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private int attempts;

    [SerializeField]
    int showAfterAttempts;

    [SerializeField]
    GameObject showThisZone;

    GameObject soundManager;

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
        if (collision.gameObject.tag == "Player")
        {
            attempts++;
        }

        if (attempts == showAfterAttempts)
        {
            showThisZone.SetActive(true);
            if(showThisZone.GetComponent<Animator>() != null)
            {
                showThisZone.GetComponent<Animator>().SetTrigger("ShowPopup");
            }
            SoundManager.Instance?.PlaySound("Tutorial", .4f); //Sound Manager Tutorial Ping
        }


    }
}
