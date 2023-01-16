using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalScrollingCameraZone : CameraZone
{
    [SerializeField]
    private float minLocalX = 0f;
    [SerializeField]
    private float maxLocalX = 0f;
    [SerializeField]
    private float minLocalY = 0f;
    [SerializeField]
    private float maxLocalY = 0f;
    [SerializeField]
    private bool lockY = true;
    [SerializeField]
    private bool lockX = false;

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
            Transform cameraTransform = cameraMock.transform;

            // calculate X
            float transitionX = cameraTransform.localPosition.x;
            if (!lockX) {
                float playerLocalX = transform.InverseTransformPoint(playerTransform.transform.position).x;
                transitionX = Mathf.Clamp(playerLocalX, minLocalX, maxLocalX);
            }

            // Calculate Y
            float transitionY = cameraTransform.localPosition.x;
            if (!lockY) {
                float playerLocalY = transform.InverseTransformPoint(playerTransform.transform.position).y;
                transitionY = Mathf.Clamp(playerLocalY, minLocalY, maxLocalY);
            }

            Vector3 transitionPoint = getTransitionPoint();

            // Move camera instantly
            PlayerCameraController.instantMoveCamera(transform, transitionPoint, cameraTransform.transform.localRotation);
        }
    }


    // Main private helper function to get the transition point
    private Vector3 getTransitionPoint() {
        Transform cameraTransform = cameraMock.transform;

        // calculate X
        float transitionX = cameraTransform.localPosition.x;
        if (!lockX) {
            float playerLocalX = transform.InverseTransformPoint(playerTransform.transform.position).x;
            transitionX = Mathf.Clamp(playerLocalX, minLocalX, maxLocalX);
        }

        // Calculate Y
        float transitionY = cameraTransform.localPosition.y;
        if (!lockY) {
            float playerLocalY = transform.InverseTransformPoint(playerTransform.transform.position).y;
            transitionY = Mathf.Clamp(playerLocalY, minLocalY, maxLocalY);
        }

        return new Vector3(transitionX, transitionY, cameraTransform.localPosition.z);
    }


    // Main function to handle player collision events
    //  Pre: collider is the player's collider
    protected override void OnPlayerTrigger(Collider2D collider) {
        // Set player transform
        playerTransform = collider.transform;
        float playerLocalX = transform.InverseTransformPoint(playerTransform.transform.position).x;

        // Transition
        Transform cameraTransform = cameraMock.transform;
        Vector3 transitionPoint = getTransitionPoint();

        PlayerCameraController.moveCamera(transform, transitionPoint, cameraTransform.transform.localRotation);

        setPlayerSpawnPoint(collider);
    }


    private void OnTriggerExit2D(Collider2D collider) {
        if (collider.tag == "Player") {
            playerTransform = null;
        }
    }
}
