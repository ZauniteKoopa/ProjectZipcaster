using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalForceField : MonoBehaviour
{
    [SerializeField]
    [Min(0f)]
    private float launchForce = 1f;
    [SerializeField]
    private AudioSource forceFieldEnterSound;

    // On Trigger enter, set player constant inertia
    private void OnTriggerStay2D(Collider2D collider) {
        PlatformerPackage platformerPackage = collider.GetComponent<PlatformerPackage>();

        if (platformerPackage != null) {
            platformerPackage.launchVerticallySpeed(launchForce);
        }
    }


    // On trigger enter, play trampoline jump sound
    private void OnTriggerEnter2D(Collider2D collider) {
        PlatformerPackage platformerPackage = collider.GetComponent<PlatformerPackage>();

        if (platformerPackage != null && forceFieldEnterSound != null) {
            forceFieldEnterSound.Play();
        }
    }
}
