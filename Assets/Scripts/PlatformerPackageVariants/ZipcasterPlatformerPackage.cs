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

    private Vector2 mouseAimPosition;
    private bool hookFiring = false;



    // Main function to set everything up
    //  Post: sets up grappling hook accordingly
    protected override void initialize() {
        if (mainCamera == null || hook == null) {
            Debug.LogError("NULL REFERENCE VARIABLES FOUND IN ZIPCASTER");
        }

        hook.onHookEnd.AddListener(onHookSequenceEnd);
    }


    // Main event handler for when pressing dash
    //  Post: will run dash when button is pressed
    public override void onDashPress(InputAction.CallbackContext context) {
        if (context.started && !hookFiring) {
            // Calculate point in world
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mouseAimPosition);

            // Calculate hook direction and fire hook
            Vector2 hookDir = (worldPoint - transform.position).normalized;
            hook.fireHook(hookDir, maxZipHookDistance, zipHookSpeed);
            hookFiring = true;

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
}
