using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IStaticVisualEffect : MonoBehaviour
{
    // Main function to run orbital sequence
    //  Pre: effectDuration > 0f
    //  Post: executes the effect
    public abstract void executeVFX(float effectDuration);
}
