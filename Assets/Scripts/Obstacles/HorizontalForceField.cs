using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalForceField : MonoBehaviour
{
    [SerializeField]
    [Min(0f)]
    private float forceSpeed = 1f;
    [SerializeField]
    [Min(0.1f)]
    private float forceLingerTime = 0.1f;
    [SerializeField]
    private bool rightDir = true;

    // On Trigger enter, set player constant inertia
    private void OnTriggerEnter2D(Collider2D collider) {
        PlatformerPackage platformerPackage = collider.GetComponent<PlatformerPackage>();

        if (platformerPackage != null) {
            platformerPackage.setConstantHorizontalForce(forceSpeed, rightDir);
        }
    }


    // On trigger exit, set lingering force
    private void OnTriggerExit2D(Collider2D collider) {
        PlatformerPackage platformerPackage = collider.GetComponent<PlatformerPackage>();

        if (platformerPackage != null) {
            platformerPackage.setConstantHorizontalForce(0f, rightDir);
            platformerPackage.runInertiaSequence(forceSpeed, forceLingerTime, rightDir);
        }
    }
}
