using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
//using UnityEditor.Build;
//using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class Agent : KeyWindable
{
    protected enum JumpState
    {
        Grounded,
        Aerial,
    }

    #region Fields
    [SerializeField] protected JumpState jumpState;

    [Header("Agent Statistics")]
    [SerializeField] protected float movementSpeed = 2f;
    [SerializeField] protected float jumpSpeed = 2f;
    protected float attackSpeed;
    [SerializeField] protected float fastScalar = 3f;

    protected const float GROUND_GRAVITY = 7.5f;

    protected Rigidbody2D rb;
    /// <summary>
    /// Degree of error for prediction built in for a less perfect agent
    /// </summary>
    protected float mistakeThreshold = 0.05f;
    protected float senseCount = 2;
    protected List<Sense> senses = new List<Sense>(2);
    protected float attackDamage;
    protected bool flightEnabled = false;

    protected Vector3 scaleVal = Vector3.zero;

    [Header("Runtime Logic")]
    protected List<GameObject> collidingObjs;
    [SerializeField] protected Vector2 direction = Vector2.zero;

    [SerializeField] protected Vector3 playerPosition = Vector3.zero;
    protected float halfHeight = 0.75f;
    protected float halfWidth = 0;

    protected float turnDelay = .5f;
    [SerializeField] protected bool processingTurn = false;
    protected float stopDelay = 0.5f;
    [SerializeField] protected bool processingStop = false;

    protected List<GameObject> nodes = new List<GameObject>();
    public PathNode pathTarget;
    protected CogIndicator cog;


    protected List<ContactPoint2D> contacts;
    protected List<ContactPoint2D> floorPts;
    protected List<ContactPoint2D> wallPts;

    [SerializeField] protected float minLedgeSize = 0.1f;
    protected float ledgeSize = 0f;

    protected bool isLost = false;
    protected float confusionTime = 5f;
    protected float lostTimer = 0f;

    protected float maxHuntTime = 2f;
    protected float huntTimer = 0f;

    protected GameObject animationTag;
    protected Animator ani;

    #endregion

    #region Properties
    public Vector3 PlayerPosition
    {
        get { return playerPosition; }
        set { playerPosition = value; }
    }

    public Vector2 Direction { get { return direction; } }

    public float Speed { get { return movementSpeed; } } 
    #endregion

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = GROUND_GRAVITY;
        scaleVal = transform.localScale;
        collidingObjs = new List<GameObject>(); 

        animationTag = FindChildWithTag(gameObject, "AgentAnimator");
        ani = animationTag.GetComponent<Animator>();
        Debug.Log("Found child: " + animationTag);

        //senses = new List<Sense>
        //{
        //    transform.GetChild(5).GetComponent<Sense>(),
        //    transform.GetChild(6).GetComponent<Sense>()
        //};
        IsGrounded();
        halfHeight = GetComponent<BoxCollider2D>().bounds.extents.y / 2;
        halfWidth = GetComponent<BoxCollider2D>().bounds.size.x / 2;

        nodes.AddRange(GameObject.FindGameObjectsWithTag("Node"));
        // Get a non magic number way pls
        //cog = transform.GetChild(7).GetComponent<CogIndicator>();

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
        

        contacts = new List<ContactPoint2D>();
        floorPts = new List<ContactPoint2D>();
        wallPts = new List<ContactPoint2D>();

        direction = Vector2.left;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(jumpState == JumpState.Aerial) IsGrounded();
        if (transform.position.y + halfHeight <= LevelData.Instance.YMin)
        {
            Transform t = transform.GetChild(transform.childCount - 1);
            if (t != null && t.name == "Key")
            {
                t.parent = null;
                t.GetComponent<KeyScript>().Detach();
                Destroy(gameObject);
            }
        }
        //cog.fast = InsertedKeyType == KeyState.Fast;

        if(transform.position.y + halfHeight < LevelData.Instance.YMin)
        {
            Destroy(gameObject);
        }      

    }

    protected virtual void BehaviorTree(float walkSpeed, bool fast)
    {
        if (!processingTurn && !processingStop)
        {
            rb.velocity = new Vector2(walkSpeed * direction.x, rb.velocity.y);
            ani.SetBool("Walking", true);
        }
    }

    protected virtual void Jump()
    {
        IsGrounded();
        if (jumpState != JumpState.Aerial && !processingStop && !processingTurn)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            jumpState = JumpState.Aerial;
        }
    }

    protected void IsGrounded()
    {
        const float BUFFER = 0.1f;

        jumpState = (RayCheck(transform.position, BUFFER, -halfWidth, halfHeight) || RayCheck(transform.position, -BUFFER, halfWidth, halfHeight) ? JumpState.Grounded: JumpState.Aerial);
        //if(floorPts!=null)
        //{
        //    jumpState = floorPts.Count >= 2 && ledgeSize > minLedgeSize / 2 ? JumpState.Grounded : JumpState.Aerial;
        //} 
    }

    protected bool RayCheck(Vector3 position, float buffer, float halfWidth, float halfHeight)
    {
        RaycastHit2D ray = Physics2D.Raycast(new Vector3(position.x - halfWidth + buffer, position.y - halfHeight, 0), Vector2.down, 10, LayerMask.GetMask("Ground"));
        return ray.collider != null && ray.distance < halfHeight + buffer;
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

    // turning animations go here
    protected IEnumerator TurnDelay()
    {

        if (!processingTurn && !processingStop)
        {
            processingTurn = true;
            // Change direction
            direction.x = -direction.x;
            Vector2 tempVelocity = rb.velocity;
            rb.velocity = new Vector2(0, rb.velocity.y);

            if (animationTag != null)
            {
                animationTag.GetComponent<Animator>().SetBool("Walking", false);
            }
            // Idle anim
            yield return new WaitForSeconds(turnDelay);

            transform.localScale = new Vector3(direction.x > 0 ? -scaleVal.x : scaleVal.x, scaleVal.y, scaleVal.z);
            // Set values back to how they used to be for a frame to prevent stunlocking


            tempVelocity.y = rb.velocity.y;
            rb.velocity = tempVelocity;
            // Wait until the agent is moving
            if (Mathf.Abs(tempVelocity.x) > 1f) yield return new WaitUntil(() => Mathf.Abs(rb.velocity.x) > 1f);
            else yield return new WaitForSeconds(turnDelay);
            processingTurn = false;
            Debug.Log("Coroutine End");
        }

        if (animationTag != null)
        {
            animationTag.GetComponent<Animator>().SetBool("Walking", true);
        }

        yield return null;
    }

    public static GameObject FindChildWithTag(GameObject parent, string tag)
    {
        GameObject child = null;

        foreach (Transform transform in parent.transform)
        {
            if (transform.CompareTag(tag))
            {
                child = transform.gameObject;
                break;
            }
        }

        return child;
    }

    public static void MatSwap(GameObject signifier, Material mat)
    {
        Material[] materials = signifier.GetComponent<SkinnedMeshRenderer>().materials;
        materials[1] = mat;
        signifier.GetComponent<Renderer>().materials = materials;
        Debug.Log("Material swapped succesfully!");
    }


    //potential idle animations go here? change via level design for stop delay @ liam
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

    protected void EdgeDetectMovement(bool detectFloorEdges, bool detectWalls)
    {
        int tempDir = EdgeDetect(detectFloorEdges, detectWalls);

        if (tempDir != direction.x && tempDir != 0)
        {
            StartCoroutine(TurnDelay());
        }
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
            if (animationTag != null)
            {
                ani.SetTrigger("Attack");

                player.playerAnimation.SetBool("Dead", true);

                PlayerInput.Instance.Locked = true;
            } else
            {
                player.Die();
            }
            
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
        if (contacts!=null)
        {
            GetComponent<BoxCollider2D>().GetContacts(contacts);

            // Floor Edges -
            // Find the contact points at the base of the agent
            // If the distance between the poles of these points is less than a proportion of the size of the hunter, turn (need to determine proportion)

            // Wall Edges -
            // Find contact points that are at any x extremity
            // Clean the list, only check ones that are in the direction of traversal
            // If the y position is at max y
            // If the player is past the wall, check the jump height
            // If a jump is possible, try it
            // Turn if fail
            // If the y position is below max y, jump
            floorPts.Clear();
            wallPts.Clear();
            foreach (ContactPoint2D contact in contacts)
            {
                if (contact.collider.tag == "Agent") continue;
                if (contact.point.y - transform.position.y <= .1f + halfHeight)
                {
                    floorPts.Add(contact);
                }
                if (Mathf.Abs(contact.point.x - transform.position.x) <= .1f + halfWidth)
                {
                    wallPts.Add(contact);
                }
            }

            // Custom Sort based on agent -> pill based on orientation
            floorPts.Sort((i, j) => { return i.point.x < j.point.x ? -1 : 1; });
            wallPts.Sort((i,j) => { return i.point.y < j.point.y ? 1 : -1; });
            // Change ray checks based on aligned axis

            float sqrDist = 0f;
            ledgeSize = 0f;

            if (detectFloorEdges)
            {
                if (floorPts.Count > 0)
                {
                    sqrDist = SquareDistance(floorPts[0].point, floorPts[floorPts.Count - 1].point);

                    ledgeSize = sqrDist;

                    if (sqrDist <= minLedgeSize)
                    {
                        bool leftRayCheck = RayCheck(transform.position, 0.1f, -halfWidth, halfHeight);
                        bool rightRayCheck = RayCheck(transform.position, -0.1f, halfWidth, halfHeight);
                        if (pathTarget != null)
                        {
                            float xDistToTarget = Mathf.Abs(transform.position.x - pathTarget.transform.position.x);
                            float sqrDistToTarget = Vector3.SqrMagnitude(transform.position - pathTarget.transform.position);
                            if (xDistToTarget < 20f)
                            {
                                RaycastHit2D results;
                                results = Physics2D.Raycast(transform.position, (pathTarget.transform.position - transform.position).normalized, 5f, LayerMask.GetMask("Ground", "Player"));
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
                                    else returnVal = floorPts[0].point.x < transform.position.x && leftRayCheck ? -1 : 1;
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
                                        bool left = false;
                                        bool right = false;

                                        // Need to turn when meeting two conditions
                                        // Central point within a threshold of minLedgeSize + x
                                        // Edge point greater than or less than x
                                        foreach(ContactPoint2D contact in floorPts)
                                        {
                                            if (contact.point.x > transform.position.x) right = true;
                                            else left = true;
                                        }
                                        if (left) returnVal = -1;
                                        else if (right) returnVal = 1;
                                        else returnVal = 0;
                                    }
                                }
                            }
                        
                            
                            else if (PlayerPosition != Vector3.zero)
                            {
                                if(Mathf.Abs(rb.velocity.x) > 0) Jump();
                                returnVal = 0;
                            }

                            else returnVal = floorPts[0].point.x < transform.position.x ? -1 : 1;

                        }
                        else returnVal = floorPts[0].point.x < transform.position.x ? -1 : 1;

                    }
                }
            }
            if (detectWalls)
            {
                if (wallPts.Count >= 2)
                {
                    float greatest = float.MinValue;
                    float least = float.MaxValue;

                    foreach(ContactPoint2D contact in wallPts)
                    {
                        if(contact.point.y < least) least = contact.point.y;
                        if(contact.point.y > greatest) greatest = contact.point.y;
                    }

                    if (greatest - least >= halfHeight - .1f)
                    {
                        returnVal = wallPts[0].point.x < transform.position.x ? 1 : -1;
                    }
                    else if (greatest - least >= halfHeight - 0.1f)
                    {
                        if (Mathf.Abs(rb.velocity.x) > 0) Jump();
                    }
                    //if (wallPts[0].collider.tag == "Ground")
                    //{
                        
                    //}
                    //else
                    //{
                    //    // Left in case we re add agent on agent collision
                    //    if (wallPts[1].collider.tag == "Agent")
                    //    {
                    //        Agent a = wallPts[0].collider.GetComponent<Agent>();
                    //        if (a.Direction != direction || movementSpeed > a.Speed)
                    //        {
                    //            // If its a small enough object, jump over it.
                    //            if (wallPts[0].collider.transform.position.y < wallPts[0].point.y && Mathf.Abs(rb.velocity.x) > 0) Jump();//Jump();
                    //        }

                    //    }
                    //}
                }
            }
        }

        //int returnVal = 0;
        //foreach (GameObject obj in collidingObjs)
        //{
        //    if(obj.GetComponent<BoxCollider2D>() != null)
        //    {
        //        if (detectWalls && transform.position.y > obj.transform.position.y - obj.GetComponent<BoxCollider2D>().bounds.size.y / 2 &&
        //        transform.position.y < obj.transform.position.y + obj.GetComponent<BoxCollider2D>().bounds.size.y / 2)
        //        {
        //            returnVal = transform.position.x > obj.transform.position.x ? 1 : -1;
        //        }
        //        else if (detectFloorEdges && transform.position.x + GetComponent<BoxCollider2D>().bounds.size.x / 2 > obj.transform.position.x + obj.GetComponent<BoxCollider2D>().bounds.size.x / 2)
        //        {
        //            jumpState = JumpState.Grounded;
        //            returnVal = -1;
        //        }
        //        else if (detectFloorEdges && transform.position.x - GetComponent<BoxCollider2D>().bounds.size.x / 2 < obj.transform.position.x - obj.GetComponent<BoxCollider2D>().bounds.size.x / 2)
        //        {
        //            jumpState = JumpState.Grounded;
        //            returnVal = 1;
        //        }
        //    }
        //    else
        //    {
        //        // If the agent is stuck at a wall, search for a node to move towards
        //        if(detectWalls && !processingTurn)
        //        {
        //            foreach(GameObject node in nodes)
        //            {
        //                if(Mathf.Sign((node.transform.position - transform.position).x) == Mathf.Sign(direction.x) &&
        //                    Vector2.Distance(node.transform.position, transform.position) <= 0.25f && 
        //                    Physics2D.Raycast(transform.position, node.transform.position, 0.6f))
        //                {
        //                    Debug.DrawLine(node.transform.position, transform.position, Color.red, 2f);
        //                    // Turn
        //                    returnVal = transform.position.x > node.transform.position.x ? 1 : -1;
        //                    break;
        //                }
        //            }
        //        }
        //        // If the collision is against a node, that means that the agent is at a ledge, so turn around
        //        if(detectFloorEdges && obj.tag == "Node" && !processingTurn)
        //        {
        //            returnVal = transform.position.x > obj.transform.position.x ? 1 : -1;
        //        }
        //    }
        //}

        //return returnVal;
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

    protected float SquareDistance(Vector2 i, Vector2 j)
    {
        return (i - j).sqrMagnitude;
    }    

    protected virtual void OnDrawGizmos()
    {
        if (contacts != null)
        {
            

            Gizmos.color = Color.blue;

            foreach (ContactPoint2D contact in floorPts)
            {
                Gizmos.DrawSphere(new Vector3(contact.point.x, contact.point.y, 0), 0.0625f);
            }

            Gizmos.color = Color.green;

            foreach (ContactPoint2D contact in wallPts)
            {
                Gizmos.DrawSphere(new Vector3(contact.point.x, contact.point.y, 0), 0.0625f);
            }
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x - halfWidth + 0.2f, transform.position.y - halfHeight, 0), .125f);
        Gizmos.DrawWireSphere(new Vector3(transform.position.x + halfWidth - 0.2f, transform.position.y - halfHeight, 0), .125f);

        Gizmos.DrawRay(new Vector3(transform.position.x - halfWidth + 0.2f, transform.position.y - halfHeight, 0), Vector2.down);
        Gizmos.DrawRay(new Vector3(transform.position.x + halfWidth - 0.2f, transform.position.y - halfHeight, 0), Vector2.down);
    }
}
