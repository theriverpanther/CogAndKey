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

    [SerializeField]
    private TextMeshProUGUI nameText, positionText;

    private bool startedCredits = false;

    void Start()
    {
        namePosDictionary = new Dictionary<string, string>();
        foreach (NameAndPosition person in nameAndPosition)
        {
            namePosDictionary[person.name] = person.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!startedCredits)
        {
            StartCoroutine(ReadNames());
        }
    }

    void updateNamePos(string name, string position)
    {
        nameText.text = name;
        positionText.text = position;   
    }

    IEnumerator ReadNames()
    {
        int index = 0;
        startedCredits = true;
        while (index < nameAndPosition.Count && startedCredits)
        {
            updateNamePos(nameAndPosition[index].name, nameAndPosition[index].position);
            yield return new WaitForSeconds(3f);
            index++;
        }
        startedCredits = false;
        yield return null;
    }

    public void EndCreditsEarly()
    {
        startedCredits = false;
    }
}
