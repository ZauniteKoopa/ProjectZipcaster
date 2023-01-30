using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserRenderer : MonoBehaviour
{
    [SerializeField]
    [Min(0.5f)]
    private float maxLaserDistance = 0.5f;
    [SerializeField]
    private Transform laserTransform = null;
    [SerializeField]
    private LayerMask laserCollisionMask;

    private float[] raycastPositionYDelta = new float[] {0f, 1f, -1f};

    // On awake, set up the 3 raycast positions
    private void Awake() {
        if (laserTransform == null) {
            Debug.LogError("No laser attached to this laser renderer");
        }
    }

    // On update, render the laser based on scale
    private void Update() {

        // Cast 3 lasers and calculate the minimum distance
        float minRayDist = maxLaserDistance;
        foreach (float curRayDelta in raycastPositionYDelta) {
            // Get distance at the laser's center and the 2 ends of the laser's width and then transform that point
            Vector2 rayLocalPos = Vector2.zero + (Vector2)(curRayDelta * laserTransform.localScale.y * 0.5f * Vector3.up);
            Vector2 rayPos = transform.TransformPoint(rayLocalPos);
            Debug.DrawRay(rayPos, transform.right * maxLaserDistance);

            // Cast ray
            RaycastHit2D hit = Physics2D.Raycast(rayPos, transform.right, maxLaserDistance, laserCollisionMask);
            if (hit.collider != null) {
                minRayDist = Mathf.Min(minRayDist, hit.distance);
            }
        }

        // set scale to distance
        transform.localScale = new Vector3(minRayDist, 1f, 1f);
    }
}
