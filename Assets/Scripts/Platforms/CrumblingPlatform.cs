using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrumblingPlatform : IDynamicPlatform
{
    // serialized static variables
    [SerializeField]
    private Color ghostColor;
    [SerializeField]
    private Color crumbleColor;
    private Color solidColor;

    [SerializeField]
    private float crumbleTime = 0.5f;
    [SerializeField]
    private float disappearTime = 3f;
    [SerializeField]
    private LayerMask collisionMask;

    // Runtime variables
    private bool isSolid = true;
    private Collider2D platformCollider;
    private AudioSource crumblePlatformSpeaker = null;

    
    // On awake, initialize solid color to the current color of the sprite currently
    private void Awake() {
        solidColor = GetComponent<SpriteRenderer>().color;
        platformCollider = GetComponent<Collider2D>();

        if (platformCollider == null) {
            Debug.LogError("no collider attached to this crumbling platform");
        }

        crumblePlatformSpeaker = GetComponent<AudioSource>();
        if (crumblePlatformSpeaker == null) {
            Debug.LogWarning("No speaker attached to crumbling platform. Crumbling platform will not make any noise");
        }
    }


    // Main sequence to do crumbling sequence
    //  Pre: isSolid was true
    //  Post: platform will disappear
    private IEnumerator crumbleSequence() {
        Debug.Assert(isSolid);

        // Make platform go to crumble state
        GetComponent<SpriteRenderer>().color = crumbleColor;
        yield return new WaitForSeconds(crumbleTime);

        // Make platform disappear
        isSolid = false;
        platformCollider.enabled = false;
        GetComponent<SpriteRenderer>().color = ghostColor;
        
        // Play crumble noise
        if (crumblePlatformSpeaker != null) {
            crumblePlatformSpeaker.Play();
        }

        // Wait disappear duration
        yield return new WaitForSeconds(disappearTime);

        // Wait for entities to get out of the way
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, transform.lossyScale, 0f, Vector2.up, 0f, collisionMask);
        while (hit.collider != null) {
            yield return 0;
            hit = Physics2D.BoxCast(transform.position, transform.lossyScale, 0f, Vector2.up, 0f, collisionMask);
        }

        // Make platform appear again
        isSolid = true;
        platformCollider.enabled = true;
        GetComponent<SpriteRenderer>().color = solidColor;
    }


    // Main function to handle the case in which an entity lands on this platform
    //  Pre: entity doesn't equal null
    public override void onEntityLand(Transform entity) {
        if (isSolid) {
            StartCoroutine(crumbleSequence());
        }
    }


    // Main function to handle the case in which an entity leaves the platform
    //  Pre: entity doesn't equal null
    public override void onEntityLeave(Transform entity) {}
}
