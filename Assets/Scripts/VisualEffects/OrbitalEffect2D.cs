using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalEffect2D : IStaticVisualEffect
{
    [Header("Radius effect")]
    [SerializeField]
    [Min(0.0f)]
    private float maxLocalRadius;
    [SerializeField]
    private AnimationCurve radiusAnimationGrowth;

    [Header("Orbital Effect")]
    [SerializeField]
    private float rotationalSpeed = 0f;

    private Coroutine runningOrbitalSequence = null;


    // On awake, check variables
    private void Awake() {
        if (transform.childCount <= 0) {
            Debug.LogError("No children found to orbit this area");
        }
    }


    // Main function to run orbital sequence
    //  Pre: effectDuration > 0f
    //  Post: executes the effect
    public override void executeVFX(float effectDuration) {
        Debug.Assert(effectDuration > 0f);

        if (runningOrbitalSequence != null) {
            StopCoroutine(runningOrbitalSequence);
        }

        gameObject.SetActive(true);
        runningOrbitalSequence = StartCoroutine(orbitalSequence(effectDuration));
    }


    // Main sequence to run orbital effect
    //  Pre: child count > 0 and orbitalDuration > 0f
    //  Post: run orbital sequence
    private IEnumerator orbitalSequence(float orbitalDuration) {
        Debug.Assert(orbitalDuration > 0f && transform.childCount > 0f);

        // Set initial positions
        float timer = 0f;
        setRadialPositions(0f);

        // Main loop
        while (timer < orbitalDuration) {
            yield return 0;
            timer += Time.deltaTime;

            // Set positions accordingly
            setRadialPositions(timer / orbitalDuration);
            transform.Rotate(rotationalSpeed * Time.deltaTime * Vector3.forward);
        }

        // Clean up
        setRadialPositions(1f);
        runningOrbitalSequence = null;
        gameObject.SetActive(false);
    }



    // Main function to set all orbital positions of this parent given a parametric value t
    //  Pre: transform has children and t is between 0 and 1
    //  Post: radial positions are set. DOES NOT INCLUDE ANGULAR POSITION
    private void setRadialPositions(float t) {
        Debug.Assert(transform.childCount > 0);

        float degreePerChild = 360f / transform.childCount;

        for (int c = 0; c < transform.childCount; c++) {
            Transform curChild = transform.GetChild(c);
            float angleDegree = degreePerChild * c;

            // Get variables for vector calculations
            float angleRads = angleDegree * Mathf.Deg2Rad;
            float curRadius = maxLocalRadius * radiusAnimationGrowth.Evaluate(Mathf.Clamp(t, 0f, 1f));

            // Set position
            curChild.localPosition = new Vector2(curRadius * Mathf.Cos(angleRads), curRadius * Mathf.Sin(angleRads));
        }
    }
}
