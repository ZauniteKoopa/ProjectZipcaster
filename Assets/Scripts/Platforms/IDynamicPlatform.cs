using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class IDynamicPlatform : MonoBehaviour
{
    // Main function to handle the case in which an entity lands on this platform
    //  Pre: entity doesn't equal null
    public abstract void onEntityLand(Transform entity);


    // Main function to handle the case in which an entity leaves the platform
    //  Pre: entity doesn't equal null
    public abstract void onEntityLeave(Transform entity);
}
