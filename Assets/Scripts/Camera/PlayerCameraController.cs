using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerCameraController : MonoBehaviour
{
    // Static variables concerning camera transition
    private static PlayerCameraController mainPlayerCamera = null;
    private static Coroutine cameraTransitionCoroutine = null;
    private readonly static float TRANSITION_SPEED = 45f;
    private readonly static float TRANSITION_TIMESCALE = 0.5f;

    // Static variables concerning original transition spot
    private static Transform playerPackage;
    private static Vector3 originalLocalPos;
    private static Quaternion originalLocalRot;


    // On awake, set this to the PlayerCameraController
    private void Awake() {
        // Error check
        PlayerCameraController[] sceneCameraControllers = Object.FindObjectsOfType<PlayerCameraController>();
        if (sceneCameraControllers.Length > 1) {
            Debug.LogError("CANNOT HAVE MORE THAN 1 CAMERA CONTROLLER SCRIPT WITHIN THE SCENE");
        }

        // set to main player camera
        playerPackage = transform.parent;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;
        mainPlayerCamera = this;
    }


    // Static function to do camera coroutine sequence
    //  Pre: parent is the transform you want the camera to parent to, localPosition is the local position of the camera relative to the parent, mainPlayerCamera != null
    //  Post: Moves the camera to specific position
    public static void moveCamera(Transform parent, Vector3 localPosition, Quaternion localRotation) {
        Debug.Assert(mainPlayerCamera != null);

        if (cameraTransitionCoroutine != null) {
            mainPlayerCamera.StopCoroutine(cameraTransitionCoroutine);
        }

        cameraTransitionCoroutine = mainPlayerCamera.StartCoroutine(mainPlayerCamera.moveCameraSequence(parent, localPosition, localRotation));
    }


    // Static function to move the camera instantly 
    //  Pre: parent is the transform you want the camera to parent to, localPosition is the local position of the camera relative to the parent, mainPlayerCamera != null
    //  Post: Moves the camera to specific position
    public static void instantMoveCamera(Transform parent, Vector3 localPosition, Quaternion localRotation) {
        Debug.Assert(mainPlayerCamera != null);

        if (cameraTransitionCoroutine != null) {
            mainPlayerCamera.StopCoroutine(cameraTransitionCoroutine);
            cameraTransitionCoroutine = null;
            Time.timeScale = 1f;
        }
        
        mainPlayerCamera.transform.parent = parent;
        mainPlayerCamera.transform.localPosition = localPosition;
        mainPlayerCamera.transform.localRotation = localRotation;
    }



    // Static function to reset the camera
    //  Pre: mainPlayerCamera != null
    //  Post: moves the camera back to the default position on top of the player
    public static void reset() {
        Debug.Assert(mainPlayerCamera != null);
        moveCamera(playerPackage, originalLocalPos, originalLocalRot);
    }


    // Static function to instantly reset the camera
    //  Pre: mainPlayerCamera != null
    //  Post: moves the camera back to the default position on top of the player
    public static void instantReset() {
        Debug.Assert(mainPlayerCamera != null);

        if (cameraTransitionCoroutine != null) {
            mainPlayerCamera.StopCoroutine(cameraTransitionCoroutine);
            cameraTransitionCoroutine = null;
            Time.timeScale = 1f;
        }
        
        mainPlayerCamera.transform.parent = playerPackage;
        mainPlayerCamera.transform.localPosition = originalLocalPos;
        mainPlayerCamera.transform.localRotation = originalLocalRot;
    }


    // Static function to get UI to face the camera, (object forward points in same direction as camera)
    //  Pre: object != null, mainPlayerCamera != null
    //  Post: Object now faces the camera
    public static void faceCamera(Transform facingObject) {

        // Get calculated X
        Vector3 rawForward = facingObject.position - mainPlayerCamera.transform.position;
        facingObject.forward = rawForward;
        float rotX = facingObject.eulerAngles.x;
        float rotY = 180f;
        float rotZ = 0f;

        // Create new rotation
        facingObject.rotation = Quaternion.Euler(rotX, rotY, rotZ);
    }


    // Static function to check if a camera sequence is running
    //  Pre: none
    //  Post: returns true if a camera sequence is running. false if it isn't
    public static bool isCameraTransitioning() {
        return mainPlayerCamera.inTransition();
    }


    // Public function to check if this instance is transitioning
    public bool inTransition() {
        return cameraTransitionCoroutine != null;
    }


    // Main IE numerator to moving the camera
    //  Pre: parent is the transform you want the camera to parent to, localPosition is the local position of the camera relative to the parent, mainPlayerCamera != null
    //  Post: moves the camera
    private IEnumerator moveCameraSequence(Transform parent, Vector3 localPosition, Quaternion localRotation) {
        // Set the parent of the camera
        transform.parent = parent;

        // Calculate the time it takes to get to position with speed
        Vector3 globalStart = transform.position;
        Vector3 globalFinish = (parent == null) ? localPosition : parent.TransformPoint(localPosition);
        float dist = Vector3.Distance(globalStart, globalFinish);
        float time = dist / TRANSITION_SPEED;

        // Get rotation starts
        Quaternion rotStart = transform.localRotation;
        Quaternion rotFinish = localRotation;

        // Transition timer
        float timer = 0f;
        float deltaTime = 0.01f;
        Time.timeScale = TRANSITION_TIMESCALE;
        WaitForSecondsRealtime waitFrame = new WaitForSecondsRealtime(deltaTime);

        while (timer < time) {
            yield return waitFrame;

            timer += deltaTime;
            transform.position = Vector3.Lerp(globalStart, globalFinish, timer / time);
            transform.localRotation = Quaternion.Lerp(rotStart, rotFinish, timer / time);
        }

        // Finish off transition
        Time.timeScale = 1f;
        transform.position = globalFinish;
        cameraTransitionCoroutine = null;
    }
}
