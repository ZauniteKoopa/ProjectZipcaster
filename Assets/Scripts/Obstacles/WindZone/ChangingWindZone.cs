using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangingWindZone : IWindZone
{
    [SerializeField]
    private float windSpeed1 = 0f;
    [SerializeField]
    private float windSpeed2 = -5f;
    [SerializeField]
    [Min(0.01f)]
    private float windStayDuration = 2f;
    [SerializeField]
    [Min(0.01f)]
    private float windTransitionDuration = 0.5f;
    private float curWindSpeed = 0f;
    private bool onWind1 = true;
    private Coroutine runningWindSequence = null;


    // Main function to start the wind zone gradually
    //  Pre: none
    //  Post: wind zone has now started
    public override void startWindZone() {
        if (runningWindSequence != null) {
            StopCoroutine(runningWindSequence);
        }

        runningWindSequence = StartCoroutine(changeWindSequence());;
    }


    // Main function to stop the wind zone
    //  Pre:
    //  Post: wind zone has been stopped
    public override void stopWindZone() {
        if (runningWindSequence != null) {
            StopCoroutine(runningWindSequence);
        }

        onWind1 = true;
    }


    // Main sequence for changing wind
    private IEnumerator changeWindSequence() {
        while (true) {
            // Staying period
            curWindSpeed = (onWind1) ? windSpeed1 : windSpeed2;
            yield return new WaitForSeconds(windStayDuration);

            // Transition period
            float timer = 0f;
            float from = curWindSpeed;
            float to = (onWind1) ? windSpeed2 : windSpeed1;

            while (timer < windTransitionDuration) {
                yield return 0;

                timer += Time.deltaTime;
                curWindSpeed = Mathf.Lerp(from, to, timer / windTransitionDuration);
            }

            onWind1 = !onWind1;
        }
    }


    // Main function to get the speed of the wind hitting the player character
    //  Pre: none
    //  Post: returns a float value. Positive corresponding with right, negative corresponding with left
    public override float getHorizontalWindSpeed() {
        return curWindSpeed;
    }

}
