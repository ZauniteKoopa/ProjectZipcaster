using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IWindZone : MonoBehaviour
{
    // Main function to get the speed of the wind hitting the player character
    //  Pre: none
    //  Post: returns a float value. Positive corresponding with right, negative corresponding with left
    public abstract float getHorizontalWindSpeed();
}
