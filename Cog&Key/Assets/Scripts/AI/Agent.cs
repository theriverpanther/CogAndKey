using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour, IKeyWindable
{
    protected enum JumpState
    {
        Grounded,
        Aerial,
    }

    #region Fields
    protected KeyState state;
    protected JumpState jumpState;

    [Header("Agent Statistics")]
    [SerializeField] protected float movementSpeed = 2f;
    [SerializeField] protected float jumpSpeed = 2f;
    protected float attackSpeed;
    [SerializeField] protected float fastScalar = 3f;

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
    protected bool keyInserted = false;
    [SerializeField] protected List<GameObject> collidingObjs;
    protected Vector2 direction = Vector2.zero;

    protected Vector3 playerPosition = Vector3.zero;
    [SerializeField] protected float distToGround;

    protected float turnDelay = 0.5f;
    protected bool processingTurn = false;

    [SerializeField] protected List<GameObject> nodes = new List<GameObject>();

    private CogIndicator cog;

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
        senses.Add(transform.GetChild(5).GetComponent<Sense>());
        senses.Add(transform.GetChild(6).GetComponent<Sense>());
        IsGrounded();
        distToGround = GetComponent<BoxCollider2D>().bounds.extents.y;

        nodes.AddRange(GameObject.FindGameObjectsWithTag("Node"));
        // Get a non magic number way pls
        cog = transform.GetChild(3).GetComponent<CogIndicator>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(!keyInserted) state = KeyState.Normal;
        if(jumpState == JumpState.Aerial) IsGrounded();
        if (transform.position.y + distToGround <= LevelData.Instance.YMin)
        {
            Transform t = transform.GetChild(transform.childCount - 1);
            if (t != null && t.name == "Key")
            {
                t.parent = null;
                t.GetComponent<KeyScript>().Detach();
                Destroy(gameObject);
            }
        }
        cog.fast = state != KeyState.Normal;
    }

    public void InsertKey(KeyState keyState)
    {
        state = keyState;
        keyInserted = keyState != KeyState.Normal;
    }

    protected virtual void BehaviorTree(float walkSpeed, bool fast)
    {
        if(!processingTurn)
        {
            rb.velocity = new Vector2(walkSpeed * direction.x, rb.velocity.y);
        }
    }

    protected virtual void Jump()
    {
        IsGrounded();
        if (jumpState != JumpState.Aerial)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            jumpState = JumpState.Aerial;
        }
    }

    protected void IsGrounded()
    {
        bool grounded = Physics2D.Raycast(transform.position, -Vector2.up, distToGround + 0.1f) && Mathf.Abs(rb.velocity.y) <= Mathf.Epsilon;
        jumpState = grounded ? JumpState.Grounded : JumpState.Aerial;
        //Debug.DrawRay(transform.position, -Vector2.up, Color.red, 2.0f);
    }

    protected List<Vector2> ValidJumps()
    {
        List<Vector2> jumps = new List<Vector2>();

        foreach(GameObject node in nodes)
        {
            if(IsPointOnJump(node.transform.position.x, node.transform.position.y, mistakeThreshold))
            {
                jumps.Add(node.transform.position);
            }
        }

        return jumps;
    }

    protected bool IsPointOnJump(float x, float y, float threshold)
    {
        float offsetX = Mathf.Abs(x - transform.position.x);
        float calcY = -rb.gravityScale * offsetX * offsetX + rb.velocity.y * offsetX + transform.position.x;
        return calcY > y - threshold && calcY < y + threshold;
    }

    protected IEnumerator TurnDelay()
    {   if(!processingTurn)
        {
            processingTurn = true;
            // Change direction
            direction.x = -direction.x;
            Vector2 tempVelocity = rb.velocity;
            rb.velocity = new Vector2(0, rb.velocity.y);
            // Idle anim
            yield return new WaitForSeconds(turnDelay);


            transform.localScale = new Vector3(direction.x > 0 ? -scaleVal.x : scaleVal.x, scaleVal.y, scaleVal.z);
            // Set values back to how they used to be for a frame to prevent stunlocking


            tempVelocity.y = rb.velocity.y;
            rb.velocity = tempVelocity;
            // Wait until the agent is moving
            yield return new WaitUntil(() => Mathf.Abs(rb.velocity.x) > 1f);
            processingTurn = false;
            //Debug.Log("Coroutine End");
        }
        
        yield return null;
    }

    #region Edge Detection
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Wall") || collision.gameObject.tag.Equals("Node"))
        {
            collidingObjs.Add(collision.gameObject);
        }

        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();
        if (player != null)
        {
            player.Die();
        }

        //if (collision.gameObject.name == "Spikes")
        //{
        //    Destroy(gameObject);
        //}
    }

    protected void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Wall") || collision.gameObject.tag.Equals("Node"))
        {
            collidingObjs.Remove(collision.gameObject);
        }
    }

    /// <summary>
    /// Change direction based off of environmental data
    /// </summary>
    /// <param name="objects">List of objects to detect based off of</param>
    /// <param name="detectFloorEdges">Detect floor edges</param>
    /// <param name="detectWalls">Detect walls to turn around at</param>
    /// <returns>x direction to turn towards</returns>
    protected int EdgeDetect(bool detectFloorEdges, bool detectWalls)
    {
        int returnVal = 0;
        foreach (GameObject obj in collidingObjs)
        {
            if(obj.GetComponent<BoxCollider2D>() != null)
            {
                if (detectWalls && transform.position.y > obj.transform.position.y - obj.GetComponent<BoxCollider2D>().bounds.size.y / 2 &&
                transform.position.y < obj.transform.position.y + obj.GetComponent<BoxCollider2D>().bounds.size.y / 2)
                {
                    returnVal = transform.position.x > obj.transform.position.x ? 1 : -1;
                }
                else if (detectFloorEdges && transform.position.x + GetComponent<BoxCollider2D>().bounds.size.x / 2 > obj.transform.position.x + obj.GetComponent<BoxCollider2D>().bounds.size.x / 2)
                {
                    jumpState = JumpState.Grounded;
                    returnVal = -1;
                }
                else if (detectFloorEdges && transform.position.x - GetComponent<BoxCollider2D>().bounds.size.x / 2 < obj.transform.position.x - obj.GetComponent<BoxCollider2D>().bounds.size.x / 2)
                {
                    jumpState = JumpState.Grounded;
                    returnVal = 1;
                }
            }
            else
            {
                // If the agent is stuck at a wall, search for a node to move towards
                if(detectWalls && !processingTurn)
                {
                    foreach(GameObject node in nodes)
                    {
                        if(Mathf.Sign((node.transform.position - transform.position).x) == Mathf.Sign(direction.x) &&
                            Vector2.Distance(node.transform.position, transform.position) <= 0.25f && 
                            Physics2D.Raycast(transform.position, node.transform.position, 0.6f))
                        {
                            Debug.DrawLine(node.transform.position, transform.position, Color.red, 2f);
                            // Turn
                            returnVal = transform.position.x > node.transform.position.x ? 1 : -1;
                            break;
                        }
                    }
                }
                // If the collision is against a node, that means that the agent is at a ledge, so turn around
                if(detectFloorEdges && obj.tag == "Node" && !processingTurn)
                {
                    returnVal = transform.position.x > obj.transform.position.x ? 1 : -1;
                }
            }
        }

        return returnVal;
        
    }

    public void PassTriggerValue(GameObject obj, bool delete = false)
    {
        if(obj.tag == "Node")
        {
            if (!delete)
            {
                collidingObjs.Add(obj);
            }
            else
            {
                collidingObjs.Remove(obj);
            }
        }
    }
    #endregion
}
