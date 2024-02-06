using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
//using UnityEditor.ShaderGraph.Internal;
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
    [SerializeField] protected float stopDelay = 0.5f;
    protected bool processingStop = false;

    [SerializeField] protected List<GameObject> nodes = new List<GameObject>();
    public PathNode pathTarget;
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
        cog = transform.GetChild(7).GetComponent<CogIndicator>();

        // Get the collection of path nodes in the scene
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Path");
        if (objs.Length > 0)
        {
            GameObject obj = objs[0];
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            float tempDist = 0;
            foreach (GameObject node in objs)
            {
                tempDist = Vector3.Distance(transform.position, node.transform.position);
                if (Vector3.Distance(transform.position, node.transform.position) < dist)
                {
                    obj = node;
                    dist = tempDist;
                }
            }

            pathTarget = obj.GetComponent<PathNode>();
        }
        

        pathTarget = obj.GetComponent<PathNode>();
        contacts = new List<ContactPoint2D>();
        floorPts = new List<ContactPoint2D>();
        wallPts = new List<ContactPoint2D>();
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
        if(!processingTurn && !processingStop)
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
    {   if(!processingTurn && !processingStop)
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

    protected IEnumerator MoveDelay()
    {
        if(!processingStop && !processingTurn)
        {
            processingStop = true;
            Vector2 tempVelocity = rb.velocity;
            rb.velocity = new Vector2(0, rb.velocity.y);
            yield return new WaitForSeconds(stopDelay);
            tempVelocity.y = rb.velocity.y;
            rb.velocity = tempVelocity;
            yield return new WaitUntil(() => Mathf.Abs(rb.velocity.x) > 1f);
            processingStop = false;
        }
        yield return null;
    }
    
    public void Stop()
    {
        StartCoroutine(MoveDelay());
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
                // If the agent is stuck at a wall, search for a node to move towards
                if(detectWalls && !processingTurn)
                {
                        bool leftRayCheck = RayCheck(transform.position, 0.1f, -distToGround);
                        bool rightRayCheck = RayCheck(transform.position, -0.1f, distToGround);
                        if (pathTarget != null)
                        {
                            float xDistToTarget = Mathf.Abs(transform.position.x - pathTarget.transform.position.x);
                            float sqrDistToTarget = Vector3.SqrMagnitude(transform.position - pathTarget.transform.position);
                            if (xDistToTarget < 20f)
                            {
                                RaycastHit2D results;
                                results = Physics2D.Raycast(transform.position, (pathTarget.transform.position - transform.position).normalized, 5f);
                                if (results.collider != null)
                                {
                                    //Debug.DrawLine(transform.position, pathTarget.transform.position);
                                    Vector3 point = results.collider.transform.position;
                                    if (Mathf.Abs(pathTarget.transform.position.y - transform.position.y) < 2f)
                                    {
                                        if (pathTarget.transform.position.y > transform.position.y) Jump();
                                        returnVal = 0;
                                        lostTimer = 0;
                                        isLost = false;
                                    }
                                    else returnVal = floorPts[0].point.x < transform.position.x && leftRayCheck ? 1 : -1;
                                    // Still end up stopping at the edge
                                    // This will also run them off the edge
                                    // Is this a garbage collection issue?
                                }
                                else
                                {
                                    //Debug.DrawLine(transform.position, pathTarget.transform.position);
                                    if (sqrDistToTarget <= 64f)
                                    {
                                        if (transform.position.y < pathTarget.transform.position.y) Jump();
                                        returnVal = 0;
                                        lostTimer = 0;
                                        isLost = false;
                                    }
                                    else
                                    {
                                        lostTimer += Time.deltaTime;
                                        if (lostTimer >= confusionTime)
                                        {
                                            isLost = true;
                                            Debug.Log(gameObject.name + " can't reach next point at " + pathTarget.transform.position + ".");
                                        }
                                        // Turn the way that is opposite of the edge the agent is at
                                        returnVal = floorPts[0].point.x < transform.position.x ? 1 : -1;
                                    }
                                }
                            }
                        
                            
                            if (PlayerPosition != Vector3.zero)
                            {
                                Jump();
                                returnVal = 0;
                            }
                        
                        }
                        else returnVal = floorPts[0].point.x < transform.position.x ? 1 : -1;

                }
                // If the collision is against a node, that means that the agent is at a ledge, so turn around
                if(detectFloorEdges && !processingTurn)
                {
                    if (wallPts[0].point.y - transform.position.y >= distToGround -.1f)
                    {
                        returnVal = wallPts[0].point.x < transform.position.x ? 1 : -1;
                        Debug.Log("inner");
                    }
                    else
                    {
                        Jump();
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
