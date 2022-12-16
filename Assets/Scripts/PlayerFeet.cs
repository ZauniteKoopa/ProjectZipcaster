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


    // Main event handler function to collect colliders as they enter the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Increments numGround
    private void OnTriggerEnter2D(Collider2D collider) {
        int colliderLayer = collider.gameObject.layer;

        // Case in which collider is a platform
        if (colliderLayer == LayerMask.NameToLayer("Collisions")){
            // Get ground height
            float curGroundHeight = collider.ClosestPoint(transform.position).y;

             // Update ground
            lock(groundLock) {
                // Update max touching ground height
                maxTouchingGroundHeight = (numGround == 0) ? curGroundHeight : Mathf.Max(maxTouchingGroundHeight, curGroundHeight);

                // If first time on ground after jumping, land
                if (numGround == 0) {
                    landingEvent.Invoke();
                }

                numGround++;
            }
        }
    }

    // Main event handler function to remove colliders when they exit the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Decrements numGround
    private void OnTriggerExit2D(Collider2D collider) {
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

                    currentCoyoteSequence = StartCoroutine(coyoteTimeSequence());
                }
            }
        }
    }


    // Main coyoteTime sequence handler
    //  Pre: no ground is being sensed in the moment that this is called, coyoteTime is set
    //  Post: feet will not report that player is falling until coyote time is finished
    private IEnumerator coyoteTimeSequence() {
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
            fallingEvent.Invoke();
        }

        currentCoyoteSequence = null;
    }


    // Main function to set the coyote time of this feet instance
    public void setCoyoteTime(float cTime) {
        Debug.Assert(cTime >= 0f);
        coyoteTime = cTime;
    }
}
