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
        int randomArmSwing = Random.Range(0, 15);
        //Debug.Log("Arm swing? " + randomArmSwing);
        if(randomArmSwing < 4)
        {
            gameObject.GetComponent<Animator>().SetInteger("IdleVariant", 1);
        }
    }

    public void ReturnToMainIdle()
    {
        gameObject.GetComponent<Animator>().SetInteger("IdleVariant", 0);

    }

    public void ForceRotationRun()
    {
        //Debug.Log("AAAAAAAAAA");
        transform.localRotation = new Quaternion(transform.rotation.x, 180f, transform.rotation.z, transform.rotation.w);
    }

    void KillPlayOnAnimationEnd()
    {
        PlayerScript player = GameObject.Find("Player").GetComponent<PlayerScript>();
        player.Die();
        player.playerAnimation.SetBool("Dead", false);
        PlayerInput.Instance.Locked = false;
    }
}
