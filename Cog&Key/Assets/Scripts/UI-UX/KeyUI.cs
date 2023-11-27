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

    void Start()
    {
       data = GameObject.FindObjectOfType<LevelData>();
       keys = new List<GameObject>();

       for (int i = 0; i < transform.childCount; i++)
        {
            keys.Add(transform.GetChild(i).gameObject);
            keys[i].SetActive(false);
        }
        keyDictionary.Add(KeyState.Fast, keys[0]);
        keyDictionary.Add(KeyState.Lock, keys[1]);
        keyDictionary.Add(KeyState.Reverse, keys[2]);

        ShowCollectedKeys();
    }

    void ShowCollectedKeys()
    {
        foreach(KeyState state in data.StartingKeys) {
            keyDictionary[state].SetActive(true);
        }
    }

    public void UpdateKeyUI(KeyState key)
    {
        keyDictionary[key].SetActive(true);
    }

    public void KeyUpdate(KeyScript.State currentState, KeyState type)
    {
        switch(currentState)
        {
            case KeyScript.State.Pickup:
            case KeyScript.State.PlayerHeld:
                keyDictionary[type].transform.GetChild(1).GetComponent<Image>().sprite = statusSprites[0];
                break;
            case KeyScript.State.Attacking:
                keyDictionary[type].transform.GetChild(1).GetComponent<Image>().sprite = statusSprites[1];
                break;
            case KeyScript.State.Attached:
                keyDictionary[type].transform.GetChild(1).GetComponent<Image>().sprite = statusSprites[2];
                break;
            case KeyScript.State.Returning:
                keyDictionary[type].transform.GetChild(1).GetComponent<Image>().sprite = statusSprites[3];
                break;
        }
    }
}
