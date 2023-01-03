using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimeTrial : MonoBehaviour
{
    private float startTime;
    private float endTime;
    private bool going = false;

    [SerializeField]
    private TMP_Text timeLabel;



    // Main function to update label if it exist
    private void Update() {
        if (timeLabel != null) {
            timeLabel.text = getCurrentTime().ToString("F3");
        }
    }


    // Main function to handle event timer starts
    public void onTimerStart() {
        if (!going) {
            StartCoroutine(timerSequence());
        }
    }


    // Main function to handle event timer ends
    public void onTimerEnd() {
        if (going) {
            going = false;
        }
    }


    // Main timer sequence
    private IEnumerator timerSequence() {
        going = true;
        startTime = Time.time;

        while (going) {
            yield return 0;
            endTime = Time.time;
        }

        Debug.Log("FINISHED WITH " + getCurrentTime());
    }


    // Main function to get the current time
    //  Post: returns the time since you first started OR if you completed a circuit, the time it took to finish that circuit
    //        if you haven;t started the circuit at all, returns 0
    public float getCurrentTime() {
        return Mathf.Round((endTime - startTime) * 1000f) / 1000f;
    }
}
