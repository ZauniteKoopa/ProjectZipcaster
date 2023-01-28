using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticWindZone : IWindZone
{
    [SerializeField]
    private float windSpeed;
    [SerializeField]
    [Min(0f)]
    private float startupTime = 0.5f;
    private float curWindSpeed = 0f;
    private Coroutine runningStartup = null;


    // Main function to get the speed of the wind hitting the player character
    //  Pre: none
    //  Post: returns a float value. Positive corresponding with right, negative corresponding with left
    public override float getHorizontalWindSpeed() {
        return curWindSpeed;
    }


    // Main function to start the wind zone gradually
    //  Pre: none
    //  Post: wind zone has now started
    public override void startWindZone() {
        if (runningStartup != null) {
            StopCoroutine(runningStartup);
        }

        runningStartup = StartCoroutine(startupSequence());
    }


    // Main function to stop the wind zone
    //  Pre:
    //  Post: wind zone has been stopped
    public override void stopWindZone() {
        if (runningStartup != null) {
            StopCoroutine(runningStartup);
        }

        curWindSpeed = 0f;
    }


    // Main sequence to start up this wind zone gradually
    private IEnumerator startupSequence() {
        
        yield return new WaitForSeconds(startupTime * 0.5f);

        float timer = 0f;
        while (timer < startupTime * 0.5f) {
            yield return 0;

            timer += Time.deltaTime;
            curWindSpeed = Mathf.Lerp(0f, windSpeed, timer / startupTime);
        }

        curWindSpeed = windSpeed;
        runningStartup = null;
    }
}
