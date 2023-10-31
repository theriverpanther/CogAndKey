using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SplashManager : MonoBehaviour
{
    private SplashScreen.StopBehavior stopBehavior;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SplashScreenController());
    }

    IEnumerator SplashScreenController()
    {
        SplashScreen.Begin();
        while (!SplashScreen.isFinished)
        {
            SplashScreen.Draw();
            if (Input.anyKeyDown)
            {
                Debug.Log("STOP");
                SplashScreen.Stop(stopBehavior);
                break;
            }
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
