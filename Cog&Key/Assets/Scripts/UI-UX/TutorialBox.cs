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

    [Header("Am I overlapping with another box?")]

    [SerializeField]
    public bool overlapping;

    [Header("Should the text appear in the top left corner?")]
    [SerializeField]
    bool topLeftCorner;

    [SerializeField]
    string textToShow;

    [Header("What button am I showcasing? (TOOLTIP)")]
    [Tooltip("Check TextOverChar in Helper_3D for names. Put none if animated/not a button")]
    [SerializeField]
    string buttonName;

    [Header("Do I need to specify controller type? (TOOLTIP)")]
    [Tooltip("Xbox or Playstation?")]
    [SerializeField]
    bool buttonIsNotLinkedToSetController;

    [Header("What image do I show if not linked to a button?")]
    [SerializeField]
    private Texture2D imgToShow;

    [Header("Center helper to the box's center?")]
    [SerializeField]
    bool attachToCenterPoint = false;

    [Header("How fast should the text be?")]
    [SerializeField]
    float textSpeed = 0.1f;

    [Header("Should the image fade if leaving the box?")]
    [SerializeField]
    bool forceFade = true;

    [Header("What size is the image?")]
    [SerializeField]
    float size = 6f;

    [Header("Which animation should I play? (TOOLTIP)")]
    [Tooltip("-1 for none, ShowImage Animation tree")]
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
            if(!topLeftCorner && textToShow != "TRIGGER") 
            {
               if (imgToShow != null)
                {
                    helperUI.ShowImage(imgToShow, size, animationIndex, forceFade, buttonName, buttonIsNotLinkedToSetController);
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
                    helperUI.ShowImage(imgToShow, size, animationIndex, forceFade, buttonName, buttonIsNotLinkedToSetController);
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
            if (!topLeftCorner)
            {
                helperUI.HideHelper();
                helperUI.IndicatorImage();
            } else
            {
                helperUI.AlertMessage(false);
                helperUI.HideHelper();
                helperUI.IndicatorImage();
            }
        }
    }

}
