using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public class MovingProjectile : MonoBehaviour
{
    private float speed;
    private bool fired = false;
    private bool invulnerable = true;

    private const float INVULNERABLE_TIME = 0.25f;
    private const float TIMEOUT = 10f;
    private float expirationTimer = 0f;


    // Main function to start bullet
    public void startBullet(float s, Vector3 dir) {
        Debug.Assert(s > 0f);

        speed = s;
        transform.right = dir.normalized;
        fired = true;

        StartCoroutine(initialInvulnerabilitySequence());
    }


    // On update, move the bullet at the specified direction and update expiration timer
    private void Update() {
        if (fired) {
            transform.Translate(Time.deltaTime * speed * transform.right, Space.World);

            expirationTimer += Time.deltaTime;
            if (expirationTimer >= TIMEOUT) {
                Object.Destroy(gameObject);
            }
        }
    }


    // Main sequence that does initial invulnerability period
    private IEnumerator initialInvulnerabilitySequence() {
        yield return new WaitForSeconds(INVULNERABLE_TIME);
        invulnerable = false;
    }


    // On trigger enter
    //  destroy object if it's a player or a platform
    private void OnTriggerEnter2D(Collider2D collider) {
        int colliderLayer = collider.gameObject.layer;
        bool hit = colliderLayer == LayerMask.NameToLayer("Collisions");

        if (!invulnerable) {
            if (colliderLayer == LayerMask.NameToLayer("Collisions")) {
                Object.Destroy(gameObject);
            }

            if (colliderLayer == LayerMask.NameToLayer("Entity")) {
                StartCoroutine(delayedDestruction(0.1f));
            }
        }
    }


    // Delayed destruction sequence
    private IEnumerator delayedDestruction(float delay) {
        Debug.Assert(delay > 0f);

        yield return new WaitForSeconds(delay);
        Object.Destroy(gameObject);
    }
}
