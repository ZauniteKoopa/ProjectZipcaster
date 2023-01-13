using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OneHitPlatformerStatus : IPlatformerStatus
{
    // Main variables concerning dying
    [Header("Dying variables")]
    [SerializeField]
    [Min(0.01f)]
    private float dyingTime = 2f;
    [SerializeField]
    private GameObject[] otherVisualParts;
    private bool dying = false;

    // Reference variables
    private SpriteRenderer entityRender;
    private PlatformerPackage platformerPackage;

    // Current spawn point
    private Vector3 spawnPoint;



    // On awake, get the following reference components
    private void Awake() {
        entityRender = GetComponent<SpriteRenderer>();
        platformerPackage = GetComponent<PlatformerPackage>();

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

        // Wait out death sequence
        yield return new WaitForSeconds(dyingTime);

        // Enable unit and teleport to spawnPoint
        platformerRespawnEvent.Invoke();
        transform.position = spawnPoint;
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
