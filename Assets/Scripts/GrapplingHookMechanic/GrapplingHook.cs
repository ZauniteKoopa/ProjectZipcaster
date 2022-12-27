using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;


public class GrapplingHook : MonoBehaviour
{
    // Main event to listen to
    public UnityEvent onHookEnd;
    private bool hookUsed = false;
    private bool hookRunning = false;

    // Main variables to keep track of 
    [SerializeField]
    private LayerMask collisionMask;
    private float distanceTimer = 0;
    private Collision2D curContactCollision;

    // Main variables for a hook usage instance
    private Vector2 hookMovementDirection;
    private float hookDistance;
    private float hookSpeed;

    // Main component to connect to
    private Transform owner;


    // On awake, initialize
    private void Awake() {
        owner = transform.parent;

        if (owner == null) {
            Debug.LogError("No parent connected to this hook object");
        }
    }


    // Main function to update timer
    private void Update() {
        if (!hookUsed && hookRunning) {
            float curDistDelta = hookSpeed * Time.deltaTime;
            distanceTimer += curDistDelta;

            // if passed distance, reset. Else, keep translating
            if (distanceTimer > hookDistance) {
                hookRunning = false;
                transform.position = owner.position;
                transform.parent = owner;
                onHookEnd.Invoke();
            } else {
                transform.Translate(curDistDelta * hookMovementDirection);
            }
        }
    }


    // Main collision handler
    //  Post: on collision, trigger the hook collision event if it hasn't already
    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("collision");
        if (!hookUsed && hookRunning) {
            hookUsed = true;
            hookRunning = false;

            transform.position = owner.position;
            transform.parent = owner;

            curContactCollision = collision;
            onHookEnd.Invoke();
        }
    }


    // Main function to fire the hook at a certain direction
    //  Pre: hook dir is the direction that the hook is fired, hookDist is the distance of the hook and speed is how fast the hook is fired
    //  Post: initiares fire hook sequence
    public void fireHook(Vector2 hookDir, float hookDist, float speed) {
        Debug.Assert(hookDist > 0f && speed > 0f);

        // Set the variables
        hookMovementDirection = hookDir.normalized;
        hookDistance = hookDist;
        hookSpeed = speed;

        curContactCollision = null;
        distanceTimer = 0f;

        // Set flags
        hookUsed = false;
        hookRunning = true;

        // Disconnect to parent
        transform.parent = null;
    }


    // Main function to check if the hook sequence is running
    //  Post: returns the current contact point that made contact with the hook
    public bool hookedEnviornment(out Vector2 collisionPoint) {
        collisionPoint = (curContactCollision != null) ? curContactCollision.GetContact(0).point : Vector2.zero;
        return curContactCollision != null;
    }
}
