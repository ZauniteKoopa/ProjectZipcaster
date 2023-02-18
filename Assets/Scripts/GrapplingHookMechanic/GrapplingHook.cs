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
    private AudioSource hookCollisionSound;
    private LineRenderer latchLine;


    // On awake, initialize
    private void Awake() {
        owner = transform.parent;
        latchLine = GetComponent<LineRenderer>();
        hookCollisionSound = GetComponent<AudioSource>();

        if (owner == null) {
            Debug.LogError("No parent connected to this hook object");
        }
    }


    // Main function to update timer
    private void Update() {
        if (!hookUsed && hookRunning) {
            // Calculate distance directly by throwing raycast
            float curDistDelta = hookSpeed * Time.deltaTime;
            float circleRadius = transform.lossyScale.x * 0.5f * 0.95f;
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, circleRadius, hookMovementDirection, curDistDelta, collisionMask);
            if (hit.collider) {
                curDistDelta = hit.distance;
            }

            // Update the distance timer
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

        if (latchLine.enabled) {
            latchLine.SetPositions(new Vector3[] {owner.position, transform.position});
        }
    }


    // Main collision handler
    //  Post: on collision, trigger the hook collision event if it hasn't already
    private void OnCollisionEnter2D(Collision2D collision) {
        if (!hookUsed && hookRunning) {
            hookUsed = true;
            hookRunning = false;

            if (collision.collider.tag != "NonStick") {
                transform.parent = collision.collider.transform;
                curContactCollision = collision;
                hookCollisionSound.Play();
            } else {
                curContactCollision = null;
            }

            onHookEnd.Invoke();
        }
    }


    // Main function to reset the hook visually by parenting
    public void reset() {
        curContactCollision = null;
        hookUsed = false;
        hookRunning = false;

        transform.position = owner.position + Vector3.forward;
        transform.parent = owner;
        latchLine.enabled = false;
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
        latchLine.enabled = true;

        // Disconnect to parent
        transform.parent = null;
    }


    // Main function to check if the hook sequence is running
    //  Post: returns the current contact point that made contact with the hook
    public bool hookedEnviornment(out Vector2 collisionPoint) {
        collisionPoint =  Vector2.zero;
        
        // If collision happened, set collision point to the contact point offseted by the normal
        if (curContactCollision != null) {
            AdjustedLaunchPoint adjLaunchPoint = curContactCollision.collider.GetComponent<AdjustedLaunchPoint>();

            if (adjLaunchPoint != null) {
                collisionPoint = adjLaunchPoint.getHookDestination();
            } else {
                Vector2 contactPoint = curContactCollision.GetContact(0).point;
                collisionPoint = contactPoint;
            }
        }


        return curContactCollision != null;
    }


    // Main function to get upward launch height after hook dash
    //  Post: returns a float >= 0 representing the launch height after the hookDash. 0 if no launch
    public float getUpwardLaunchHeight() {
        if (curContactCollision != null) {
            AdjustedLaunchPoint adjLaunchPoint = curContactCollision.collider.GetComponent<AdjustedLaunchPoint>();

            if (adjLaunchPoint != null) {
                float launchHeight = adjLaunchPoint.getUpwardLaunchHeight();
                Debug.Assert(launchHeight >= 0f);
                return launchHeight;
            }
        }

        return 0f;
    }
}
