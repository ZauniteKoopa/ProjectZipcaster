using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class IBlockerSensor : MonoBehaviour
{
    // Main event handler for when blocking starts
    public UnityEvent blockedStartEvent;


    // Main function to check if the sensor senses something
    //  Pre: none, make sure collision layers are specified to reduce performance cost
    //  Post: return if something is touching this sensor
    public abstract bool isBlocked();
}
