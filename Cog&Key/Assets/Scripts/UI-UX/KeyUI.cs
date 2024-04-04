using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyUI : MonoBehaviour
{
    // Start is called before the first frame update
    List<GameObject> keys;
    Dictionary<KeyState, GameObject> keyDictionary = new Dictionary<KeyState, GameObject>();
    LevelData data;

    [SerializeField]
    List<Sprite> statusSprites;

    KeyState current = KeyState.None;


    void Awake()
    {
       data = GameObject.FindObjectOfType<LevelData>();

        UpdateDictionary();

    }

    public void Start()
    {
        ShowCollectedKeys();
    }

    public void ShowCollectedKeys()
    {
        foreach(KeyState keyType in new KeyState[3] { KeyState.Fast, KeyState.Lock, KeyState.Reverse }) {
            if(PlayerScript.CurrentPlayer.GetComponent<PlayerScript>().EquippedKeys[keyType]) {
                keyDictionary[keyType].SetActive(true);
            }
        }
    }

    public void UpdateDictionary()
    {
        keys = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            keys.Add(transform.GetChild(i).gameObject);
            keys[i].SetActive(false);
        }
        keyDictionary.Add(KeyState.Fast, keys[0]);
        keyDictionary.Add(KeyState.Lock, keys[1]);
        keyDictionary.Add(KeyState.Reverse, keys[2]);

    }

    public void UpdateKeyUI(KeyState key)
    {
        keyDictionary[key].SetActive(true);
        SetArrowSelector();
    }

    private void Update()
    {
        //keyDictionary[PlayerInput.Instance.SelectedKey].transform.GetChild(3).gameObject.SetActive(true);
    }

    /// <summary>
    /// Display arrow based on button press
    /// </summary>
    public void SetArrowSelector()
    {
        foreach(KeyState key in keyDictionary.Keys)
        {
            if(key != PlayerInput.Instance.SelectedKey)
            {
                keyDictionary[key].transform.GetChild(3).GetComponent<Image>().color = Color.clear;
            } else
            {
                keyDictionary[key].transform.GetChild(3).GetComponent<Image>().color = Color.white;
                current = key;
            }

        }

    }

    public void KeyUpdate(KeyScript.State currentState, KeyState type)
    {
        if(current != PlayerInput.Instance.SelectedKey)
        {
            SetArrowSelector();
        }

        switch(currentState)
        {
            case KeyScript.State.FloatPickup:
            case KeyScript.State.PlayerHeld:
                keyDictionary[type].transform.GetChild(1).GetComponent<Image>().sprite = statusSprites[0];
                break;
            case KeyScript.State.Attacking:
                keyDictionary[type].transform.GetChild(1).GetComponent<Image>().sprite = statusSprites[1];
                break;
            case KeyScript.State.Attached:
                keyDictionary[type].transform.GetChild(1).GetComponent<Image>().sprite = statusSprites[2];
                keyDictionary[type].transform.GetComponent<Animator>().SetBool("Inserted", true);
                break;
            case KeyScript.State.Returning:
                keyDictionary[type].transform.GetChild(1).GetComponent<Image>().sprite = statusSprites[3];
                keyDictionary[type].transform.GetComponent<Animator>().SetBool("Inserted", false);
                break;
        }
    }
}
