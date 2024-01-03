using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
// capitalizedString will now be "Hello World"

public class HelperAnimations : MonoBehaviour
{
    [SerializeField] List<string> idleChances;
    [SerializeField] List<string> emotes;

    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
    private Animator visualAni;
    private float rng = 0;

    public void Start()
    {
        visualAni = GetComponent<Animator>();
    }
    public void IdleDivisionChance()
    {
        rng = Random.Range(0, 100);

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
}
