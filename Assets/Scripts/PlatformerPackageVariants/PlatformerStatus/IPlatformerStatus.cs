using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class IPlatformerStatus : MonoBehaviour
{
    public UnityEvent platformerRespawnEvent;

    // Main function for platformer package to take damage
    //  Pre: int damage will be greater than 0
    public abstract void damage(int dmgTaken);


    // Main function to change platformer package spawnPoint
    //  Pre: Vector3 newSpawnPoint is the spawn point that the player must travel to
    //  Post: player will now change to that spawn point
    public abstract void changeCheckpoint(Vector3 newSpawnPoint);
}
