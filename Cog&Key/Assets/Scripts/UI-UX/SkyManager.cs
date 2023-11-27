using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyManager : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(CoBlendSkies());
    }

    private IEnumerator CoBlendSkies()
    {
        while (true)
        {
            EnviromentManager.Instance.BlendEnviroment("Sunrise", 0f);
            yield return new WaitForSeconds(5.0f);
            EnviromentManager.Instance.BlendEnviroment("Day", 25.0f);
            yield return new WaitForSeconds(30.0f);
            EnviromentManager.Instance.BlendEnviroment("Sunset", 5.0f);
            yield return new WaitForSeconds(10.0f);
            EnviromentManager.Instance.BlendEnviroment("Night", 25.0f);
            yield return new WaitForSeconds(30.0f);
            EnviromentManager.Instance.BlendEnviroment("Sunrise", 15.0f);
            yield return new WaitForSeconds(20.0f);
        }
    }
}
