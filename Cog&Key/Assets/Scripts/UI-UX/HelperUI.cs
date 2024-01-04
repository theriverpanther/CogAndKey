using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelperUI : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    bool topLeftCorner;

    [SerializeField]
    TextMeshProUGUI textToMod;

    GameObject upperText;
    TextMeshProUGUI textToModUpper;

    [SerializeField]
    Image imgToShow;
    Image imgIndicator;
    [SerializeField]
    Texture2D empty;
    Sprite emptySprite;

    [SerializeField]
    bool forceFade;
    private float textSpeed = 0.1f;
    public string fullText;
    private string currentTxt = "";
    private bool showImage = false;

    [SerializeField]
    private Animator imageAnimator;
    [SerializeField]
    private HelperAnimations helperAnimation;

    void Start()
    {
        upperText = GameObject.Find("HelperUIUpperGroup");
        textToModUpper = upperText.transform.Find("HelperText").GetComponent<TextMeshProUGUI>();

        imgIndicator = GameObject.Find("HelperIndicator").GetComponent<Image>();

        forceFade = true;
        GetComponent<Canvas>().worldCamera = Camera.main;
        
        AlertMessage(false);
        imageAnimator.SetInteger("animationPlayerIndex", -1);
        emptySprite = Sprite.Create(empty, new Rect(0.0f, 0.0f, empty.width, empty.height), new Vector2(0.5f, 0.5f), 100.0f);
    }

    IEnumerator ShowText()
    {

        helperAnimation.PlayStopEmote("Chat");
        if (fullText.Length <= 1 && !topLeftCorner)
        {
            gameObject.GetComponent<Animator>().SetBool("Fade", true);
        }

        if(topLeftCorner)
        {
            for (int i = 0; i < fullText.Length; i++)
            {
                currentTxt = fullText.Substring(0, i);
                textToModUpper.text = currentTxt;
                yield return new WaitForSeconds(textSpeed);
            }
        } else
        {
            for (int i = 0; i < fullText.Length; i++)
            {
                currentTxt = fullText.Substring(0, i);
                textToMod.text = currentTxt;
                yield return new WaitForSeconds(textSpeed);
            }
        }

        if (fullText.Length <= 1 && !topLeftCorner)
        {
            gameObject.GetComponent<Animator>().SetBool("Fade", true);
        }

        helperAnimation.PlayStopEmote("Chat");

        yield return null;
    }

    public void StartText(string text, bool inCorner = false)
    {
        topLeftCorner = inCorner;
        fullText = text + " ";
        StartCoroutine(ShowText());
    }

    public void ShowImage(Texture2D img, float size, int index, bool forceFader)
    {
        forceFade = forceFader;
        if (index != -1)
        {
            imageAnimator.enabled = true;
            imageAnimator.SetInteger("animationPlayerIndex", index);
        }
        else
        {
            imageAnimator.enabled = false;
        }

        imgToShow.sprite = Sprite.Create(img, new Rect(0.0f, 0.0f, img.width, img.height), new Vector2(0.5f, 0.5f), 100.0f);
        imgToShow.rectTransform.sizeDelta = new Vector2(size, size);
        showImage = true;

    }

    public void EnableDisableImage()
    {
        showImage = !showImage;
    }

    public void HideHelper()
    {
        if(forceFade && !topLeftCorner)
        {
            gameObject.GetComponent<Animator>().SetBool("Fade", false);
        }

        //imgToShow.gameObject.GetComponent<Animator>().SetBool("Fade", false);
        Debug.Log("Hiding Helper...");
    }
    public void ShowHelper(bool showTextAboveHead = true)
    {
            if (imgToShow.sprite != null)
                    {
                        Debug.Log("Show image!");
                        imgToShow.gameObject.SetActive(true);
                        //imgToShow.gameObject.GetComponent<Animator>().SetBool("Fade", true);
                        gameObject.GetComponent<Animator>().SetBool("Fade", true);
                    }
                    else
                    {
                        gameObject.GetComponent<Animator>().SetBool("Fade", true);
                        imgToShow.gameObject.SetActive(false);
                    }

        /// shows/hides text box above the head
        imgToShow.transform.parent.transform.GetChild(1).gameObject.SetActive(showTextAboveHead);
    }

    public void SetTextSpeed(float speed)
    {
        textSpeed = speed;
    }

    public void IndicatorImage(Texture2D img = null)
    {
        if(img != null)
        {
            imgIndicator.sprite = Sprite.Create(img, new Rect(0.0f, 0.0f, img.width, img.height), new Vector2(0.5f, 0.5f), 100.0f);
        } else
        {
            imgIndicator.sprite = emptySprite;
        }
    }

    public void AlertMessage(bool state)
    {
        upperText.SetActive(state);
    }
}
