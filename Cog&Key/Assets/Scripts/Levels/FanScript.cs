using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FanScript : KeyWindable
{
    private GameObject Wind1;
    private GameObject Wind2;
    private float windTime;
    private ParticleSystem particles;
    private Vector3 basePos;

    private const float MAX_BASE_FORCE = 10.0f;
    private static Dictionary<KeyState, float> keyToMultiplier = new Dictionary<KeyState, float>() {
        { KeyState.Fast, 3f },
        { KeyState.None, 1f },
        { KeyState.Lock, 0f },
        { KeyState.Reverse, -1.5f }
    };

    private List<Rigidbody2D> affectedEntities = new List<Rigidbody2D>();
    private Vector2 direction;
    private float fanRange;

    void Start()
    {
        float angle = Mathf.Deg2Rad * (transform.rotation.eulerAngles.z + 90f);
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        fanRange = GetComponents<BoxCollider2D>()[1].size.y;

        Wind1 = transform.GetChild(0).gameObject;
        Wind2 = transform.GetChild(1).gameObject;
        particles = transform.GetChild(4).GetComponent<ParticleSystem>();
        basePos = transform.GetChild(4).transform.position;
    }

    void Update()
    {
        foreach (Rigidbody2D physBod in affectedEntities)
        {
            float distanceFromFan = Vector3.Project(physBod.transform.position - transform.position, direction).magnitude;
            float rangeMultiplier = (fanRange - distanceFromFan) / fanRange;
            if (rangeMultiplier < 0)
            {
                rangeMultiplier = 0;
            }
            float force = MAX_BASE_FORCE * Mathf.Sqrt(rangeMultiplier);
            physBod.AddForce(keyToMultiplier[InsertedKeyType] * force * direction * 250f * Time.deltaTime);
        }

        // TEMP VISUAL EFFECT
        particles.startSpeed = 10 * keyToMultiplier[InsertedKeyType];
        ParticleSystem.EmissionModule emit = particles.emission;
        if (particles.startSpeed == 0) emit.rateOverTime = 0;
        else emit.rateOverTime = 50;


        //windTime += keyToMultiplier[InsertedKeyType] * 1.5f * Time.deltaTime;
        //float range = windTime % 1f;
        //float range2 = (windTime + 0.5f) % 1.0f;
        //Wind1.transform.localPosition = new Vector3(-0.2f, range * fanRange, 0);
        //Wind2.transform.localPosition = new Vector3(0.2f, range2 * fanRange, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null && !affectedEntities.Contains(rb))
        {
            affectedEntities.Add(rb);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (affectedEntities.Contains(rb))
        {
            affectedEntities.Remove(rb);
        }
    }

    protected override void OnKeyInserted(KeyState newKey)
    {
     
        if(newKey == KeyState.Reverse)
        {
            StartCoroutine(ReverseParticle("insert"));
        }
      
    }

    protected override void OnKeyRemoved(KeyState removedKey)
    {

        if (removedKey == KeyState.Reverse)
        {
            StartCoroutine(ReverseParticle("remove"));
        }
    }

    IEnumerator ReverseParticle(string state)
    {
        particles.Clear();
        particles.Stop();
        yield return new WaitForSeconds(0.05f);
        if(state == "remove")
        {
            particles.transform.position = basePos;
        } else
        {
            particles.transform.position = new Vector3(basePos.x, basePos.y + 10, basePos.z);
        }
        particles.Play();
        yield return new WaitForSeconds(0.05f);
        yield return null;
    }
}
