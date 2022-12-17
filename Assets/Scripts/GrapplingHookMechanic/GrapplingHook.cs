using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;


public class GrapplingHook : MonoBehaviour
{
    // Main event to listen to
    public UnityEvent<Collision2D> onHookCollision;
    public UnityEvent onHookEnd;
    private bool hookUsed = false;
    private bool hookRunning = false;

    // Main variables to keep track of 
    [SerializeField]
    private LayerMask collisionMask;
    private float distanceTimer = 0;

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
            } else {
                transform.Translate(curDistDelta * hookMovementDirection);
            }
        }
    }


    // Main collision handler
    //  Post: on collision, trigger the hook collision event if it hasn't already
    private void OnCollision2D(Collision2D collision) {
        if (!hookUsed) {
            hookUsed = true;
            hookRunning = false;

            transform.position = owner.position;
            transform.parent = owner;
            
            onHookCollision.Invoke(collision);
        }
    }


    // Main function to fire the hook at a certain direction
    //  Pre:
    //  Post:
    public void fireHook(Vector2 hookDir, float hookDist, float speed) {
        Debug.Assert(hookDist > 0f && speed > 0f);

        // Set the variables
        hookMovementDirection = hookDir.normalized;
        hookDistance = hookDist;
        hookSpeed = speed;

        // Set flags
        hookUsed = false;
        hookRunning = true;

        // Disconnect to parent
        transform.parent = null;
    }
}
