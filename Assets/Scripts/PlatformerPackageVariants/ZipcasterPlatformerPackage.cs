using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZipcasterPlatformerPackage : PlatformerPackage
{
    [Header("Grappling Hook Variables")]
    [SerializeField]
    private GrapplingHook hook;
    [SerializeField]
    private Transform reticle;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    [Min(0f)]
    private float zipHookSpeed = 10f;
    [SerializeField]
    [Min(0f)]
    private float maxZipHookDistance = 3f;
    [SerializeField]
    [Min(0.01f)]
    private float zipcastMaxFallSpeed = 2f;
    [SerializeField]
    [Range(0f, 1f)]
    private float zipcastHorizontalSpeedReduction = 0.5f;
    [SerializeField]
    [Min(0)]
    private int numHookCasts = 1;
    [SerializeField]
    private LayerMask hookLayerMask;
    [SerializeField]
    private LineRenderer aimLine;
    [SerializeField]
    private XY_AimAssist aimAssist;

    [Header("Zip Dash Variables")]
    [SerializeField]
    [Min(0f)]
    private float startingDashSpeed = 0f;
    [SerializeField]
    [Min(0.01f)]
    private float accelerationDuration = 0.25f;
    [SerializeField]
    [Min(0f)]
    private float zipDashSpeed = 10f;
    [SerializeField]
    [Min(0.01f)]
    private float zipDashOffset;
    [SerializeField]
    private LayerMask collisionZipMask;

    [Header("Zip Dash Momentum")]
    [SerializeField]
    [Min(0f)]
    private float dashMomentum = 1.5f;
    [SerializeField]
    [Min(0f)]
    private float momentumDuration = 0.5f;
    [SerializeField]
    [Min(0f)]
    private float dashCancelJumpHeight = 1.5f;
    [SerializeField]
    [Range(0f, 1f)]
    private float minVerticalDashCancelReq = 0.3f;
    [SerializeField]
    [Min(0.01f)]
    private float maxCloseDistance = 0.5f;


    private Vector2 mouseAimPosition;
    private bool hookFiring = false;
    private int curHooksLeft;
    private Coroutine runningZipSequence;
    private Vector2 zipDir;


    // Main function to set everything up
    //  Post: sets up grappling hook accordingly
    protected override void initialize() {
        if (mainCamera == null || hook == null) {
            Debug.LogError("NULL REFERENCE VARIABLES FOUND IN ZIPCASTER");
        }

        base.initialize();

        curHooksLeft = numHookCasts;
        hook.onHookEnd.AddListener(onHookSequenceEnd);
        reticle.transform.parent = null;
    }


    // Main event handler for when pressing dash
    //  Post: will run dash when button is pressed
    public void onZipHookPress(InputAction.CallbackContext context) {
        if (context.started && !hookFiring && curHooksLeft > 0 && !isZipping()) {
            // Calculate point in world
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mouseAimPosition);

            // Calculate hook direction and fire hook
            Vector2 hookDir = (worldPoint - transform.position).normalized;
            if (aimAssist != null) {
                hookDir = aimAssist.adjustAim(hookDir, transform.position);
            }
            hook.fireHook(hookDir, maxZipHookDistance, zipHookSpeed);
            if (!grounded) {
                curHooksLeft--;
            }
            hookFiring = true;

            reticle.gameObject.SetActive(false);
        }
    }


    // Main function to update reticle as well as movement
    //  Post: reticle position is updated and movement controls are handled by parent classes
    protected override void handleMovement() {
        updateReticlePosition();

        // Parents handles the rest of the movement
        if (!isZipping()) {
            base.handleMovement();
        }
    }


    // Main function to handle jump
    protected override void handleJump() {
        if (!isZipping()) {
            base.handleJump();
        }
    }


    // Main event handler function for when jump button has been pressed
    //  Post: when jump button pressed, set velocity to jump velocity
    public override void onJumpPress(InputAction.CallbackContext context) {
        if (isZipping() && context.started) {
            cancelZip(true);
            
        } else {
            base.onJumpPress(context);
        }
    }


    // Main event hanler function for when you are walking
    public void onMouseAimChange(InputAction.CallbackContext context) {
        // Set inputVector value
        mouseAimPosition = context.ReadValue<Vector2>();
    }


    // Private helper function to update reticle position
    //  Pre: none
    //  Post: updates reticle appropriately
    private void updateReticlePosition() {
        // Update reticle position: calculate components for ray
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mouseAimPosition);
        Vector2 rayDir = (worldPoint - transform.position).normalized;
        if (aimAssist != null) {
            rayDir = aimAssist.adjustAim(rayDir, transform.position);
        }

        float rayDist = maxZipHookDistance - 0.1f;

        Vector2 noCollisionPosition = (Vector2.Distance(worldPoint, transform.position) > maxZipHookDistance) ?
            (Vector2)transform.position + (rayDist * rayDir.normalized) :
            (Vector2)worldPoint;

        // Update reticle position: send out ray
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, rayDist, hookLayerMask);
        reticle.position = (hit.collider != null) ? hit.point : noCollisionPosition;
        reticle.GetComponent<SpriteRenderer>().color = (hit.collider != null) ? Color.red : Color.green;

        if (!isZipping()) {
            aimLine.SetPositions(new Vector3[] {transform.position, reticle.position});
        }
    }



    // Main functon to handle the event when the hooking has ended
    //  Pre: grappling hook has signaled that it ended
    //  Post: Handle the case when it actually collided with something
    private void onHookSequenceEnd() {
        Vector2 collisionPoint;
        hookFiring = false;

        if (hook.hookedEnviornment(out collisionPoint)) {
            // Run sequence
            if (runningZipSequence != null) {
                StopCoroutine(runningZipSequence);
            }

            stopVerticalVelocity();
            stopAllMomentum();
            runningZipSequence = StartCoroutine(zipSequence(collisionPoint, zipDashSpeed, startingDashSpeed));

        } else {
            reticle.gameObject.SetActive(true);
        }
    }


    // Main function to handle the event when the hook dash ends
    private void onHookDashEnd() {
        hook.reset();
        reticle.gameObject.SetActive(true);
    }


    // Private helper function to calculate the max fall speed for this current frame
    //  Pre: none
    //  Post: returns a non-negqative float representing the max fall speed of this frame
    protected override float getMaxFallSpeed() {
        float maxFallSpeed = (hookFiring) ? Mathf.Min(base.getMaxFallSpeed(), zipcastMaxFallSpeed) :
                                            base.getMaxFallSpeed();

        Debug.Assert(maxFallSpeed > 0f);
        return maxFallSpeed;
    }


    // Main IEnumerator for dashing
    //  Pre: dir is the direction of the dash, dashDist is the position of the dash, dashDuration is how long it will last
    //  Post: do a running dash sequence
    private IEnumerator zipSequence(Vector2 zipDest, float zipSpeed, float startSpeed) {
        Debug.Assert(startSpeed >= 0f && zipSpeed > 0f && zipSpeed >= startSpeed);

        // Set up loop
        float curDistance = Vector2.Distance(transform.position, zipDest);
        Vector2 curZipDir = adjustMovementForCollision(zipDest - (Vector2)transform.position);
        float aTimer = 0f;

        // Actual loop
        while (curDistance > maxCloseDistance && !isZeroVector(curZipDir)) {
            yield return 0;

            // Get current speed
            aTimer = Mathf.Clamp(aTimer + Time.deltaTime, 0f, accelerationDuration);
            float curZipSpeed = Mathf.Lerp(startSpeed, zipSpeed, aTimer / accelerationDuration);

            // Calculate distance
            curZipDir = curZipDir.normalized;
            zipDir = curZipDir;
            float distDelta = curZipSpeed * Time.deltaTime;
            RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.lossyScale * 0.95f, 0f, curZipDir, distDelta, collisionZipMask);
            if (hit.collider) {
                distDelta = hit.distance - zipDashOffset;
            }

            // Translate
            transform.Translate(distDelta * curZipDir);

            // Establish loop variables again
            curDistance = Vector2.Distance(transform.position, zipDest);
            curZipDir = adjustMovementForCollision(zipDest - (Vector2)transform.position);
        }

        // Cleanup
        stopVerticalVelocity();
        stopAllMomentum();
        runningZipSequence = null;
        onHookDashEnd();
    }


    // Main private helper function to check if you're zipping
    private bool isZipping() {
        return runningZipSequence != null;
    }


    // Main function to check if a vector is approximating zero
    //  Post: returns a boolean if the vector is close enough to zero
    private bool isZeroVector(Vector2 v) {
        return (v.x < 0.05f && v.x > -0.05f) && (v.y < 0.05f && v.y > -0.05f);
    }


    // Private helper function to get current move speed at current frame
    //  Pre: none
    //  Post: returns a non-negative float representing the max waking speed
    protected override float getCurrentWalkingSpeed() {
        float curWalkSpeed = base.getCurrentWalkingSpeed();
        curWalkSpeed *= (hookFiring) ? zipcastHorizontalSpeedReduction : 1f;

        Debug.Assert(curWalkSpeed > 0f);
        return curWalkSpeed;
    }


    // Refresh resources on landing
    //  Pre: unit has landed
    //  Post: refresh any resources that the specific package variant may need
    protected override void refreshResourcesOnLanding() {
        curHooksLeft = numHookCasts;
    }


    // Main function to cancel the dash 
    //  Pre: none
    //  Post:  cancels dash immediately
    public void cancelZip(bool applyMomentum = false) {
        if (isZipping()) {
            StopCoroutine(runningZipSequence);
            runningZipSequence = null;

            if (applyMomentum) {
                runInertiaSequence((zipDir.normalized.x) * dashMomentum, momentumDuration);
                
                if (zipDir.normalized.y > -minVerticalDashCancelReq) {
                    launchVertically(dashCancelJumpHeight);
                } else {
                    float verticalSpeed = 10f;
                    launchVerticallySpeed(zipDir.normalized.y * verticalSpeed);
                }
            }

            onHookDashEnd();
        }
    }
}
