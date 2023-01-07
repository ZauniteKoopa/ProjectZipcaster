using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XY_AimAssist : MonoBehaviour
{
    // Private instance variables
    private readonly object targetLock = new object();
    private HashSet<Transform> nearbyTargets = new HashSet<Transform>();

    [SerializeField]
    [Min(0.01f)]
    private float enemyAimAssistRadius = 5f;
    [SerializeField]
    private LayerMask aimMask;
    [SerializeField]
    private LayerMask collisionMask;


    // Main function to adjust the aim direction so that it can accurately hit an enemy: O(E) Time and O(E) space (E = enemies considered)
    //  Pre: aimDirection is the direction of the attack, playerPosition is the position of the player, excludedEnemy is a twitch unit to exclude in aiming
    //  Post: returns adjusted aimDirection IFF there's an enemy in that general direction
    public Vector2 adjustAim(Vector2 aimDirection, Vector2 playerPosition) {

        // Cull enemies that do not fit the conditions of adjusting aimDirection
        List<Transform> enemyCandidates = forwardCull(aimDirection, playerPosition);
        enemyCandidates = proximityCull(aimDirection, playerPosition, enemyCandidates);

        // If there are no enemies to consider, just return the aimDirection as is. Else, return normalized direction from player to closest enemy
        if (enemyCandidates.Count == 0) {
            return aimDirection.normalized;
        } else {
            // Calculate the viable enemy candidate with the smallest distance to the player
            float minDistance = Vector2.Distance(playerPosition, enemyCandidates[0].transform.position);
            Transform topCandidate = enemyCandidates[0];

            for (int i = 1; i < enemyCandidates.Count; i++) {
                float curDistance = Vector2.Distance(playerPosition, enemyCandidates[i].position);

                if (curDistance < minDistance) {
                    minDistance = curDistance;
                    topCandidate = enemyCandidates[i];
                }
            }

            // Return normalized, flatten version of that vector
            Vector2 adjustedAim = (Vector2)topCandidate.position - playerPosition;
            return adjustedAim.normalized;
        }
    }

    
    // Main function to cull all enemies so that only enemies in the direction of aimDirection are considered AND ray is not blocked. This is THE FIRST CULL: O(E) time, O(E) space
    //  Pre: aimDirection is the direction of the attack (aimDirection.y == 0f), playerPosition is the position of the player, excludedEnemy is the enemy to be excluded
    //  Post: returns a list of transform that are in the direction of aimDirection relative to the player (Not opposite). If there are none, returns an empty list
    private List<Transform> forwardCull(Vector2 aimDirection, Vector2 playerPosition) {

        List<Transform> forwardCullResults = new List<Transform>();

        // Iterate over every enemy in range to see if they are in the direction of aimDirection (not opposite)
        lock (targetLock) {
            foreach (Transform target in nearbyTargets) {
                // Get distance vector
                Vector2 tgtPosition = target.position;
                Vector2 distanceVector = tgtPosition - playerPosition;

                // check if there's a ray collision, indicating that the aim is blocked. Also make sure that the one blocking isn't the target itself
                RaycastHit2D hitInfo = Physics2D.Raycast(transform.position, distanceVector.normalized, distanceVector.magnitude, collisionMask);
                bool aimBlocked = hitInfo.collider != null;
                if (aimBlocked) {
                    aimBlocked = (hitInfo.collider.transform != target);
                }

                // Get the Cos of the angle between distance vector and aim direction. (If positive, in the direction of aim. Else, in the opposite direction of aim)
                float cosAngle = Vector3.Dot(distanceVector, aimDirection);
                if (cosAngle > 0f && !aimBlocked) {
                    forwardCullResults.Add(target);
                }
            }
        }

        Debug.Assert(forwardCullResults != null);
        return forwardCullResults;
    }


    // Main function to cull all enemies so that only enemies that are in proximity to the aim direction (hits a circle around them) are considered: O(E) time, O(E) space
    //  Pre: aimDirection is the direction of the attack (aimDirection.y == 0f), playerPosition is the position of the player, enemyCandidates is a non-null list, enemyAimAssistRadius >= 0f
    //  Post: returns a list of enemies that are still viable. If there are none, returns an empty list
    private List<Transform> proximityCull(Vector2 aimDirection, Vector2 playerPosition, List<Transform> enemyCandidates) {
        Debug.Assert(enemyCandidates != null && enemyAimAssistRadius >= 0f);

        List<Transform> proximityCullResults = new List<Transform>();

        // Go through all enemies that are still viable to cull
        foreach (Transform target in enemyCandidates) {
            // Get distance vector
            Vector2 tgtPosition = target.position;
            Vector2 distanceVector = tgtPosition - playerPosition;

            // Get radius vector perpendicular to distance vector on the XZ plane 
            Vector2 radiusVector = Vector3.Cross(distanceVector, Vector3.forward);
            radiusVector = radiusVector.normalized;

            // Get distance vector from playerPosition to radius point = (enemyPosition + (radius * radiusVector))
            float radius = target.lossyScale.x + enemyAimAssistRadius;
            Vector2 radiusPoint = tgtPosition + (radius * radiusVector);
            Vector2 radiusDistanceVector = radiusPoint - playerPosition;

            // Compare the angles between (distanceVector and aimDirection) AND (distanceVector and radiusDistanceVector). If aimDirction has a smaller angle (Cosine is bigger), add to the list
            float aimCosine = (Vector2.Dot(distanceVector, aimDirection)) / (distanceVector.magnitude * aimDirection.magnitude);
            float radiusCosine = (Vector2.Dot(distanceVector, radiusDistanceVector)) / (distanceVector.magnitude * radiusDistanceVector.magnitude);

            if (aimCosine >= radiusCosine) {
                proximityCullResults.Add(target);
            }
        }

        Debug.Assert(proximityCullResults != null);
        return proximityCullResults;
    }


    // On trigger enter: when enemy is in range
    private void OnTriggerEnter2D(Collider2D collider) {
        lock (targetLock) {
            nearbyTargets.Add(collider.transform);
        }
    }


    // On trigger exit: when enemy is out of range
    private void OnTriggerExit2D(Collider2D collider) {
        lock (targetLock) {
            nearbyTargets.Add(collider.transform);
        }
    }
}
