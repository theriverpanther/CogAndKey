using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

enum Directional
{
    Right,
    Left,
    Up,
    Down,
    Custom
}

public class PathChildSettings : MonoBehaviour
{
    [Header("Default is right")]
    [SerializeField]
    Directional direction;

    [SerializeField]
    GameObject hideOther;

    // Start is called before the first frame update
    void Start()
    {
        ChangeDirection();

        if(hideOther != null)
        {
            if(hideOther.GetComponent<Animator>() != null && hideOther.active)
            {
                hideOther.GetComponent<Animator>().SetBool("Fade", false);
            }

            hideOther.SetActive(false);        
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeDirection()
    {
        Debug.Log("My Direction: " + direction);
        switch(direction)
        {
            case Directional.Left:
                transform.localRotation = Quaternion.Euler(0, 0, 180f);
                break;
            case Directional.Up:
                transform.localRotation = Quaternion.Euler(0, 0, 90f);
                break;
            case Directional.Right:
                transform.localRotation = Quaternion.Euler(0, 0, 0f);
                break;
            case Directional.Down:
                transform.localRotation = Quaternion.Euler(0, 0, 270f);
                break;
            case Directional.Custom:
                //do nothing
                break;
        }
    }
}