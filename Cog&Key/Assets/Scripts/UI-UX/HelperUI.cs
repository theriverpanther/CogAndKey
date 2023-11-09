using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelperUI : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    TextMeshProUGUI textToMod;

    [SerializeField]
    Image imgToShow;

    [SerializeField]
    bool forceFade;
    private float textSpeed = 0.1f;
    public string fullText;
    private string currentTxt = "";
    private bool showImage = false;

    [SerializeField]
    private Animator imageAnimator;

    void Start()
    {
        forceFade = true;
        GetComponent<Canvas>().worldCamera = Camera.main;

        imageAnimator.SetInteger("animationPlayerIndex", -1);
    }

    IEnumerator ShowText()
    {
        if(fullText.Length <= 1)
        {
            gameObject.GetComponent<Animator>().SetBool("Fade", true);
        }


        for (int i = 0; i < fullText.Length; i++)
        {
            currentTxt = fullText.Substring(0,i);
            textToMod.text = currentTxt;
            yield return new WaitForSeconds(textSpeed);
        }

        if (fullText.Length <= 1)
        {
            gameObject.GetComponent<Animator>().SetBool("Fade", true);
        }
        //imgToShow.gameObject.GetComponent<Animator>().SetBool("Fade", true);
        yield return null;
    }

    public void StartText(string text)
    {
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
        if(forceFade)
        {
            gameObject.GetComponent<Animator>().SetBool("Fade", false);
        }

        //imgToShow.gameObject.GetComponent<Animator>().SetBool("Fade", false);
        Debug.Log("Hiding Helper...");
    }
    public void ShowHelper()
    {
        Debug.Log("Showing Helper...");

        if(imgToShow.sprite != null)
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
    }

    public void SetTextSpeed(float speed)
    {
        textSpeed = speed;
    }
}
