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

    Rigidbody rb;

    [SerializeField]
    Animator animator;

    [SerializeField]
    bool timeToMove = false;
    public float speed = 3.0f;

    void Start()
    {
        foreach(Transform child in connectors.transform)
        {
            connectPoints.Add(child.gameObject);
        }

        currentPoint = connectPoints[0];
        previousPoint = connectPoints[0];

        if (LevelData.Instance != null && LevelData.Instance.BossRespawnPoint != null)
        {
            transform.position = LevelData.Instance.BossRespawnPoint.transform.position;
            currentPoint = LevelData.Instance.BossRespawnPoint;
        }

        transform.position = currentPoint.transform.position;

        rb = GetComponent<Rigidbody>();
       
    }

    // Update is called once per frame
    void Update()
    {
        if (timeToMove)
        {
            animator.SetBool("Walking", true);
            rb.velocity = (currentPoint.transform.position - transform.position).normalized * speed;

            //Debug.Log(Vector3.Distance(transform.position, currentPoint.transform.position));
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

        GameObject.Find("Level Data Saver").GetComponent<LevelData>().TriggerBossCheckpoint(currentPoint.GetComponent<ConnectionPoint>());

        Debug.Log("WAAAAAA " + GameObject.Find("Level Data Saver").GetComponent<LevelData>().BossRespawnPoint.name);

        return;


    }

}
