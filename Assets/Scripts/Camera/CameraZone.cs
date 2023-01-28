using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField]
    protected Camera cameraMock;
    private IWindZone windZone;
    [SerializeField]
    private Transform[] possibleSpawnPoints;
    [SerializeField]
    private LockedDoor[] lockedDoors;
    private bool visited = false;


    // On awake, disable cameraMock
    private void Awake() {
        if (cameraMock == null) {
            Debug.LogError("Camera mock not set up for camera zone!");
        }

        // Disable elements that are not necessary for render within the game
        foreach(Transform spawnPoint in possibleSpawnPoints) {
            spawnPoint.gameObject.SetActive(false);
        }

        windZone = GetComponent<IWindZone>();
        cameraMock.gameObject.SetActive(false);
        initialize();
    }


    // Main function to do any additional initialization
    protected virtual void initialize() {}


    // On trigger enter, set camera to this position
    private void OnTriggerEnter2D(Collider2D collider) {
        if (collider.tag == "Player") {
            OnPlayerTrigger(collider);
        }
    }


    // Main function to handle player collision events
    //  Pre: collider is the player's collider
    protected virtual void OnPlayerTrigger(Collider2D collider) {
        // Move the camera
        Transform cameraTransform = cameraMock.transform;
        PlayerCameraController.moveCamera(transform, cameraTransform.localPosition, cameraTransform.transform.localRotation);

        // Get spawn point if there's any
        setPlayerSpawnPoint(collider);

        // Connect locked doors to player respawn event
        connectPlayerToRoom(collider);
    }


    // Main function to set spawn pont of player
    protected void setPlayerSpawnPoint(Collider2D playerCollider) {
        IPlatformerStatus playerStatus = playerCollider.GetComponent<IPlatformerStatus>();
        Vector2 playerPosition = playerCollider.transform.position;

        if (playerStatus != null && possibleSpawnPoints.Length > 0) {
            float minDistance = Vector2.Distance(playerPosition, possibleSpawnPoints[0].position);
            Vector2 curSpawnPoint = possibleSpawnPoints[0].position;

            // If there's more than 1 spawn point, choose the one that's closest to the unit when they entered
            for (int i = 1; i < possibleSpawnPoints.Length; i++) {
                float curDistance = Vector2.Distance(playerPosition, possibleSpawnPoints[i].position);

                if (curDistance < minDistance) {
                    minDistance = curDistance;
                    curSpawnPoint = possibleSpawnPoints[i].position;
                }
            }

            playerStatus.changeCheckpoint(curSpawnPoint);
        }
    }

    // Main function to connect player to room
    protected void connectPlayerToRoom(Collider2D playerCollider) {
        if (!visited) {

            // Connect events to locked doors
            IPlatformerStatus playerStatus = playerCollider.GetComponent<IPlatformerStatus>();
            if (playerStatus != null) {
                foreach(LockedDoor lockedDoor in lockedDoors) {
                    playerStatus.platformerRespawnEvent.AddListener(lockedDoor.reset);
                }
            }
        }

        PlatformerPackage playerPackage = playerCollider.GetComponent<PlatformerPackage>();
        if (playerPackage != null) {
            playerPackage.setWindZone(windZone);
        }

        visited = true;
    }
}
