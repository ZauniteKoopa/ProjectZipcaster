using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPath : MonoBehaviour
{
    [SerializeField]
    private Vector2[] localPositions;
    [SerializeField]
    [Min(0.01f)]
    private float movementSpeed = 0.2f;
    [SerializeField]
    [Min(0f)]
    private float stayTime = 0.2f;

    private int posIndex = 0;

    
    // On awake, error check and start sequence
    private void Awake() {
        if (localPositions.Length < 2) {
            Debug.LogError("AT LEAST 2 POSITIONS SHOULD BE SET FOR THE PATH OF THIS MOVING HITBOX");
        }

        StartCoroutine(moveSequence());
    }
    
    
    // Main IEnumerator
    private IEnumerator moveSequence() {
        transform.localPosition = localPositions[0];

        while (true) {
            // Wait at current position 
            yield return new WaitForSeconds(stayTime);

            // Calculate positions and time it takes
            posIndex = (posIndex + 1) % localPositions.Length;
            Vector2 from = transform.position;
            Vector2 to = (transform.parent == null) ? localPositions[posIndex] : (Vector2)transform.parent.TransformPoint(localPositions[posIndex]);

            float timer = 0f;
            float maxTime = Vector2.Distance(from, to) / movementSpeed;

            // Set timer loop and do a simple LERP
            while (timer < maxTime) {
                yield return 0;

                timer += Time.deltaTime;
                transform.position = Vector2.Lerp(from, to, timer / maxTime);
            }

            transform.position = to;
        }
    }
}
