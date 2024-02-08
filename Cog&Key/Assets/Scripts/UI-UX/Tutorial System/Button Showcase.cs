using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(img, new Rect(0, 0, img.width, img.height), Vector2.zero);
        }

        ani = GetComponent<Animator>();
        ani.SetBool("Fade", true);

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



    // Update is called once per frame
    void Update()
    {
        
    }
}
