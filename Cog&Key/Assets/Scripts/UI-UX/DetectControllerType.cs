using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class ControllerKeyToImgPair
{
    public string key;
    public Texture2D imagePair;
}

public class DetectControllerType : MonoBehaviour
{
    [SerializeField]
    public List<ControllerKeyToImgPair> xboxPairs;
    [SerializeField]
    public List<ControllerKeyToImgPair> playstationPairs;
    [SerializeField]
    public List<ControllerKeyToImgPair> keyboardPairs;
    [SerializeField]
    public List<ControllerKeyToImgPair> otherPairs;
    [SerializeField]
    public List<ControllerKeyToImgPair> genPairs;

    private Dictionary<string, Texture2D> xboxPairsDictionary;
    private Dictionary<string, Texture2D> playstationPairsDictionary;
    private Dictionary<string, Texture2D> keyboardPairsDictionary;
    private Dictionary<string, Texture2D> otherPairsDictionary;
    private Dictionary<string, Texture2D> genPairsDictionary;

    string current;

    public string Current
    {
        get { return current; }
    }

    // Start is called before the first frame update
    void Start()
    {
        PopulateDictionary();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PopulateDictionary()
    {
        current = getControllerType();

        xboxPairsDictionary = new Dictionary<string, Texture2D>();
        playstationPairsDictionary = new Dictionary<string, Texture2D>();
        keyboardPairsDictionary = new Dictionary<string, Texture2D>();
        otherPairsDictionary = new Dictionary<string, Texture2D>();
        genPairsDictionary = new Dictionary<string, Texture2D>();

        foreach (ControllerKeyToImgPair pair in xboxPairs)
        {
            xboxPairsDictionary[pair.key] = pair.imagePair;
        }

        foreach (ControllerKeyToImgPair pair in playstationPairs)
        {
            playstationPairsDictionary[pair.key] = pair.imagePair;
        }

        foreach (ControllerKeyToImgPair pair in keyboardPairs)
        {
            keyboardPairsDictionary[pair.key] = pair.imagePair;
        }

        foreach (ControllerKeyToImgPair pair in otherPairs)
        {
            otherPairsDictionary[pair.key] = pair.imagePair;
        }

        foreach (ControllerKeyToImgPair pair in genPairs)
        {
            genPairsDictionary[pair.key] = pair.imagePair;
        }
    }

    private string getControllerType()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad != null)
        {
            if (gamepad is DualShockGamepad)
            {
                current = "playstation";
            }
            else if (gamepad is XInputController)
            {
                current = "xbox";
            }
            else
            {
                current = "other";
            }
        }
        else if (Keyboard.current != null || Mouse.current != null)
        {
            current = "keyboard";
        }
        //Debug.Log(current);
        return current;
    }

    public Texture2D ReturnImage(string mapping, bool specific = false)
    {
        Texture2D texture = null;
        //Debug.Log("Trying to get an image one min with " + mapping + " " + current);

        current = getControllerType();

        if(xboxPairsDictionary == null)
        {
            PopulateDictionary();
        }

        if (!specific)
        {
            switch (current)
            {
                case "xbox":
                    xboxPairsDictionary.TryGetValue(mapping, out texture);
                    break;
                case "playstation":
                    playstationPairsDictionary.TryGetValue(mapping, out texture);
                    break;
                case "keyboard":
                    keyboardPairsDictionary.TryGetValue(mapping, out texture);
                    break;
                default:
                    otherPairsDictionary.TryGetValue(mapping, out texture);
                    break;
            }
        } else
        {
            if(current == "keyboard")
            {
                keyboardPairsDictionary.TryGetValue(mapping, out texture);
            }
            else
            {
                genPairsDictionary.TryGetValue(mapping, out texture);
            }
        }

        //Debug.Log("Returning new texture...");

        return texture;
    }


}
