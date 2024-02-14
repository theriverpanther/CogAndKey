using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class NameAndPosition
{
    public string name;
    public string position;
}

public class CreditFiller : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    public List<NameAndPosition> nameAndPosition;
    private Dictionary<string, string> namePosDictionary;

    private GameObject namePosObj;

    RectTransform rectT;

    void Start()
    {
        rectT = GetComponent<RectTransform>();
        namePosDictionary = new Dictionary<string, string>();
        namePosObj = gameObject.transform.GetChild(1).gameObject;
        namePosObj.SetActive(false);
        foreach (NameAndPosition person in nameAndPosition)
        {
            namePosDictionary[person.name] = person.position;
            GameObject namePos = Instantiate(namePosObj, transform);
            updateNamePos(person.name, person.position, namePos);
            namePos.SetActive(true);
        }

    }

    // Update is called once per frame
    void Update()
    {
    }

    void updateNamePos(string name, string position, GameObject obj)
    {
        obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = name;
        obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = position;
    }
}
