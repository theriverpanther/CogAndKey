using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WindKeySplash : MonoBehaviour
{
    // Start is called before the first frame update

    Animator animator;

    [SerializeField]
    TextMeshProUGUI keyInformation;
    [SerializeField]
    TextMeshProUGUI keyType;


    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowInformation(KeyState key)
    {
        switch(key)
        {
            case KeyState.Lock:
                keyType.text = "Lock Key";
                keyInformation.text = "Lock any object or creature with a key slot in place.";
                break;
            case KeyState.Reverse:
                keyType.text = "Reverse Key";
                keyInformation.text = "Reverse the direction of any object or creature with a key slot.";
                break;
            case KeyState.Fast:
                keyType.text = "Speed Key";
                keyInformation.text = "Speed up any object or creature with a key slot in it's current direction.";
                break;
        }

        StartCoroutine(ShowInformationKey());
    }

    IEnumerator ShowInformationKey()
    {
        animator.SetBool("Fade", true);

        // PlayerInput.Instance.Locked = true;

         yield return new WaitForSeconds(3f);

        animator.SetBool("Fade", false);

        // PlayerInput.Instance.Locked = false;

        yield return null;
    }
}
