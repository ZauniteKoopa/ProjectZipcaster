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

    // Instance variables
    private int numGround = 0;
    private readonly object groundLock = new object();
    private float maxTouchingGroundHeight = 0f;
    private HashSet<IDynamicPlatform> touchingDynamicPlatforms = new HashSet<IDynamicPlatform>();
    private ContactPoint2D initialLandingContact;


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

             // Update ground
            lock(groundLock) {
                // Update max touching ground height
                maxTouchingGroundHeight = (numGround == 0) ? curGroundHeight : Mathf.Max(maxTouchingGroundHeight, curGroundHeight);
                bool wasNotGrounded = numGround == 0;
                numGround++;

                // If first time on ground after jumping, land
                if (wasNotGrounded) {
                    initialLandingContact = collision.GetContact(0);
                    landingEvent.Invoke();
                }

                addDynamicPlatform(collider);
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


    // Main function to get offsetted position
    //  Pre: playerPosition is the position of the player using this feet and offset is how much you want to be offset from the ground
    //  Post: returns the new player position on landing
    public Vector2 getAutomatedGroundPosition(Vector2 playerPosition, float offset) {
        // If no landing contact right now, return player position as is
        if (numGround == 0) {
            return playerPosition;
        }

        // Get variables from contact point
        Vector2 contactNormal = initialLandingContact.normal;
        Vector2 contactPosition = initialLandingContact.point;

        // Calculate the center point in which (playerPosition - centerPoint).dir == contactNormal
        Vector2 distanceVector = playerPosition - contactPosition;
        distanceVector = Vector3.Project(distanceVector, Vector2.Perpendicular(contactNormal));
        Vector2 centerPoint = contactPosition + distanceVector;

        // Now adjust the height from that center point and return that value
        return centerPoint + (offset * contactNormal.normalized);
    }
}
