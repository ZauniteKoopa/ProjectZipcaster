using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider) {
        IPlatformerStatus platformerStatus = collider.GetComponent<IPlatformerStatus>();

        if (platformerStatus != null) {
            platformerStatus.damage(1);
        }
    }
}
