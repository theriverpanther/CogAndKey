using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionPoint : MonoBehaviour
{
    [SerializeField]
    BossManager bossManager;

    // Start is called before the first frame update
    void Start()
    {
        bossManager = GameObject.Find("Spider Boss").GetComponent<BossManager>();


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit(Collider other)
    {
        if(bossManager.connectPoints.IndexOf(transform.parent.gameObject) < bossManager.connectPoints.Count)
        {
            bossManager.speed = 6.0f;
            bossManager.SwitchOutConnectPoint(bossManager.connectPoints[bossManager.connectPoints.IndexOf(transform.parent.gameObject) + 1]);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (bossManager.currentPoint != transform.parent.gameObject)
        {
            bossManager.speed = 12.0f;
            bossManager.SwitchOutConnectPoint(transform.parent.gameObject);
            
        }

    }
}
