using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BossManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    GameObject eyeLooker;

    [SerializeField]
    GameObject connectors;

    [SerializeField]
    public List<GameObject> connectPoints;

    public GameObject currentPoint;
    public GameObject previousPoint;

    [SerializeField]
    Rigidbody rb;

    [SerializeField]
    Animator animator;

    [SerializeField]
    bool timeToMove = false;
    public float speed = 3.0f;

    void Awake()
    {
        foreach(Transform child in connectors.transform)
        {
            connectPoints.Add(child.gameObject);
        }

        currentPoint = connectPoints[0];
        previousPoint = connectPoints[0];

        if (LevelData.Instance != null && LevelData.Instance.BossRespawnPosition != null)
        {
            Debug.Log("Loading from death point...");
            foreach(GameObject t in connectPoints)
            {
                if(t.transform.position.Equals(LevelData.Instance.BossRespawnPosition))
                {
                    currentPoint = t;
                    Debug.Log("FOUND YA!...");
                    break;
                }
            }
            
        }

        Debug.Log("EEEEEEEHh " + LevelData.Instance?.BossRespawnPosition);

        transform.position = currentPoint.transform.position;
       
    }

    // Update is called once per frame
    void Update()
    {
        if (timeToMove)
        {
            animator.SetBool("Walking", true);
            rb.velocity = (currentPoint.transform.position - transform.position).normalized * speed;

            if (Vector3.Distance(transform.position, currentPoint.transform.position) <= 0.75f)
            {
                timeToMove = false;
                transform.position = currentPoint.transform.position;
            }

        } else
        {
            rb.velocity = Vector3.zero;
            animator.SetBool("Walking", false);
        }
    }

    public void SwitchOutConnectPoint(GameObject moveTo)
    {
        previousPoint = currentPoint;
        currentPoint = moveTo;
        timeToMove = true;


        LevelData.Instance.TriggerBossCheckpoint(moveTo.transform.position);

        Debug.Log("WAAAAAA " + moveTo.name + LevelData.Instance.BossRespawnPosition);

        return;


    }

}
