using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySensor2D : MonoBehaviour
{
    // Main unity events to connect to
    public UnityEvent sensePlayerEvent;
    public UnityEvent losePlayerEvent;

    // Forget functionality
    [SerializeField]
    [Min(0.05f)]
    private float forgetDuration = 1.5f;
    private Coroutine runningForgetSequence = null;
    private IPlatformerStatus currentTarget = null;


    // On trigger enter, if it's a player, fire
    private void OnTriggerEnter2D(Collider2D collider) {
        IPlatformerStatus playerStatus = collider.GetComponent<IPlatformerStatus>();

        if (playerStatus != null) {
            // If in the process of forgetting, stop process
            if (runningForgetSequence != null) {
                StopCoroutine(runningForgetSequence);
                runningForgetSequence = null;
            }

            if (currentTarget == null) {
                currentTarget = playerStatus;
                sensePlayerEvent.Invoke();
            }
        }
    }


    // On trigger exit, if it's a player trigger forgetting sequence
    private void OnTriggerExit2D(Collider2D collider) {
        IPlatformerStatus playerStatus = collider.GetComponent<IPlatformerStatus>();

        // Check that sensor is in aggro state and not already in the process of forgetting before forgetting
        if (playerStatus != null && currentTarget != null && runningForgetSequence == null) {
            runningForgetSequence = StartCoroutine(forgetSequence());
        }
    }


    // Main forgetting sequence
    //  Pre: none
    //  Post: after a delay, enemy will stop targeting the player
    private IEnumerator forgetSequence() {
        yield return new WaitForSeconds(forgetDuration);

        currentTarget = null;
        losePlayerEvent.Invoke();
        runningForgetSequence = null;
    }


    // Main function to access the current target of this sensor. if no target found, return null
    public IPlatformerStatus getTarget() {
        return currentTarget;
    }
}
