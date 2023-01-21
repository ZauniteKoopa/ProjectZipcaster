using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public abstract class AbstractLock : MonoBehaviour
{
    // Main unity event when the lock has be unlocked
    public UnityEvent unlockEvent;
    
    
    // Main function to reset the locks
    //  Pre: none
    //  Post: locks will be reset in their original positions in their deactivated state
    public abstract void reset();


    // Main function to unlock the lock
    //  Pre: none
    //  Post: door will now be unlocked. SHOULD ONLY TRIGGER ONCE UNTIL NEXT RESET
    public void unlock() {
        unlockEvent.Invoke();
        onUnlock();
    }


    // Abstract function to handle the event for when the lock is unlocked
    //  Pre: unlockEvent has been triggered
    //  Post: lock components are changed to reflect their unlocked state
    protected abstract void onUnlock();
}
