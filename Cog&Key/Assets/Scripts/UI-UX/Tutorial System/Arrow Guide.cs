using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowGuide : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    private List<GameObject> currentPath;

    [SerializeField]
    float waitInBetweenSpeed = 0.5f;
    void OnEnable()
    {
        // Get all children
        foreach (Transform child in transform)
        {
            currentPath.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }

        StartCoroutine(displayPath());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator displayPath()
    {
        int index = 0;
        while(index < currentPath.Count)
        {
            currentPath[index].SetActive(true);
            currentPath[index].GetComponent<Animator>().SetBool("Fade", true);

            if(currentPath[index].transform.childCount > 0)
            {
                foreach(Transform childInner in currentPath[index].transform)
                {
                    childInner.gameObject.SetActive(true);
                    childInner.GetComponent<Animator>().SetBool("Fade", true);
                }
            }

            index++;
            yield return new WaitForSeconds(waitInBetweenSpeed);
        }

        foreach (GameObject child in currentPath)
        {
            child.GetComponent<Animator>().SetBool("Fade", false);

            if (child.transform.childCount > 0)
            {
                foreach (Transform childInner in child.transform)
                {
                    childInner.gameObject.SetActive(false);
                    childInner.GetComponent<Animator>().SetBool("Fade", false);
                }
            }

            yield return new WaitForSeconds(waitInBetweenSpeed);
            child.SetActive(false);
        }

        StartCoroutine(displayPath());
    }

    void hideAllInOrderSlowly()
    {
        foreach(GameObject child in currentPath)
        {
            child.GetComponent<Animator>().SetBool("Fade", false);
        }
    }

    void hideAllInOrderForce()
    {
        foreach (GameObject child in currentPath)
        {
            child.GetComponent<Animator>().SetTrigger("Default");
            child.GetComponent<Animator>().SetBool("Fade", false);

            if (child.transform.childCount > 0)
            {
                foreach (Transform childInner in transform)
                {

                    child.GetComponent<Animator>().SetTrigger("Default");
                    childInner.GetComponent<Animator>().SetBool("Fade", false);
                    childInner.gameObject.SetActive(false);
                }
            }

            child.SetActive(false);
        }
    }
}
