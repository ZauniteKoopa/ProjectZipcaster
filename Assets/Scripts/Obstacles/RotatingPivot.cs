using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPivot : MonoBehaviour
{
    // Main variables 
    [SerializeField]
    private bool fullRotation = true;
    [SerializeField]
    private bool clockwise = true;
    [SerializeField]
    private float[] anglePoints;
    [SerializeField]
    [Min(0.01f)]
    private float stayTime = 0.01f;
    [SerializeField]
    [Min(0.01f)]
    private float rotationalSpeed = 0.01f;

    private int rotPointIndex = 0;


    // On initialization, error check
    private void Awake() {
        if (!fullRotation && anglePoints.Length < 2) {
            Debug.LogError("Rotation pivot not configured correctly. If not full rotation, check that at least 2 angle points are found");
        }

        StartCoroutine(rotationSequence());
    }


    // Main sequences to run
    private IEnumerator rotationSequence() {
        // Case of full rotation
        if (fullRotation) {
            float rVelocity = (clockwise) ? -1f * rotationalSpeed : rotationalSpeed;

            while (true) {
                yield return 0;
                transform.Rotate(Vector3.forward * rVelocity * Time.deltaTime);
            }

        // Case of specific points
        } else {
            transform.eulerAngles = Vector3.forward * anglePoints[rotPointIndex];

            while (true) {
                yield return new WaitForSeconds(stayTime);

                float start = anglePoints[rotPointIndex];
                rotPointIndex = (rotPointIndex + 1) % anglePoints.Length;
                float end = anglePoints[rotPointIndex];

                float t = Mathf.Abs(end - start) / rotationalSpeed;
                float rTimer = 0f;

                while (rTimer < t) {
                    yield return 0;

                    rTimer += Time.deltaTime;
                    transform.eulerAngles = Vector3.forward * Mathf.Lerp(start, end, rTimer / t);
                }
            }
        }
    }
}
