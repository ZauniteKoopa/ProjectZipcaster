using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformerScaleAnimator : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve xScale;
    [SerializeField]
    private AnimationCurve yScale;
    [SerializeField]
    private AnimationCurve splatLandingAnimation;
    [SerializeField]
    private PlatformerPackage platformer;
    [SerializeField]
    [Min(0.01f)]
    private float landingTime = 0.25f;

    private Vector3 originalScale;
    private Coroutine runningLandingSequence = null;


    // On awake, check
    private void Awake() {
        if (platformer == null) {
            Debug.LogError("No platformer connected to this animator");
        }

        platformer.platformerLandEvent.AddListener(onPlatformerLand);
        originalScale = transform.localScale;
    }
    
    // On update, render the scale
    private void Update() {
        if (platformer.isJumping) {
            // Reset runningLandingSequence if that's running
            if (runningLandingSequence != null) {
                StopCoroutine(runningLandingSequence);
                runningLandingSequence = null;
            }

            // Calculate jump scale dynamically
            float jumpState = platformer.jumpScaleStatus;

            transform.localScale = new Vector3(originalScale.x * xScale.Evaluate(jumpState),
                                                originalScale.y * yScale.Evaluate(jumpState),
                                                originalScale.z);

        } else if (runningLandingSequence == null) {
            transform.localScale = originalScale;
        }
    }


    // Main event handler function for when 
    private void onPlatformerLand() {
        if (runningLandingSequence == null) {
            runningLandingSequence = StartCoroutine(landingSequence());
        }
    }


    // Main sequence when landing, do a small squash
    private IEnumerator landingSequence() {
        // Set up loop
        Vector3 squashScale = new Vector3(originalScale.x * xScale.Evaluate(0f),
                                        originalScale.y * yScale.Evaluate(0f),
                                        originalScale.z);

        float timer = 0f;
        transform.localScale = squashScale;

        // Loop: interpolate the squash scale to original scale
        while (timer < landingTime) {
            yield return 0;

            timer += Time.deltaTime;
            float frameProgress = splatLandingAnimation.Evaluate(timer / landingTime);
            transform.localScale = Vector3.Lerp(squashScale, originalScale, frameProgress);
        }

        // Clean up
        transform.localScale = originalScale;
        runningLandingSequence = null;
    }
}
