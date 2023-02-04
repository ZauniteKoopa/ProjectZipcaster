using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyLock : AbstractLock
{
    private bool locked = true;
    [SerializeField]
    [Min(1)]
    private int keysRequired = 1;


    // Main function to reset the locks
    //  Pre: none
    //  Post: locks will be reset in their original positions in their deactivated state
    public override void reset() {
        locked = true;
    }


    // Abstract function to handle the event for when the lock is unlocked
    //  Pre: unlockEvent has been triggered
    //  Post: lock components are changed to reflect their unlocked state
    protected override void onUnlock() {
        Debug.Assert(locked);

        locked = false;
    }


    // Main trigger box function
    private void OnTriggerEnter2D(Collider2D collider) {
        if (locked) {
            IPlatformerStatus playerStatus = collider.GetComponent<IPlatformerStatus>();

            if (playerStatus != null && playerStatus.useKey(keysRequired)) {
                unlock();
            }
        }
    }
}
