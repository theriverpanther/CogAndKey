using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyShowcaser : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Color Information")]

    [SerializeField]
    List<Color> colorsActive;

    [SerializeField]
    List<Color> colorsDisabled;

    [Header("Key Objects")]

    [SerializeField]
    List<GameObject> keyManage;

    private Dictionary<KeyState, GameObject> keys = new Dictionary<KeyState, GameObject>();

    [SerializeField]
    GameObject mainKey;

    private void Awake()
    {
        keys.Add(KeyState.Normal, keyManage[0]);
        keys.Add(KeyState.Fast, keyManage[0]);
        keys.Add(KeyState.Lock, keyManage[1]);
        keys.Add(KeyState.Reverse, keyManage[2]);
    }

    /// <summary>
    /// Updates the main key to display the currently used key, in inv or disabled
    /// </summary>
    /// <param name="status">If in use or not</param>
    /// <param name="keyNumber">0, 1, 2 key values</param>
    public void MainKeyStatusUpdate(bool status, KeyState keyState)
    {
        int keyNumber = GetPlacementNum(keyState);
        if (status)
        {
            mainKey.GetComponent<Image>().color = colorsActive[keyNumber];
            return;
        } 
        mainKey.GetComponent<Image>().color = colorsDisabled[keyNumber];
    }

    /// <summary>
    /// Updates the smaller UI keys to display the newly picked up key as an option.
    /// </summary>
    /// <param name="keyNumber"></param>
    public void SmallKeyStatusUpdate(bool status, KeyState keyState)
    {
        int keyNumber = GetPlacementNum(keyState);

        if (status)
        {
            keyManage[keyNumber].GetComponent<Image>().color = colorsActive[keyNumber];
            return;
        }
        keyManage[keyNumber].GetComponent<Image>().color = colorsDisabled[keyNumber];
    }

    /// <summary>
    /// returns the placement num in the big list from dictionary 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private int GetPlacementNum(KeyState keyState)
    {
        GameObject key = null;
        bool success = keys.TryGetValue(keyState, out key);
        Debug.Log("Found?: " + keyState.ToString());
        if (success)
        {
            return keyManage.IndexOf(key);
        }
        return -1;
    }
}
