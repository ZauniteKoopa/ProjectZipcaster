using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class DashPlatformerPackage : PlatformerPackage
{
    // Dash properties
    [Header("Dash Properties")]
    [SerializeField]
    [Min(0f)]
    private float dashDistance = 3f;
    [SerializeField]
    [Min(0f)]
    private float dashDuration = 0.5f;
    [SerializeField]
    [Min(0f)]
    private float timeBetweenDashes = 0.5f;
    [SerializeField]
    [Min(0f)]
    private float dashOffset = 0.05f;
    private bool canDash = true;
    private Coroutine runningDashCoroutine;

    // Dash momentum aftereffects
    [Header("Dash Momentum")]
    [SerializeField]
    private bool applyDashMomentumAfter = false;
    [SerializeField]
    [Min(0f)]
    private float dashMomentum = 3.0f;
    [SerializeField]
    [Min(0f)]
    private float momentumDuration = 0.5f;
    [SerializeField]
    [Min(0f)]
    private float dashCancelJumpHeight = 10f;
    [SerializeField]
    [Range(0f, 1f)]
    private float minVerticalDashCancelReq = 0.3f;
    private Vector2 dashDir;

    protected UnityEvent dashEndEvent;


    // Initialize event
    protected override void initialize() {
        dashEndEvent = new UnityEvent();
    }


    // Main private helper function to handle the jumping action per frame
    //  Post: moves the player unit down based on jump
    protected override void handleJump() {
        if (runningDashCoroutine == null) {
            base.handleJump();
        }
    }


    // Main private helper function to handle the act of horizontal movewment per frame
    //  Post: move the player if the player is pressing a button
    protected override void handleMovement() {
        if (runningDashCoroutine == null) {
            base.handleMovement();
        }
    }


    // Main event handler for when pressing dash
    //  Post: will run dash when button is pressed
    public virtual void onDashPress(InputAction.CallbackContext context) {
        if (context.started) {
            Vector2 dashDir = forwardDir;
            runDashSequence(dashDir, dashDistance, dashDistance / dashDuration);
        }
    }


    // Wrapper for running dash sequence
    //  Pre: dir is the direction of the dash, dashDist is the position of the dash, dashDuration is how long it will last
    //  Post: run dash sequence if you aren't dashing already
    public void runDashSequence(Vector2 dir, float dashDist, float dashSpeed) {
        if (canDash) {
            stopVerticalVelocity();
            stopAllMomentum();

            float dashTime = dashDist / dashSpeed;
            runningDashCoroutine = StartCoroutine(dashSequence(dir, dashDist, dashTime));
        }
    }


    // Main IEnumerator for dashing
    //  Pre: dir is the direction of the dash, dashDist is the position of the dash, dashDuration is how long it will last
    //  Post: do a running dash sequence
    private IEnumerator dashSequence(Vector2 dir, float dashDist, float dashDuration) {
        Debug.Assert(dashDuration > 0f && dashDist > 0f);

        // Calculate the speed of the actual dash itself
        canDash = false;
        float dashSpeed = dashDist / dashDuration;

        // Cast a boxcast ray to get the actual distance to travel
        dir = dir.normalized;
        dashDir = dir;
        float actualDistance = dashDist;
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.lossyScale, 0f, dir, dashDist, collisionMask);
        if (hit.collider) {
            actualDistance = hit.distance - dashOffset;
        }

        // Move in that assumed direction
        float curDist = 0f;
        float timer = 0f;

        while (timer < dashDuration) {
            yield return 0;

            // Update time variables
            timer += Time.deltaTime;

            // Update distance variables
            if (curDist < actualDistance) {
                float distDelta = dashSpeed * Time.deltaTime;
                curDist += distDelta;

                // Adjust distance so it hits dest point exactly if it goes over
                distDelta -= (curDist > actualDistance) ? (curDist - actualDistance) : 0f;
                transform.Translate(distDelta * dir);
            }
        }

        // Set coroutine to null to indicate you're not dashing anymore and apply momentum if that's enabled
        runningDashCoroutine = null;
        dashEndEvent.Invoke();
        if (applyDashMomentumAfter && !grounded) {
            runInertiaSequence((dir.normalized.x) * dashMomentum, momentumDuration);
        }

        // Wait for a delay before you can dash again
        yield return new WaitForSeconds(timeBetweenDashes);
        canDash = true;
    }

    
    // Public function to check if you're dashing
    //  Pre: none
    //  Post: returns bool true if dashing, false if not
    public bool isDashing() {
        return runningDashCoroutine != null;
    }


    // Main function to cancel the dash 
    //  Pre: none
    //  Post:  cancels dash immediately
    public void cancelDash(bool applyMomentum = false) {
        if (runningDashCoroutine != null) {
            StopCoroutine(runningDashCoroutine);
            runningDashCoroutine = null;
            canDash = true;

            if (applyMomentum) {
                runInertiaSequence((dashDir.normalized.x) * dashMomentum, momentumDuration);
                
                if (dashDir.normalized.y > -minVerticalDashCancelReq) {
                    launchVertically(dashCancelJumpHeight);
                }
            }

            dashEndEvent.Invoke();
        }
    }

}
