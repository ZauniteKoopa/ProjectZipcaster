using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSensor2D : IBlockerSensor
{
    // Variables to update number of walls touching
    private int numWallsTouched = 0;
    private readonly object numWallsLock = new object();
    private HashSet<IDynamicPlatform> touchingDynamicPlatforms = new HashSet<IDynamicPlatform>();
    [SerializeField]
    private bool triggerDynamicPlatforms = true;


    // Main function to check sensor
    //  returns true is numWallsTocuhed > 0
    public override bool isBlocked() {
        bool blocked = false;

        lock(numWallsLock) {
            blocked = numWallsTouched > 0;
        }

        return blocked;
    }


    // Main event handler function to collect colliders as they enter the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Increments numWallsTouched
    private void OnTriggerEnter2D(Collider2D collider) {
        int colliderLayer = collider.gameObject.layer;

        // Case if you hit the enviornment
        if (colliderLayer == LayerMask.NameToLayer("Collisions")){
            lock(numWallsLock) {
                if (numWallsTouched == 0) {
                    blockedStartEvent.Invoke();
                }
                
                numWallsTouched++;
                if (triggerDynamicPlatforms) {
                    addDynamicPlatform(collider);
                }
            }
        }
    }

    // Main event handler function to remove colliders when they exit the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Decrements numWallsTouched
    private void OnTriggerExit2D(Collider2D collider) {
        int colliderLayer = collider.gameObject.layer;

        // Case if you hit the enviornment
        if (colliderLayer == LayerMask.NameToLayer("Collisions")){
            lock(numWallsLock) {
                numWallsTouched -= (numWallsTouched == 0) ? 0 : 1;

                if (triggerDynamicPlatforms) {
                    removeDynamicPlatform(collider);
                }
            }
        }
    }


    // Private helper function to add dynamic platform to the list if it isn't already
    //  Pre: collider != null
    //  Post: if collider has a dynamic platform component, then process that
    private void addDynamicPlatform(Collider2D collider) {
        Debug.Assert(collider != null);

        IDynamicPlatform curDynamicPlatform =  collider.GetComponent<IDynamicPlatform>();
        if (curDynamicPlatform != null) {
            touchingDynamicPlatforms.Add(curDynamicPlatform);
            curDynamicPlatform.onEntityLand(transform.parent);
        }
    }


    // Private helper function to add dynamic platform to the list if it isn't already
    //  Pre: collider != null
    //  Post: if collider has a dynamic platform component, then process that
    private void removeDynamicPlatform(Collider2D collider) {
        Debug.Assert(collider != null);

        IDynamicPlatform curDynamicPlatform =  collider.GetComponent<IDynamicPlatform>();
        if (curDynamicPlatform != null && touchingDynamicPlatforms.Contains(curDynamicPlatform)) {
            touchingDynamicPlatforms.Remove(curDynamicPlatform);
            curDynamicPlatform.onEntityLeave(transform.parent);
        }
    }
}
