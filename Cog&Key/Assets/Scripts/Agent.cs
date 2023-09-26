using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class Agent : MonoBehaviour, IKeyWindable
{
    #region Fields
    protected KeyState state;

    [Header("Agent Statistics")]
    [SerializeField]
    protected float movementSpeed = 1f;
    [SerializeField]
    protected float attackSpeed;
    [SerializeField]
    protected float fastScalar = 3f;
    protected Rigidbody2D rb;
    /// <summary>
    /// Degree of error for prediction built in for a less perfect agent
    /// </summary>
    protected float mistakeThreshold = 0.05f;
    protected float visionRange;
    protected float attackDamage;
    protected bool flightEnabled = false;

    [Header("Runtime Logic")]
    [SerializeField]
    protected bool keyInserted = false;
    [SerializeField]
    protected List<GameObject> collidingObjs;
    [SerializeField]
    protected Vector2 direction = Vector2.zero;
    #endregion

    #region Properties
    public bool KeyInserted
    {
        get { return keyInserted; }
        set { keyInserted = value; }
    }
    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(!keyInserted) state = KeyState.Normal;
    }

    public void InsertKey(KeyState keyState)
    {
        state = keyState;
        keyInserted = keyState != KeyState.Normal;
    }


    #region Edge Detection
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Wall"))
        {
            collidingObjs.Add(collision.gameObject);
        }

        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        if (player != null)
        {
            player.Die();
        }
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Wall"))
        {
            collidingObjs.Remove(collision.gameObject);
        }
    }

    protected int EdgeDetect(List<GameObject> objects)
    {
        int returnVal = 0;
        foreach (GameObject obj in collidingObjs)
        {
            if (transform.position.y > obj.transform.position.y - obj.GetComponent<BoxCollider2D>().bounds.size.y / 2 &&
                transform.position.y < obj.transform.position.y + obj.GetComponent<BoxCollider2D>().bounds.size.y / 2)
            {
                returnVal = transform.position.x > obj.transform.position.x ? 1 : -1;
            }
            else if (transform.position.x + GetComponent<BoxCollider2D>().bounds.size.x / 2 > obj.transform.position.x + obj.GetComponent<BoxCollider2D>().bounds.size.x / 2)
            {
                returnVal = -1;
            }
            else if(transform.position.x - GetComponent<BoxCollider2D>().bounds.size.x / 2 < obj.transform.position.x - obj.GetComponent<BoxCollider2D>().bounds.size.x / 2)
            {
                returnVal = 1;
            }
        }

        return returnVal;
        
    }
    #endregion
}
