using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustedLaunchPoint : MonoBehaviour
{
    [Header("Launch Variables")]
    [SerializeField]
    [Min(0f)]
    private float upwardLaunchHeight = 0f;


    // Main function to get the estimated launch position
    public Vector2 getHookDestination() {
        return transform.position;
    }


    // Main function to get the launch height associated with the hook
    public float getUpwardLaunchHeight() {
        return upwardLaunchHeight;
    }
}
