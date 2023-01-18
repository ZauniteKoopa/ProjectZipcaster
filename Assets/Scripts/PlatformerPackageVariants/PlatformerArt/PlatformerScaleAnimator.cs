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
    private PlatformerPackage platformer;

    private Vector3 originalScale;


    // On awake, check
    private void Awake() {
        if (platformer == null) {
            Debug.LogError("No platformer connected to this animator");
        }

        originalScale = transform.localScale;
    }
    
    // On update, render the scale
    private void Update() {
        if (platformer.isJumping) {
            float jumpState = platformer.jumpScaleStatus;

            transform.localScale = new Vector3(originalScale.x * xScale.Evaluate(jumpState),
                                                originalScale.y * yScale.Evaluate(jumpState),
                                                originalScale.z);
        } else {
            transform.localScale = originalScale;
        }
    }
}
