using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider) {
        IPlatformerStatus platformerStatus = collider.GetComponent<IPlatformerStatus>();

        if (platformerStatus != null) {
            platformerStatus.addKey();
            platformerStatus.changeCheckpoint(transform.position);
            Object.Destroy(gameObject);
        }
    }
}
