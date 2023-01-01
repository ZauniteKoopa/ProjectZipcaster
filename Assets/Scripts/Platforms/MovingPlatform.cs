using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : IDynamicPlatform
{
    // Main positions to travel between in a moving platform in sequential order (uses LOCAL POSITION for ease of implementation)
    [SerializeField]
    private Vector2[] platformWaypoints;
    [SerializeField]
    [Min(0.01f)]
    private float platformSpeed = 3f;
    [SerializeField]
    [Min(0f)]
    private float platformRestTime = 1.25f;

    // Main runtime variables
    private int curDestIndex = 0;


    // On initialization, check parameters and setup
    private void Awake() {
        if (platformWaypoints.Length < 1) {
            Debug.LogError("Moving platform must contain at least 2 waypoints");
        }

        // Set the platform position automatically to the first destination
        transform.localPosition = platformWaypoints[curDestIndex];
        StartCoroutine(platformMoveSequence());
    }

    
    // Main platform move sequence, replaces the update function since it lasts forever
    private IEnumerator platformMoveSequence() {
        while (true) {
            // Calculate variables for linear move
            curDestIndex = (curDestIndex + 1) % platformWaypoints.Length;
            Vector3 curPosition = transform.parent.TransformPoint(platformWaypoints[curDestIndex]);

            Vector2 moveDir = (curPosition - transform.position);
            float travelDist = moveDir.magnitude;
            moveDir = moveDir.normalized;

            float distanceTimer = 0f;

            // Loop for moving the platform
            while (distanceTimer < travelDist) {
                yield return 0;

                // Update timer
                float distDelta = Time.deltaTime * platformSpeed;
                distanceTimer += distDelta;

                // Translate
                if (distanceTimer > travelDist) {
                    transform.position = curPosition;
                } else {
                    transform.Translate(distDelta * moveDir);
                }
            }

            // Wait at destination
            yield return new WaitForSeconds(platformRestTime);
        }
    }


    // Main function to handle the case in which an entity lands on this platform
    //  Pre: entity doesn't equal null
    //  Post: attach entity to this moving platform
    public override void onEntityLand(Transform entity) {
        entity.parent = transform;
    }


    // Main function to handle the case in which an entity leaves the platform
    //  Pre: entity doesn't equal null
    //  Post: detach entity if entity was attached to this platform
    public override void onEntityLeave(Transform entity) {
        if (entity.parent == transform) {
            entity.parent = null;
        }
    }
}
