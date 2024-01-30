using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;
// capitalizedString will now be "Hello World"

public class HelperAnimations : MonoBehaviour
{
    [SerializeField] List<string> idleChances;
    [SerializeField] List<string> emotes;

    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
    private Animator visualAni;
    private float rng = 0;

    private GameObject visualUI;
    private RectTransform visualUIRect;

    private float anchorX;

    public void Start()
    {
        visualAni = GetComponent<Animator>();
        visualUI = GameObject.Find("HelperGuy_VisualFromScene");
        visualUIRect = visualUI.GetComponent<RectTransform>();
        anchorX = 105f;
    }

    public void Update()
    {
        FlipHelperVisualConstantly();
    }
    public void IdleDivisionChance()
    {
        rng = UnityEngine.Random.Range(0, 100);

        if(rng >= 0 && rng <= 10)
        {
            visualAni.SetTrigger(idleChances[0]);
        }

    }

    public void PlayStopEmote(string name)
    {
        name = textInfo.ToTitleCase(name);
        if (emotes.Contains(name)) {
            visualAni.SetBool(name, !visualAni.GetBool(name));
        }
    }

    public void FlipHelperVisualConstantly()
    {
        visualUI.transform.localScale = new Vector3(transform.lossyScale.x, 1f, 1f);
        if(transform.lossyScale.x == -1f)
        {
            visualUIRect.anchoredPosition = new Vector2(anchorX - 19f, visualUIRect.anchoredPosition.y);
        } else
        {
            visualUIRect.anchoredPosition = new Vector2(anchorX, visualUIRect.anchoredPosition.y);
        }
    }
}
