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

    [SerializeField]
    GameObject mainKey;

    /// <summary>
    /// Updates the main key to display the currently used key, in inv or disabled
    /// </summary>
    /// <param name="status">If in use or not</param>
    /// <param name="keyNumber">0, 1, 2 key values</param>
    void MainKeyStatusUpdate(bool status, int keyNumber)
    {
        if(status)
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
    void PickedUpKey(int keyNumber)
    {
        keyManage[keyNumber].GetComponent<Image>().color = colorsDisabled[keyNumber];
    }
}
