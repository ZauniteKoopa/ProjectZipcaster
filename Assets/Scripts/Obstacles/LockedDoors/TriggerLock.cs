using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TriggerLock : AbstractLock
{
    [SerializeField]
    private Color lockedColor;
    [SerializeField]
    private Color unlockedColor;
    private SpriteRenderer render;
    private bool locked = true;


    // ON awake, initialize variables
    private void Awake() {
        render = GetComponent<SpriteRenderer>();

        if (render == null) {
            Debug.LogError("No renderer attached to this trigger lock");
        }

        render.color = lockedColor;
    }
    
    
    // Main function to reset the locks
    //  Pre: none
    //  Post: locks will be reset in their original positions in their deactivated state
    public override void reset() {
        locked = true;
        render.color = lockedColor;
    }


    // Abstract function to handle the event for when the lock is unlocked
    //  Pre: unlockEvent has been triggered
    //  Post: lock components are changed to reflect their unlocked state
    protected override void onUnlock() {
        Debug.Assert(locked);

        locked = false;
        render.color = unlockedColor;
    }


    // Main trigger box function
    private void OnTriggerEnter2D(Collider2D collider) {
        if (locked) {
            IPlatformerStatus playerStatus = collider.GetComponent<IPlatformerStatus>();

            if (playerStatus != null) {
                unlock();
            }
        }
    }
}
