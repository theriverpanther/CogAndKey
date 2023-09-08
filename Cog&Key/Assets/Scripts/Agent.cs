using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField]
    protected float health = 10f;
    [SerializeField]
    protected float maxHealth = 10f;
    [SerializeField]
    protected float movementSpeed = 1f;
    [SerializeField]
    protected float attackSpeed;
    /// <summary>
    /// Degree of error for prediction built in for a less perfect agent
    /// </summary>
    protected float mistakeThreshold = 0.05f;
    protected float visionRange;
    protected float attackDamage;
    protected bool flightEnabled = false;
    [SerializeField]
    protected Vector3 direction = Vector3.zero;

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
}
