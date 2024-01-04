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
    public bool overlapping;

    [SerializeField]
    bool topRightCorner;

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
            if(!topRightCorner && textToShow != "TRIGGER") 
            {
                if (imgToShow != null)
                {
                    helperUI.ShowImage(imgToShow, size, animationIndex, forceFade);
                }

                helperUI.SetTextSpeed(textSpeed);

                helperUI.ShowHelper();

                helperUI.StartText(textToShow);
            } else
            {
                if(textToShow != "TRIGGER")
                {
                    helperUI.AlertMessage(true);
                    helperUI.SetTextSpeed(textSpeed);
                    helperUI.StartText(textToShow, true);
                    helperUI.IndicatorImage(imgToShow);
                    helperUI.ShowImage(imgToShow, size, animationIndex, forceFade);
                    helperUI.ShowHelper(false);
                }

            }

            if (attachToCenterPoint)
            {
                helperScript.followPlayer = false;
                helperScript.SetGoPoint(transform.position);
                Debug.Log("Not following player.");
            }
            else
            {
                helperScript.followPlayer = true;
            }

        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !overlapping)
        {
            if (!topRightCorner)
            {
                helperUI.HideHelper();
                helperUI.IndicatorImage();
            } else
            {
                helperUI.AlertMessage(false);
                helperUI.IndicatorImage();
            }
        }
    }
}
