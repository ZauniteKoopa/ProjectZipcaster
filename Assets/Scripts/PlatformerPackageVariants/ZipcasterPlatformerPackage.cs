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

        }
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
        }
    }


    // Main function to handle the event when the hook dash ends
    private void onHookDashEnd() {
        hook.reset();
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
