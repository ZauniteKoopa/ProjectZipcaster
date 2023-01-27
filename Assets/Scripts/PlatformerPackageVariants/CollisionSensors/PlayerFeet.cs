using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

public class PlayerFeet : MonoBehaviour
{
    // Unity events to connect to
    public UnityEvent landingEvent;
    public UnityEvent fallingEvent;

    // Coyote Time variables
    private float coyoteTime = 0.25f;
    private Coroutine currentCoyoteSequence = null;

    // Angle threshold variables
    private float angleThreshold = 35f;

    // Instance variables
    private int numGround = 0;
    private readonly object groundLock = new object();
    private float maxTouchingGroundHeight = 0f;
    private HashSet<IDynamicPlatform> touchingDynamicPlatforms = new HashSet<IDynamicPlatform>();

    private Collider2D curSteepWall;
    private Vector2 steepWallNormal;


    // Main event handler function to collect colliders as they enter the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Increments numGround
    private void OnCollisionEnter2D(Collision2D collision) {
        Collider2D collider = collision.collider;
        int colliderLayer = collider.gameObject.layer;

        // Case in which collider is a platform
        if (colliderLayer == LayerMask.NameToLayer("Collisions")){
            // Get ground height
            float curGroundHeight = collider.ClosestPoint(transform.position).y;
            float normalAngle = Vector2.Angle(Vector2.up, collision.GetContact(0).normal);

             // Update ground
            lock(groundLock) {
                // If the angle with the normal is greater than angle threshold, update normal
                if (normalAngle > angleThreshold) {
                    curSteepWall = collider;
                    steepWallNormal = collision.GetContact(0).normal;

                // Else, treat it as ground you can land on
                } else {
                    // Update max touching ground height
                    maxTouchingGroundHeight = (numGround == 0) ? curGroundHeight : Mathf.Max(maxTouchingGroundHeight, curGroundHeight);

                    // If first time on ground after jumping, land
                    if (numGround == 0) {
                        landingEvent.Invoke();
                    }

                    numGround++;
                    addDynamicPlatform(collider);
                }
            }
        }
    }

    // Main event handler function to remove colliders when they exit the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Decrements numGround
    private void OnCollisionExit2D(Collision2D collision) {
        Collider2D collider = collision.collider;
        int colliderLayer = collider.gameObject.layer;

        if (colliderLayer == LayerMask.NameToLayer("Collisions")) {

            // Update ground
            lock(groundLock) {
                // If steep wall is the collider, turn that to null
                if (curSteepWall == collider) {
                    curSteepWall = null;
                
                // If under angle, its probably ground you stepped on
                } else {
                    int prevGround = numGround;
                    numGround -= (numGround == 0) ? 0 : 1;

                    // If you stepped off of ground, start coyote time
                    if (numGround == 0 && prevGround > 0) {
                        if (currentCoyoteSequence != null) {
                            StopCoroutine(currentCoyoteSequence);
                        }

                        currentCoyoteSequence = StartCoroutine(coyoteTimeSequence(collider));
                    }
                }
            }
        }
    }


    // Main coyoteTime sequence handler
    //  Pre: no ground is being sensed in the moment that this is called, coyoteTime is set
    //  Post: feet will not report that player is falling until coyote time is finished
    private IEnumerator coyoteTimeSequence(Collider2D leftPlatform) {
        Debug.Assert(coyoteTime >= 0f);

        // Setup
        bool hitGround = false;
        float coyoteTimer = 0f;

        // Timer loop
        while (coyoteTimer < coyoteTime && !hitGround) {
            yield return 0;

            // Update conditional variables
            coyoteTimer += Time.deltaTime;
            lock(groundLock) {
                hitGround = numGround > 0;
            }
        }

        // If you still haven't hit the ground at the end of coyote time, indicate that you're falling
        if (!hitGround) {
            removeDynamicPlatform(leftPlatform);
            fallingEvent.Invoke();
        }

        currentCoyoteSequence = null;
    }


    // Private helper function to add dynamic platform to the list if it isn't already
    //  Pre: collider != null
    //  Post: if collider has a dynamic platform component, then process that
    private void addDynamicPlatform(Collider2D collider) {
        Debug.Assert(collider != null);

        IDynamicPlatform curDynamicPlatform =  collider.GetComponent<IDynamicPlatform>();
        if (curDynamicPlatform != null) {
            touchingDynamicPlatforms.Add(curDynamicPlatform);
            curDynamicPlatform.onEntityLand(transform.parent);
        }
    }


    // Private helper function to add dynamic platform to the list if it isn't already
    //  Pre: collider != null
    //  Post: if collider has a dynamic platform component, then process that
    private void removeDynamicPlatform(Collider2D collider) {
        Debug.Assert(collider != null);

        IDynamicPlatform curDynamicPlatform =  collider.GetComponent<IDynamicPlatform>();
        if (curDynamicPlatform != null && touchingDynamicPlatforms.Contains(curDynamicPlatform)) {
            touchingDynamicPlatforms.Remove(curDynamicPlatform);
            curDynamicPlatform.onEntityLeave(transform.parent);
        }
    }


    // Main function to set the coyote time of this feet instance
    public void setCoyoteTime(float cTime) {
        Debug.Assert(cTime >= 0f);
        coyoteTime = cTime;
    }


    // Main function to get falling direction
    //  Pre: none
    //  Post: get falling direction
    public Vector2 getFallingDirection() {
        return (curSteepWall == null) ? Vector2.down : (Vector2)Vector3.Project(Vector2.down, Vector2.Perpendicular(steepWallNormal));
    }
}
