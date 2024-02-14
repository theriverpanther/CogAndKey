using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonShowcase : MonoBehaviour
{
    [SerializeField]
    string mapping;

    [SerializeField]
    bool specific;

    [SerializeField]
    bool animated;

    [SerializeField]
    float scale;

    DetectControllerType detectControllerType;

    [SerializeField]
    public GameObject hideOther;

    private Animator ani;
    private void Start()
    {
        detectControllerType = GetComponent<DetectControllerType>();
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        detectControllerType = GetComponent<DetectControllerType>();


        transform.localScale = new Vector3(scale, scale, scale);    

        Texture2D img = detectControllerType.ReturnImage(mapping, specific);
        Debug.Log(img);

        if (img != null)
        {
            if(GetComponent<SpriteRenderer>() != null)
            {
                GetComponent<SpriteRenderer>().sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), Vector2.zero);
            } else
            {
                Sprite spr = Sprite.Create(img, new Rect(0, 0, img.width, img.height), new Vector2(0.5f, 0.5f), 100.0f);
                GetComponent<Image>().sprite = spr;
                Debug.Log("UI IMG: " + spr.texture.name);
            }
        }

        ani = GetComponent<Animator>();
        if(ani != null)
        {
            ani.SetBool("Fade", true);
        }

        if(animated)
        {
            switch(detectControllerType.Current)
            {
                case "xbox":
                    ani.SetTrigger("Xbox");
                    break;
                case "playstation":
                    ani.SetTrigger("Playstation");
                    break;
                case "keyboard":
                    ani.SetTrigger("Keyboard");
                    break;
            }
        }
    }

}
