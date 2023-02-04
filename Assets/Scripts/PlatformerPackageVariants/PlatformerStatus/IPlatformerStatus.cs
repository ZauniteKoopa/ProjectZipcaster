using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class IPlatformerStatus : MonoBehaviour
{
    public UnityEvent platformerRespawnEvent;
    public UnityEvent platformerDeathEvent;

    [SerializeField]
    protected PlayerUI mainPlayerUI = null;
    private int heldKeys = 0;

    // Main function for platformer package to take damage
    //  Pre: int damage will be greater than 0
    public abstract void damage(int dmgTaken);


    // Main function to change platformer package spawnPoint
    //  Pre: Vector3 newSpawnPoint is the spawn point that the player must travel to
    //  Post: player will now change to that spawn point
    public abstract void changeCheckpoint(Vector3 newSpawnPoint);


    // Main function to obtain a key
    //  Pre: none
    //  Post: IPlatformerStatus now holds a key
    public void addKey() {
        heldKeys++;
        mainPlayerUI.displayKeys(heldKeys);
    }


    // Main function to use a key
    //  Pre: lockReq > 0
    //  Post: returns true if a key has been successfully used. False if no keys available
    public bool useKey(int lockReq = 1) {
        Debug.Assert(lockReq > 0);

        if (heldKeys >= lockReq) {
            heldKeys -= lockReq;
            mainPlayerUI.displayKeys(heldKeys);
            return true;
        }

        return false;
    }
}
