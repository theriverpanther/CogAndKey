using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CogIndicator : MonoBehaviour
{
    public float spinSpeed = 0.25f;
    public float fastSpinSpeed = 1f;
    public bool fast = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, (-1 * Mathf.Rad2Deg * (!fast ? spinSpeed : fastSpinSpeed) * Time.deltaTime)));
    }
}
