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

    protected Vector3 originalScale;
    private Coroutine runningLandingSequence = null;
    protected Animator animator;
    protected SpriteRenderer render;


    // On awake, check
    private void Awake() {
        if (platformer == null) {
            Debug.LogError("No platformer connected to this animator");
        }

        platformer.platformerLandEvent.AddListener(onPlatformerLand);
        originalScale = transform.localScale;

        animator = GetComponent<Animator>();
        if (animator == null) {
            Debug.LogWarning("No animator attached. Please attach an animator to animate platformer");
        }

        render = GetComponent<SpriteRenderer>();
        if (render == null) {
            Debug.LogError("No sprite render attached to animator. Please attach something that I can animate");
        }

        initialize(platformer);
    }


    // Main function to intialize for other children
    protected virtual void initialize(PlatformerPackage p) {}
    
    // On update, render the scale
    private void Update() {
        updateAnimatorVariables(platformer);
        render.flipX = (isFacingLeft(platformer));
        updateSpriteTransform(platformer);
    }


    // Main function to update sprite transform
    //  Pre: none
    //  Post: Transform variables are updated based on variables from the platformer package
    protected virtual void updateSpriteTransform(PlatformerPackage p) {
        if (p.isJumping) {
            // Reset runningLandingSequence if that's running
            if (runningLandingSequence != null) {
                StopCoroutine(runningLandingSequence);
                runningLandingSequence = null;
            }

            // Calculate jump scale dynamically
            float jumpState = p.jumpScaleStatus;

            transform.localScale = new Vector3(originalScale.x * xScale.Evaluate(jumpState),
                                                originalScale.y * yScale.Evaluate(jumpState),
                                                originalScale.z);

        } else if (runningLandingSequence == null) {
            transform.localScale = originalScale;
        }
    }


    // Main event handler function for when 
    private void onPlatformerLand() {
        if (runningLandingSequence == null && render.gameObject.activeInHierarchy) {
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


    // Main function to update animator variables
    //  Pre: none
    //  Post: updates animator variables based on platformer package state
    protected virtual void updateAnimatorVariables(PlatformerPackage p) {
        Debug.Assert(p != null);

        if (animator != null) {
            animator.SetBool("Grounded", p.grounded);
            animator.SetBool("Wallgrabbing", p.isGrabbingWall());
            animator.SetFloat("HorizontalSpeed", Mathf.Abs(p.getHorizontalAxis));
            animator.SetFloat("VerticalSpeed", p.getVerticalSpeed);
        }
    }


    // Main function to check if whether or not the sprite will face left
    //  Pre: platformer != null
    //  Post: returns a boolean to check if platformer is facing left
    protected virtual bool isFacingLeft(PlatformerPackage p) {
        Debug.Assert(p != null);

        return p.forwardDir != Vector2.right;
    }

}
