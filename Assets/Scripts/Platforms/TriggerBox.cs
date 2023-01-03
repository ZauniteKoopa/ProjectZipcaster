using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerBox : MonoBehaviour
{
    // Main Unity Event to do trigger box
    public UnityEvent triggerEvent;


    // Main function for handling collision triggers
    private void OnTriggerEnter2D(Collider2D collider) {
        triggerEvent.Invoke();
    }
}
