using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBox : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject helperUIObj;
    private GameObject helperObj;

    private HelperUI helperUI;
    private HelperCreature helperScript;

    [SerializeField]
    string textToShow;
    [SerializeField]
    Texture2D imgToShow;
    [SerializeField]
    bool attachToCenterPoint = false;
    [SerializeField]
    float textSpeed = 0.1f;


    void Start()
    {
        helperObj = GameObject.FindWithTag("Helper");
        helperUIObj = helperObj.gameObject.transform.GetChild(1).gameObject;
        helperUI = helperUIObj.GetComponent<HelperUI>();
        helperScript = helperObj.GetComponent<HelperCreature>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if(attachToCenterPoint)
            {
                helperScript.followPlayer = false;
                helperScript.SetGoPoint(transform.position);
                Debug.Log("Not following player.");
            }
            helperUI.SetTextSpeed(textSpeed);
            helperUI.ShowHelper();
            if (imgToShow != null)
            {
                helperUI.ShowImage(imgToShow);
            }
            helperUI.StartText(textToShow);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            helperUI.HideHelper();
            helperScript.followPlayer = true;

            Debug.Log("Following player.");
        }
    }
}
