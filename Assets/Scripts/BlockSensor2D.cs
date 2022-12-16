using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSensor2D : IBlockerSensor
{
    // Variables to update number of walls touching
    private int numWallsTouched = 0;
    private readonly object numWallsLock = new object();


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
            }
        }
    }

}
