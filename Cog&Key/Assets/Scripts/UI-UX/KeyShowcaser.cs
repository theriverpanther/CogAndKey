using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [SerializeField]
    Color colorGrayed;

    [Header("Key Objects")]

    [SerializeField]
    List<GameObject> keyManage;

    private Dictionary<KeyState, GameObject> keys = new Dictionary<KeyState, GameObject>();

    [SerializeField]
    List<Texture2D> keyImages;
    [SerializeField]
    List<Texture2D> keyImagesBlank;

    List<Sprite> keyImagesConvert = new List<Sprite>();
    List<Sprite> keyImagesBlankConvert = new List<Sprite>();

    [SerializeField]
    GameObject mainKey;

    private void Awake()
    {
        keys.Add(KeyState.Fast, keyManage[0]);
        keys.Add(KeyState.Lock, keyManage[1]);
        keys.Add(KeyState.Reverse, keyManage[2]);

        ConvertKeys(keyImages, keyImagesConvert);
        ConvertKeys(keyImagesBlank, keyImagesBlankConvert);

        DisplayXCircle();
    }

    /// <summary>
    /// Updates the main key to display the currently used key, in inv or disabled
    /// </summary>
    /// <param name="status">If in use or not</param>
    /// <param name="keyNumber">0, 1, 2 key values</param>
    public void MainKeyStatusUpdate(bool status, KeyState keyState = KeyState.Normal)
    {
        int keyNumber = GetPlacementNum(keyState);

        if(keyNumber == -1)
        {
            mainKey.GetComponent<Image>().sprite = keyImagesConvert[keyImagesConvert.Count - 1];
            mainKey.GetComponent<Image>().color = colorGrayed;
            return;
        }

        mainKey.GetComponent<Image>().sprite = keyImagesConvert[keyNumber];
        if (status)
        {
            mainKey.GetComponent<Image>().color = colorsActive[keyNumber];
            mainKey.GetComponent<Image>().sprite = keyImagesConvert[keyNumber];
            return;
        } 
        mainKey.GetComponent<Image>().color = colorsDisabled[keyNumber];
        mainKey.GetComponent<Image>().sprite = keyImagesBlankConvert[keyNumber];
    }

    /// <summary>
    /// Updates the smaller UI keys to display the newly picked up key as an option.
    /// </summary>
    /// <param name="keyNumber"></param>
    public void SmallKeyStatusUpdate(bool status, KeyState keyState)
    {
        int keyNumber = GetPlacementNum(keyState);

        if (keyNumber == -1)
        {
            return;
        }

        if (status)
        {
            keyManage[keyNumber].GetComponent<Image>().color = colorsActive[keyNumber];
            keyManage[keyNumber].GetComponent<Image>().sprite = keyImagesConvert[keyNumber];
            keyManage[keyNumber].transform.localScale = new Vector3(1,1,1);

            return;
        }
        keyManage[keyNumber].GetComponent<Image>().color = colorsDisabled[keyNumber];
        keyManage[keyNumber].GetComponent<Image>().sprite = keyImagesBlankConvert[keyNumber];
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

    private void ConvertKeys(List<Texture2D> imgs, List<Sprite> addTo)
    {
        foreach(Texture2D keyImg in imgs)
        {
            Rect rec = new Rect(0, 0, keyImg.width, keyImg.height);
            addTo.Add(Sprite.Create(keyImg, rec, new Vector2(0, 0), 1));
        }
    }

    private void DisplayXCircle()
    {
        PlayerScript player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        if (player.FastKey == null && player.ReverseKey == null && player.LockKey == null)
        {
            MainKeyStatusUpdate(true);
        }
    }
}
