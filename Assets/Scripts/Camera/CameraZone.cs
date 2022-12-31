using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField]
    protected Camera cameraMock;


    // On awake, disable cameraMock
    private void Awake() {
        if (cameraMock == null) {
            Debug.LogError("Camera mock not set up for camera zone!");
        }

        cameraMock.gameObject.SetActive(false);
        initialize();
    }


    // Main function to do any additional initialization
    protected virtual void initialize() {}


    // On trigger enter, set camera to this position
    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player") {
            OnPlayerTrigger(collider);
        }
    }


    // Main function to handle player collision events
    //  Pre: collider is the player's collider
    protected virtual void OnPlayerTrigger(Collider2D collider) {
        Transform cameraTransform = cameraMock.transform;
        PlayerCameraController.moveCamera(transform, cameraTransform.localPosition, cameraTransform.transform.localRotation);
    }
}
