using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Death Sequence")]
    [SerializeField]
    private Image blackScreen = null;

    private Coroutine runningBlackScreenSequence;


    // Main public function to run black screen fade sequence
    //  Pre: all floats >= 0f and blackScreen != null
    //  Post: runs a black screen fade sequence using the black screen image
    public void runBlackScreenSequence(float fadeInTime, float solidTime, float fadeOutTime) {
        Debug.Assert(fadeInTime >= 0 && solidTime >= 0f && fadeOutTime >= 0f);

        if (runningBlackScreenSequence != null) {
            StopCoroutine(runningBlackScreenSequence);
            runningBlackScreenSequence = null;
        }

        if (blackScreen != null) {
            runningBlackScreenSequence = StartCoroutine(blackScreenFadeSequence(fadeInTime, solidTime, fadeOutTime));
        } else {
            Debug.LogWarning("Trying to run black screen sequence but no black screen attached to UI component");
        }
    }


    // Main IEnumerator sequence to do a black screen sequence if there isn't any
    //  Pre: all floats >= 0f and blackScreen != null
    //  Post: runs a black screen fade sequence using the black screen image
    private IEnumerator blackScreenFadeSequence(float fadeInTime, float solidTime, float fadeOutTime) {
        Debug.Assert(blackScreen != null);
        Debug.Assert(fadeInTime >= 0 && solidTime >= 0f && fadeOutTime >= 0f);

        float timer = 0f;
        blackScreen.color = Color.clear;

        // Fade in time
        while (timer < fadeInTime) {
            yield return 0;

            timer += Time.deltaTime;
            blackScreen.color = Color.Lerp(Color.clear, Color.black, timer / fadeInTime);
        }
        blackScreen.color = Color.black;
        timer = 0f;

        // Solid black time
        while (timer < solidTime) {
            yield return 0;

            timer += Time.deltaTime;
        }
        timer = 0f;

        // Fade out time
        while (timer < fadeOutTime) {
            yield return 0;

            timer += Time.deltaTime;
            blackScreen.color = Color.Lerp(Color.black, Color.clear, timer / fadeOutTime);
        }
        blackScreen.color = Color.clear;

        // Reset flags
        runningBlackScreenSequence = null;
    }
}
