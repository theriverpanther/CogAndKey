using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingWallScript : MonoBehaviour
{
    [SerializeField] private bool LoopToStart; // false, back and forth
    private const float MOVE_SPEED = 5.0f;

    private List<Vector2> pathPoints = new List<Vector2>();
    private int nextPointIndex;
    private bool forward; // false: moving backwards through the path

    public KeyState InsertedKey { get; set; }

    void Start()
    {
        
    }

    void Update()
    {
        Vector2 target = pathPoints[nextPointIndex];
        float currentSpeed = MOVE_SPEED;
        if(InsertedKey == KeyState.Lock) {
            return;
        }
        if(InsertedKey == KeyState.Fast) { 
            currentSpeed *= 2;
        }

        float displacement = currentSpeed * Time.deltaTime;
        if(Vector2.Distance(target, transform.position) <= displacement) {
            transform.position = target;

            // go to next spot
        } else {
            transform.position += displacement * ((Vector3)target - transform.position).normalized;
    }
}
