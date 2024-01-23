using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceRotationScript : MonoBehaviour
{
    // Start is called before the first frame update
    Quaternion forceRotation = Quaternion.Euler(0, -90, 0);
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = forceRotation;
    }
}
