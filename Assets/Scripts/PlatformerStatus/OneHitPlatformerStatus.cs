using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OneHitPlatformerStatus : IPlatformerStatus
{
    // Main UI component
    [SerializeField]
    PlayerUI mainPlayerUI = null;
    AudioSource speaker = null;

    // Main variables concerning dying
    [Header("Dying variables")]
    [SerializeField]
    private GameObject[] otherVisualParts;
    [SerializeField]
    private AudioClip deathSound;
    private bool dying = false;

    // Main death UI sequence
    [Header("Death UI Sequence")]
    [SerializeField]
    [Min(0.01f)]
    private float dyingTime = 2f;
    [SerializeField]
    [Min(0f)]
    private float deathFadeIn = 0f;
    [SerializeField]
    [Min(0f)]
    private float deathSolidTime = 0f;
    [SerializeField]
    [Min(0f)]
    private float deathFadeOut = 0f;

    // Reference variables
    private SpriteRenderer entityRender;
    private PlatformerPackage platformerPackage;

    // Current spawn point
    private Vector3 spawnPoint;



    // On awake, get the following reference components
    private void Awake() {
        entityRender = GetComponent<SpriteRenderer>();
        platformerPackage = GetComponent<PlatformerPackage>();
        speaker = GetComponent<AudioSource>();

        if (entityRender == null) {
            Debug.LogError("Renderer not found on this platformer character");
        }

        if (platformerPackage == null) {
            Debug.LogError("No platformer package connected to this character");
        }

        spawnPoint = transform.position;
    }
    
    
    // Main function for platformer package to take damage
    //  Pre: int damage will be greater than 0
    public override void damage(int dmgTaken) {
        if (!dying) {
            dying = true;
            StartCoroutine(deathSequence());
        }
    }


    // Main sequence to do the death sequence
    //  Post: disable unit and make him invisible for a given amount of time. Then respawn at last spawnPoint
    private IEnumerator deathSequence() {
        dying = true;

        // Disable unit
        entityRender.enabled = false;
        platformerPackage.reset();
        platformerPackage.enabled = false;

        foreach (GameObject visualPart in otherVisualParts) {
            visualPart.SetActive(false);
        }

        // showcase death
        speaker.clip = deathSound;
        speaker.Play();
        yield return new WaitForSeconds(dyingTime);

        // Run black screen sequence and wait out fade in
        mainPlayerUI.runBlackScreenSequence(deathFadeIn, deathSolidTime, deathFadeOut);
        yield return new WaitForSeconds(deathFadeIn);

        // Teleport to spawn point in darkness and then wait out the rest of the black out sequence
        transform.position = spawnPoint;
        yield return new WaitForSeconds(deathSolidTime + deathFadeOut);

        // Enable unit 
        platformerRespawnEvent.Invoke();
        entityRender.enabled = true;
        platformerPackage.enabled = true;
        foreach (GameObject visualPart in otherVisualParts) {
            visualPart.SetActive(true);
        }

        dying = false;
    }
    


    // Main function to change platformer package spawnPoint
    //  Pre: Vector3 newSpawnPoint is the spawn point that the player must travel to
    //  Post: player will now change to that spawn point
    public override void changeCheckpoint(Vector3 newSpawnPoint) {
        spawnPoint = newSpawnPoint;
    }
}
