using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [SerializeField]
    private AbstractLock[] doorLocks;
    [SerializeField]
    private Vector3 localOpenPosition;
    [SerializeField]
    [Min(0.01f)]
    private float openingSpeed = 4f;
    [SerializeField]
    private bool resetOnComplete = false;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private int locksLeft;
    private AudioSource unlockedSpeaker;

    private Coroutine runningOpeningSequence = null;


    // Start is called before the first frame update
    void Awake()
    {
        // Error check
        if (doorLocks.Length <= 0 || doorLocks == null) {
            Debug.LogError("No locks attached to this locked door");
        }

        unlockedSpeaker = GetComponent<AudioSource>();
        if (unlockedSpeaker == null) {
            Debug.LogWarning("No speaker attached to this locked door: no sound will come out when you unlock it!");
        }

        // Set up lock state
        foreach(AbstractLock doorLock in doorLocks) {
            if (doorLock == null) {
                Debug.LogError("NULL LOCK FOUND UNDER LOCKED DOOR");
            }

            // Listen to unlock event
            doorLock.unlockEvent.AddListener(onUnlockedLock);

            // Detach locks if they're attached to this locked door for level organization
            if (doorLock.transform.parent == transform) {
                doorLock.transform.parent = transform.parent;
            }
        }

        locksLeft = doorLocks.Length;

        // Set up door state
        closedPosition = transform.position;
        openPosition = (transform.parent == null) ? localOpenPosition : transform.parent.TransformPoint(localOpenPosition);

    }

    // Main event handler function to handle when locks are unlocked
    //  Pre: a lock under this door has been unlocked, locksLeft > 0
    //  Post: decrement locksLeft, If locksLeft goes to 0, open the door
    private void onUnlockedLock() {
        if (locksLeft <= 0) {
            Debug.LogError("Approaching negative locksLeft! Did one of the locks trigger twice?");

        } else {
            locksLeft--;

            // If no locks left, open the door
            if (locksLeft <= 0) {
                runningOpeningSequence = StartCoroutine(openingSequence());
            }
        }


    }


    // Main IEnumerator sequence for opening the door
    //  Pre: none
    //  Post: the door will now open
    private IEnumerator openingSequence() {
        float openTime = Vector3.Distance(closedPosition, openPosition) / openingSpeed;
        float timer = 0f;

        if (unlockedSpeaker != null) {
            unlockedSpeaker.Play();
        }

        while (timer < openTime) {
            yield return 0;

            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(closedPosition, openPosition, timer / openTime);
        }

        transform.position = openPosition;
        runningOpeningSequence = null;
    }


    // Main function to reset the door
    //  Pre: none
    //  Post: resets all the locks and door states IMMEDIATELY
    public void reset() {

        // Check if you should reset
        if (resetOnComplete || locksLeft > 0) {
            // Stop opening coroutine if one is running
            if (runningOpeningSequence != null) {
                StopCoroutine(runningOpeningSequence);
                runningOpeningSequence = null;
            }

            // Reset all locks
            foreach(AbstractLock doorLock in doorLocks) {
                doorLock.reset();
            }

            // Reset door
            locksLeft = doorLocks.Length;
            transform.position = closedPosition;
        }
    }

}
