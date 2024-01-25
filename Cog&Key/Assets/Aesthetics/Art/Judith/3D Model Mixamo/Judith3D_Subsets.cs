using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Judith3D_Subsets : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RandomChanceArmSwing()
    {
        int randomArmSwing = Random.Range(0, 4);
        Debug.Log("Arm swing? " + randomArmSwing);
        if(randomArmSwing < 2)
        {
            gameObject.GetComponent<Animator>().SetInteger("IdleVariant", 1);
        }
    }

    public void ReturnToMainIdle()
    {
        gameObject.GetComponent<Animator>().SetInteger("IdleVariant", 0);

    }
}
