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

    [SerializeField]
    bool forceFade = true;

    [SerializeField]
    float size = 6f;

    [SerializeField]
    public int animationIndex = -1;


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
            if (imgToShow != null)
            {
                helperUI.ShowImage(imgToShow , size, animationIndex, forceFade);
            }

            helperUI.SetTextSpeed(textSpeed);
          
            helperUI.ShowHelper();

            if (attachToCenterPoint)
            {
                helperScript.followPlayer = false;
                helperScript.SetGoPoint(transform.position);
                Debug.Log("Not following player.");
            } else
            {
                helperScript.followPlayer = true;
            }

            helperUI.StartText(textToShow);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            helperUI.HideHelper();
            
        }
    }
}
