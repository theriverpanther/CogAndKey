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
    private float textSpeed = 0.1f;
    public string fullText;
    private string currentTxt = "";
    private bool showImage = false;

    void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    IEnumerator ShowText()
    {
        for(int i = 0; i < fullText.Length; i++)
        {
            currentTxt = fullText.Substring(0,i);
            textToMod.text = currentTxt;
            yield return new WaitForSeconds(textSpeed);
        }

        imgToShow.gameObject.GetComponent<Animator>().SetBool("Fade", true);
        yield return null;
    }

    public void StartText(string text)
    {
        fullText = text;
        StartCoroutine(ShowText());
    }

    public void ShowImage(Texture2D img)
    {
        imgToShow.sprite = Sprite.Create(img, new Rect(0.0f, 0.0f, img.width, img.height), new Vector2(0.5f, 0.5f), 100.0f);
        showImage = true;
    }

    public void EnableDisableImage()
    {
        showImage = !showImage;
    }

    public void HideHelper()
    {
        gameObject.GetComponent<Animator>().SetBool("Fade", false);
        imgToShow.gameObject.GetComponent<Animator>().SetBool("Fade", false);
    }
    public void ShowHelper()
    {
        gameObject.GetComponent<Animator>().SetBool("Fade", true);

        if(imgToShow != null)
        {
            imgToShow.gameObject.GetComponent<Animator>().SetBool("Fade", true);
        }
    }

    public void SetTextSpeed(float speed)
    {
        textSpeed = speed;
    }
}
