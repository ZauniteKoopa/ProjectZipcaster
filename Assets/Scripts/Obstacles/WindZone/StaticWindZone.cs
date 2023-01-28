using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticWindZone : IWindZone
{
    [SerializeField]
    private float windSpeed;


    // Main function to get the speed of the wind hitting the player character
    //  Pre: none
    //  Post: returns a float value. Positive corresponding with right, negative corresponding with left
    public override float getHorizontalWindSpeed() {
        return windSpeed;
    }
}
