using System.Collections;
using System.Collections.Generic;
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
    protected float senseCount = 2;
    protected List<Sense> senses;
    protected float attackDamage;
    protected bool flightEnabled = false;

    protected Vector3 scaleVal = Vector3.zero;

    [Header("Runtime Logic")]
    [SerializeField]
    protected bool keyInserted = false;
    [SerializeField]
    protected List<GameObject> collidingObjs;
    [SerializeField]
    protected Vector2 direction = Vector2.zero;

    private Vector3 playerPosition = Vector3.zero;

    #endregion

    #region Properties
    public bool KeyInserted
    {
        get { return keyInserted; }
        set { keyInserted = value; }
    }

    public Vector3 PlayerPosition
    {
        get { return playerPosition; }
        set { playerPosition = value; }
    }
    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        scaleVal = transform.localScale;
        senses = new List<Sense>();
        for(int i = 0; i < senseCount; i++) 
        {
            senses.Add(transform.GetChild(i+1).GetComponent<Sense>());
        }
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

    protected virtual void BehaviorTree(float walkSpeed, bool fast)
    {
        rb.velocity = new Vector2(walkSpeed * direction.x, rb.velocity.y);
    }
    #region Edge Detection
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Wall"))
        {
            collidingObjs.Add(collision.gameObject);
        }

        //PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        //if (player != null)
        //{
        //    player.Die();
        //}
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Wall"))
        {
            collidingObjs.Remove(collision.gameObject);
        }
    }

    protected int EdgeDetect(List<GameObject> objects, bool detectFloorEdges, bool detectWalls)
    {
        int returnVal = 0;
        foreach (GameObject obj in collidingObjs)
        {
            if (detectWalls && transform.position.y > obj.transform.position.y - obj.GetComponent<BoxCollider2D>().bounds.size.y / 2 &&
                transform.position.y < obj.transform.position.y + obj.GetComponent<BoxCollider2D>().bounds.size.y / 2)
            {
                returnVal = transform.position.x > obj.transform.position.x ? 1 : -1;
            }
            else if (detectFloorEdges && transform.position.x + GetComponent<BoxCollider2D>().bounds.size.x / 2 > obj.transform.position.x + obj.GetComponent<BoxCollider2D>().bounds.size.x / 2)
            {
                returnVal = -1;
            }
            else if(detectFloorEdges && transform.position.x - GetComponent<BoxCollider2D>().bounds.size.x / 2 < obj.transform.position.x - obj.GetComponent<BoxCollider2D>().bounds.size.x / 2)
            {
                returnVal = 1;
            }
        }

        return returnVal;
        
    }
    #endregion
}
