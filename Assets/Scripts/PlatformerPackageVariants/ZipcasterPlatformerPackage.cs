using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZipcasterPlatformerPackage : DashPlatformerPackage
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
    private float zipDashSpeed = 10f;
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


    private Vector2 mouseAimPosition;
    private bool hookFiring = false;
    private int curHooksLeft;



    // Main function to set everything up
    //  Post: sets up grappling hook accordingly
    protected override void initialize() {
        if (mainCamera == null || hook == null) {
            Debug.LogError("NULL REFERENCE VARIABLES FOUND IN ZIPCASTER");
        }

        base.initialize();

        curHooksLeft = numHookCasts;
        hook.onHookEnd.AddListener(onHookSequenceEnd);
        dashEndEvent.AddListener(onHookDashEnd);
        reticle.transform.parent = null;
    }


    // Main event handler for when pressing dash
    //  Post: will run dash when button is pressed
    public override void onDashPress(InputAction.CallbackContext context) {
        if (context.started && !hookFiring && curHooksLeft > 0 && !isDashing()) {
            // Calculate point in world
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mouseAimPosition);

            // Calculate hook direction and fire hook
            Vector2 hookDir = (worldPoint - transform.position).normalized;
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
        // Update reticle position: calculate components for ray
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mouseAimPosition);
        Vector2 rayDir = (worldPoint - transform.position).normalized;
        float rayDist = maxZipHookDistance - 0.1f;

        Vector2 noCollisionPosition = (Vector2.Distance(worldPoint, transform.position) > maxZipHookDistance) ?
            (Vector2)transform.position + (rayDist * rayDir.normalized) :
            (Vector2)worldPoint;

        // Update reticle position: send out ray
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, rayDist, hookLayerMask);
        reticle.position = (hit.collider != null) ? hit.point : noCollisionPosition;
        reticle.GetComponent<SpriteRenderer>().color = (hit.collider != null) ? Color.red : Color.green;

        if (!isDashing()) {
            aimLine.SetPositions(new Vector3[] {transform.position, reticle.position});
        }

        // Parents handles the rest of the movement
        base.handleMovement();
    }


    // Main event handler function for when jump button has been pressed
    //  Post: when jump button pressed, set velocity to jump velocity
    public override void onJumpPress(InputAction.CallbackContext context) {
        if (isDashing() && context.started) {
            cancelDash(true);
            
        } else {
            base.onJumpPress(context);
        }
    }


    // Main event hanler function for when you are walking
    public void onMouseAimChange(InputAction.CallbackContext context) {
        // Set inputVector value
        mouseAimPosition = context.ReadValue<Vector2>();
    }


    // Main functon to handle the event when the hooking has ended
    //  Pre: grappling hook has signaled that it ended
    //  Post: Handle the case when it actually collided with something
    private void onHookSequenceEnd() {
        Vector2 collisionPoint;
        hookFiring = false;

        if (hook.hookedEnviornment(out collisionPoint)) {
            float hookedDistance = Vector2.Distance(collisionPoint, transform.position);

            Vector2 hookedDashDir = (collisionPoint - (Vector2)transform.position);
            hookedDashDir = hookedDashDir.normalized;

            runDashSequence(hookedDashDir, hookedDistance, zipDashSpeed);
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
}
