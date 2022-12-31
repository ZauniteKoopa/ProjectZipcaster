using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalScrollingCameraZone : CameraZone
{
    [SerializeField]
    private float minLocalX = 0f;
    [SerializeField]
    private float maxLocalX = 0f;

    private Transform playerTransform = null;


    // Main initialization for error check
    protected override void initialize() {
        if (minLocalX > maxLocalX) {
            Debug.LogError("Invalid min and max X values for horizontal scrolling camera zone");
        }
    }



    // Update is called once per frame
    void Update()
    {
        // If player captured and camera isn't in transition mode, set the camera to player's position
        if (playerTransform != null && !PlayerCameraController.isCameraTransitioning()) {
            // Calculate transition point
            float playerLocalX = transform.InverseTransformPoint(playerTransform.transform.position).x;
            Transform cameraTransform = cameraMock.transform;
            float transitionX = Mathf.Clamp(playerLocalX, minLocalX, maxLocalX);
            Vector3 transitionPoint = new Vector3(transitionX, cameraTransform.localPosition.y, cameraTransform.localPosition.z);

            // Move camera instantly
            PlayerCameraController.instantMoveCamera(transform, transitionPoint, cameraTransform.transform.localRotation);
        }
    }


    // Main function to handle player collision events
    //  Pre: collider is the player's collider
    protected override void OnPlayerTrigger(Collider2D collider) {
        // Set player transform
        playerTransform = collider.transform;
        float playerLocalX = transform.InverseTransformPoint(playerTransform.transform.position).x;

        // Transition
        Transform cameraTransform = cameraMock.transform;
        float transitionX = Mathf.Clamp(playerLocalX, minLocalX, maxLocalX);
        Vector3 transitionPoint = new Vector3(transitionX, cameraTransform.localPosition.y, cameraTransform.localPosition.z);

        PlayerCameraController.moveCamera(transform, transitionPoint, cameraTransform.transform.localRotation);
    }


    private void OnTriggerExit2D(Collider2D collider) {
        if (collider.tag == "Player") {
            playerTransform = null;
        }
    }
}
